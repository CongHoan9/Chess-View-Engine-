using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using static Chess.Bitboards;
using static Chess.FuncBit;
using static Chess.Types;
namespace Chess
{
    using Key = UInt64;
    unsafe public partial class Position
    {
        public Piece[] Board = new Piece[(int)SQ_NB];
        public Bitboard[] ByTypeBB = new Bitboard[(int)SQ_NB];
        public Bitboard[] ByColorBB = new Bitboard[(int)COLOR_NB];
        public int[] PieceCount = new int[(int)PIECE_NB];
        public int[] CastlingRightsMask = new int[(int)SQ_NB];
        public Square[] CastlingRookSquare = new Square[(int)CASTLING_RIGHR_NB];
        public Bitboard[] CastlingPath = new Bitboard[(int)CASTLING_RIGHR_NB];
        public StateInfo* st;
        public int gamePly;
        public int chess960;
        public DirtyPiece scratch_dp;
        public DirtyThreats scratch_dts;
        public static readonly Key[] Cuckoo = new Key[8192];
        public static readonly Move[] CuckooMove = new Move[8192];
        public const string PieceToChar = " PNBRQK  pnbrqk";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(Board, 0, Board.Length);
            Array.Clear(ByTypeBB, 0, ByTypeBB.Length);
            Array.Clear(ByColorBB, 0, ByColorBB.Length);
            Array.Clear(PieceCount, 0, PieceCount.Length);
            Array.Clear(CastlingRightsMask, 0, CastlingRightsMask.Length);
            Array.Clear(CastlingRookSquare, 0, CastlingRookSquare.Length);
            Array.Clear(CastlingPath, 0, CastlingPath.Length);
            st = null;
            gamePly = 0;
            chess960 = 0;
            scratch_dp = default;
            scratch_dts = default;
            Array.Clear(Cuckoo, 0, Cuckoo.Length);
            Array.Clear(CuckooMove, 0, CuckooMove.Length);
        }
        public Position()
        {
            PRNG rng = new(1070372);
            foreach (Piece pc in Pieces)
            {
                for (Square s = SQ_A1; s <= SQ_H8; ++s)
                {
                    Zobrist.Psq[(int)pc][(int)s] = rng.Rand<Key>();
                }    
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)W_PAWN][(int)SQ_A8 + i] = 0;
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)B_PAWN][i] = 0;
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
            Array.Fill<Key>(Cuckoo, 0);
            Array.Fill<Move>(CuckooMove, 0);
            int count = 0;
            foreach (Piece pc in Pieces)
            {
                for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
                {
                    for (Square s2 = (s1 + 1); s2 <= SQ_H8; ++s2)
                    {
                        if ((Type_Of(pc) != PAWN) && (Attacks_BB(pc, s1, 0) & s2) != 0)
                        {
                            Move move = new(s1, s2);
                            Key key = Zobrist.Psq[(int)pc][(int)s1] ^ Zobrist.Psq[(int)pc][(int)s2] ^ Zobrist.Side;
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
        unsafe public Position Set(string fenStr, bool isChess960, StateInfo* si)
        {
            char col, row, token;
            int idx;
            Square sq = SQ_A8;
            var ss = new StringReader(fenStr);
            Clear();
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
                Piece rook = Make_Piece(cside, ROOK);
                token = char.ToUpper(token);
                if (token == 'K')
                {
                    for (rsq = Relativ_Square(cside, SQ_H1); Piece_On_Square(rsq) != rook; rsq--)
                    { }
                }
                else if (token == 'Q')
                {
                    for (rsq = Relativ_Square(cside, SQ_A1); Piece_On_Square(rsq) != rook; rsq++)
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
                Bitboard pawns = Attacks_BB_Square<Pawn>(st->EpSquare, them) & Get_Pieces_Of_Color<Pawn>(us);
                Bitboard target = Get_Pieces_Of_Color<Pawn>(them) & (st->EpSquare + (int)Pawn_Push(them));
                Bitboard occ = Get_Pieces() ^ target ^ Square_BB(st->EpSquare);
                enpassant = pawns != 0 && (Get_Pieces() & (st->EpSquare | (st->EpSquare + (int)(Pawn_Push(them))))) == 0;
                while (pawns != 0)
                {
                    legalEP |= (Attackers_To(Get_Square_Of_Color<King>(them), occ ^ Pop_Lsb(ref pawns)) & Get_Pieces_Of_Color(them) & ~target) == 0;
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
            Set_State(us);
            Console.WriteLine(Show(us));
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Empty(Square s)
        {
            return Piece_On_Square(s) == NO_PIECE; 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string Fen(Color c)
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
                        sb.Append(PieceToChar[(int)Piece_On_Square(Make_Square(f, r))]);
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
            Square kfrom = Get_Square_Of_Color<King>(c);
            var QueenSide = c == WHITE ? White.QueenSide : Black.QueenSide;
            var KingSide = c == WHITE ? White.KingSide : Black.KingSide;
            CastlingRights cr = kfrom < rfrom ? KingSide : QueenSide;
            st->CastlingRights |= (int)cr;
            CastlingRightsMask[(int)kfrom] |= (int)cr;
            CastlingRightsMask[(int)rfrom] |= (int)cr;
            CastlingRookSquare[(int)cr] = rfrom;
            Square kto = Relativ_Square(c, (cr & KingSide) != 0 ? SQ_G1 : SQ_C1);
            Square rto = Relativ_Square(c, (cr & KingSide) != 0 ? SQ_F1 : SQ_D1);
            CastlingPath[(int)cr] = (Between_BB(rfrom, rto) | Between_BB(kfrom, kto)) & ~(Square_BB(kfrom) | Square_BB(rfrom));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Set_Check_Info<C>() where C : struct, IColor
        {
            Bitboard pieces = Get_Pieces();
            Update_Slider_Blockers(C.Us);
            Update_Slider_Blockers(C.Them);
            Square ksq = Get_Square_Of_Color<King>(C.Them);
            st->CheckSquares[(int)PAWN] = Attacks_BB_Square<Pawn>(ksq, C.Them);
            st->CheckSquares[(int)KNIGHT] = Attacks_BB_Square<Knight>(ksq);
            st->CheckSquares[(int)BISHOP] = Attacks_BB_Square<Bishop>(ksq, pieces);
            st->CheckSquares[(int)ROOK] = Attacks_BB_Square<Rook>(ksq, pieces);
            st->CheckSquares[(int)QUEEN] = st->CheckSquares[(int)BISHOP] | st->CheckSquares[(int)ROOK];
            st->CheckSquares[(int)KING] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Set_State(Color us) 
        {
            Color them = us == WHITE ? BLACK : WHITE;
            st->Key = 0;
            st->MinorPieceKey = 0;
            st->NonPawnKey[(int)WHITE] = st->NonPawnKey[(int)BLACK] = 0;
            st->PawnKey = Zobrist.NoPawns;
            st->NonPawnMaterial[(int)WHITE] = st->NonPawnMaterial[(int)BLACK] = VALUE_ZERO;
            st->CheckersBB = Attackers_To(Get_Square_Of_Color<King>(us)) & Get_Pieces_Of_Color(them);
            if (us == WHITE)
            {
                Set_Check_Info<White>();
            }
            else  
            {
                Set_Check_Info<Black>();
            }
            Bitboard b = Get_Pieces();
            while (b != 0)
            {
                Square sq = Pop_Lsb(ref b);
                Piece pc = Piece_On_Square(sq);
                st->Key ^= Zobrist.Psq[(int)pc][(int)sq];
                if (Type_Of(pc) == PAWN)
                {
                    st->PawnKey ^= Zobrist.Psq[(int)pc][(int)sq];
                }
                else
                {
                    st->NonPawnKey[(int)Color_Of(pc)] ^= Zobrist.Psq[(int)pc][(int)sq];
                    if (Type_Of(pc) != KING)
                    {
                        st->NonPawnMaterial[(int)Color_Of(pc)] += PieceValue[(int)pc];
                        if (Type_Of(pc) <= BISHOP)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)sq];
                        }
                    }
                }
            }
            if (st->EpSquare != SQ_NONE)
            {
                st->Key ^= Zobrist.EnPassant[(int)File_Of(st->EpSquare)];
            }
            if (us == BLACK)
            {
                st->Key ^= Zobrist.Side;
            }
            st->Key ^= Zobrist.Castling[st->CastlingRights];
            st->MaterialKey = Compute_Material_Key();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Key Compute_Material_Key()
        {
            Key k = 0;
            foreach (Piece pc in Pieces)
            {
                for (int cnt = 0; cnt < PieceCount[(int)pc]; ++cnt)
                {
                    k ^= Zobrist.Psq[(int)pc][8 + cnt];
                }
            }
            return k;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update_Slider_Blockers(Color c)
        {
            Color them = c == WHITE ? BLACK : WHITE;
            Square ksq = Get_Square_Of_Color<King>(c);
            st->BlockersForKing[(int)c] = 0;
            st->Pinners[(int)them] = 0;
            Bitboard snipers = ((Attacks_BB_Square<Rook>(ksq) & Get_Pieces<Pieces<Rook, Queen>>()) | (Attacks_BB_Square<Bishop>(ksq) & Get_Pieces<Pieces<Bishop, Queen>>())) & Get_Pieces_Of_Color(them);
            Bitboard occupancy = Get_Pieces() ^ snipers;
            while (snipers != 0)
            {
                Square sniperSq = Pop_Lsb(ref snipers);
                Bitboard b = Between_BB(ksq, sniperSq) & occupancy;
                if (b != 0 && !More_Than_One(b))
                {
                    st->BlockersForKing[(int)c] |= b;
                    if ((b & Get_Pieces_Of_Color(c)) != 0)
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
            return (Attacks_BB_Square<Rook>(s, occupied) & Get_Pieces<Pieces<Rook, Queen>>())
                 | (Attacks_BB_Square<Bishop>(s, occupied) & Get_Pieces<Pieces<Bishop, Queen>>())
                 | (Attacks_BB_Square<Pawn>(s, WHITE) & Get_Pieces_Of_Color<Pawn>(WHITE))
                 | (Attacks_BB_Square<Pawn>(s, BLACK) & Get_Pieces_Of_Color<Pawn>(BLACK))
                 | (Attacks_BB_Square<Knight>(s) & Get_Pieces<Knight>())
                 | (Attacks_BB_Square<King>(s) & Get_Pieces<King>());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Attackers_To_Exist(Square s, Bitboard occupied, Color c)
        {
            return (Attacks_BB_Square<Rook>(s, occupied) & Get_Pieces_Of_Color<Pieces<Rook, Queen>>(c)) != 0
                || (Attacks_BB_Square<Bishop>(s, occupied) & Get_Pieces_Of_Color<Pieces<Bishop, Queen>>(c)) != 0
                || (Attacks_BB_Square<Pawn>(s, c == WHITE ? BLACK : WHITE) & Get_Pieces_Of_Color<Pawn>(c)) !=0
                || (Attacks_BB_Square<Knight>(s) & Get_Pieces_Of_Color<Knight>(c)) !=0
                || (Attacks_BB_Square<King>(s) & Get_Pieces_Of_Color<King>(c)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Legal<C>(Move m) where C : struct, IColor
        {
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            Bitboard pieces = Get_Pieces();
            if (Type_Of(m) == EN_PASSANT)
            {
                Square ksq = Get_Square_Of_Color<King>(C.Us);
                Square capsq = to - (int)Pawn_Push(C.Us);
                Bitboard occupied = (pieces ^ from ^ capsq) | to;
                return (Attacks_BB_Square<Rook>(ksq, occupied) & Get_Pieces_Of_Color<Pieces<Queen, Rook>>(C.Them)) == 0 
                    && (Attacks_BB_Square<Bishop>(ksq, occupied) & Get_Pieces_Of_Color<Pieces<Queen, Bishop>>(C.Them)) == 0;
            }
            if (Type_Of(m) == CASTLING)
            {
                to = Relativ_Square(C.Us, to > from ? SQ_G1 : SQ_C1);
                Direction step = to > from ? WEST : EAST;
                for (Square s = to; s != from; s += (int)step)
                {
                    if (Attackers_To_Exist(s, pieces, C.Them))
                    {
                        return false;
                    }    
                }
                return chess960 == 0 || (Blockers_For_King(C.Us) & To_Sq(m)) == 0;
            }
            if (Type_Of(Piece_On_Square(from)) == KING)
            {
                return !(Attackers_To_Exist(to, pieces ^ from, C.Them));
            }
            return (Blockers_For_King(C.Us) & from) == 0 || (Line_BB(from, to) & Get_Pieces_Of_Color<King>(C.Us)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Put_Piece(Piece pc, Square s, DirtyThreats* dts = null) 
        {
            Color c = Color_Of(pc);
            PieceType t = Type_Of(pc);
            Board[(int)s] = pc;
            ByTypeBB[(int)ALL_PIECE] |= ByTypeBB[(int)t] |= s;
            ByColorBB[(int)c] |= s;
            PieceCount[(int)pc]++;
            PieceCount[(int)Make_Piece(c, ALL_PIECE)]++;
            if (dts != null)
            {
                Updat_Piece_Threats<True>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Remove_Piece(Square s, DirtyThreats* dts = null) 
        {
            Piece pc = Board[(int)s];
            if (dts != null)
            {
                Updat_Piece_Threats<False>(pc, s, dts);
            }
            ByTypeBB[(int)ALL_PIECE] ^= s;
            ByTypeBB[(int)Type_Of(pc)] ^= s;
            ByColorBB[(int)Color_Of(pc)] ^= s;
            Board[(int)s] = NO_PIECE;
            PieceCount[(int)pc]--;
            PieceCount[(int)Make_Piece(Color_Of(pc), ALL_PIECE)]--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Move_Piece(Square from, Square to, DirtyThreats* dts = null)
        {
            Piece pc = Board[(int)from];
            Bitboard fromTo = Square_BB(from) | Square_BB(to);
            if (dts != null)
            {
                Updat_Piece_Threats<False>(pc, from, dts, fromTo);
            }
            ByTypeBB[(int)ALL_PIECE] ^= fromTo;
            ByTypeBB[(int)Type_Of(pc)] ^= fromTo;
            ByColorBB[(int)Color_Of(pc)] ^= fromTo;
            Board[(int)from] = NO_PIECE;
            Board[(int)to] = pc;
            if (dts != null)
            {
                Updat_Piece_Threats<True>(pc, to, dts, fromTo);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Swap_Piece(Square s, Piece pc, DirtyThreats* dts)
        {
            Piece old = Board[(int)s];
            Remove_Piece(s);
            if (dts != null)
            {
                Updat_Piece_Threats<False, False>(old, s, dts);
            }
            Put_Piece(pc, s);
            if (dts != null)
            {
                Updat_Piece_Threats<True, False>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Check_Squares(PieceType pt) 
        { 
            return st->CheckSquares[(int)pt]; 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Gives_Check<C>(Move m) where C : struct, IColor
        {
            Bitboard pieces = Get_Pieces();
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            if ((Check_Squares(Type_Of(Piece_On_Square(from))) & to) != 0)
            {
                return true;
            }
            if ((Blockers_For_King(C.Them) & from) != 0)
            {
                return (Line_BB(from, to) & Get_Pieces_Of_Color<King>(C.Them)) == 0 || Type_Of(m) == CASTLING;
            }
            switch (Type_Of(m))
            {
                case NORMAL:
                    return false;
                case PROMOTION:
                    return (Attacks_BB(Promotion_Type(m), to, pieces ^ from) & Get_Pieces_Of_Color<King>(C.Them)) != 0;
                case EN_PASSANT : 
                {
                    Square capsq = Make_Square(File_Of(to), Rank_Of(from));
                    Bitboard b = (pieces ^ from ^ capsq) | to;
                    return ((Attacks_BB_Square<Rook>(Get_Square_Of_Color<King>(C.Them), b) & Get_Pieces_Of_Color<Pieces<Queen, Rook>>(C.Us)) 
                         | (Attacks_BB_Square<Bishop>(Get_Square_Of_Color<King>(C.Them), b) & Get_Pieces_Of_Color<Pieces<Queen, Bishop>>(C.Us))) != 0;
                }
                default : 
                {
                    Square rto = Relativ_Square(C.Us, to > from ? SQ_F1 : SQ_D1);
                    return (Check_Squares(ROOK) & rto) != 0;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public void Do_Move<C>(Move m, ref StateInfo newSt, bool Gives_Check, ref DirtyPiece dp, ref DirtyThreats dts) where C : struct, IColor
        {
            fixed (StateInfo* sPts = &newSt)
            fixed (DirtyPiece* dpPts = &dp)
            fixed (DirtyThreats* dtsPts = &dts)
            {
                MoveType movetype = Type_Of(m);
                Key k = st->Key ^ Zobrist.Side;
                Unsafe.CopyBlock(sPts, st, (uint)Unsafe.ByteOffset(ref Unsafe.As<StateInfo, byte>(ref *st), ref Unsafe.As<Key, byte>(ref st->Key)));
                newSt.Previous = st;
                st = sPts;
                ++gamePly;
                ++st->Rule50;
                ++st->PliesFromNull;
                Square from = From_Sq(m); 
                Square to = To_Sq(m);
                Piece pc = Piece_On_Square(from);
                Piece captured = movetype == EN_PASSANT ? Make_Piece(C.Them, PAWN) : Piece_On_Square(to);
                dp.Pc = pc;
                dp.From = from;
                dp.To = to;
                dp.Add_Sq = SQ_NONE;
                dts.Us = C.Us;
                dts.PrevKsq = Get_Square_Of_Color<King>(C.Us);
                dts.ThreatenedSqs = dts.ThreateningSqs = 0;
                int pawn_push = (int)Pawn_Push(C.Us);
                if (movetype == CASTLING)
                {
                    Square rfrom = default, rto = default;
                    Do_Castling<True>(C.Us, from, ref to, ref rfrom, ref rto, dtsPts, dpPts);
                    k ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                    st->NonPawnKey[(int)C.Us] ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                    captured = NO_PIECE;
                }
                if (captured != 0)
                {
                    Square capsq = to;
                    if (Type_Of(captured) == PAWN)
                    {
                        if (movetype == EN_PASSANT)
                        {
                            capsq -= pawn_push;
                            Remove_Piece(capsq, dtsPts);
                        }
                        st->PawnKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                    }
                    else
                    {
                        st->NonPawnMaterial[(int)C.Them] -= PieceValue[(int)pc];
                        st->PawnKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                        if (Type_Of(captured) <= BISHOP)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                        }
                    }
                    dpPts->Remove_Pc = captured;
                    dpPts->Remove_Sq = capsq;
                    k ^= Zobrist.Psq[(int)captured][(int)capsq];
                    st->MaterialKey ^= Zobrist.Psq[(int)captured][8 + PieceCount[(int)captured] - ((Type_Of(m) != EN_PASSANT) ? 1 : 0)];
                    st->Rule50 = 0;
                }
                else
                {
                    dpPts->Remove_Pc = NO_PIECE;
                }
                k ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                if (st->EpSquare != SQ_NONE)
                {
                    k ^= Zobrist.EnPassant[(int)File_Of(st->EpSquare)];
                    st->EpSquare = SQ_NONE;
                }
                k ^= Zobrist.Castling[st->CastlingRights];
                st->CastlingRights &= ~(CastlingRightsMask[(int)from] | CastlingRightsMask[(int)to]);
                k ^= Zobrist.Castling[st->CastlingRights];
                if (movetype != CASTLING)
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
                        Bitboard pawns = Attacks_BB_Square<Pawn>(epSquare, C.Us) & Get_Pieces_Of_Color<Pawn>(C.Them);
                        if (pawns != 0)
                        {
                            Square ksq = Get_Square_Of_Color<King>(C.Them);
                            Bitboard notBlockers = ~st->Previous->BlockersForKing[(int)C.Them];
                            bool noDiscovery = (notBlockers & from) != 0 || File_Of(from) == File_Of(ksq);
                            if (noDiscovery && (pawns & (notBlockers | Line_BB(epSquare, ksq))) != 0)
                            {
                                st->EpSquare = epSquare;
                                k ^= Zobrist.EnPassant[(int)File_Of(epSquare)];
                            }
                        }
                    }
                    else if (movetype == PROMOTION)
                    {
                        Piece promotion = Make_Piece(C.Us, Promotion_Type(m));
                        PieceType promotionType = Type_Of(promotion);
                        Swap_Piece(to, promotion, dtsPts);
                        
                        dpPts->Add_Pc = promotion;
                        dpPts->Add_Sq = to;
                        dpPts->To = SQ_NONE;
                        k ^= Zobrist.Psq[(int)promotion][(int)to];
                        st->MaterialKey ^= Zobrist.Psq[(int)promotion][8 + PieceCount[(int)promotion] - 1]
                                         ^ Zobrist.Psq[(int)pc][8 + PieceCount[(int)pc]];
                        st->NonPawnKey[(int)C.Us] ^= Zobrist.Psq[(int)promotion][(int)to];
                        if (promotionType <= BISHOP)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)promotion][(int)to];
                        }
                        st->NonPawnMaterial[(int)C.Us] += PieceValue[(int)promotion];
                    }
                    st->PawnKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    st->Rule50 = 0;
                }
                else
                {
                    st->NonPawnKey[(int)C.Us] ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    if (Type_Of(pc) <= BISHOP)
                    {
                        st->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    }
                }
                st->Key = k;
                st->CapturedPiece = captured;
                st->CheckersBB = Gives_Check ? Attackers_To(Get_Square_Of_Color<King>(C.Them)) & Get_Pieces_Of_Color(C.Us) : 0;
                if (C.Us == WHITE)
                {
                    Set_Check_Info<Black>();
                }
                else
                {
                    Set_Check_Info<White>();
                }
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
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public void Do_Move<C>(Move m, ref StateInfo newSt) where C : struct, IColor
        {
            Do_Move<C>(m, ref newSt, Gives_Check<C>(m), ref scratch_dp, ref scratch_dts);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public void Undo_Move<C>(Move m) where C : struct, IColor
        {
            MoveType movetype = Type_Of(m);
            Square from = From_Sq(m);
            Square to = To_Sq(m);
            Piece pc = Piece_On_Square(to);
            if (movetype == PROMOTION)
            {
                Remove_Piece(to);
                pc = Make_Piece(C.Them, PAWN);
                Put_Piece(pc, to);
            }
            if (movetype == CASTLING)
            {
                Square rfrom = 0, rto = 0;
                Do_Castling<False>(C.Them, from, ref to, ref rfrom, ref rto);
            }
            else
            {
                Move_Piece(to, from);
                if (st->CapturedPiece != 0)
                {
                    Square capsq = to;
                    if (Type_Of(m) == EN_PASSANT)
                    {
                        capsq -= (int)Pawn_Push(C.Them);
                    }
                    Put_Piece(st->CapturedPiece, capsq);
                }
            }
            st = st->Previous;
            --gamePly;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public void Do_Castling<D>(Color us, Square from, ref Square to, ref Square rfrom, ref Square rto, DirtyThreats* dts = null, DirtyPiece* dp = null) where D : struct, IBool
        {
            if (dp != null)
            {
                bool kingSide = to > from;
                rfrom = to; 
                rto = Relativ_Square(us, kingSide? SQ_F1 : SQ_D1);
                to = Relativ_Square(us, kingSide? SQ_G1 : SQ_C1);
                if (D.Value || dp != null )
                {
                    dp->To = to;
                    dp->Remove_Pc = dp->Add_Pc = Make_Piece(us, ROOK);
                    dp->Remove_Sq = rfrom;
                    dp->Add_Sq = rto;
                }
                Remove_Piece(D.Value ? from : to, dts);
                Remove_Piece(D.Value ? rfrom : rto, dts);
                Board[(int)(D.Value ? from : to)] = Board[(int)(D.Value ? rfrom : rto)] = NO_PIECE;
                Put_Piece(Make_Piece(us, KING), D.Value ? to : from, dts);
                Put_Piece(Make_Piece(us, ROOK), D.Value ? rto : rfrom, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private static void Add_Dirty_Threat<B>(DirtyThreats* dts, Piece piece, Piece threatened, Square s, Square threatenedSq) where B : struct, IBool
        {
            if (B.Value)
            {
                dts->ThreatenedSqs |= threatenedSq;
                dts->ThreateningSqs |= s;
            }
            //Console.WriteLine("Add start");
            dts->List.Push_Back(new DirtyThreat(piece, threatened, s, threatenedSq, B.Value));
            //Console.WriteLine("Add done");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Updat_Piece_Threats<B>(Piece pc, Square s, DirtyThreats* dts, Bitboard noRaysContaining = default) where B : struct, IBool
        {
            Updat_Piece_Threats<B, True>(pc, s, dts, noRaysContaining);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private void Updat_Piece_Threats<B, C>(Piece piece, Square sq, DirtyThreats* dts, Bitboard noRaysContaining = default) where B : struct, IBool where C : struct, IBool
        {
            Bitboard occupied = Get_Pieces();
            Bitboard rookQueens = Get_Pieces<Pieces<Rook, Queen>>();
            Bitboard bishopQueens = Get_Pieces< Pieces<Bishop, Queen>>();
            Bitboard rAttacks = Attacks_BB_Square<Rook>(sq, occupied);
            Bitboard bAttacks = Attacks_BB_Square<Bishop>(sq, occupied);
            Bitboard kings = Get_Pieces<King>();
            Bitboard occupiedNoK = occupied ^ kings;
            Bitboard sliders = (rookQueens & rAttacks) | (bishopQueens & bAttacks);
            void ProcessSliders(bool addDirectAttacks)
            {
                while (sliders != 0)
                {
                    Square sliderSq = Pop_Lsb(ref sliders);
                    Piece slider = Piece_On_Square(sliderSq);
                    Console.WriteLine(sliderSq);
                    Bitboard ray = RayPassBB[(int)sliderSq][(int)sq];
                    Bitboard discovered = ray & (rAttacks | bAttacks) & occupiedNoK;
                    if (discovered != 0 && (RayPassBB[(int)sliderSq][(int)sq] & noRaysContaining) != noRaysContaining)
                    {
                        Square threatenedSq = Lsb(discovered);
                        Piece threatenedPc = Piece_On_Square(threatenedSq);
                        Add_Dirty_Threat<UnBool<B>>(dts, slider, threatenedPc, sliderSq, threatenedSq);
                    }
                    if (addDirectAttacks)
                    {
                        Add_Dirty_Threat<B>(dts, slider, piece, sliderSq, sq);
                    }
                }
            }
            if (Type_Of(piece) == KING)
            {
                if (C.Value)
                {
                    ProcessSliders(false);
                }
                return;
            }
            Bitboard threatened = Attacks_BB(piece, sq, occupied) & occupiedNoK;
            while (threatened != 0)
            {
                Square threatenedSq = Pop_Lsb(ref threatened);
                Piece threatenedPc = Piece_On_Square(threatenedSq);
                Add_Dirty_Threat<B>(dts, piece, threatenedPc, sq, threatenedSq);
            }
            if (C.Value)
            {
                ProcessSliders(false);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece Piece_On_Square(Square s)
        {
            return Board[(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Get_Square_Of_Color<T>(Color c) where T : struct, IPieceTypes
        {
            return Lsb(Get_Pieces_Of_Color<T>(c));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces()
        {
            return ByColorBB[(int)WHITE] | ByColorBB[(int)BLACK];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces_Of_Color<P>(Color c) where P : struct, IPieceTypes
        {
            return Get_Pieces_Of_Color(c) & P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces_Of_Color(Color c)
        {
            return ByColorBB[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Get_Pieces<P>() where P : struct, IPieceTypes
        {
            return P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Blockers_For_King(Color c)
        {
            return st->BlockersForKing[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Can_Castle(CastlingRights cr)
        {
            return (st->CastlingRights & (int)cr) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Castling_Impeded(CastlingRights cr) 
        {
            return (Get_Pieces() & CastlingPath[(int)cr]) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Castling_Rook_Square(CastlingRights cr)
        {
            return CastlingRookSquare[(int)cr];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Checkers()
        {
            return st->CheckersBB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square Ep_Square()
        {
            return st->EpSquare;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChess960()
        {
            return chess960 != 0;
        }
        //┏━┳┓╔═╦╗┌─┬┐
        //┣━╋┫╠═╬╣├─┼┤
        //┃ ┃┃║ ║║│ ││
        //┗━┻┛╚═╩╝└─┴┘
        public string Show(Color c)
        {
            StringBuilder sb = new();
            sb.AppendLine(" ┌───┬───┬───┬───┬───┬───┬───┬───┐");
            for (Rank r = RANK_8; ; r--)
            {
                for (File f = FILE_A; f <= FILE_H; f++)
                {
                    Square sq = Make_Square(f, r);
                    Piece p = Piece_On_Square(sq);
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
