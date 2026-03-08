using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    unsafe public partial class Position
    {
        public Piece[] Board = new Piece[(int)Square.SquareNB];
        public BitBoard[] ByTypeBB = new BitBoard[(int)PieceType.PieceTypeNB];
        public BitBoard[] ByColorBB = new BitBoard[(int)Color.ColorNB];
        public int[] PieceCount = new int[(int)Piece.PieceNB];
        public int[] CastlingRightsMask = new int[(int)Square.SquareNB];
        public Square[] castlingRookSquare = new Square[(int)CastlingRights.CastlingRightNB];
        public BitBoard[] CastlingPath = new BitBoard[(int)CastlingRights.CastlingRightNB];
        public StateInfo* previous;
        public int gamePly;
        public Color SideToMove;
        public int chess960;
        public DirtyPiece scratch_dp;
        public DirtyThreats scratch_dts;
        public void SetCheckInfo()
        {
            Color them = SideToMove == Color.White ? Color.Black : Color.White;
            Square ksq = GetSquare<King>(them);
            previous->CheckSquares[(int)PieceType.Pawn - 1] = BitBoards.AttacksBB<Pawn>(ksq, them);
            previous->CheckSquares[(int)PieceType.Knight - 1] = BitBoards.AttacksBB<Knight>(ksq);
            previous->CheckSquares[(int)PieceType.Bishop - 1] = BitBoards.AttacksBB<Bishop>(ksq, GetPieces());
            previous->CheckSquares[(int)PieceType.Rook - 1] = BitBoards.AttacksBB<Rook>(ksq, GetPieces());
            previous->CheckSquares[(int)PieceType.Queen - 1] = previous->CheckSquares[(int)PieceType.Bishop] | previous->CheckSquares[(int)PieceType.Rook];
            previous->CheckSquares[(int)PieceType.King - 1] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSliderBlockers(Color c)
        {
            Color them = c == Color.White ? Color.Black : Color.White;
            Square ksq = GetSquare<King>(c);
            previous->BlockersForKing[(int)c] = 0;
            previous->Pinners[(int)them] = 0;
            BitBoard snipers = ((BitBoards.AttacksBB<Rook>(ksq) & GetPieces<Pieces<Rook, Queen>>())
                             | (BitBoards.AttacksBB<Bishop>(ksq) & GetPieces<Pieces<Bishop, Queen>>()))
                             & GetPieces(them);
            BitBoard occupancy = GetPieces() ^ snipers;
            while (snipers != 0)
            {
                Square sniperSq = BitBoards.PopLsb(ref snipers);
                BitBoard b = BitBoards.BetweenBB(ksq, sniperSq) & occupancy;
                if (b != 0 && !BitBoards.MoreThanOne(b))
                {
                    previous->BlockersForKing[(int)c] |= b;
                    if ((b & GetPieces(c)) != 0)
                    {
                        previous->Pinners[(int)them] |= sniperSq;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard AttackersTo(Square s)
        {
            return AttackersTo(s, GetPieces());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard AttackersTo(Square s, BitBoard occupied)
        {
            return (BitBoards.AttacksBB<Rook>(s, occupied) & GetPieces<Pieces<Rook, Queen>>())
                 | (BitBoards.AttacksBB<Bishop>(s, occupied) & GetPieces<Pieces<Bishop, Queen>>())
                 | (BitBoards.AttacksBB<Pawn>(s, Color.White) & GetPieces<Pawn>(Color.White))
                 | (BitBoards.AttacksBB<Pawn>(s, Color.Black) & GetPieces<Pawn>(Color.Black))
                 | (BitBoards.AttacksBB<Knight>(s) & GetPieces<Knight>())
                 | (BitBoards.AttacksBB<King>(s) & GetPieces<King>());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackersToExist(Square s, BitBoard occupied, Color c)
        {
            return (BitBoards.AttacksBB<Rook>(s, occupied) & GetPieces<Pieces<Rook, Queen>>(c)) != 0
                || (BitBoards.AttacksBB<Bishop>(s, occupied) & GetPieces<Pieces<Bishop, Queen>>(c)) != 0
                || (BitBoards.AttacksBB<Pawn>(s, c == Color.White ? Color.Black : Color.White) & GetPieces<Pawn>(c)) !=0
                || (BitBoards.AttacksBB<Knight>(s) & GetPieces<Knight>(c)) !=0
                || (BitBoards.AttacksBB<King>(s) & GetPieces<King>(c)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Legal(Move m)
        {
            Color us = SideToMove;
            Color them = us == Color.White ? Color.Black : Color.White;
            Square from = m.FromSq();
            Square to = m.ToSq();
            if (m.TypeOf() == MoveType.EnPassant)
            {
                Square ksq = GetSquare<King>(us);
                Square capsq = to - (int)Types.PawnPush(us);
                BitBoard occupied = (GetPieces() ^ from ^ capsq) | to;
                return !((BitBoards.AttacksBB<Rook>(ksq, occupied) & GetPieces<Pieces<Queen, Rook>>(them)) != 0)
                    && !((BitBoards.AttacksBB<Bishop>(ksq, occupied) & GetPieces<Pieces<Queen, Bishop>>(them)) != 0);
            }
            if (m.TypeOf() == MoveType.Castling)
            {
                to = Types.RelativeSquare(us, to > from ? Square.SQ_G1 : Square.SQ_C1);
                Direction step = to > from ? Direction.West : Direction.East;
                for (int s = (int)to; s != (int)from; s += (int)step)
                {
                    if (AttackersToExist((Square)s, GetPieces(), them))
                    {
                        return false;
                    }    
                }
                return !(chess960 != 0) || !((BlockersForKing(us) & m.ToSq()) != 0);
            }
            if (Types.TypeOf(PieceOn(from)) == PieceType.King)
            {
                return !(AttackersToExist(to, GetPieces() ^ from, them));
            }
            return !((BlockersForKing(us) & from) != 0) || (BitBoards.LineBB(from, to) & GetPieces<King>(us)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void PutPiece(Piece pc, Square s, DirtyThreats* dts) 
        {
            Color c = Types.ColorOf(pc);
            PieceType t = Types.TypeOf(pc);
            Board[(int)s] = pc;
            ByTypeBB[(int)PieceType.AllPieces] |= ByTypeBB[(int)t] |= s;
            ByColorBB[(int)c] |= s;
            PieceCount[(int)pc]++;
            PieceCount[(int)Types.MakePiece(c, PieceType.AllPieces)]++;
            if (dts != null)
            {
                UpdatePieceThreats<True>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void RemovePiece(Square s, DirtyThreats* dts) 
        {
            Piece pc = Board[(int)s];
            if (dts != null)
            {
                UpdatePieceThreats<False>(pc, s, dts);
            }
            ByTypeBB[(int)PieceType.AllPieces] ^= s;
            ByTypeBB[(int)Types.TypeOf(pc)] ^= s;
            ByColorBB[(int)Types.ColorOf(pc)] ^= s;
            Board[(int)s] = Piece.NoPiece;
            PieceCount[(int)pc]--;
            PieceCount[(int)Types.MakePiece(Types.ColorOf(pc), PieceType.AllPieces)]--;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void MovePiece(Square from, Square to, DirtyThreats* dts)
        {
            Piece pc = Board[(int)from];
            BitBoard fromTo = (BitBoard)((ulong)from | (ulong)to);
            if (dts != null)
            {
                UpdatePieceThreats<False>(pc, from, dts, fromTo);
            }
            ByTypeBB[(int)PieceType.AllPieces] ^= fromTo;
            ByTypeBB[(int)Types.TypeOf(pc)] ^= fromTo;
            ByColorBB[(int)Types.ColorOf(pc)] ^= fromTo;
            Board[(int)from] = Piece.NoPiece;
            Board[(int)to] = pc;
            if (dts != null)
            {
                UpdatePieceThreats<True>(pc, to, dts, fromTo);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void SwapPiece(Square s, Piece pc, DirtyThreats* dts)
        {
            Piece old = Board[(int)s];
            RemovePiece(s, null);
            if (dts != null)
            {
                UpdatePieceThreats<False, False>(old, s, dts);
            }
            PutPiece(pc, s, null);
            if (dts != null)
            {
                UpdatePieceThreats<True, False>(pc, s, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void DoMove(Move m, StateInfo* newSt, bool givesCheck, DirtyPiece* dp, DirtyThreats* dts) 
        {
            Key k = previous->Key ^ Zobrist.Side;
            unsafe
            {
                int size = (int)Marshal.OffsetOf<StateInfo>("Key");
                Buffer.MemoryCopy(previous, &newSt, size, size);
            }
            newSt->Previous = previous;
            previous = newSt;
            ++gamePly;
            ++previous->Rule50;
            ++previous->PliesFromNull;
            Color us = SideToMove;
            Color them = us == Color.White ? Color.Black : Color.White;
            Square from = m.FromSq();
            Square to = m.ToSq();
            Piece pc = PieceOn(from);
            Piece captured = m.TypeOf() == MoveType.EnPassant ? Types.MakePiece(them, PieceType.Pawn) : PieceOn(to);
            dp->Pc = pc;
            dp->From = from;
            dp->To = to;
            dp->AddSq = Square.SquareNone;
            dts->Us = us;
            dts->PrevKsq = GetSquare<King>(us);
            dts->ThreatenedSqs = dts->ThreateningSqs = 0;
            if (m.TypeOf() == MoveType.Castling)
            {
                Square rfrom = Square.SquareNone, rto = Square.SquareNone;
                DoCastling<True>(us, from, ref to, ref rfrom, ref rto, dts, dp);
                k ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                previous->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                captured = Piece.NoPiece;
            }
            else if (captured != 0)
            {
                Square capsq = to;
                if (Types.TypeOf(captured) == PieceType.Pawn)
                {
                    if (m.TypeOf() == MoveType.EnPassant)
                    {
                        capsq -= (int)Types.PawnPush(us);
                        RemovePiece((Square)capsq, dts);
                    }
                    previous->PawnKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                }
                else
                {
                    previous->NonPawnMaterial[(int)them] -= Types.PieceValue[(int)captured];
                    previous->NonPawnKey[(int)them] ^= Zobrist.Psq[(int)captured][(int)capsq];
                    if (Types.TypeOf(captured) <= PieceType.Bishop)
                    {
                        previous->MinorPieceKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                    }
                }
                dp->RemovePc = captured;
                dp->RemoveSq = capsq;
                k ^= Zobrist.Psq[(int)captured][(int)capsq];
                previous->MaterialKey ^= Zobrist.Psq[(int)captured][8 + PieceCount[(int)captured] - ((m.TypeOf() != MoveType.EnPassant) ? 1 : 0)];
                previous->Rule50 = 0;
            }
            else
            {
                dp->RemoveSq = Square.SquareNone;
            }
            k ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
            if (previous->EpSquare != Square.SquareNone)
            {
                k ^= Zobrist.EnPassant[(int)Types.FileOf(previous->EpSquare)];
                previous->EpSquare = Square.SquareNone;
            }
            k ^= Zobrist.Castling[previous->CastlingRights];
            previous->CastlingRights &= ~(CastlingRightsMask[(int)from] | CastlingRightsMask[(int)to]);
            k ^= Zobrist.Castling[previous->CastlingRights];
            if (m.TypeOf() != MoveType.Castling)
            {
                if (captured != 0 && m.TypeOf() != MoveType.EnPassant)
                {
                    RemovePiece(from, dts);
                    SwapPiece(to, pc, dts);
                }
                else
                {
                    MovePiece(from, to, dts);
                }
            }
            if (Types.TypeOf(pc) == PieceType.Pawn)
            {
                if (((int)to ^ (int)from) == 16)
                {
                    Square epSquare = to - (int)Types.PawnPush(us);
                    BitBoard pawns = BitBoards.AttacksBB<Pawn>(epSquare, us) & GetPieces<Pawn>(them);
                    if (pawns != 0)
                    {
                        Square ksq = GetSquare<King>(them);
                        BitBoard notBlockers = ~previous->BlockersForKing[(int)them];
                        bool noDiscovery = (notBlockers % from) || Types.FileOf(from) == Types.FileOf(ksq);
                        if (noDiscovery && (pawns & (notBlockers | BitBoards.LineBB(epSquare, ksq))) != 0)
                        {
                            previous->EpSquare = epSquare;
                            k ^= Zobrist.EnPassant[(int)Types.FileOf(epSquare)];
                        }
                    }
                }
                else if (m.TypeOf() == MoveType.Promotion)
                {
                    Piece promotion = Types.MakePiece(us, m.PromotionType());
                    PieceType promotionType = Types.TypeOf(promotion);
                    SwapPiece(to, promotion, dts);
                    dp->AddPc = promotion;
                    dp->AddSq = to;
                    dp->To = Square.SquareNone;
                    k ^= Zobrist.Psq[(int)promotion][(int)to];
                    previous->MaterialKey ^= Zobrist.Psq[(int)promotion][8 + PieceCount[(int)promotion] - 1] ^ Zobrist.Psq[(int)pc][8 + PieceCount[(int)pc]];
                    previous->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)promotion][(int)to];
                    if (promotionType <= PieceType.Bishop)
                    {
                        previous->MinorPieceKey ^= Zobrist.Psq[(int)promotion][(int)to];
                    }
                    previous->NonPawnMaterial[(int)us] += Types.PieceValue[(int)promotion];
                }
                previous->PawnKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                previous->Rule50 = 0;
            }
            else
            {
                previous->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                if (Types.TypeOf(pc) <= PieceType.Bishop)
                {
                    previous->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                }
            }
            previous->Key = k;
            previous->CapturedPiece = captured;
            previous->CheckersBB = givesCheck ? AttackersTo(GetSquare<King>(them)) & GetPieces(us) : 0;
            SideToMove = SideToMove == Color.White ? Color.Black : Color.White;
            SetCheckInfo();
            previous->Repetition = 0;
            int end = Math.Min(previous->Rule50, previous->PliesFromNull);
            if (end >= 4)
            {
                StateInfo* stp = previous;
                stp = stp->Previous; stp = stp->Previous;
                for (int i = 4; i <= end; i += 2)
                {
                    stp = stp->Previous; stp = stp->Previous;
                    if (stp->Key == previous->Key)
                    {
                        previous->Repetition = stp->Repetition != 0 ? -i : i;
                        break;
                    }
                }
            }
            dts->Ksq = GetSquare<King>(us);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void DoCastling<D>(Color us, Square from, ref Square to, ref Square rfrom, ref Square rto, DirtyThreats* dts, DirtyPiece* dp) where D : struct, IBool
        {
            if (dp != null)
            {
                bool kingSide = to > from;
                rfrom = to;  // Castling is encoded as "king captures friendly rook"
                rto = Types.RelativeSquare(us, kingSide? Square.SQ_F1 : Square.SQ_D1);
                to = Types.RelativeSquare(us, kingSide? Square.SQ_G1 : Square.SQ_C1);
                if (D.Value)
                {
                    dp->To = to;
                    dp->RemovePc = dp->AddPc = Types.MakePiece(us, PieceType.Rook);
                    dp->RemoveSq = rfrom;
                    dp->AddSq = rto;
                }
                // Remove both pieces first since squares could overlap in Chess960
                RemovePiece(D.Value ? from : to, dts);
                RemovePiece(D.Value ? rfrom : rto, dts);
                PutPiece(Types.MakePiece(us, PieceType.King), D.Value ? to : from, dts);
                PutPiece(Types.MakePiece(us, PieceType.Rook), D.Value ? rto : rfrom, dts);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static void AddDirtyThreat<B>(DirtyThreats* dts, Piece pc, Piece threatened, Square s, Square threatenedSq) where B : struct, IBool
        {
            if (B.Value)
            {
                dts->ThreatenedSqs |= threatenedSq;
                dts->ThreateningSqs |= s;
            }
            dts->List.Add(new DirtyThreat(pc, threatened, s, threatenedSq, B.Value));
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void UpdatePieceThreats<P>(Piece pc, Square s, DirtyThreats* dts, BitBoard noRaysContaining = default) where P : struct, IBool
        {
            UpdatePieceThreats<P, True>(pc, s, dts, noRaysContaining);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void UpdatePieceThreats<P, C>(Piece pc, Square s, DirtyThreats* dts, BitBoard noRaysContaining = default) where P : struct, IBool where C : struct, IBool
        {
            BitBoard occupied = GetPieces();
            BitBoard rookQueens = GetPieces<Rook>() | GetPieces<Queen>();
            BitBoard bishopQueens = GetPieces<Bishop>() | GetPieces<Queen>();
            BitBoard rAttacks = BitBoards.AttacksBB<Rook>(s, occupied);
            BitBoard bAttacks = BitBoards.AttacksBB<Bishop>(s, occupied);
            BitBoard kings = GetPieces<King>();
            BitBoard occupiedNoK = occupied ^ kings;
            BitBoard sliders = (rookQueens & rAttacks) | (bishopQueens & bAttacks);
            void ProcessSliders(bool addDirectAttacks)
            {
                while (sliders != 0)
                {
                    Square sliderSq = BitBoards.PopLsb(ref sliders);
                    Piece slider = PieceOn(sliderSq);
                    BitBoard ray = BitBoards.RayPassBB[(int)sliderSq][(int)s];
                    BitBoard discovered = ray & (rAttacks | bAttacks) & occupiedNoK;
                    if (discovered != 0 && (BitBoards.RayPassBB[(int)sliderSq][(int)s] & noRaysContaining) != noRaysContaining)
                    {
                        Square threatenedSq = BitBoards.Lsb(discovered);
                        Piece threatenedPc = PieceOn(threatenedSq);
                        AddDirtyThreat<UnBool<P>>(dts, slider, threatenedPc, sliderSq, threatenedSq);
                    }
                    if (addDirectAttacks)
                    {
                        AddDirtyThreat<P>(dts, slider, pc, sliderSq, s);
                    }
                }
            }
            if (Types.TypeOf(pc) == PieceType.King)
            {
                if (C.Value)
                {
                    ProcessSliders(false);
                }
                return;
            }
            BitBoard knights = GetPieces<Knight>();
            BitBoard whitePawns = GetPieces<Pawn>(Color.White);
            BitBoard blackPawns = GetPieces<Pawn>(Color.Black);
            BitBoard threatened = BitBoards.AttacksBB(pc, s, occupied) & occupiedNoK;
            BitBoard incomingThreats = (BitBoards.pseudoAttacks[(int)PieceType.Knight][(int)s] & knights) | (BitBoards.AttacksBB<Pawn>(s, Color.White) & blackPawns) | (BitBoards.AttacksBB<Pawn>(s, Color.Black) & whitePawns) | (BitBoards.pseudoAttacks[(int)PieceType.King][(int)s] & kings);
            while (threatened != 0)
            {
                Square threatenedSq = BitBoards.PopLsb(ref threatened);
                Piece threatenedPc = PieceOn(threatenedSq);
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
                Square srcSq = BitBoards.PopLsb(ref incomingThreats);
                Piece srcPc = PieceOn(srcSq);
                AddDirtyThreat<P>(dts, srcPc, pc, srcSq, s);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece PieceOn(Square s)
        {
            return Board[(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetSquare<T>(Color c) where T : struct, IPieceTypes
        {
            return BitBoards.Lsb(GetPieces<T>(c));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieces()
        {
            return ByColorBB[(int)Color.White] | ByColorBB[(int)Color.Black];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieces<P>(Color c) where P : struct, IPieceTypes
        {
            return GetPieces(c) & P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieces(Color c)
        {
            return ByColorBB[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieces<P>() where P : struct, IPieceTypes
        {
            return P.Get(ByTypeBB);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard BlockersForKing(Color c)
        {
            return previous->BlockersForKing[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(CastlingRights cr)
        {
            return (previous->CastlingRights & (int)cr) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CastlingImpeded(CastlingRights cr) 
        {
            return (GetPieces() & CastlingPath[(int)cr]) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square CastlingRookSquare(CastlingRights cr)
        {
            return castlingRookSquare[(int)cr];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Checkers()
        {
            return previous->CheckersBB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square EpSquare()
        {
            return previous->EpSquare;
        }
    }
}
