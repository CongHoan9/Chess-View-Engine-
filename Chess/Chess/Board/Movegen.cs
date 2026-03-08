using System.Runtime.CompilerServices;
namespace Chess
{
    public static class MoveGen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* SplatPawnMoves<O>(Move* moveList, BitBoard toBB) where O : struct, IPawnOffset
        {
            while (toBB != 0)
            {
                Square to = BitBoards.PopLsb(ref toBB);
                Square from = to - (int)O.Value;
                *moveList++ = new Move(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* SplatMoves(Move* moveList, Square from, BitBoard toBB)
        {
            while (toBB != 0)
            {
                *moveList++ = new Move(from, BitBoards.PopLsb(ref toBB));
            }    
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* MakePromotions<T, O>(Move* moveList, Square to) where T : struct, IMoveGen where O : struct, IPawnOffset
        {
            Square from = to - (int)O.Value;
            bool all = T.Type == GenType.Evasions || T.Type == GenType.NonEvasions;
            if (T.Type == GenType.Captures || all)
            {
                *moveList++ = Move.Make<Promotion, Queen>(from, to);
            }
            if ((T.Type == GenType.Captures) || (T.Type == GenType.Quiets) || all)
            {
                *moveList++ = Move.Make<Promotion, Rook>(from, to);
                *moveList++ = Move.Make<Promotion, Bishop>(from, to);
                *moveList++ = Move.Make<Promotion, Knight>(from, to);
            }
            return moveList;
        }
        // generate_pawn_moves equivalent
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* GeneratePawnMoves<C, G>(Position pos, Move* moveList, BitBoard target) where C : struct, IColor where G : struct, IMoveGen
        {
            BitBoard emptySquares = ~pos.GetPieces();
            BitBoard enemies = (G.Type == GenType.Evasions) ? pos.Checkers() : pos.GetPieces(C.Them);
            BitBoard pawnsOn7 = pos.GetPieces<Pawn>(C.Us) & C.Rank7BB;
            BitBoard pawnsNotOn7 = pos.GetPieces<Pawn>(C.Us) & ~C.Rank7BB;
            if (G.Type != GenType.Captures)
            {
                BitBoard b1 = BitBoards.Shift<PawnUp<C>>(pawnsNotOn7) & emptySquares;
                BitBoard b2 = BitBoards.Shift<PawnDoubleUp<C>>(b1 & C.Rank3BB) & emptySquares;
                if (G.Type == GenType.Evasions)
                {
                    b1 &= target;
                    b2 &= target;
                }
                moveList = SplatPawnMoves<PawnUp<C>>(moveList, b1);
                moveList = SplatPawnMoves<PawnDoubleUp<C>>(moveList, b2);
            }
            if (pawnsOn7 != 0)
            {
                BitBoard b1 = BitBoards.Shift<PawnUpRight<C>>(pawnsOn7) & enemies;
                BitBoard b2 = BitBoards.Shift<PawnUpLeft<C>>(pawnsOn7) & enemies;
                BitBoard b3 = BitBoards.Shift<PawnUp<C>>(pawnsOn7) & emptySquares;
                if (G.Type == GenType.Evasions)
                {
                    b3 &= target;
                }
                while (b1 != 0)
                {
                    moveList = MakePromotions<G, PawnUpRight<C>>(moveList, BitBoards.PopLsb(ref b1));
                }    
                while (b2 != 0)
                {
                    moveList = MakePromotions<G, PawnUpLeft<C>>(moveList, BitBoards.PopLsb(ref b2));
                }
                while (b3 != 0)
                {
                    moveList = MakePromotions<G, PawnUp<C>>(moveList, BitBoards.PopLsb(ref b3));
                }
            }
            if (G.Type == GenType.Captures || G.Type == GenType.Evasions || G.Type == GenType.NonEvasions)
            {
                BitBoard b1 = BitBoards.Shift<PawnUpRight<C>>(pawnsNotOn7) & enemies;
                BitBoard b2 = BitBoards.Shift<PawnUpLeft<C>>(pawnsNotOn7) & enemies;
                moveList = SplatPawnMoves<PawnUpRight<C>>(moveList, b1);
                moveList = SplatPawnMoves<PawnUpLeft<C>>(moveList, b2);
                if (pos.EpSquare() != Square.SquareNone)
                {
                    if (G.Type == GenType.Evasions && (target & (pos.EpSquare() + (int)C.Up)) != 0)
                    {
                        return moveList;
                    }
                    b1 = pawnsNotOn7 & BitBoards.AttacksBB<Pawn>(pos.EpSquare(), C.Them);
                    while (b1 != 0)
                    {
                        *moveList++ = Move.Make<EnPassant>(BitBoards.PopLsb(ref b1), pos.EpSquare());
                    }
                }
            }
            return moveList;   
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* GenerateMoves<C, P>(Position pos, Move* moveList, BitBoard target) where C : struct, IColor where P : struct, IPieceType, IPieceTypes
        {
            BitBoard bb = pos.GetPieces<P>(C.Us);
            while (bb != 0)
            {
                Square from = BitBoards.PopLsb(ref bb);
                BitBoard b = BitBoards.AttacksBB<P>(from, pos.GetPieces()) & target;
                moveList = SplatMoves(moveList, from, b); // Null move to indicate checking piece
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* GenerateAll<C, G>(Position pos, Move* moveList) where C : struct, IColor where G : struct, IMoveGen
        {
            Square ksq = pos.GetSquare<King>(C.Us);
            BitBoard target = 0;
            if (G.Type != GenType.Evasions || !BitBoards.MoreThanOne(pos.Checkers()))
            {
                target = G.Type == GenType.Evasions ? BitBoards.BetweenBB(ksq, BitBoards.Lsb(pos.Checkers()))
                    : G.Type == GenType.NonEvasions ? ~pos.GetPieces(C.Us)
                    : G.Type == GenType.Captures    ? pos.GetPieces(C.Them)
                                                    : ~pos.GetPieces();
                moveList = GeneratePawnMoves<C, G>(pos, moveList, target);
                moveList = GenerateMoves<C, Knight>(pos, moveList, target);
                moveList = GenerateMoves<C, Bishop>(pos, moveList, target);
                moveList = GenerateMoves<C, Rook>(pos, moveList, target);
                moveList = GenerateMoves<C, Queen>(pos, moveList, target);
            }
            BitBoard b = BitBoards.AttacksBB<King>(ksq) & (G.Type == GenType.Evasions ? ~pos.GetPieces(C.Us) : target);
            moveList = SplatMoves(moveList, ksq, b);
            if ((G.Type == GenType.Quiets || G.Type == GenType.NonEvasions) && pos.CanCastle(C.CastlingRights))
            {
                foreach (CastlingRights cr in new[] { C.KingSide, C.QueenSide })
                {
                    if (!pos.CastlingImpeded(cr) && pos.CanCastle(cr))
                    {
                        *moveList++ = Move.Make<Castling>(ksq, pos.CastlingRookSquare(cr));
                    }    
                }
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* Generate<G>(Position pos, Move* moveList) where G : struct, IMoveGen
        {
            return pos.SideToMove == Color.White ? GenerateAll<White, G>(pos, moveList) : GenerateAll<Black, G>(pos, moveList);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* GenerateLegal(Position pos, Move* moveList)
        {
            Color us = pos.SideToMove;
            BitBoard pinned = pos.BlockersForKing(us) & pos.GetPieces(us);
            Square ksq = pos.GetSquare<King>(us);
            Move* cur = moveList;
            moveList = pos.Checkers() != 0 ? Generate<Evasions>(pos, moveList) : Generate<NonEvasions>(pos, moveList);
            while (cur != moveList)
            {
                if (((pinned & cur->FromSq()) != 0 || cur->FromSq() == ksq || cur->TypeOf() == MoveType.EnPassant) && !pos.Legal(*cur))
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
