using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using static Chess.Bitboards;
using static Chess.FuncBit;
using static Chess.GenType;
using static Chess.MoveType;
using static Chess.Square;

namespace Chess
{
    public static class MoveGen
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Move* Splat_Pawn_Moves<Direction>(Move* moveList, Bitboard toBB) where Direction : struct, IDirection
        {
            while (toBB != 0)
            {
                Square to = Pop_Lsb(ref toBB);
                Square from = to - (int)Direction.Offset;
                *moveList++ = new Move(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Move* Splat_Moves(Move* moveList, Square from, Bitboard toBB)
        {
            while (toBB != 0)
            {
                *moveList++ = new Move(from, Pop_Lsb(ref toBB));
            }    
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Move* Make_Promotions<GenType, Direction, Bool>(Move* moveList, Square to) where GenType : struct, IGenType where Direction : struct, IDirection where Bool : struct, IBool
        {
            Square from = to - (int)Direction.Offset;
            bool all = GenType.Type == EVASION || GenType.Type == NON_EVASION;
            if (GenType.Type == CAPTURE || all)
            {
                *moveList++ = Move.Make_Move<Promotion, Queen>(from, to);
            }
            if ((GenType.Type == CAPTURE && Bool.Value) || (GenType.Type == QUIET && Bool.Value) || all)
            {
                *moveList++ = Move.Make_Move<Promotion, Rook>(from, to);
                *moveList++ = Move.Make_Move<Promotion, Bishop>(from, to);
                *moveList++ = Move.Make_Move<Promotion, Knight>(from, to);
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Generate_Pawn_Moves<GenType, Us, Next>(ref Position pos, Move* moveList, Bitboard target) where GenType : struct, IGenType where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            Bitboard emptySquares = ~pos.Get_Pieces();
            Bitboard enemies = GenType.Enemies<Us, Next>(ref pos);
            Bitboard Pawn_sOn7 = pos.Get_Pieces<Pawn>(Us.Value) & Us.Rank7;
            Bitboard Pawn_sNotOn7 = Bmi1.X64.IsSupported ? (Bitboard)Bmi1.X64.AndNot(Us.Rank7.Raw, pos.Get_Pieces<Pawn>(Us.Value).Raw) : pos.Get_Pieces<Pawn>(Us.Value) & ~Us.Rank7;
            if (GenType.Type != CAPTURE)
            {
                Bitboard b1 = Shift<Pawn_Up<Us, Next>>(Pawn_sNotOn7) & emptySquares;
                Bitboard b2 = Shift<Pawn_Up<Us, Next>>(b1 & Us.Rank3) & emptySquares;
                if (GenType.Type == EVASION)
                {
                    b1 &= target;
                    b2 &= target;
                }
                moveList = Splat_Pawn_Moves<Pawn_Up<Us, Next>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Double_Up<Us, Next>>(moveList, b2);
            }
            if (Pawn_sOn7 != 0)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<Us, Next>>(Pawn_sOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<Us, Next>>(Pawn_sOn7) & enemies;
                Bitboard b3 = Shift<Pawn_Up<Us, Next>>(Pawn_sOn7) & emptySquares;
                if (GenType.Type == EVASION)
                {
                    b3 &= target;
                }
                while (b1 != 0)
                {
                    moveList = Make_Promotions<GenType, Pawn_Up_Right<Us, Next>, True>(moveList, Pop_Lsb(ref b1));
                }    
                while (b2 != 0)
                {
                    moveList = Make_Promotions<GenType, Pawn_Up_Left<Us, Next>, True>(moveList, Pop_Lsb(ref b2));
                }
                while (b3 != 0)
                {
                    moveList = Make_Promotions<GenType, Pawn_Up<Us, Next>, False>(moveList, Pop_Lsb(ref b3));
                }
            }
            if (GenType.Type == CAPTURE || GenType.Type == EVASION || GenType.Type == NON_EVASION)
            {
                Bitboard b1 = Shift<Pawn_Up_Right<Us, Next>>(Pawn_sNotOn7) & enemies;
                Bitboard b2 = Shift<Pawn_Up_Left<Us, Next>>(Pawn_sNotOn7) & enemies;
                moveList = Splat_Pawn_Moves<Pawn_Up_Right<Us, Next>>(moveList, b1);
                moveList = Splat_Pawn_Moves<Pawn_Up_Left<Us, Next>>(moveList, b2);
                if (pos.Ep_Square() != SQ_NONE)
                {
                    if (GenType.Type == EVASION && (target & (pos.Ep_Square() + (int)Us.Up)) != 0)
                    {
                        return moveList;
                    }
                    b1 = Pawn_sNotOn7 & Attacks_BB<Pawn>(pos.Ep_Square(), Next.Value);
                    while (b1 != 0)
                    {
                        *moveList++ = Move.Make_Move<EnPassant>(Pop_Lsb(ref b1), pos.Ep_Square());
                    }
                }
            }
            return moveList;   
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Move* Generate_Moves<PieceType, Us, Next>(ref Position pos, Move* moveList, Bitboard target) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us> where PieceType : struct, IPieceType, IPieceTypes
        {
            Bitboard bb = pos.Get_Pieces<PieceType>(Us.Value);
            while (bb != 0)
            {
                Square from = Pop_Lsb(ref bb);
                Bitboard b = Attacks_BB<PieceType>(from, pos.Get_Pieces()) & target;
                moveList = Splat_Moves(moveList, from, b); 
            }
            return moveList;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static unsafe Move* Generate_All<GenType, Us, Next>(ref Position pos, Move* moveList) where GenType : struct, IGenType where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            Square ksq = pos.Get_Square<King>(Us.Value);
            Bitboard target = 0;
            if (GenType.Type != EVASION || !More_Than_One(pos.Checkers()))
            {
                target = GenType.Type == EVASION    ? Between_BB(ksq, Lsb(pos.Checkers()))
                       : GenType.Type == NON_EVASION ? ~pos.Get_Pieces(Us.Value)
                       : GenType.Type == CAPTURE    ? pos.Get_Pieces(Next.Value)
                                                        : ~pos.Get_Pieces();
                moveList = Generate_Pawn_Moves<GenType, Us, Next>(ref pos, moveList, target);
                moveList = Generate_Moves<Knight, Us, Next>(ref pos, moveList, target);
                moveList = Generate_Moves<Bishop, Us, Next>(ref pos, moveList, target);
                moveList = Generate_Moves<Rook, Us, Next>(ref pos, moveList, target);
                moveList = Generate_Moves<Queen, Us, Next>(ref pos, moveList, target);
            }
            Bitboard b = Attacks_BB<King>(ksq) & (GenType.Type == EVASION ? ~pos.Get_Pieces(Us.Value) : target);
            moveList = Splat_Moves(moveList, ksq, b);
            if ((GenType.Type == QUIET || GenType.Type == NON_EVASION) && pos.Can_Castle(Us.CastlingRights))
            {
                fixed (CastlingRights* crStart = &Us.AllCastlingRights[0])
                {
                    for (CastlingRights* crPtr = crStart, crEnd = crStart + AllCastlingRights.Length; crPtr != crEnd; ++crPtr)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Move* Generate<GenType, Us, Next>(ref Position pos, Move* moveList) where GenType : struct, IGenType where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return Generate_All<GenType, Us, Next>(ref pos, moveList);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Move* Generate_Legal<Us, Next>(ref Position pos, Move* moveList) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            Bitboard pinned = pos.Blockers_For_King(Us.Value) & pos.Get_Pieces(Us.Value);
            Square ksq = pos.Get_Square<King>(Us.Value);
            Move* cur = moveList;
            moveList = pos.Checkers() != 0 ? Generate<Evasions, Us, Next>(ref pos, moveList) : Generate<Non_Evasions, Us, Next>(ref pos, moveList);
            while (cur != moveList)
            {
                if (((pinned & cur->From_Sq()) != 0 || cur->From_Sq() == ksq || cur->Type_Of() == EN_PASSANT) && !pos.Legal<Us, Next>(*cur))
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
