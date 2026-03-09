using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        public StateInfo* previous;
        public int gamePly;
        public EColor SideToMove;
        public int chess960;
        public DirtyPiece scratch_dp;
        public DirtyThreats scratch_dts;
        public void SetCheckInfo()
        {
            EColor them = SideToMove == EColor.White ? EColor.Black : EColor.White;
            ESquare ksq = GetSquare<SKing>(them);
            previous->CheckSquares[(int)EPieceType.Pawn - 1] = BitBoard.AttacksBB<SPawn>(ksq, them);
            previous->CheckSquares[(int)EPieceType.Knight - 1] = BitBoard.AttacksBB<SKnight>(ksq);
            previous->CheckSquares[(int)EPieceType.Bishop - 1] = BitBoard.AttacksBB<SBishop>(ksq, GetPieces());
            previous->CheckSquares[(int)EPieceType.Rook - 1] = BitBoard.AttacksBB<SRook>(ksq, GetPieces());
            previous->CheckSquares[(int)EPieceType.Queen - 1] = previous->CheckSquares[(int)EPieceType.Bishop - 1] | previous->CheckSquares[(int)EPieceType.Rook - 1];
            previous->CheckSquares[(int)EPieceType.King - 1] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSliderBlockers(EColor c)
        {
            EColor them = c == EColor.White ? EColor.Black : EColor.White;
            ESquare ksq = GetSquare<SKing>(c);
            previous->BlockersForKing[(int)c] = 0;
            previous->Pinners[(int)them] = 0;
            SBitBoard snipers = ((BitBoard.AttacksBB<SRook>(ksq) & GetPieces<SPieces<SRook, SQueen>>())
                              | (BitBoard.AttacksBB<SBishop>(ksq) & GetPieces<SPieces<SBishop, SQueen>>()))
                              & GetPieces(them);
            SBitBoard occupancy = GetPieces() ^ snipers;
            while (snipers != 0)
            {
                ESquare sniperSq = BitBoard.PopLsb(ref snipers);
                SBitBoard b = BitBoard.BetweenBB(ksq, sniperSq) & occupancy;
                if (b != 0 && !BitBoard.MoreThanOne(b))
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
        public SBitBoard AttackersTo(ESquare s)
        {
            return AttackersTo(s, GetPieces());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SBitBoard AttackersTo(ESquare s, SBitBoard occupied)
        {
            return (BitBoard.AttacksBB<SRook>(s, occupied) & GetPieces<SPieces<SRook, SQueen>>())
                 | (BitBoard.AttacksBB<SBishop>(s, occupied) & GetPieces<SPieces<SBishop, SQueen>>())
                 | (BitBoard.AttacksBB<SPawn>(s, EColor.White) & GetPieces<SPawn>(EColor.White))
                 | (BitBoard.AttacksBB<SPawn>(s, EColor.Black) & GetPieces<SPawn>(EColor.Black))
                 | (BitBoard.AttacksBB<SKnight>(s) & GetPieces<SKnight>())
                 | (BitBoard.AttacksBB<SKing>(s) & GetPieces<SKing>());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackersToExist(ESquare s, SBitBoard occupied, EColor c)
        {
            return (BitBoard.AttacksBB<SRook>(s, occupied) & GetPieces<SPieces<SRook, SQueen>>(c)) != 0
                || (BitBoard.AttacksBB<SBishop>(s, occupied) & GetPieces<SPieces<SBishop, SQueen>>(c)) != 0
                || (BitBoard.AttacksBB<SPawn>(s, c == EColor.White ? EColor.Black : EColor.White) & GetPieces<SPawn>(c)) !=0
                || (BitBoard.AttacksBB<SKnight>(s) & GetPieces<SKnight>(c)) !=0
                || (BitBoard.AttacksBB<SKing>(s) & GetPieces<SKing>(c)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Legal(SMove m)
        {
            EColor us = SideToMove;
            EColor them = us == EColor.White ? EColor.Black : EColor.White;
            ESquare from = m.FromSq();
            ESquare to = m.ToSq();
            if (m.TypeOf() == EMoveType.EnPassant)
            {
                ESquare ksq = GetSquare<SKing>(us);
                ESquare capsq = to - (int)Types.PawnPush(us);
                SBitBoard occupied = (GetPieces() ^ from ^ capsq) | to;
                return !((BitBoard.AttacksBB<SRook>(ksq, occupied) & GetPieces<SPieces<SQueen, SRook>>(them)) != 0)
                    && !((BitBoard.AttacksBB<SBishop>(ksq, occupied) & GetPieces<SPieces<SQueen, SBishop>>(them)) != 0);
            }
            if (m.TypeOf() == EMoveType.Castling)
            {
                to = Types.RelativeSquare(us, to > from ? ESquare.SQ_G1 : ESquare.SQ_C1);
                EDirection step = to > from ? EDirection.West : EDirection.East;
                for (int s = (int)to; s != (int)from; s += (int)step)
                {
                    if (AttackersToExist((ESquare)s, GetPieces(), them))
                    {
                        return false;
                    }    
                }
                return !(chess960 != 0) || !((BlockersForKing(us) & m.ToSq()) != 0);
            }
            if (Types.TypeOf(PieceOn(from)) == EPieceType.King)
            {
                return !(AttackersToExist(to, GetPieces() ^ from, them));
            }
            return !((BlockersForKing(us) & from) != 0) || (BitBoard.LineBB(from, to) & GetPieces<SKing>(us)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void PutPiece(EPiece pc, ESquare s, DirtyThreats* dts) 
        {
            EColor c = Types.ColorOf(pc);
            EPieceType t = Types.TypeOf(pc);
            Board[(int)s] = pc;
            ByTypeBB[(int)EPieceType.AllPieces] |= ByTypeBB[(int)t] |= s;
            ByColorBB[(int)c] |= s;
            PieceCount[(int)pc]++;
            PieceCount[(int)Types.MakePiece(c, EPieceType.AllPieces)]++;
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
            ByTypeBB[(int)Types.TypeOf(pc)] ^= s;
            ByColorBB[(int)Types.ColorOf(pc)] ^= s;
            Board[(int)s] = EPiece.NoPiece;
            PieceCount[(int)pc]--;
            PieceCount[(int)Types.MakePiece(Types.ColorOf(pc), EPieceType.AllPieces)]--;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void MovePiece(ESquare from, ESquare to, DirtyThreats* dts)
        {
            EPiece pc = Board[(int)from];
            SBitBoard fromTo = (SBitBoard)((ulong)from | (ulong)to);
            if (dts != null)
            {
                UpdatePieceThreats<SFalse>(pc, from, dts, fromTo);
            }
            ByTypeBB[(int)EPieceType.AllPieces] ^= fromTo;
            ByTypeBB[(int)Types.TypeOf(pc)] ^= fromTo;
            ByColorBB[(int)Types.ColorOf(pc)] ^= fromTo;
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
        unsafe public void DoMove(SMove m, StateInfo* newSt, bool givesCheck, DirtyPiece* dp, DirtyThreats* dts) 
        {
            SKey k = previous->Key ^ Zobrist.Side;
            Unsafe.CopyBlock(
                &newSt,
                previous,
                (uint)(int)Marshal.OffsetOf<StateInfo>(nameof(StateInfo.Key))
            );
            newSt->Previous = previous;
            previous = newSt;
            ++gamePly;
            ++previous->Rule50;
            ++previous->PliesFromNull;
            EColor us = SideToMove;
            EColor them = us == EColor.White ? EColor.Black : EColor.White;
            ESquare from = m.FromSq();
            ESquare to = m.ToSq();
            EPiece pc = PieceOn(from);
            EPiece captured = m.TypeOf() == EMoveType.EnPassant ? Types.MakePiece(them, EPieceType.Pawn) : PieceOn(to);
            dp->Pc = pc;
            dp->From = from;
            dp->To = to;
            dp->AddSq = ESquare.SquareNone;
            dts->Us = us;
            dts->PrevKsq = GetSquare<SKing>(us);
            dts->ThreatenedSqs = dts->ThreateningSqs = 0;
            if (m.TypeOf() == EMoveType.Castling)
            {
                ESquare rfrom = ESquare.SquareNone, rto = ESquare.SquareNone;
                DoCastling<STrue>(us, from, ref to, ref rfrom, ref rto, dts, dp);
                k ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                previous->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)captured][(int)rfrom] ^ Zobrist.Psq[(int)captured][(int)rto];
                captured = EPiece.NoPiece;
            }
            else if (captured != 0)
            {
                ESquare capsq = to;
                if (Types.TypeOf(captured) == EPieceType.Pawn)
                {
                    if (m.TypeOf() == EMoveType.EnPassant)
                    {
                        capsq -= (int)Types.PawnPush(us);
                        RemovePiece((ESquare)capsq, dts);
                    }
                    previous->PawnKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                }
                else
                {
                    previous->NonPawnMaterial[(int)them] -= Types.PieceValue[(int)captured];
                    previous->NonPawnKey[(int)them] ^= Zobrist.Psq[(int)captured][(int)capsq];
                    if (Types.TypeOf(captured) <= EPieceType.Bishop)
                    {
                        previous->MinorPieceKey ^= Zobrist.Psq[(int)captured][(int)capsq];
                    }
                }
                dp->RemovePc = captured;
                dp->RemoveSq = capsq;
                k ^= Zobrist.Psq[(int)captured][(int)capsq];
                previous->MaterialKey ^= Zobrist.Psq[(int)captured][8 + PieceCount[(int)captured] - ((m.TypeOf() != EMoveType.EnPassant) ? 1 : 0)];
                previous->Rule50 = 0;
            }
            else
            {
                dp->RemoveSq = ESquare.SquareNone;
            }
            k ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
            if (previous->EpSquare != ESquare.SquareNone)
            {
                k ^= Zobrist.EnPassant[(int)Types.FileOf(previous->EpSquare)];
                previous->EpSquare = ESquare.SquareNone;
            }
            k ^= Zobrist.Castling[previous->CastlingRights];
            previous->CastlingRights &= ~(CastlingRightsMask[(int)from] | CastlingRightsMask[(int)to]);
            k ^= Zobrist.Castling[previous->CastlingRights];
            if (m.TypeOf() != EMoveType.Castling)
            {
                if (captured != 0 && m.TypeOf() != EMoveType.EnPassant)
                {
                    RemovePiece(from, dts);
                    SwapPiece(to, pc, dts);
                }
                else
                {
                    MovePiece(from, to, dts);
                }
            }
            if (Types.TypeOf(pc) == EPieceType.Pawn)
            {
                if (((int)to ^ (int)from) == 16)
                {
                    ESquare epSquare = to - (int)Types.PawnPush(us);
                    SBitBoard pawns = BitBoard.AttacksBB<SPawn>(epSquare, us) & GetPieces<SPawn>(them);
                    if (pawns != 0)
                    {
                        ESquare ksq = GetSquare<SKing>(them);
                        SBitBoard notBlockers = ~previous->BlockersForKing[(int)them];
                        bool noDiscovery = (notBlockers % from) || Types.FileOf(from) == Types.FileOf(ksq);
                        if (noDiscovery && (pawns & (notBlockers | BitBoard.LineBB(epSquare, ksq))) != 0)
                        {
                            previous->EpSquare = epSquare;
                            k ^= Zobrist.EnPassant[(int)Types.FileOf(epSquare)];
                        }
                    }
                }
                else if (m.TypeOf() == EMoveType.Promotion)
                {
                    EPiece promotion = Types.MakePiece(us, m.PromotionType());
                    EPieceType promotionType = Types.TypeOf(promotion);
                    SwapPiece(to, promotion, dts);
                    dp->AddPc = promotion;
                    dp->AddSq = to;
                    dp->To = ESquare.SquareNone;
                    k ^= Zobrist.Psq[(int)promotion][(int)to];
                    previous->MaterialKey ^= Zobrist.Psq[(int)promotion][8 + PieceCount[(int)promotion] - 1] ^ Zobrist.Psq[(int)pc][8 + PieceCount[(int)pc]];
                    previous->NonPawnKey[(int)us] ^= Zobrist.Psq[(int)promotion][(int)to];
                    if (promotionType <= EPieceType.Bishop)
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
                if (Types.TypeOf(pc) <= EPieceType.Bishop)
                {
                    previous->MinorPieceKey ^= Zobrist.Psq[(int)pc][(int)from] ^ Zobrist.Psq[(int)pc][(int)to];
                }
            }
            previous->Key = k;
            previous->CapturedPiece = captured;
            previous->CheckersBB = givesCheck ? AttackersTo(GetSquare<SKing>(them)) & GetPieces(us) : 0;
            SideToMove = SideToMove == EColor.White ? EColor.Black : EColor.White;
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
            dts->Ksq = GetSquare<SKing>(us);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public void DoCastling<D>(EColor us, ESquare from, ref ESquare to, ref ESquare rfrom, ref ESquare rto, DirtyThreats* dts, DirtyPiece* dp) where D : struct, IBool
        {
            if (dp != null)
            {
                bool kingSide = to > from;
                rfrom = to; 
                rto = Types.RelativeSquare(us, kingSide? ESquare.SQ_F1 : ESquare.SQ_D1);
                to = Types.RelativeSquare(us, kingSide? ESquare.SQ_G1 : ESquare.SQ_C1);
                if (D.Value)
                {
                    dp->To = to;
                    dp->RemovePc = dp->AddPc = Types.MakePiece(us, EPieceType.Rook);
                    dp->RemoveSq = rfrom;
                    dp->AddSq = rto;
                }
                RemovePiece(D.Value ? from : to, dts);
                RemovePiece(D.Value ? rfrom : rto, dts);
                PutPiece(Types.MakePiece(us, EPieceType.King), D.Value ? to : from, dts);
                PutPiece(Types.MakePiece(us, EPieceType.Rook), D.Value ? rto : rfrom, dts);
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
            SBitBoard rAttacks = BitBoard.AttacksBB<SRook>(s, occupied);
            SBitBoard bAttacks = BitBoard.AttacksBB<SBishop>(s, occupied);
            SBitBoard kings = GetPieces<SKing>();
            SBitBoard occupiedNoK = occupied ^ kings;
            SBitBoard sliders = (rookQueens & rAttacks) | (bishopQueens & bAttacks);
            void ProcessSliders(bool addDirectAttacks)
            {
                while (sliders != 0)
                {
                    ESquare sliderSq = BitBoard.PopLsb(ref sliders);
                    EPiece slider = PieceOn(sliderSq);
                    SBitBoard ray = BitBoard.RayPassBB[(int)sliderSq][(int)s];
                    SBitBoard discovered = ray & (rAttacks | bAttacks) & occupiedNoK;
                    if (discovered != 0 && (BitBoard.RayPassBB[(int)sliderSq][(int)s] & noRaysContaining) != noRaysContaining)
                    {
                        ESquare threatenedSq = BitBoard.Lsb(discovered);
                        EPiece threatenedPc = PieceOn(threatenedSq);
                        AddDirtyThreat<SUnBool<P>>(dts, slider, threatenedPc, sliderSq, threatenedSq);
                    }
                    if (addDirectAttacks)
                    {
                        AddDirtyThreat<P>(dts, slider, pc, sliderSq, s);
                    }
                }
            }
            if (Types.TypeOf(pc) == EPieceType.King)
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
            SBitBoard threatened = BitBoard.AttacksBB(pc, s, occupied) & occupiedNoK;
            SBitBoard incomingThreats = (BitBoard.pseudoAttacks[(int)EPieceType.Knight][(int)s] & knights) 
                                      | (BitBoard.AttacksBB<SPawn>(s, EColor.White) & blackPawns) 
                                      | (BitBoard.AttacksBB<SPawn>(s, EColor.Black) & whitePawns) 
                                      | (BitBoard.pseudoAttacks[(int)EPieceType.King][(int)s] & kings);
            while (threatened != 0)
            {
                ESquare threatenedSq = BitBoard.PopLsb(ref threatened);
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
                ESquare srcSq = BitBoard.PopLsb(ref incomingThreats);
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
            return BitBoard.Lsb(GetPieces<T>(c));
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
            return previous->BlockersForKing[(int)c];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(ECastlingRights cr)
        {
            return (previous->CastlingRights & (int)cr) != 0;
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
            return previous->CheckersBB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare EpSquare()
        {
            return previous->EpSquare;
        }
    }
}

