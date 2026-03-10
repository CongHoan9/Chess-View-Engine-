using Chess;
using System.Collections;
using System.IO;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using WinRT;
using static Chess.BitBoard;
using static Chess.Types;
namespace Chess
{
    unsafe public partial class Position
    {
        public EPiece[] Board = new EPiece[(int)ESquare.SquareNB];
        public SBitBoard[] ByTypeBB = new SBitBoard[(int)EPieceType.PieceTypeNB];
        public SBitBoard[] ByColorBB = new SBitBoard[(int)EColor.ColorNB];
        public int[] PieceCount = new int[(int)EPiece.PieceNB];
        public int[] CastlingRightsMask = new int[(int)ESquare.SquareNB];
        public ESquare[] castlingRookSquare = new ESquare[(int)ECastlingRights.CastlingRightNB];
        public SBitBoard[] CastlingPath = new SBitBoard[(int)ECastlingRights.CastlingRightNB];
        public StateInfo* st;
        public int gamePly;
        public EColor SideToMove;
        public int chess960;
        public DirtyPiece scratch_dp;
        public DirtyThreats scratch_dts;
        public static SKey[] Cuckoo { get; } = new SKey[8192];
        public static SMove[] CuckooMove { get; } = new SMove[8192];
        private readonly string PieceToChar = " PNBRQK  pnbrqk";
        public Position()
        {
            Console.WriteLine("Tạo pos");
            PRNG rng = new(1070372);
            foreach (EPiece pc in Pieces)
            {
                for (ESquare s = ESquare.SQ_A1; s <= ESquare.SQ_H8; ++s)
                {
                    Zobrist.Psq[(int)pc][(int)s] = rng.Rand<SKey>();
                }    
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)EPiece.WPawn][(int)ESquare.SQ_A8 + i] = 0;
            }
            for (int i = 0; i < 8; i++)
            {
                Zobrist.Psq[(int)EPiece.BPawn][i] = 0;
            }
            for (EFile f = EFile.FileA; f <= EFile.FileH; ++f)
            {
                Zobrist.EnPassant[(int)f] = rng.Rand<SKey>();
            }
            for (ECastlingRights cr = ECastlingRights.NoCastling; cr <= ECastlingRights.AnyCastling; ++cr)
            {
                Zobrist.Castling[(int)cr] = rng.Rand<SKey>();
            }    
            Zobrist.Side = rng.Rand<SKey>();
            Zobrist.NoPawns = rng.Rand<SKey>();
            Array.Fill<SKey>(Cuckoo, 0);
            Array.Fill<SMove>(CuckooMove, 0);
            int count = 0;
            foreach (EPiece pc in Pieces)
            {
                for (ESquare s1 = ESquare.SQ_A1; s1 <= ESquare.SQ_H8; ++s1)
                {
                    for (ESquare s2 = (s1 + 1); s2 <= ESquare.SQ_H8; ++s2)
                    {
                        if ((TypeOf(pc) != EPieceType.Pawn) && (AttacksBB(TypeOf(pc), s1, 0) & s2) != 0)//
                        {
                            SMove move = new(s1, s2);
                            SKey key = Zobrist.Psq[(int)pc][(int)s1] ^ Zobrist.Psq[(int)pc][(int)s2] ^ Zobrist.Side;
                            int i = H1(key);
                            while (true)
                            {
                                Swap(ref Cuckoo[i], ref key);
                                Swap(ref CuckooMove[i], ref move);
                                if (move == SMove.None())
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
            Console.WriteLine("Tạo pos xong");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(Board, 0, Board.Length);
            Array.Clear(ByTypeBB, 0, ByTypeBB.Length);
            Array.Clear(ByColorBB, 0, ByColorBB.Length);
            Array.Clear(PieceCount, 0, PieceCount.Length);
            Array.Clear(CastlingRightsMask, 0, CastlingRightsMask.Length);
            Array.Clear(castlingRookSquare, 0, castlingRookSquare.Length);
            Array.Clear(CastlingPath, 0, CastlingPath.Length);
            st = null;
            gamePly = 0;
            SideToMove = default;
            chess960 = 0;
            scratch_dp = default;
            scratch_dts = default;
            Array.Clear(Cuckoo, 0, Cuckoo.Length);
            Array.Clear(CuckooMove, 0, CuckooMove.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe Position Set(string fenStr, bool isChess960, StateInfo* si)
        {
            char col, row, token;
            int idx;
            ESquare sq = ESquare.SQ_A8;
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
                    sq += (token - '0') * (int)EDirection.East;
                }    
                else if (token == '/')
                {
                    sq += 2 * (int)EDirection.South;
                }   
                else if ((idx = PieceToChar.IndexOf(token)) != -1)
                {
                    PutPiece((EPiece)idx, sq, null);
                    sq++;
                }
            }
            token = (char)ss.Read();
            SideToMove = token == 'w' ? EColor.White : EColor.Black;
            EColor them = SideToMove == EColor.White ? EColor.Black : EColor.White;
            ss.Read(); 
            while ((c = ss.Read()) != -1)
            {
                token = (char)c;
                if (char.IsWhiteSpace(token))
                {
                    break;
                }    
                ESquare rsq;
                EColor cside = char.IsLower(token) ? EColor.Black : EColor.White;
                EPiece rook = MakePiece(cside, EPieceType.Rook);
                token = char.ToUpper(token);
                if (token == 'K')
                {
                    for (rsq = RelativeSquare(cside, ESquare.SQ_H1); PieceOn(rsq) != rook; rsq--)
                    { }
                }
                else if (token == 'Q')
                {
                    for (rsq = RelativeSquare(cside, ESquare.SQ_A1); PieceOn(rsq) != rook; rsq++)
                    { }
                }
                else if (token >= 'A' && token <= 'H')
                {
                    rsq = MakeSquare((EFile)(token - 'A'), RelativeRank(cside, ERank.Rank1));
                }
                else
                {
                    continue;
                }
                SetCastlingRight(cside, rsq);
            }
            bool enpassant = false, legalEP = false;
            col = (char)ss.Read();
            row = (char)ss.Read();
            if (col >= 'a' && col <= 'h' && row == (SideToMove == EColor.White ? '6' : '3'))
            {
                st->EpSquare = MakeSquare((EFile)(col - 'a'), (ERank)(row - '1'));
                SBitBoard pawns = AttacksBB<SPawn>(st->EpSquare, them) & GetPieces<SPawn>(SideToMove);
                SBitBoard target = GetPieces<SPawn>(them) & (st->EpSquare + (int)PawnPush(them));
                SBitBoard occ = GetPieces() ^ target ^ st->EpSquare;
                enpassant = pawns != 0 && target != 0 && !((GetPieces() & (st->EpSquare | (st->EpSquare + (int)PawnPush(SideToMove)))) != 0);
                while (pawns != 0)
                {
                    ESquare p = PopLsb(ref pawns);
                    legalEP |= !((AttackersTo(GetSquare<SKing>(SideToMove), occ ^ p) & GetPieces(them) & ~target) !=0);
                }
            }
            if (!enpassant || !legalEP)
            {
                st->EpSquare = ESquare.SquareNone;
            }    
            string rest = ss.ReadToEnd();
            var parts = rest.Trim().Split(' ');
            st->Rule50 = int.Parse(parts[0]);
            gamePly = int.Parse(parts[1]);
            gamePly = Math.Max(2 * (gamePly - 1), 0) + (SideToMove == EColor.Black ? 1 : 0);
            chess960 = isChess960 ? 1 : 0;
            SetState();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public Position Set(string code, EColor c, StateInfo* si)
        {
            return Set(code, false, si);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCastlingRight(EColor c, ESquare rfrom)
        {
            ESquare kfrom = GetSquare<SKing>(c);
            var QueenSide = c == EColor.White ? SWhite.QueenSide : SBlack.QueenSide;
            var KingSide = c == EColor.White ? SWhite.KingSide : SBlack.KingSide;
            ECastlingRights cr = kfrom < rfrom ? KingSide : QueenSide;
            st->CastlingRights |= (int)cr;
            CastlingRightsMask[(int)kfrom] |= (int)cr;
            CastlingRightsMask[(int)rfrom] |= (int)cr;
            castlingRookSquare[(int)cr] = rfrom;
            ESquare kto = RelativeSquare(c, (cr & ECastlingRights.KingSide) != 0 ? ESquare.SQ_G1 : ESquare.SQ_C1);
            ESquare rto = RelativeSquare(c, (cr & ECastlingRights.KingSide) != 0 ? ESquare.SQ_F1 : ESquare.SQ_D1);
            CastlingPath[(int)cr] = (BetweenBB(rfrom, rto) | BetweenBB(kfrom, kto)) & ~(SquareBB(kfrom) | SquareBB(rfrom));
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void SetState() 
        {
            SBitBoard b = GetPieces();
            int white = (int)EColor.White;
            int black = (int)EColor.Black;
            EColor them = SideToMove == EColor.White ? EColor.Black : EColor.White;
            st->Key = 0;
            st->MinorPieceKey = 0;
            st->NonPawnKey[white] = st->NonPawnKey[black] = 0;
            st->PawnKey = Zobrist.NoPawns;
            st->NonPawnMaterial[white] = st->NonPawnMaterial[black] = VALUE_ZERO;
            st->CheckersBB = AttackersTo(GetSquare<SKing>(SideToMove)) & GetPieces(them);
            SetCheckInfo();
            while (b != 0)
            {
                ESquare s = PopLsb(ref b);
                EPiece pc = PieceOn(s);
                st->Key ^= Zobrist.Psq[(int)pc][(int)s];
                if (TypeOf(pc) == EPieceType.Pawn)
                {
                    st->PawnKey ^= Zobrist.Psq[(int)pc][(int)s];
                }
                else
                {
                    st->NonPawnKey[(int)ColorOf(pc)] ^= Zobrist.Psq[(int)pc][(int)s];
                    if (TypeOf(pc) != EPieceType.King)
                    {
                        st->NonPawnMaterial[(int)ColorOf(pc)] += PieceValue[(int)pc];
                        if (TypeOf(pc) <= EPieceType.Bishop)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)s];
                        }
                    }
                }
            }

            if (st->EpSquare != ESquare.SquareNone)
            {
                st->Key ^= Zobrist.EnPassant[(int)FileOf(st->EpSquare)];
            }   
            if (SideToMove == EColor.Black)
            {
                st->Key ^= Zobrist.Side;
            }
            st->Key ^= Zobrist.Castling[st->CastlingRights];
            st->MaterialKey = ComputeMaterialKey();
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private SKey ComputeMaterialKey()
        {
            SKey k = 0;
            for (EPiece pc = 0; pc < EPiece.PieceNB; pc++)
            {
                for (int cnt = 0; cnt< PieceCount[(int)pc]; ++cnt)
                {
                    k ^= Zobrist.Psq[(int)pc][8 + cnt];
                }    
            }
            return k;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCheckInfo()
        {
            SBitBoard pieces = GetPieces();
            EColor them = SideToMove == EColor.White ? EColor.Black : EColor.White;
            ESquare ksq = GetSquare<SKing>(them);
            st->CheckSquares[(int)EPieceType.Pawn - 1] = AttacksBB<SPawn>(ksq, them);
            st->CheckSquares[(int)EPieceType.Knight - 1] = AttacksBB<SKnight>(ksq);
            st->CheckSquares[(int)EPieceType.Bishop - 1] = AttacksBB<SBishop>(ksq, pieces);
            st->CheckSquares[(int)EPieceType.Rook - 1] = AttacksBB<SRook>(ksq, pieces);
            st->CheckSquares[(int)EPieceType.Queen - 1] = st->CheckSquares[(int)EPieceType.Bishop - 1] | st->CheckSquares[(int)EPieceType.Rook - 1];
            st->CheckSquares[(int)EPieceType.King - 1] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UpdateSliderBlockers(EColor c)
        {
            EColor them = c == EColor.White ? EColor.Black : EColor.White;
            ESquare ksq = GetSquare<SKing>(c);
            st->BlockersForKing[(int)c] = 0;
            st->Pinners[(int)them] = 0;
            SBitBoard snipers = ((AttacksBB<SRook>(ksq) & GetPieces<SPieces<SRook, SQueen>>())
                              | (AttacksBB<SBishop>(ksq) & GetPieces<SPieces<SBishop, SQueen>>()))
                              & GetPieces(them);
            SBitBoard occupancy = GetPieces() ^ snipers;
            while (snipers != 0)
            {
                ESquare sniperSq = PopLsb(ref snipers);
                SBitBoard b = BetweenBB(ksq, sniperSq) & occupancy;
                if (b != 0 && !MoreThanOne(b))
                {
                    st->BlockersForKing[(int)c] |= b;
                    if ((b & GetPieces(c)) != 0)
                    {
                        st->Pinners[(int)them] |= sniperSq;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard AttackersTo(ESquare s)
        {
            return AttackersTo(s, GetPieces());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard AttackersTo(ESquare s, SBitBoard occupied)
        {
            return (AttacksBB<SRook>(s, occupied) & GetPieces<SPieces<SRook, SQueen>>())
                 | (AttacksBB<SBishop>(s, occupied) & GetPieces<SPieces<SBishop, SQueen>>())
                 | (AttacksBB<SPawn>(s, EColor.White) & GetPieces<SPawn>(EColor.White))
                 | (AttacksBB<SPawn>(s, EColor.Black) & GetPieces<SPawn>(EColor.Black))
                 | (AttacksBB<SKnight>(s) & GetPieces<SKnight>())
                 | (AttacksBB<SKing>(s) & GetPieces<SKing>());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackersToExist(ESquare s, SBitBoard occupied, EColor c)
        {
            return (AttacksBB<SRook>(s, occupied) & GetPieces<SPieces<SRook, SQueen>>(c)) != 0
                || (AttacksBB<SBishop>(s, occupied) & GetPieces<SPieces<SBishop, SQueen>>(c)) != 0
                || (AttacksBB<SPawn>(s, c == EColor.White ? EColor.Black : EColor.White) & GetPieces<SPawn>(c)) !=0
                || (AttacksBB<SKnight>(s) & GetPieces<SKnight>(c)) !=0
                || (AttacksBB<SKing>(s) & GetPieces<SKing>(c)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Legal(SMove m)
        {
            EColor us = SideToMove;
            EColor them = us == EColor.White ? EColor.Black : EColor.White;
            ESquare from = m.FromSq();
            ESquare to = m.ToSq();
            SBitBoard pieces = GetPieces();
            if (m.TypeOf() == EMoveType.EnPassant)
            {
                ESquare ksq = GetSquare<SKing>(us);
                ESquare capsq = to - (int)PawnPush(us);
                SBitBoard occupied = (pieces ^ from ^ capsq) | to;
                return !((AttacksBB<SRook>(ksq, occupied) & GetPieces<SPieces<SQueen, SRook>>(them)) != 0)
                    && !((AttacksBB<SBishop>(ksq, occupied) & GetPieces<SPieces<SQueen, SBishop>>(them)) != 0);
            }
            if (m.TypeOf() == EMoveType.Castling)
            {
                to = RelativeSquare(us, to > from ? ESquare.SQ_G1 : ESquare.SQ_C1);
                EDirection step = to > from ? EDirection.West : EDirection.East;
                for (int s = (int)to; s != (int)from; s += (int)step)
                {
                    if (AttackersToExist((ESquare)s, pieces, them))
                    {
                        return false;
                    }    
                }
                return !(chess960 != 0) || !((BlockersForKing(us) & m.ToSq()) != 0);
            }
            if (TypeOf(PieceOn(from)) == EPieceType.King)
            {
                return !(AttackersToExist(to, pieces ^ from, them));
            }
            return !((BlockersForKing(us) & from) != 0) || (LineBB(from, to) & GetPieces<SKing>(us)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void PutPiece(EPiece pc, ESquare s, DirtyThreats* dts) 
        {
            EColor c = ColorOf(pc);
            EPieceType t = TypeOf(pc);
            Board[(int)s] = pc;
            ByTypeBB[(int)EPieceType.AllPieces] |= ByTypeBB[(int)t] |= s;
            ByColorBB[(int)c] |= s;
            PieceCount[(int)pc]++;
            PieceCount[(int)MakePiece(c, EPieceType.AllPieces)]++;
            if (dts != null)
            {
                UpdatePieceThreats<STrue>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void RemovePiece(ESquare s, DirtyThreats* dts) 
        {
            EPiece pc = Board[(int)s];
            if (dts != null)
            {
                UpdatePieceThreats<SFalse>(pc, s, dts);
            }
            ByTypeBB[(int)EPieceType.AllPieces] ^= s;
            ByTypeBB[(int)TypeOf(pc)] ^= s;
            ByColorBB[(int)ColorOf(pc)] ^= s;
            Board[(int)s] = EPiece.NoPiece;
            PieceCount[(int)pc]--;
            PieceCount[(int)MakePiece(ColorOf(pc), EPieceType.AllPieces)]--;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void MovePiece(ESquare from, ESquare to, DirtyThreats* dts)
        {
            EPiece pc = Board[(int)from];
            SBitBoard fromTo = SquareBB(from) | SquareBB(to);
            if (dts != null)
            {
                UpdatePieceThreats<SFalse>(pc, from, dts, fromTo);
            }
            ByTypeBB[(int)EPieceType.AllPieces] ^= fromTo;
            ByTypeBB[(int)TypeOf(pc)] ^= fromTo;
            ByColorBB[(int)ColorOf(pc)] ^= fromTo;
            Board[(int)from] = EPiece.NoPiece;
            Board[(int)to] = pc;
            if (dts != null)
            {
                UpdatePieceThreats<STrue>(pc, to, dts, fromTo);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void SwapPiece(ESquare s, EPiece pc, DirtyThreats* dts)
        {
            EPiece old = Board[(int)s];
            RemovePiece(s, null);
            if (dts != null)
            {
                UpdatePieceThreats<SFalse, SFalse>(old, s, dts);
            }
            PutPiece(pc, s, null);
            if (dts != null)
            {
                UpdatePieceThreats<STrue, SFalse>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public SBitBoard CheckSquares(EPieceType pt) 
        { 
            return st->CheckSquares[(int)pt]; 
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool GivesCheck(SMove m)
        {
            SBitBoard pieces = GetPieces();
            ESquare from = m.FromSq();
            ESquare to = m.ToSq();
            if ((CheckSquares(TypeOf(PieceOn(from))) & to) != 0)
            {
                return true;
            }
            EColor them = SideToMove == EColor.White ? EColor.Black : EColor.White;
            if ((BlockersForKing(them) & from) != 0)
            {
                return !((LineBB(from, to) & GetPieces<SKing>(them)) != 0) || m.TypeOf() == EMoveType.Castling;
            }
            switch (m.TypeOf())
            {
                case EMoveType.Normal:
                    return false;
                case EMoveType.Promotion:
                    return (AttacksBB(m.PromotionType(), to, pieces ^ from) & GetPieces<SKing>(them)) != 0;
                case EMoveType.EnPassant : 
                {
                    ESquare capsq = MakeSquare(FileOf(to), RankOf(from));
                    SBitBoard b = (pieces ^ from ^ capsq) | to;
                    return ((AttacksBB<SRook>(GetSquare<SKing>(them), b) & GetPieces<SPieces<SQueen, SRook>>(SideToMove))
                         | (AttacksBB<SBishop>(GetSquare<SKing>(them), b) & GetPieces<SPieces<SQueen, SBishop>>(SideToMove))) != 0;
                }
                default : 
                {
                    ESquare rto = RelativeSquare(SideToMove, to > from ? ESquare.SQ_F1 : ESquare.SQ_D1);
                    return (CheckSquares(EPieceType.Rook) & rto) != 0;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void DoMove(SMove m, ref StateInfo newSt, bool givesCheck, ref DirtyPiece dp, ref DirtyThreats dts)
        {
            fixed (DirtyThreats* d = &dts)
            fixed (DirtyPiece* p = &dp)
            fixed (StateInfo* s = &newSt)
            {
                SKey k = st->Key ^ Zobrist.Side;
                newSt.Previous = st;
                Unsafe.CopyBlock(s, st, (uint)Unsafe.ByteOffset(ref Unsafe.As<StateInfo, byte>(ref *st), ref Unsafe.As<SKey, byte>(ref st->Key)));
                st = s;
                ++gamePly;
                ++st->Rule50;
                ++st->PliesFromNull;
                EColor us = SideToMove;
                EColor them = us == EColor.White ? EColor.Black : EColor.White;
                ESquare from = m.FromSq();
                ESquare to = m.ToSq();
                EPiece pc = PieceOn(from);
                EPiece captured = m.TypeOf() == EMoveType.EnPassant ? MakePiece(them, EPieceType.Pawn) : PieceOn(to);
                dp.Pc = pc;
                dp.From = from;
                dp.To = to;
                dp.AddSq = ESquare.SquareNone;
                dts.Us = us;
                dts.PrevKsq = GetSquare<SKing>(us);
                dts.ThreatenedSqs = dts.ThreateningSqs = 0;
                if (m.TypeOf() == EMoveType.Castling)
                {
                    ESquare rfrom = ESquare.SquareNone, rto = ESquare.SquareNone;
                    DoCastling<STrue>(us, from, ref to, ref rfrom, ref rto, d, p);
                    k ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                    st->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                    captured = EPiece.NoPiece;
                }
                else if (captured != 0)
                {
                    ESquare capsq = to;
                    if (TypeOf(captured) == EPieceType.Pawn)
                    {
                        if (m.TypeOf() == EMoveType.EnPassant)
                        {
                            capsq -= (int)PawnPush(us);
                            RemovePiece(capsq, d);
                        }
                        st->PawnKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                    }
                    else
                    {
                        st->NonPawnMaterial[(int)them] -= PieceValue[(int)captured];
                        st->NonPawnKey[(int)them] ^= Zobrist.Psq[(int)captured][(int)capsq];
                        if (TypeOf(captured) <= EPieceType.Bishop)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                        }
                    }
                    dp.RemovePc = captured;
                    dp.RemoveSq = capsq;
                    k ^= Zobrist.Psq[(int)captured][(int)capsq];
                    st->MaterialKey ^= Zobrist.Psq[(int)captured][8 + PieceCount[(int)captured] - ((m.TypeOf() != EMoveType.EnPassant) ? 1 : 0)];
                    st->Rule50 = 0;
                }
                else
                {
                    dp.RemoveSq = ESquare.SquareNone;
                }
                k ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                if (st->EpSquare != ESquare.SquareNone)
                {
                    k ^= Zobrist.EnPassant[(int)FileOf(st->EpSquare)];
                    st->EpSquare = ESquare.SquareNone;
                }
                k ^= Zobrist.Castling[st->CastlingRights];
                st->CastlingRights &= ~(CastlingRightsMask[(int)from] | CastlingRightsMask[(int)to]);
                k ^= Zobrist.Castling[st->CastlingRights];
                if (m.TypeOf() != EMoveType.Castling)
                {
                    if (captured != 0 && m.TypeOf() != EMoveType.EnPassant)
                    {
                        RemovePiece(from, d);
                        SwapPiece(to, pc, d);
                    }
                    else
                    {
                        MovePiece(from, to, d);
                    }
                }
                if (TypeOf(pc) == EPieceType.Pawn)
                {
                    if (((int)to ^ (int)from) == 16)
                    {
                        ESquare epSquare = to - (int)PawnPush(us);
                        SBitBoard pawns = AttacksBB<SPawn>(epSquare, us) & GetPieces<SPawn>(them);
                        if (pawns != 0)
                        {
                            ESquare ksq = GetSquare<SKing>(them);
                            SBitBoard notBlockers = ~st->BlockersForKing[(int)them];
                            bool noDiscovery = (notBlockers % from) || FileOf(from) == FileOf(ksq);
                            if (noDiscovery && (pawns & (notBlockers | LineBB(epSquare, ksq))) != 0)
                            {
                                st->EpSquare = epSquare;
                                k ^= Zobrist.EnPassant[(int)FileOf(epSquare)];
                            }
                        }
                    }
                    else if (m.TypeOf() == EMoveType.Promotion)
                    {
                        EPiece promotion = MakePiece(us, m.PromotionType());
                        EPieceType promotionType = TypeOf(promotion);
                        SwapPiece(to, promotion, d);
                        dp.AddPc = promotion;
                        dp.AddSq = to;
                        dp.To = ESquare.SquareNone;
                        k ^= Zobrist.Psq[(int)promotion][(int)to];
                        st->MaterialKey ^= Zobrist.Psq[(int)promotion][8 + PieceCount[(int)promotion] - 1] ^ Zobrist.Psq[(int)pc][8 + PieceCount[(int)pc]];
                        st->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)promotion][(int)to];
                        if (promotionType <= EPieceType.Bishop)
                        {
                            st->MinorPieceKey ^= Zobrist.Psq[(int)promotion][(int)to];
                        }
                        st->NonPawnMaterial[(int)us] += PieceValue[(int)promotion];
                    }
                    st->PawnKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    st->Rule50 = 0;
                }
                else
                {
                    st->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    if (TypeOf(pc) <= EPieceType.Bishop)
                    {
                        st->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                    }
                }
                st->Key = k;
                st->CapturedPiece = captured;
                st->CheckersBB = givesCheck ? AttackersTo(GetSquare<SKing>(them)) & GetPieces(us) : 0;
                SideToMove = SideToMove == EColor.White ? EColor.Black : EColor.White;
                SetCheckInfo();
                st->Repetition = 0;
                int end = Math.Min(st->Rule50, st->PliesFromNull);
                if (end >= 4)
                {
                    StateInfo* stp = st;
                    stp = stp->Previous; stp = stp->Previous;
                    for (int i = 4; i <= end; i += 2)
                    {
                        stp = stp->Previous; stp = stp->Previous;
                        if (stp->Key == st->Key)
                        {
                            st->Repetition = stp->Repetition != 0 ? -i : i;
                            break;
                        }
                    }
                }
                dts.Ksq = GetSquare<SKing>(us);

            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void DoMove(SMove m, ref StateInfo newSt)
        {
            scratch_dts = new DirtyThreats();
            DoMove(m, ref newSt, GivesCheck(m), ref scratch_dp, ref scratch_dts);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void UndoMove(SMove m)
        {
            SideToMove = SideToMove == EColor.White ? EColor.Black : EColor.White;
            EColor us = SideToMove;
            ESquare from = m.FromSq();
            ESquare to = m.ToSq();
            EPiece pc = PieceOn(to);
            if (m.TypeOf() == EMoveType.Promotion)
            {
                RemovePiece(to, null);
                pc = MakePiece(us, EPieceType.Pawn);
                PutPiece(pc, to, null);
            }
            if (m.TypeOf() == EMoveType.Castling)
            {
                ESquare rfrom = 0, rto = 0;
                DoCastling<SFalse>(us, from, ref to, ref rfrom, ref rto, null, null);
            }
            else
            {
                MovePiece(to, from, null);
                if (st->CapturedPiece != 0)
                {
                    ESquare capsq = to;
                    if (m.TypeOf() == EMoveType.EnPassant)
                    {
                        capsq -= (int)PawnPush(us);
                    }
                    PutPiece(st->CapturedPiece, capsq, null);
                }
            }
            st = st->Previous;
            --gamePly;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void DoCastling<D>(EColor us, ESquare from, ref ESquare to, ref ESquare rfrom, ref ESquare rto, DirtyThreats* dts, DirtyPiece* dp) where D : struct, IBool
        {
            if (dp != null)
            {
                bool kingSide = to > from;
                rfrom = to; 
                rto = RelativeSquare(us, kingSide? ESquare.SQ_F1 : ESquare.SQ_D1);
                to = RelativeSquare(us, kingSide? ESquare.SQ_G1 : ESquare.SQ_C1);
                if (D.Value)
                {
                    dp->To = to;
                    dp->RemovePc = dp->AddPc = MakePiece(us, EPieceType.Rook);
                    dp->RemoveSq = rfrom;
                    dp->AddSq = rto;
                }
                RemovePiece(D.Value ? from : to, dts);
                RemovePiece(D.Value ? rfrom : rto, dts);
                PutPiece(MakePiece(us, EPieceType.King), D.Value ? to : from, dts);
                PutPiece(MakePiece(us, EPieceType.Rook), D.Value ? rto : rfrom, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static void AddDirtyThreat<B>(DirtyThreats* dts, EPiece pc, EPiece threatened, ESquare s, ESquare threatenedSq) where B : struct, IBool
        {
            if (B.Value)
            {
                dts->ThreatenedSqs |= threatenedSq;
                dts->ThreateningSqs |= s;
            }
            dts->List.Add(new DirtyThreat(pc, threatened, s, threatenedSq, B.Value));
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void UpdatePieceThreats<P>(EPiece pc, ESquare s, DirtyThreats* dts, SBitBoard noRaysContaining = default) where P : struct, IBool
        {
            UpdatePieceThreats<P, STrue>(pc, s, dts, noRaysContaining);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void UpdatePieceThreats<P, C>(EPiece pc, ESquare s, DirtyThreats* dts, SBitBoard noRaysContaining = default) where P : struct, IBool where C : struct, IBool
        {
            SBitBoard occupied = GetPieces();
            SBitBoard rookQueens = GetPieces<SRook>() | GetPieces<SQueen>();
            SBitBoard bishopQueens = GetPieces<SBishop>() | GetPieces<SQueen>();
            SBitBoard rAttacks = AttacksBB<SRook>(s, occupied);
            SBitBoard bAttacks = AttacksBB<SBishop>(s, occupied);
            SBitBoard kings = GetPieces<SKing>();
            SBitBoard occupiedNoK = occupied ^ kings;
            SBitBoard sliders = (rookQueens & rAttacks) | (bishopQueens & bAttacks);
            void ProcessSliders(bool addDirectAttacks)
            {
                while (sliders != 0)
                {
                    ESquare sliderSq = PopLsb(ref sliders);
                    EPiece slider = PieceOn(sliderSq);
                    SBitBoard ray = RayPassBB[(int)sliderSq][(int)s];
                    SBitBoard discovered = ray & (rAttacks | bAttacks) & occupiedNoK;
                    if (discovered != 0 && (RayPassBB[(int)sliderSq][(int)s] & noRaysContaining) != noRaysContaining)
                    {
                        ESquare threatenedSq = Lsb(discovered);
                        EPiece threatenedPc = PieceOn(threatenedSq);
                        AddDirtyThreat<SUnBool<P>>(dts, slider, threatenedPc, sliderSq, threatenedSq);
                    }
                    if (addDirectAttacks)
                    {
                        AddDirtyThreat<P>(dts, slider, pc, sliderSq, s);
                    }
                }
            }
            if (TypeOf(pc) == EPieceType.King)
            {
                if (C.Value)
                {
                    ProcessSliders(false);
                }
                return;
            }
            SBitBoard knights = GetPieces<SKnight>();
            SBitBoard whitePawns = GetPieces<SPawn>(EColor.White);
            SBitBoard blackPawns = GetPieces<SPawn>(EColor.Black);
            SBitBoard threatened = AttacksBB(pc, s, occupied) & occupiedNoK;
            SBitBoard incomingThreats = (pseudoAttacks[(int)EPieceType.Knight][(int)s] & knights) 
                                      | (AttacksBB<SPawn>(s, EColor.White) & blackPawns) 
                                      | (AttacksBB<SPawn>(s, EColor.Black) & whitePawns) 
                                      | (pseudoAttacks[(int)EPieceType.King][(int)s] & kings);
            while (threatened != 0)
            {
                ESquare threatenedSq = PopLsb(ref threatened);
                EPiece threatenedPc = PieceOn(threatenedSq);
                AddDirtyThreat<P>(dts, pc, threatenedPc, s, threatenedSq);
            }
            if (C.Value)
            {
                ProcessSliders(true);
            }
            else
            {
                incomingThreats |= sliders;
            }
            while (incomingThreats != 0)
            {
                ESquare srcSq = PopLsb(ref incomingThreats);
                EPiece srcPc = PieceOn(srcSq);
                AddDirtyThreat<P>(dts, srcPc, pc, srcSq, s);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPiece PieceOn(ESquare s)
        {
            return Board[(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare GetSquare<T>(EColor c) where T : struct, IPieceTypes
        {
            return Lsb(GetPieces<T>(c));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard GetPieces()
        {
            return ByColorBB[(int)EColor.White] | ByColorBB[(int)EColor.Black];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard GetPieces<P>(EColor c) where P : struct, IPieceTypes
        {
            return GetPieces(c) & P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard GetPieces(EColor c)
        {
            return ByColorBB[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard GetPieces<P>() where P : struct, IPieceTypes
        {
            return P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard BlockersForKing(EColor c)
        {
            return st->BlockersForKing[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(ECastlingRights cr)
        {
            return (st->CastlingRights & (int)cr) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CastlingImpeded(ECastlingRights cr) 
        {
            return (GetPieces() & CastlingPath[(int)cr]) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare CastlingRookSquare(ECastlingRights cr)
        {
            return castlingRookSquare[(int)cr];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard Checkers()
        {
            return st->CheckersBB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare EpSquare()
        {
            return st->EpSquare;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChess960()
        {
            return chess960 != 0;
        }
    }
}
