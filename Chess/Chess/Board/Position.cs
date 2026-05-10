using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Chess.Bitboards;
using static Chess.CastlingRights;
using static Chess.Color;
using static Chess.Direction;
using static Chess.File;
using static Chess.FuncBit;
using static Chess.MoveType;
using static Chess.Piece;
using static Chess.PieceType;
using static Chess.Rank;
using static Chess.Square;
using static Chess.Types;
namespace Chess
{
    using Key = UInt64;
    public unsafe struct Position()
    {
        public Board Board;
        public ByTypeBB ByTypeBB;
        public ByColorBB ByColorBB;
        public Color SideToMove;
        public fixed int PieceCount[(int)PIECE_NB];
        public fixed int CastlingRightsMask[(int)SQ_NB];
        public CastlingRookSquare CastlingRookSquare;
        public CastlingPath CastlingPath;
        public StateInfo* st;
        public int gamePly;
        public int chess960;
        public DirtyPiece scratch_dp;
        public DirtyThreats scratch_dts;
        public static readonly Cuckoo Cuckoo;
        public static readonly CuckooMove CuckooMove;
        public const string PieceToChar = " PNBRQK  pnbrqk";
        static Position()
        {
            PRNG rng = new(1070372);
            Piece* pcStart = (Piece*)Unsafe.AsPointer(ref Unsafe.AsRef(in Pieces[0]));
            for (Piece* pcPtr = pcStart, pcEnd = pcStart + PieceArray12.Length; pcPtr != pcEnd; ++pcPtr)
            {
                Piece pc = *pcPtr;
                for (Square s = SQ_A1; s <= SQ_H8; ++s)
                {
                    Zobrist.Psq[(int)pc, (int)s] = rng.Rand<Key>();
                }
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)W_PAWN, (int)SQ_A8 + i] = 0;
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)B_PAWN, i] = 0;
            }
            for (File f = FILE_A; f <= FILE_H; ++f)
            {
                Zobrist.EnPassant[(int)f] = rng.Rand<Key>();
            }
            for (CastlingRights cr = NO_CASTLING; cr <= ANY_CASTLING; ++cr)
            {
                Zobrist.Castling[(int)cr] = rng.Rand<Key>();
            }
            Zobrist.Side = rng.Rand<Key>();
            Zobrist.NoPawns = rng.Rand<Key>();
            Cuckoo.Clear();
            CuckooMove.Clear();
            int count = 0;
            pcStart = (Piece*)Unsafe.AsPointer(ref Unsafe.AsRef(in Pieces[0]));
            for (Piece* pcPtr = pcStart, pcEnd = pcStart + PieceArray12.Length; pcPtr != pcEnd; ++pcPtr)
            {
                Piece pc = *pcPtr;
                for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
                {
                    for (Square s2 = (s1 + 1); s2 <= SQ_H8; ++s2)
                    {
                        if ((Type_Of(pc) != PAWN) && (Attacks_BB(pc, s1, 0) & s2) != 0)
                        {
                            Move move = new(s1, s2);
                            Key key = Zobrist.Psq[(int)pc, (int)s1] ^ Zobrist.Psq[(int)pc, (int)s2] ^ Zobrist.Side;
                            int i = H1(key);
                            while (true)
                            {
                                Swap(ref Cuckoo[i], ref key);
                                Swap(ref CuckooMove[i], ref move);
                                if (move == Move.None())
                                {
                                    break;
                                }
                                i = (i == H1(key)) ? H2(key) : H1(key);
                            }
                            count++;
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Position Set(string fenStr, bool isChess960, StateInfo* si)
        {
            char col, row, token;
            int idx;
            Square sq = SQ_A8;
            var ss = new StringReader(fenStr);
            Unsafe.InitBlock(Unsafe.AsPointer(ref this), 0, (uint)sizeof(Position));
            Unsafe.InitBlock(ref Unsafe.As<StateInfo, byte>(ref *si), 0, (uint)Unsafe.SizeOf<StateInfo>());
            st = si;
            int c;
            while ((c = ss.Read()) != -1)
            {
                token = (char)c;
                if (char.IsWhiteSpace(token))
                {
                    break;
                }   
                if (char.IsDigit(token))
                {
                    sq += (token - '0') * (int)EAST;
                }    
                else if (token == '/')
                {
                    sq += 2 * (int)SOUTH;
                }   
                else if ((idx = PieceToChar.IndexOf(token)) != -1)
                {
                    Put_Piece((Piece)idx, sq);
                    ++sq;
                }
            }
            token = (char)ss.Read();
            Color us = token == 'w' ? WHITE : BLACK;
            Color them = us == WHITE ? BLACK : WHITE;
            SideToMove = us;
            ss.Read(); 
            while ((c = ss.Read()) != -1)
            {
                token = (char)c;
                if (char.IsWhiteSpace(token))
                {
                    break;
                }    
                Square rsq;
                Color cside = char.IsLower(token) ? BLACK : WHITE;
                Piece rook = Make_Piece<Rook>(cside);
                token = char.ToUpper(token);
                if (token == 'K')
                {
                    for (rsq = Relativ_Square(cside, SQ_H1); Piece_On(rsq) != rook; rsq--)
                    { }
                }
                else if (token == 'Q')
                {
                    for (rsq = Relativ_Square(cside, SQ_A1); Piece_On(rsq) != rook; rsq++)
                    { }
                }
                else if (token >= 'A' && token <= 'H')
                {
                    rsq = Make_Square((File)(token - 'A'), Relativ_Rank(cside, RANK_1));
                }
                else
                {
                    continue;
                }
                Set_Castling_Right(cside, rsq);
            }
            bool enpassant = false, legalEP = false;
            col = (char)ss.Read();
            row = (char)ss.Read();
            if (col >= 'a' && col <= 'h' && row == (us == WHITE ? '6' : '3'))
            {
                st->EpSquare = Make_Square((File)col - 'a', (Rank)row - '1');
                Bitboard pawns = Attacks_BB<Pawn>(st->EpSquare, them) & Get_Pieces<Pawn>(us);
                Bitboard target = Get_Pieces<Pawn>(them) & (st->EpSquare + (int)Pawn_Push(them));
                Bitboard occ = Get_Pieces() ^ target ^ Square_BB(st->EpSquare);
                enpassant = pawns != 0 && target != 0 && (Get_Pieces() & (st->EpSquare | (st->EpSquare + (int)Pawn_Push(us)))) == 0;
                while (pawns != 0)
                {
                    legalEP |= (Attackers_To(Get_Square<King>(us), occ ^ Pop_Lsb(ref pawns)) & Get_Pieces(them) & ~target) == 0;
                }
            }
            if (!enpassant || !legalEP)
            {
                st->EpSquare = SQ_NONE;
            }    
            string rest = ss.ReadToEnd();
            var parts = rest.Trim().Split(' ');
            st->Rule50 = int.Parse(parts[0]);
            gamePly = int.Parse(parts[1]);
            gamePly = Math.Max(2 * (gamePly - 1), 0) + (us == BLACK ? 1 : 0);
            chess960 = isChess960 ? 1 : 0;
            if (us == WHITE)
            {
                Set_State<White, Black>();
            }
            else
            {
                Set_State<Black, White>();
            }
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool Empty(Square s)
        {
            return Piece_On(s) == NO_PIECE; 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string Fen(Color c)
        {
            int emptyCnt;
            StringBuilder sb = new();
            for (Rank r = RANK_8; ; r--)
            {
                for (File f = FILE_A; f <= FILE_H; f++)
                {
                    emptyCnt = 0;
                    while (f <= FILE_H && Empty(Make_Square(f, r)))
                    {
                        emptyCnt++;
                        f++;
                    }
                    if (emptyCnt != 0)
                    {
                        sb.Append(emptyCnt); 
                    }
                    if (f <= FILE_H)
                    {
                        sb.Append(PieceToChar[(int)Piece_On(Make_Square(f, r))]);
                    }
                }
                if (r == RANK_1)
                {
                    break;
                }
                sb.Append('/');
            }
            sb.Append(c == WHITE ? " w " : " b ");
            if (Can_Castle(WHITE_OO))
            {
                sb.Append(chess960 != 0 ? (char)('A' + File_Of(Castling_Rook_Square(WHITE_OO))) : 'K');
            }
            if (Can_Castle(WHITE_OOO))
            { 
                sb.Append(chess960 != 0 ? (char)('A' + File_Of(Castling_Rook_Square(WHITE_OOO))) : 'Q');
            }
            if (Can_Castle(BLACK_OO))
            { 
                sb.Append(chess960 != 0 ? (char)('a' + File_Of(Castling_Rook_Square(BLACK_OO))) : 'k');
            }
            if (Can_Castle(BLACK_OOO))
            {
                sb.Append(chess960 != 0 ? (char)('a' + File_Of(Castling_Rook_Square(BLACK_OOO))) : 'q');
            }
            if (!Can_Castle(ANY_CASTLING))
            {
                sb.Append('-');
            }
            if (Ep_Square() == SQ_NONE)
            {
                sb.Append(" - ");
            }
            else
            {
                sb.Append($" {Square_To_String(Ep_Square())} ");
            }
            sb.Append(st->Rule50);
            sb.Append(' ');
            sb.Append(1 + (gamePly - (c == BLACK ? 1 : 0)) / 2);

            return sb.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Set_Castling_Right(Color c, Square rfrom)
        {
            Square kfrom = Get_Square<King>(c);
            var QueenSide = c == WHITE ? WHITE_OOO : BLACK_OOO;
            var KingSide = c == WHITE ? WHITE_OO : BLACK_OO;
            CastlingRights cr = kfrom < rfrom ? KingSide : QueenSide;
            st->CastlingRights |= (int)cr;
            CastlingRightsMask[(int)kfrom] |= (int)cr;
            CastlingRightsMask[(int)rfrom] |= (int)cr;
            CastlingRookSquare[(int)cr] = rfrom;
            Square kto = Relativ_Square(c, (cr & KingSide) != 0 ? SQ_G1 : SQ_C1);
            Square rto = Relativ_Square(c, (cr & KingSide) != 0 ? SQ_F1 : SQ_D1);
            CastlingPath[(int)cr] = (Between_BB(rfrom, rto) | Between_BB(kfrom, kto)) 
                                  & ~(Square_BB(kfrom) | Square_BB(rfrom));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Set_Check_Info<C, N>() where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Bitboard pieces = Get_Pieces();
            Update_Slider_Blockers(C.Value, N.Value);
            Update_Slider_Blockers(N.Value, C.Value);
            Square ksq = Get_Square<King>(N.Value);
            st->CheckSquares[(int)PAWN] = Attacks_BB<Pawn>(ksq, N.Value);
            st->CheckSquares[(int)KNIGHT] = Attacks_BB<Knight>(ksq);
            st->CheckSquares[(int)BISHOP] = Attacks_BB<Bishop>(ksq, pieces);
            st->CheckSquares[(int)ROOK] = Attacks_BB<Rook>(ksq, pieces);
            st->CheckSquares[(int)QUEEN] = st->CheckSquares[(int)BISHOP] | st->CheckSquares[(int)ROOK];
            st->CheckSquares[(int)KING] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Set_State<C, N>() where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            st->Key = 0;
            st->MinorPieceKey = 0;
            st->NonPawnKey[(int)WHITE] = st->NonPawnKey[(int)BLACK] = 0;
            st->PawnKey = Zobrist.NoPawns;
            st->NonPawnMaterial[(int)WHITE] = st->NonPawnMaterial[(int)BLACK] = VALUE_ZERO;
            st->CheckersBB = Attackers_To(Get_Square<King>(C.Value)) & Get_Pieces(N.Value);
            Set_Check_Info<C, N>();
            Bitboard b = Get_Pieces();
            while (b != 0)
            {
                Square sq = Pop_Lsb(ref b);
                Piece pc = Piece_On(sq);
                st->Key ^= Zobrist.Psq[(int)pc, (int)sq];
                if (Type_Of(pc) == PAWN)
                {
                    st->PawnKey ^= Zobrist.Psq[(int)pc, (int)sq];
                }
                else
                {
                    st->NonPawnKey[(int)Color_Of(pc)] ^= Zobrist.Psq[(int)pc, (int)sq];
                    if (Type_Of(pc) != KING)
                    {
                        st->NonPawnMaterial[(int)Color_Of(pc)] += Piece_Value(pc);
                        if (Type_Of(pc) == BISHOP)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)pc, (int)sq];
                        }
                    }
                }
            }
            if (st->EpSquare != SQ_NONE)
            {
                st->Key ^= Zobrist.EnPassant[(int)File_Of(st->EpSquare)];
            }
            if (C.Value == BLACK)
            {
                st->Key ^= Zobrist.Side;
            }
            st->Key ^= Zobrist.Castling[st->CastlingRights];
            st->MaterialKey = Compute_Material_Key();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Key Compute_Material_Key()
        {
            Key k = 0;
            fixed (Piece* pcStart = &Pieces[0])
            {
                for (Piece* pcPtr = pcStart, pcEnd = pcStart + PieceArray12.Length; pcPtr != pcEnd; ++pcPtr)
                {
                    Piece pc = *pcPtr;
                    for (int cnt = 0; cnt < PieceCount[(int)pc]; ++cnt)
                    {
                        k ^= Zobrist.Psq[(int)pc, 8 + cnt];
                    }
                }
            }
            return k;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update_Slider_Blockers(Color us, Color them)
        {
            Square ksq = Get_Square<King>(us);
            st->BlockersForKing[(int)us] = 0;
            st->Pinners[(int)them] = 0;
            Bitboard snipers = ((Attacks_BB<Rook>(ksq) & Get_Pieces<Pieces<Rook, Queen>>()) 
                            | (Attacks_BB<Bishop>(ksq) & Get_Pieces<Pieces<Bishop, Queen>>())) & Get_Pieces(them);
            Bitboard occupancy = Get_Pieces() ^ snipers;
            while (snipers != 0)
            {
                Square sniperSq = Pop_Lsb(ref snipers);
                Bitboard b = Between_BB(ksq, sniperSq) & occupancy;
                if (b != 0 && !More_Than_One(b))
                {
                    st->BlockersForKing[(int)us] |= b;
                    if ((b & Get_Pieces(us)) != 0)
                    {
                        st->Pinners[(int)them] |= sniperSq;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Bitboard Attackers_To(Square s)
        {
            return Attackers_To(s, Get_Pieces());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Bitboard Attackers_To(Square s, Bitboard occupied)
        {
            return (Attacks_BB<Rook>(s, occupied) & Get_Pieces<Pieces<Rook, Queen>>())
                 | (Attacks_BB<Bishop>(s, occupied) & Get_Pieces<Pieces<Bishop, Queen>>())
                 | (Attacks_BB<Pawn>(s, BLACK) & Get_Pieces<Pawn>(WHITE))
                 | (Attacks_BB<Pawn>(s, WHITE) & Get_Pieces<Pawn>(BLACK))
                 | (Attacks_BB<Knight>(s) & Get_Pieces<Knight>())
                 | (Attacks_BB<King>(s) & Get_Pieces<King>());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Attackers_To_Exist<C, N>(Square s, Bitboard occupied) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return (Attacks_BB<Rook>(s, occupied) & Get_Pieces<Pieces<Rook, Queen>>(C.Value)) != 0
                || (Attacks_BB<Bishop>(s, occupied) & Get_Pieces<Pieces<Bishop, Queen>>(C.Value)) != 0
                || (Attacks_BB<Pawn>(s, N.Value) & Get_Pieces<Pawn>(C.Value)) !=0
                || (Attacks_BB<Knight>(s) & Get_Pieces<Knight>(C.Value)) !=0
                || (Attacks_BB<King>(s) & Get_Pieces<King>(C.Value)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Legal<C, N>(Move m) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            Bitboard pieces = Get_Pieces();
            if (Type_Of(m) == EN_PASSANT)
            {
                Square ksq = Get_Square<King>(C.Value);
                Square capsq = to - (int)C.Up;
                Bitboard occupied = (pieces ^ from ^ capsq) | to;
                return (Attacks_BB<Rook>(ksq, occupied) & Get_Pieces<Pieces<Queen, Rook>>(N.Value)) == 0 
                    && (Attacks_BB<Bishop>(ksq, occupied) & Get_Pieces<Pieces<Queen, Bishop>>(N.Value)) == 0;
            }
            if (Type_Of(m) == CASTLING)
            {
                to = Relativ_Square(C.Value, to > from ? SQ_G1 : SQ_C1);
                Direction step = to > from ? WEST : EAST;
                for (Square s = to; s != from; s += (int)step)
                {
                    if (Attackers_To_Exist<N, C>(s, pieces))
                    {
                        return false;
                    }    
                }
                return chess960 == 0 || (Blockers_For_King(C.Value) & To_Sq(m)) == 0;
            }
            if (Type_Of(Piece_On(from)) == KING)
            {
                return !(Attackers_To_Exist<N, C>(to, pieces ^ from));
            }
            return (Blockers_For_King(C.Value) & from) == 0 || (Line_BB(from, to) & Get_Pieces<King>(C.Value)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Put_Piece(Piece pc, Square s, DirtyThreats* dts = null) 
        {
            Color c = Color_Of(pc);
            Board[(int)s] = pc;
            ByTypeBB[(int)ALL_PIECES] |= ByTypeBB[(int)Type_Of(pc)] |= s;
            ByColorBB[(int)c] |= s;
            PieceCount[(int)pc]++;
            PieceCount[(int)Make_Piece<AllPiece>(c)]++;
            if (dts != null)
            {
                Update_Piece_Threats<True, True>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove_Piece(Square s, DirtyThreats* dts = null) 
        {
            Piece pc = Board[(int)s];
            if (dts != null)
            {
                Update_Piece_Threats<False, True>(pc, s, dts);
            }
            ByTypeBB[(int)ALL_PIECES] ^= s;
            ByTypeBB[(int)Type_Of(pc)] ^= s;
            ByColorBB[(int)Color_Of(pc)] ^= s;
            Board[(int)s] = NO_PIECE;
            PieceCount[(int)pc]--;
            PieceCount[(int)Make_Piece<AllPiece>(Color_Of(pc))]--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Move_Piece(Square from, Square to, DirtyThreats* dts = null)
        {
            Piece pc = Board[(int)from];
            Bitboard fromTo = Square_BB(from) | Square_BB(to);
            if (dts != null)
            {
                Update_Piece_Threats<False, True>(pc, from, dts, fromTo);
            }
            ByTypeBB[(int)ALL_PIECES] ^= fromTo;
            ByTypeBB[(int)Type_Of(pc)] ^= fromTo;
            ByColorBB[(int)Color_Of(pc)] ^= fromTo;
            Board[(int)from] = NO_PIECE;
            Board[(int)to] = pc;
            if (dts != null)
            {
                Update_Piece_Threats<True, True>(pc, to, dts, fromTo);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap_Piece(Square s, Piece pc, DirtyThreats* dts)
        {
            Piece old = Board[(int)s];
            Remove_Piece(s);
            if (dts != null)
            {
                Update_Piece_Threats<False, False>(old, s, dts);
            }
            Put_Piece(pc, s);
            if (dts != null)
            {
                Update_Piece_Threats<True, False>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Bitboard Check_Squares(PieceType pt) 
        { 
            return st->CheckSquares[(int)pt];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Capture(Move m)
        {
            return (Piece_On(To_Sq(m)) != NO_PIECE && Type_Of(m) != CASTLING) || Type_Of(m) == EN_PASSANT;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Capture_Stage(Move m)
        {
            return Capture(m) || Promotion_Type(m) == QUEEN;
        }
        public bool See_Ge(Move m, int threshold = 0)
        {
            if (Type_Of(m) != NORMAL)
            {
                return VALUE_ZERO >= threshold;
            }
            Square from = From_Sq(m), to = To_Sq(m);
            int swap = Piece_Value(Piece_On(to)) - threshold;
            if (swap < 0)
            {
                return false;
            }
            swap = Piece_Value(Piece_On(from)) - swap;
            if (swap <= 0)
            {
                return true;
            }
            Bitboard occupied = Get_Pieces() ^ Square_BB(from) ^ Square_BB(to); 
            Bitboard attackers = Attackers_To(to, occupied);
            return SideToMove == WHITE ? See_Ge_Next<Black, White>(to, occupied, attackers, swap, 1)
                                       : See_Ge_Next<White, Black>(to, occupied, attackers, swap, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool See_Ge_Next<C, N>(Square to, Bitboard occupied, Bitboard attackers, int swap, int res) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            attackers &= occupied;
            Bitboard stmAttackers, bb;
            if ((stmAttackers = attackers & Get_Pieces(C.Value)) == 0)
            {
                return res != 0;
            }
            if ((st->Pinners[(int)N.Value] & occupied) != 0)
            {
                stmAttackers &= ~Blockers_For_King(C.Value);

                if (stmAttackers == 0)
                {
                    return res != 0;
                }
            }

            res ^= 1;

            // Locate and remove the next least valuable attacker, and add to
            // the bitboard 'attackers' any X-ray attackers behind it.
            if ((bb = stmAttackers & Get_Pieces<Pawn>()) != 0)
            {
                if ((swap = PawnValue - swap) < res)
                {
                    return res != 0;
                }
                occupied ^= Square_BB(Lsb(bb));

                attackers |= Attacks_BB<Bishop>(to, occupied) & Get_Pieces<Pieces<Bishop, Queen>>();
            }

            else if ((bb = stmAttackers & Get_Pieces<Knight>()) != 0)
            {
                if ((swap = KnightValue - swap) < res)
                {
                    return res != 0;
                }
                occupied ^= Square_BB(Lsb(bb));
            }

            else if ((bb = stmAttackers & Get_Pieces<Bishop>()) != 0)
            {
                if ((swap = BishopValue - swap) < res)
                {
                    return res != 0;
                }
                occupied ^= Square_BB(Lsb(bb));

                attackers |= Attacks_BB<Bishop>(to, occupied) & Get_Pieces<Pieces<Bishop, Queen>>();
            }

            else if ((bb = stmAttackers & Get_Pieces<Rook>()) != 0)
            {
                if ((swap = RookValue - swap) < res)
                {
                    return res != 0;
                }
                occupied ^= Square_BB(Lsb(bb));

                attackers |= Attacks_BB<Rook>(to, occupied) & Get_Pieces<Pieces<Rook, Queen>>();
            }

            else if ((bb = stmAttackers & Get_Pieces<Queen>()) != 0)
            {
                swap = QueenValue - swap;
                occupied ^= Square_BB(Lsb(bb));

                attackers |= (Attacks_BB<Bishop>(to, occupied) & Get_Pieces<Pieces<Bishop, Queen>>())
                           | (Attacks_BB<Rook>(to, occupied) & Get_Pieces<Pieces<Rook, Queen>>());
            }

            else  // KING
                  // If we "capture" with the king but the opponent still has attackers,
                  // reverse the result.
            {
                return (attackers & ~Get_Pieces(C.Value)) != 0 ? (res ^ 1) != 0 : res != 0;
            }

            return See_Ge_Next<N, C>(to, occupied, attackers, swap, res);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Gives_Check<C, N>(Move m) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Bitboard pieces = Get_Pieces();
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            if ((Check_Squares(Type_Of(Piece_On(from))) & to) != 0)
            {
                return true;
            }
            if ((Blockers_For_King(N.Value) & from) != 0)
            {
                return (Line_BB(from, to) & Get_Pieces<King>(N.Value)) == 0 || Type_Of(m) == CASTLING;
            }
            switch (Type_Of(m))
            {
                case NORMAL:
                    return false;
                case PROMOTION:
                    return (Attacks_BB(Promotion_Type(m), to, pieces ^ from) & Get_Pieces<King>(N.Value)) != 0;
                case EN_PASSANT : 
                {
                    Square capsq = Make_Square(File_Of(to), Rank_Of(from));
                    Bitboard b = (pieces ^ from ^ capsq) | to;
                    return ((Attacks_BB<Rook>(Get_Square<King>(N.Value), b) & Get_Pieces<Pieces<Queen, Rook>>(C.Value)) 
                         | (Attacks_BB<Bishop>(Get_Square<King>(N.Value), b) & Get_Pieces<Pieces<Queen, Bishop>>(C.Value))) != 0;
                }
                default : 
                {
                    Square rto = Relativ_Square(C.Value, to > from ? SQ_F1 : SQ_D1);
                    return (Check_Squares(ROOK) & rto) != 0;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do_Move<C, N>(Move m, ref StateInfo newSt, bool Gives_Check, ref DirtyPiece dp, ref DirtyThreats dts) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            StateInfo* sPts = (StateInfo*)Unsafe.AsPointer(ref newSt);
            DirtyPiece* dpPts = (DirtyPiece*)Unsafe.AsPointer(ref dp);
            DirtyThreats* dtsPts = (DirtyThreats*)Unsafe.AsPointer(ref dts);
            Key k = st->Key ^ Zobrist.Side;
            Unsafe.CopyBlock(sPts, st, (uint)Unsafe.ByteOffset(ref Unsafe.As<StateInfo, byte>(ref *st), ref Unsafe.As<Key, byte>(ref st->Key)));
            newSt.Previous = st;
            st = sPts;
            ++gamePly;
            ++st->Rule50;
            ++st->PliesFromNull;
            Square from = From_Sq(m); 
            Square to = To_Sq(m);
            Piece pc = Piece_On(from);
            Piece captured = Type_Of(m) == EN_PASSANT ? Make_Piece<Pawn>(N.Value) : Piece_On(to);
            dp.Pc = pc;
            dp.From = from;
            dp.To = to;
            dp.Add_Sq = SQ_NONE;
            dts.Us = C.Value;
            dts.PrevKsq = Get_Square<King>(C.Value);
            dts.ThreatenedSqs = dts.ThreateningSqs = 0;
            int pawn_push = (int)C.Up;
            if (Type_Of(m) == CASTLING)
            {
                Square rfrom = default, rto = default;
                Do_Castling<True, C, N>(from, ref to, ref rfrom, ref rto, dtsPts, dpPts);
                k ^= Zobrist.Psq[(int)captured, (int)rfrom] ^ Zobrist.Psq[(int)captured, (int)rto];
                st->NonPawnKey[(int)C.Value] ^= Zobrist.Psq[(int)captured, (int)rfrom] ^ Zobrist.Psq[(int)captured, (int)rto];
                captured = NO_PIECE;
            }
            else if (captured != 0)
            {
                Square capsq = to;
                if (Type_Of(captured) == PAWN)
                {
                    if (Type_Of(m) == EN_PASSANT)
                    {
                        capsq -= pawn_push;
                        Remove_Piece(capsq, dtsPts);
                    }
                    st->PawnKey ^= Zobrist.Psq[(int)captured, (int)capsq];
                }
                else
                {
                    st->NonPawnMaterial[(int)N.Value] -= Piece_Value(captured);
                    st->NonPawnKey[(int)N.Value] ^= Zobrist.Psq[(int)captured, (int)capsq];
                    if (Type_Of(captured) <= BISHOP)
                    {
                        st->MinorPieceKey ^= Zobrist.Psq[(int)captured, (int)capsq];
                    }
                }
                dp.Remove_Pc = captured;
                dp.Remove_Sq = capsq;
                k ^= Zobrist.Psq[(int)captured, (int)capsq];
                st->MaterialKey ^= Zobrist.Psq[(int)captured, 8 + PieceCount[(int)captured] - (Type_Of(m) == EN_PASSANT ? 0 : 1)];
                st->Rule50 = 0;
            }
            else
            {
                dp.Remove_Sq = SQ_NONE;
            }
            k ^= Zobrist.Psq[(int)pc, (int)from] ^ Zobrist.Psq[(int)pc, (int)to];
            if (st->EpSquare != SQ_NONE)
            {
                k ^= Zobrist.EnPassant[(int)File_Of(st->EpSquare)];
                st->EpSquare = SQ_NONE;
            }
            k ^= Zobrist.Castling[st->CastlingRights];
            st->CastlingRights &= ~(CastlingRightsMask[(int)from] | CastlingRightsMask[(int)to]);
            k ^= Zobrist.Castling[st->CastlingRights];
            if (Type_Of(m) != CASTLING)
            {
                if (captured != 0 && Type_Of(m) != EN_PASSANT)
                {
                    Remove_Piece(from, dtsPts);
                    Swap_Piece(to, pc, dtsPts);
                }
                else
                {
                    Move_Piece(from, to, dtsPts);
                }
            }
            if (Type_Of(pc) == PAWN)
            {
                if ((int)(to ^ from) == 16)
                {
                    Square epSquare = to - pawn_push;
                    Bitboard pawns = Attacks_BB<Pawn>(epSquare, C.Value) & Get_Pieces<Pawn>(N.Value);
                    if (pawns != 0)
                    {
                        Square ksq = Get_Square<King>(N.Value);
                        Bitboard notBlockers = ~st->Previous->BlockersForKing[(int)N.Value];
                        bool noDiscovery = (notBlockers & from) != 0 || File_Of(from) == File_Of(ksq);
                        if (noDiscovery && (pawns & (notBlockers | Line_BB(epSquare, ksq))) != 0)
                        {
                            st->EpSquare = epSquare;
                            k ^= Zobrist.EnPassant[(int)File_Of(epSquare)];
                        }
                    }
                }
                else if (Type_Of(m) == PROMOTION)
                {
                    Piece promotion = Make_Piece(C.Value, Promotion_Type(m));
                    PieceType promotionType = Type_Of(promotion);
                    Swap_Piece(to, promotion, dtsPts);
                    dp.Add_Pc = promotion;
                    dp.Add_Sq = to;
                    dp.To = SQ_NONE;
                    k ^= Zobrist.Psq[(int)promotion, (int)to];
                    st->MaterialKey ^= Zobrist.Psq[(int)promotion, 8 + PieceCount[(int)promotion] - 1]
                                        ^ Zobrist.Psq[(int)pc, 8 + PieceCount[(int)pc]];
                    st->NonPawnKey[(int)C.Value] ^= Zobrist.Psq[(int)promotion, (int)to];
                    if (promotionType <= BISHOP)
                    {
                        st->MinorPieceKey ^= Zobrist.Psq[(int)promotion, (int)to];
                    }
                    st->NonPawnMaterial[(int)C.Value] += Piece_Value(promotion);
                }
                st->PawnKey ^= Zobrist.Psq[(int)pc, (int)from] ^ Zobrist.Psq[(int)pc, (int)to];
                st->Rule50 = 0;
            }
            else
            {
                st->NonPawnKey[(int)C.Value] ^= Zobrist.Psq[(int)pc, (int)from] ^ Zobrist.Psq[(int)pc, (int)to];
                if (Type_Of(pc) <= BISHOP)
                {
                    st->MinorPieceKey ^= Zobrist.Psq[(int)pc, (int)from] ^ Zobrist.Psq[(int)pc, (int)to];
                }
            }
            st->Key = k;
            st->CapturedPiece = captured;
            st->CheckersBB = Gives_Check ? Attackers_To(Get_Square<King>(N.Value)) & Get_Pieces(C.Value) : 0;
            Set_Check_Info<N, C>();
            st->Repetition = 0;
            int end = Math.Min(st->Rule50, st->PliesFromNull);
            if (end >= 4)
            {
                StateInfo* stp = st->Previous->Previous;
                for (int i = 4; i <= end; i += 2)
                {
                    stp = stp->Previous->Previous;
                    if (stp->Key == st->Key)
                    {
                        st->Repetition = stp->Repetition != 0 ? -i : i;
                        break;
                    }
                }
            }
            SideToMove = N.Value;
            dts.Ksq = Get_Square<King>(C.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do_Move<C, N>(Move m, ref StateInfo newSt) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            scratch_dts = default;
            Do_Move<C, N>(m, ref newSt, Gives_Check<C, N>(m), ref scratch_dp, ref scratch_dts);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Undo_Move<C, N>(Move m) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            MoveType movetype = Type_Of(m);
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            Piece pc = Piece_On(to);
            if (movetype == PROMOTION)
            {
                Remove_Piece(to);
                pc = Make_Piece<Pawn>(C.Value);
                Put_Piece(pc, to);
            }
            if (movetype == CASTLING)
            {
                Square rfrom = 0, rto = 0;
                Do_Castling<False, C, N>(from, ref to, ref rfrom, ref rto);
            }
            else
            {
                Move_Piece(to, from);
                if (st->CapturedPiece != 0)
                {
                    Square capsq = to;
                    if (Type_Of(m) == EN_PASSANT)
                    {
                        capsq -= (int)C.Up;
                    }
                    Put_Piece(st->CapturedPiece, capsq);
                }
            }
            st = st->Previous;
            --gamePly;
            SideToMove = C.Value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Do_Castling<D, C, N>(Square from, ref Square to, ref Square rfrom, ref Square rto, DirtyThreats* dts = null, DirtyPiece* dp = null) where D : struct, IBool where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            bool kingSide = to > from;
            rfrom = to; 
            rto = Relativ_Square(C.Value, kingSide? SQ_F1 : SQ_D1);
            to = Relativ_Square(C.Value, kingSide? SQ_G1 : SQ_C1);
            if (D.Value)
            {
                dp->To = to;
                dp->Remove_Pc = dp->Add_Pc = Make_Piece<Rook>(C.Value);
                dp->Remove_Sq = rfrom;
                dp->Add_Sq = rto;
            }
            Remove_Piece(D.Value ? from : to, dts);
            Remove_Piece(D.Value ? rfrom : rto, dts);
            Put_Piece(Make_Piece<King>(C.Value), D.Value ? to : from, dts);
            Put_Piece(Make_Piece<Rook>(C.Value), D.Value ? rto : rfrom, dts);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add_Dirty_Threat<B>(DirtyThreats* dts, Piece piece, Piece threatened, Square s, Square threatenedSq) where B : struct, IBool
        {
            if (B.Value)
            {
                dts->ThreatenedSqs |= threatenedSq;
                dts->ThreateningSqs |= s;
            }
            dts->List.Push_Back(new DirtyThreat(piece, threatened, s, threatenedSq, B.Value));
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void Update_Piece_Threats<B, C>(Piece piece, Square sq, DirtyThreats* dts, Bitboard noRaysContaining = default) where B : struct, IBool where C : struct, IBool
        {
            Bitboard occupied = Get_Pieces();
            Bitboard rookQueens = Get_Pieces<Pieces<Rook, Queen>>();
            Bitboard bishopQueens = Get_Pieces< Pieces<Bishop, Queen>>();
            Bitboard rAttacks = Attacks_BB<Rook>(sq, occupied);
            Bitboard bAttacks = Attacks_BB<Bishop>(sq, occupied);
            Bitboard kings = Get_Pieces<King>();
            Bitboard occupiedNoK = occupied ^ kings;
            Bitboard knights = Get_Pieces<Knight>();
            Bitboard whitePawns = Get_Pieces<Pawn>(WHITE);
            Bitboard blackPawns = Get_Pieces<Pawn>(BLACK);
            Bitboard sliders = (rookQueens & rAttacks) | (bishopQueens & bAttacks);
            Bitboard incomingThreats = (Bitboards.PseudoAttacks[(int)KNIGHT, (int)sq] & knights)
                                     | (Attacks_BB<Pawn>(sq, WHITE) & blackPawns)
                                     | (Attacks_BB<Pawn>(sq, BLACK) & whitePawns)
                                     | (Bitboards.PseudoAttacks[(int)KING, (int)sq] & kings);
            if (Type_Of(piece) == KING)
            {
                if (C.Value)
                {
                    ProcessSliders<B>(false, sliders, rAttacks, bAttacks, occupiedNoK, noRaysContaining, dts, piece, sq);
                }
                return;
            }
            Bitboard threatened = Attacks_BB(piece, sq, occupied) & occupiedNoK; 
            while (threatened != 0)
            {
                Square threatenedSq = Pop_Lsb(ref threatened);
                Piece threatenedPc = Piece_On(threatenedSq);
                Add_Dirty_Threat<B>(dts, piece, threatenedPc, sq, threatenedSq);
            }
            if (C.Value)
            {
                ProcessSliders<B>(true, sliders, rAttacks, bAttacks, occupiedNoK, noRaysContaining, dts, piece, sq);
            }
            else
            {
                incomingThreats |= sliders;
            }
            while (incomingThreats != 0)
            {
                Square srcSq = Pop_Lsb(ref incomingThreats);
                Piece srcPc = Piece_On(srcSq);
                Add_Dirty_Threat<B>(dts, srcPc, piece, srcSq, sq);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void ProcessSliders<B>(bool addDirectAttacks, Bitboard sliders, Bitboard rAttacks, Bitboard bAttacks, Bitboard occupiedNoK, Bitboard noRaysContaining, DirtyThreats* dts, Piece piece, Square sq) where B : struct, IBool
        {
            while (sliders != 0)
            {
                Square sliderSq = Pop_Lsb(ref sliders);
                Piece slider = Piece_On(sliderSq);
                Bitboard ray = Bitboards.RayPassBB[(int)sliderSq, (int)sq];
                Bitboard discovered = ray & (rAttacks | bAttacks) & occupiedNoK;
                if (discovered != 0 && (Bitboards.RayPassBB[(int)sliderSq, (int)sq] & noRaysContaining) != noRaysContaining)
                {
                    Square threatenedSq = Lsb(discovered);
                    Piece threatenedPc = Piece_On(threatenedSq);
                    Add_Dirty_Threat<UnBool<B>>(dts, slider, threatenedPc, sliderSq, threatenedSq);
                }
                if (addDirectAttacks)
                {
                    Add_Dirty_Threat<B>(dts, slider, piece, sliderSq, sq);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Piece Piece_On(Square s)
        {
            return Board[(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Get_Square<T>(Color c) where T : struct, IPieceTypes
        {
            return Lsb(Get_Pieces<T>(c));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Bitboard Get_Pieces()
        {
            return ByColorBB[(int)WHITE] | ByColorBB[(int)BLACK];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces<P>(Color c) where P : struct, IPieceTypes
        {   
            return Get_Pieces(c) & P.Get(ref ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Bitboard Get_Pieces(Color c)
        {
            return ByColorBB[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces<P>() where P : struct, IPieceTypes
        {
            return P.Get(ref ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Bitboard Blockers_For_King(Color c)
        {
            return st->BlockersForKing[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Can_Castle(CastlingRights cr)
        {
            return (st->CastlingRights & (int)cr) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Castling_Impeded(CastlingRights cr) 
        {
            return (Get_Pieces() & CastlingPath[(int)cr]) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Square Castling_Rook_Square(CastlingRights cr)
        {
            return CastlingRookSquare[(int)cr];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Bitboard Checkers()
        {
            return st->CheckersBB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Square Ep_Square()
        {
            return st->EpSquare;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsChess960()
        {
            return chess960 != 0;
        }
        //┏━┳┓╔═╦╗┌─┬┐
        //┣━╋┫╠═╬╣├─┼┤
        //┃ ┃┃║ ║║│ ││
        //┗━┻┛╚═╩╝└─┴┘
        public readonly string Show(Color c)
        {
            StringBuilder sb = new();
            sb.AppendLine(" ┌───┬───┬───┬───┬───┬───┬───┬───┐");
            for (Rank r = RANK_8; ; --r)
            {
                for (File f = FILE_A; f <= FILE_H; ++f)
                {
                    Square sq = Make_Square(f, r);
                    Piece p = Piece_On(sq);
                    sb.Append(" │ ");
                    sb.Append(PieceToChar[(int)p]);
                }
                sb.Append(" │ ");
                sb.Append((int)r + 1);
                sb.AppendLine();
                if (r == RANK_1)
                {
                    sb.AppendLine(" └───┴───┴───┴───┴───┴───┴───┴───┘");
                    break;
                }
                sb.AppendLine(" ├───┼───┼───┼───┼───┼───┼───┼───┤");
            }
            sb.AppendLine("   a   b   c   d   e   f   g   h");
            sb.AppendLine();
            sb.AppendLine($"Fen: {Fen(c)}");
            sb.AppendLine($"Key: {st->Key:X16}");
            sb.AppendLine($"Checkers: ");
            Bitboard b = Checkers();
            while (b != 0)
            {
                Square sq = Pop_Lsb(ref b);
                sb.Append($"{Square_To_String(sq)} ");
            }
            return sb.ToString();
        }
    }
}
