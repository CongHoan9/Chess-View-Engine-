using System.Runtime.CompilerServices;
namespace Chess
{
    public static class MoveGen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static SMove* SplatPawnMoves<O>(SMove* moveList, SBitBoard toBB) where O : struct, IPawnOffset
        {
            while (toBB != 0)
            {
                ESquare to = BitBoard.PopLsb(ref toBB);
                ESquare from = to - (int)O.Offset;
                *moveList++ = new SMove(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static SMove* SplatMoves(SMove* moveList, ESquare from, SBitBoard toBB)
        {
            while (toBB != 0)
            {
                *moveList++ = new SMove(from, BitBoard.PopLsb(ref toBB));
            }    
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static SMove* MakePromotions<T, O, B>(SMove* moveList, ESquare to) where T : struct, IMoveGen where O : struct, IPawnOffset where B : struct, IBool
        {
            ESquare from = to - (int)O.Offset;
            bool all = T.Type == EGenType.Evasions || T.Type == EGenType.NonEvasions;
            if (T.Type == EGenType.Captures || all)
            {
                *moveList++ = SMove.Make<Promotion, SQueen>(from, to);
            }
            if ((T.Type == EGenType.Captures && B.Value) || (T.Type == EGenType.Quiets && B.Value) || all)
            {
                *moveList++ = SMove.Make<Promotion, SRook>(from, to);
                *moveList++ = SMove.Make<Promotion, SBishop>(from, to);
                *moveList++ = SMove.Make<Promotion, SKnight>(from, to);
            }
            return moveList;
        }
        // generate_pawn_moves equivalent
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static SMove* GeneratePawnMoves<C, G>(Position pos, SMove* moveList, SBitBoard target) where C : struct, IColor where G : struct, IMoveGen
        {
            SBitBoard emptySquares = ~pos.GetPieces();
            SBitBoard enemies = (G.Type == EGenType.Evasions) ? pos.Checkers() : pos.GetPieces(C.Them);
            SBitBoard pawnsOn7 = pos.GetPieces<SPawn>(C.Us) & C.Rank7BB;
            SBitBoard pawnsNotOn7 = pos.GetPieces<SPawn>(C.Us) & ~C.Rank7BB;
            if (G.Type != EGenType.Captures)
            {
                SBitBoard b1 = BitBoard.Shift<SPawnUp<C>>(pawnsNotOn7) & emptySquares;
                SBitBoard b2 = BitBoard.Shift<SPawnDoubleUp<C>>(b1 & C.Rank3BB) & emptySquares;
                if (G.Type == EGenType.Evasions)
                {
                    b1 &= target;
                    b2 &= target;
                }
                moveList = SplatPawnMoves<SPawnUp<C>>(moveList, b1);
                moveList = SplatPawnMoves<SPawnDoubleUp<C>>(moveList, b2);
            }
            if (pawnsOn7 != 0)
            {
                SBitBoard b1 = BitBoard.Shift<SPawnUpRight<C>>(pawnsOn7) & enemies;
                SBitBoard b2 = BitBoard.Shift<SPawnUpLeft<C>>(pawnsOn7) & enemies;
                SBitBoard b3 = BitBoard.Shift<SPawnUp<C>>(pawnsOn7) & emptySquares;
                if (G.Type == EGenType.Evasions)
                {
                    b3 &= target;
                }
                while (b1 != 0)
                {
                    moveList = MakePromotions<G, SPawnUpRight<C>, STrue>(moveList, BitBoard.PopLsb(ref b1));
                }    
                while (b2 != 0)
                {
                    moveList = MakePromotions<G, SPawnUpLeft<C>, STrue>(moveList, BitBoard.PopLsb(ref b2));
                }
                while (b3 != 0)
                {
                    moveList = MakePromotions<G, SPawnUp<C>, SFalse>(moveList, BitBoard.PopLsb(ref b3));
                }
            }
            if (G.Type == EGenType.Captures || G.Type == EGenType.Evasions || G.Type == EGenType.NonEvasions)
            {
                SBitBoard b1 = BitBoard.Shift<SPawnUpRight<C>>(pawnsNotOn7) & enemies;
                SBitBoard b2 = BitBoard.Shift<SPawnUpLeft<C>>(pawnsNotOn7) & enemies;
                moveList = SplatPawnMoves<SPawnUpRight<C>>(moveList, b1);
                moveList = SplatPawnMoves<SPawnUpLeft<C>>(moveList, b2);
                if (pos.EpSquare() != ESquare.SquareNone)
                {
                    if (G.Type == EGenType.Evasions && (target & (pos.EpSquare() + (int)C.Up)) != 0)
                    {
                        return moveList;
                    }
                    b1 = pawnsNotOn7 & BitBoard.AttacksBB<SPawn>(pos.EpSquare(), C.Them);
                    while (b1 != 0)
                    {
                        *moveList++ = SMove.Make<EnPassant>(BitBoard.PopLsb(ref b1), pos.EpSquare());
                    }
                }
            }
            return moveList;   
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static SMove* GenerateMoves<C, P>(Position pos, SMove* moveList, SBitBoard target) where C : struct, IColor where P : struct, IPieceType, IPieceTypes
        {
            SBitBoard bb = pos.GetPieces<P>(C.Us);
            while (bb != 0)
            {
                ESquare from = BitBoard.PopLsb(ref bb);
                SBitBoard b = BitBoard.AttacksBB<P>(from, pos.GetPieces()) & target;
                moveList = SplatMoves(moveList, from, b); 
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static SMove* GenerateAll<C, G>(Position pos, SMove* moveList) where C : struct, IColor where G : struct, IMoveGen
        {
            ESquare ksq = pos.GetSquare<SKing>(C.Us);
            SBitBoard target = 0;
            if (G.Type != EGenType.Evasions || !BitBoard.MoreThanOne(pos.Checkers()))
            {
                target = G.Type == EGenType.Evasions ? BitBoard.BetweenBB(ksq, BitBoard.Lsb(pos.Checkers()))
                    : G.Type == EGenType.NonEvasions ? ~pos.GetPieces(C.Us)
                    : G.Type == EGenType.Captures    ? pos.GetPieces(C.Them)
                                                    : ~pos.GetPieces();
                moveList = GeneratePawnMoves<C, G>(pos, moveList, target);
                moveList = GenerateMoves<C, SKnight>(pos, moveList, target);
                moveList = GenerateMoves<C, SBishop>(pos, moveList, target);
                moveList = GenerateMoves<C, SRook>(pos, moveList, target);
                moveList = GenerateMoves<C, SQueen>(pos, moveList, target);
            }
            SBitBoard b = BitBoard.AttacksBB<SKing>(ksq) & (G.Type == EGenType.Evasions ? ~pos.GetPieces(C.Us) : target);
            moveList = SplatMoves(moveList, ksq, b);
            if ((G.Type == EGenType.Quiets || G.Type == EGenType.NonEvasions) && pos.CanCastle(C.CastlingRights))
            {
                foreach (ECastlingRights cr in C.AllCastlingRights)
                {
                    if (!pos.CastlingImpeded(cr) && pos.CanCastle(cr))
                    {
                        *moveList++ = SMove.Make<Castling>(ksq, pos.CastlingRookSquare(cr));
                    }    
                }
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static SMove* Generate<G>(Position pos, SMove* moveList) where G : struct, IMoveGen
        {
            return pos.SideToMove == EColor.White ? GenerateAll<SWhite, G>(pos, moveList) : GenerateAll<SBlack, G>(pos, moveList);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static SMove* GenerateLegal(Position pos, SMove* moveList)
        {
            EColor us = pos.SideToMove;
            SBitBoard pinned = pos.BlockersForKing(us) & pos.GetPieces(us);
            ESquare ksq = pos.GetSquare<SKing>(us);
            SMove* cur = moveList;
            moveList = pos.Checkers() != 0 ? Generate<Evasions>(pos, moveList) : Generate<NonEvasions>(pos, moveList);
            while (cur != moveList)
            {
                if (((pinned & cur->FromSq()) != 0 || cur->FromSq() == ksq || cur->TypeOf() == EMoveType.EnPassant) && !pos.Legal(*cur))
                {
                    *cur = *(--moveList);
                }
                else
                {
                    ++cur;
                }
            }    
            return moveList;
        }
    }
}
