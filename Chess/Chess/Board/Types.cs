using System;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Input;
#pragma warning disable IDE0130
namespace Chess
{
    public static class Types
    {
        public static readonly Value MAX_PLY = 246;
        public static readonly Value VALUE_ZERO = 0;
        public static readonly Value VALUE_DRAW = 0;
        public static readonly Value VALUE_NONE = 32002;
        public static readonly Value VALUE_INFINITE = 32001;
        public static readonly Value VALUE_MATE = 32000;
        public static readonly Value VALUE_MATE_IN_MAX_PLY = VALUE_MATE - MAX_PLY;
        public static readonly Value VALUE_MATED_IN_MAX_PLY = -VALUE_MATE_IN_MAX_PLY;
        public static readonly Value VALUE_TB = VALUE_MATE_IN_MAX_PLY - 1;
        public static readonly Value VALUE_TB_WIN_IN_MAX_PLY = VALUE_TB - MAX_PLY;
        public static readonly Value VALUE_TB_LOSS_IN_MAX_PLY = -VALUE_TB_WIN_IN_MAX_PLY;
        public static readonly Value PawnValue = 208;
        public static readonly Value KnightValue = 781;
        public static readonly Value BishopValue = 825;
        public static readonly Value RookValue = 1276;
        public static readonly Value QueenValue = 2538;
        public static readonly Value[] PieceValue =
        [
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO
        ];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Rotate180(Square sq)
        {
            return (Square)((int)sq ^ 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromTo(Move m)
        {
            return (int)m & 0xFFF;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveType TypeOf(Move m)
        {
            return (MoveType)((int)m & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType PromotionType(Move m)
        {
            return (PieceType)(((int)m >> 12) & 3) + (int)PieceType.Knight;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move MakeMove(Square from, Square to)
        {
            return (Move)((int)from << 6 + (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move MakeMove<T>(Square from, Square to, PieceType pt = PieceType.Knight) where T : struct, IMoveType
        {
            return (Move)((int)T.Type + (((int)pt - (int)PieceType.Knight) << 12) + ((int)from << 6) + (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece MakePiece(Color c, PieceType pt) 
        { 
            return (Piece)(((int)c << 3) + pt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square FromSq(Move m)
        {
            return (Square)(m.Raw >> 6 & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square ToSq(Move m)
        {
            return (Square)(m.Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(Move m)
        {
            return FromSq(m) != ToSq(m);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(Square s)
        { 
            return s >= Square.SQ_A1 && s <= Square.SQ_H8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File FileOf(Square s) 
        { 
            return (File)((int)s & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank RankOf(Square s) 
        {
            return (Rank)((int)s >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Key MakeKey(ulong seed)
        {
            return seed * 6364136223846793005UL + 1442695040888963407UL;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnPush(Color c) 
        { 
            return c == Color.White ? Direction.North : Direction.South;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType TypeOf(Piece pc)
        { 
            return (PieceType)((int)pc & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color ColorOf(Piece pc)
        {
            return (Color)((int)pc >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square RelativeSquare(Color c, Square s) 
        { 
            return (Square)((int)s ^ ((int)c * 56));
        }
    }
}