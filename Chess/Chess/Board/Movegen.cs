using System.Runtime.CompilerServices;
using static Chess.MoveType;
using static Chess.GenType;
using static Chess.Square;
using static Chess.Color;
using static Chess.Bitboards;
using static Chess.FuncBit;
namespace Chess
{
    public static class MoveGen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe Move* Splat_Pawn_Moves<O>(Move* moveList, Bitboard toBB) where O : struct, IDirection
        {
            while (toBB != 0)
            {
                Square to = Pop_Lsb(ref toBB);
                Square from = to - (int)O.Offset;
                *moveList++ = new Move(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Splat_Moves(Move* moveList, Square from, Bitboard toBB)
        {
            while (toBB != 0)
            {
                *moveList++ = new Move(from, Pop_Lsb(ref toBB));
            }    
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Make_Promotions<T, O, B>(Move* moveList, Square to) where T : struct, IGenType where O : struct, IDirection where B : struct, IBool
        {
            Square from = to - (int)O.Offset;
            bool all = T.Type == EVASION || T.Type == NON_EVASION;
            if (T.Type == CAPTURE || all)
            {
                *moveList++ = Move.Make_Move<Promotion, Queen>(from, to);
            }
            if ((T.Type == CAPTURE && B.Value) || (T.Type == QUIET && B.Value) || all)
            {
                *moveList++ = Move.Make_Move<Promotion, Rook>(from, to);
                *moveList++ = Move.Make_Move<Promotion, Bishop>(from, to);
                *moveList++ = Move.Make_Move<Promotion, Knight>(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Generate_Pawn_Moves<G, C, N>(ref Position pos, Move* moveList, Bitboard target) where G : struct, IGenType where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Bitboard emptySquares = ~pos.Get_Pieces();
            Bitboard enemies = G.Enemies<C, N>(ref pos);
            Bitboard Pawn_sOn7 = pos.Get_Pieces<Pawn>(C.Value) & C.Rank7;
            Bitboard Pawn_sNotOn7 = pos.Get_Pieces<Pawn>(C.Value) & ~C.Rank7;
            if (G.Type != CAPTURE)
            {
                Bitboard b1 = Shift<Pawn_Up<C, N>>(Pawn_sNotOn7) & emptySquares;
                Bitboard b2 = Shift<Pawn_Up<C, N>>(b1 & C.Rank3) & emptySquares;
                if (G.Type == EVASION)
                {
                    b1 &= target;
                    b2 &= target;
                }
                moveList = Splat_Pawn_Moves<Pawn_Up<C, N>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Double_Up<C, N>>(moveList, b2);
            }
            if (Pawn_sOn7 != 0)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<C, N>>(Pawn_sOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<C, N>>(Pawn_sOn7) & enemies;
                Bitboard b3 = Shift<Pawn_Up<C, N>>(Pawn_sOn7) & emptySquares;
                if (G.Type == EVASION)
                {
                    b3 &= target;
                }
                while (b1 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up_Right<C, N>, True>(moveList, Pop_Lsb(ref b1));
                }    
                while (b2 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up_Left<C, N>, True>(moveList, Pop_Lsb(ref b2));
                }
                while (b3 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up<C, N>, False>(moveList, Pop_Lsb(ref b3));
                }
            }
            if (G.Type == CAPTURE || G.Type == EVASION || G.Type == NON_EVASION)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<C, N>>(Pawn_sNotOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<C, N>>(Pawn_sNotOn7) & enemies;
                moveList = Splat_Pawn_Moves<Pawn_Up_Right<C, N>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Up_Left<C, N>>(moveList, b2);
                if (pos.Ep_Square() != SQ_NONE)
                {
                    if (G.Type == EVASION && (target & (pos.Ep_Square() + (int)C.Up)) != 0)
                    {
                        return moveList;
                    }
                    b1 = Pawn_sNotOn7 & Attacks_BB<Pawn>(pos.Ep_Square(), N.Value);
                    while (b1 != 0)
                    {
                        *moveList++ = Move.Make_Move<EnPassant>(Pop_Lsb(ref b1), pos.Ep_Square());
                    }
                }
            }
            return moveList;   
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Generate_Moves<P, C, N>(ref Position pos, Move* moveList, Bitboard target) where C : struct, IColor<C, N> where N : struct, IColor<N, C> where P : struct, IPieceType, IPieceTypes
        {
            Bitboard bb = pos.Get_Pieces<P>(C.Value);
            while (bb != 0)
            {
                Square from = Pop_Lsb(ref bb);
                Bitboard b = Attacks_BB<P>(from, pos.Get_Pieces()) & target;
                moveList = Splat_Moves(moveList, from, b); 
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Generate_All<G, C, N>(ref Position pos, Move* moveList) where C : struct, IColor<C, N> where N : struct, IColor<N, C> where G : struct, IGenType
        {
            Square ksq = pos.Get_Square<King>(C.Value);
            Bitboard target = 0;
            if (G.Type != EVASION || !More_Than_One(pos.Checkers()))
            {
                target = G.Type == EVASION    ? Between_BB(ksq, Lsb(pos.Checkers()))
                       : G.Type == NON_EVASION ? ~pos.Get_Pieces(C.Value)
                       : G.Type == CAPTURE    ? pos.Get_Pieces(N.Value)
                                                        : ~pos.Get_Pieces();
                moveList = Generate_Pawn_Moves<G, C, N>(ref pos, moveList, target);
                moveList = Generate_Moves<Knight, C, N>(ref pos, moveList, target);
                moveList = Generate_Moves<Bishop, C, N>(ref pos, moveList, target);
                moveList = Generate_Moves<Rook, C, N>(ref pos, moveList, target);
                moveList = Generate_Moves<Queen, C, N>(ref pos, moveList, target);
            }
            Bitboard b = Attacks_BB<King>(ksq) & (G.Type == EVASION ? ~pos.Get_Pieces(C.Value) : target);
            moveList = Splat_Moves(moveList, ksq, b);
            if ((G.Type == QUIET || G.Type == NON_EVASION) && pos.Can_Castle(C.CastlingRights))
            {
                fixed (CastlingRights* crStart = &C.AllCastlingRights[0])
                {
                    for (CastlingRights* crPtr = crStart, crEnd = crStart + CastlingRightsArray2.Length; crPtr != crEnd; ++crPtr)
                    {
                        CastlingRights cr = *crPtr;
                        if (!pos.Castling_Impeded(cr) && pos.Can_Castle(cr))
                        {
                            *moveList++ = Move.Make_Move<Castling>(ksq, pos.Castling_Rook_Square(cr));
                        }
                    }
                }
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe Move* Generate<G, C, N>(ref Position pos, Move* moveList) where G : struct, IGenType where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return Generate_All<G, C, N>(ref pos, moveList);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe Move* Generate_Legal<C, N>(ref Position pos, Move* moveList) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Bitboard pinned = pos.Blockers_For_King(C.Value) & pos.Get_Pieces(C.Value);
            Square ksq = pos.Get_Square<King>(C.Value);
            Move* cur = moveList;
            moveList = pos.Checkers() != 0 ? Generate<Evasions, C, N>(ref pos, moveList) : Generate<NON_EVASIONs, C, N>(ref pos, moveList);
            while (cur != moveList)
            {
                if (((pinned & cur->From_Sq()) != 0 || cur->From_Sq() == ksq || cur->Type_Of() == EN_PASSANT) && !pos.Legal<C, N>(*cur))
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
