using System.Runtime.CompilerServices;
using static Chess.Bitboards;
using static Chess.FuncBit;
using static Chess.Types;
namespace Chess
{
    public static class MoveGen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* Splat_Pawn_Moves<O>(Move* moveList, Bitboard toBB) where O : struct, IDirection
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
        unsafe private static Move* Splat_Moves(Move* moveList, Square from, Bitboard toBB)
        {
            while (toBB != 0)
            {
                *moveList++ = new Move(from, Pop_Lsb(ref toBB));
            }    
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* Make_Promotions<T, O, B>(Move* moveList, Square to) where T : struct, IGenType where O : struct, IDirection where B : struct, IBool
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
        unsafe private static Move* Generate_Pawn_Moves<C, G>(Position pos, Move* moveList, Bitboard target) where C : struct, IColor where G : struct, IGenType
        {
            Bitboard emptySquares = ~pos.Get_Pieces();
            Bitboard enemies = (G.Type == EVASION) ? pos.Checkers() : pos.Get_Pieces_Of_Color(C.Them);
            Bitboard Pawn_sOn7 = pos.Get_Pieces_Of_Color<Pawn>(C.Us) & C.Rank7;
            Bitboard Pawn_sNotOn7 = pos.Get_Pieces_Of_Color<Pawn>(C.Us) & ~C.Rank7;
            if (G.Type != CAPTURE)
            {
                Bitboard b1 = Shift<Pawn_Up<C>>(Pawn_sNotOn7) & emptySquares;
                Bitboard b2 = Shift<Pawn_Up<C>>(b1 & C.Rank3) & emptySquares;
                if (G.Type == EVASION)
                {
                    b1 &= target;
                    b2 &= target;
                }
                moveList = Splat_Pawn_Moves<Pawn_Up<C>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Double_Up<C>>(moveList, b2);
            }
            if (Pawn_sOn7 != 0)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<C>>(Pawn_sOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<C>>(Pawn_sOn7) & enemies;
                Bitboard b3 = Shift<Pawn_Up<C>>(Pawn_sOn7) & emptySquares;
                if (G.Type == EVASION)
                {
                    b3 &= target;
                }
                while (b1 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up_Right<C>, True>(moveList, Pop_Lsb(ref b1));
                }    
                while (b2 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up_Left<C>, True>(moveList, Pop_Lsb(ref b2));
                }
                while (b3 != 0)
                {
                    moveList = Make_Promotions<G, Pawn_Up<C>, False>(moveList, Pop_Lsb(ref b3));
                }
            }
            if (G.Type == CAPTURE || G.Type == EVASION || G.Type == NON_EVASION)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<C>>(Pawn_sNotOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<C>>(Pawn_sNotOn7) & enemies;
                moveList = Splat_Pawn_Moves<Pawn_Up_Right<C>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Up_Left<C>>(moveList, b2);
                if (pos.Ep_Square() != SQ_NONE)
                {
                    if (G.Type == EVASION && (target & (pos.Ep_Square() + (int)C.Up)) != 0)
                    {
                        return moveList;
                    }
                    b1 = Pawn_sNotOn7 & Attacks_BB_Square<Pawn>(pos.Ep_Square(), C.Them);
                    while (b1 != 0)
                    {
                        *moveList++ = Move.Make_Move<EnPassant>(Pop_Lsb(ref b1), pos.Ep_Square());
                    }
                }
            }
            return moveList;   
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* Generate_Moves<C, P>(Position pos, Move* moveList, Bitboard target) where C : struct, IColor where P : struct, IPieceType, IPieceTypes
        {
            Bitboard bb = pos.Get_Pieces_Of_Color<P>(C.Us);
            while (bb != 0)
            {
                Square from = Pop_Lsb(ref bb);
                Bitboard b = Attacks_BB_Square<P>(from, pos.Get_Pieces()) & target;
                moveList = Splat_Moves(moveList, from, b); 
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static Move* Generate_All<C, G>(Position pos, Move* moveList) where C : struct, IColor where G : struct, IGenType
        {
            Square ksq = pos.Get_Square_Of_Color<King>(C.Us);
            Bitboard target = 0;
            if (G.Type != EVASION || !More_Than_One(pos.Checkers()))
            {
                target = G.Type == EVASION    ? Between_BB(ksq, Lsb(pos.Checkers()))
                       : G.Type == NON_EVASION ? ~pos.Get_Pieces_Of_Color(C.Us)
                       : G.Type == CAPTURE    ? pos.Get_Pieces_Of_Color(C.Them)
                                                        : ~pos.Get_Pieces();
                moveList = Generate_Pawn_Moves<C, G>(pos, moveList, target);
                moveList = Generate_Moves<C, Knight>(pos, moveList, target);
                moveList = Generate_Moves<C, Bishop>(pos, moveList, target);
                moveList = Generate_Moves<C, Rook>(pos, moveList, target);
                moveList = Generate_Moves<C, Queen>(pos, moveList, target);
            }
            Bitboard b = Attacks_BB_Square<King>(ksq) & (G.Type == EVASION ? ~pos.Get_Pieces_Of_Color(C.Us) : target);
            moveList = Splat_Moves(moveList, ksq, b);
            if ((G.Type == QUIET || G.Type == NON_EVASION) && pos.Can_Castle(C.CastlingRights))
            {
                foreach (CastlingRights cr in C.AllCastlingRights)
                {
                    if (!pos.Castling_Impeded(cr) && pos.Can_Castle(cr))
                    {
                        *moveList++ = Move.Make_Move<Castling>(ksq, pos.Castling_Rook_Square(cr));
                    }    
                }
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* Generate<G, C>(Position pos, Move* moveList) where G : struct, IGenType where C : struct, IColor
        {
            return C.Us == WHITE ? Generate_All<White, G>(pos, moveList) : Generate_All<Black, G>(pos, moveList);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static Move* Generate_Legal<C>(Position pos, Move* moveList) where C : struct, IColor
        {
            Bitboard pinned = pos.Blockers_For_King(C.Us) & pos.Get_Pieces_Of_Color(C.Us);
            Square ksq = pos.Get_Square_Of_Color<King>(C.Us);
            Move* cur = moveList;
            moveList = pos.Checkers() != 0 ? Generate<Evasions, C>(pos, moveList) : Generate<NON_EVASIONs, C>(pos, moveList);
            while (cur != moveList)
            {
                if (((pinned & cur->From_Sq()) != 0 || cur->From_Sq() == ksq || cur->Type_Of() == EN_PASSANT) && !pos.Legal<C>(*cur))
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
