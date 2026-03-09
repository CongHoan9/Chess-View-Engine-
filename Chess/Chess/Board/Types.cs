using System;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Input;
#pragma warning disable IDE0130
namespace Chess
{
    public static class Types
    {
        public static readonly SValue MAX_PLY = 246;
        public static readonly SValue VALUE_ZERO = 0;
        public static readonly SValue VALUE_DRAW = 0;
        public static readonly SValue VALUE_NONE = 32002;
        public static readonly SValue VALUE_INFINITE = 32001;
        public static readonly SValue VALUE_MATE = 32000;
        public static readonly SValue VALUE_MATE_IN_MAX_PLY = VALUE_MATE - MAX_PLY;
        public static readonly SValue VALUE_MATED_IN_MAX_PLY = -VALUE_MATE_IN_MAX_PLY;
        public static readonly SValue VALUE_TB = VALUE_MATE_IN_MAX_PLY - 1;
        public static readonly SValue VALUE_TB_WIN_IN_MAX_PLY = VALUE_TB - MAX_PLY;
        public static readonly SValue VALUE_TB_LOSS_IN_MAX_PLY = -VALUE_TB_WIN_IN_MAX_PLY;
        public static readonly SValue PawnValue = 208;
        public static readonly SValue KnightValue = 781;
        public static readonly SValue BishopValue = 825;
        public static readonly SValue RookValue = 1276;
        public static readonly SValue QueenValue = 2538;
        public static readonly SValue[] PieceValue =
        [
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO
        ];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare Rotate180(ESquare sq)
        {
            return (ESquare)((int)sq ^ 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromTo(SMove m)
        {
            return (int)m & 0xFFF;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EMoveType TypeOf(SMove m)
        {
            return (EMoveType)((int)m & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EPieceType PromotionType(SMove m)
        {
            return (EPieceType)(((int)m >> 12) & 3) + (int)EPieceType.Knight;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SMove MakeMove(ESquare from, ESquare to)
        {
            return (SMove)((int)from << 6 + (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SMove MakeMove<T>(ESquare from, ESquare to, EPieceType pt = EPieceType.Knight) where T : struct, IMoveType
        {
            return (SMove)((int)T.Type + (((int)pt - (int)EPieceType.Knight) << 12) + ((int)from << 6) + (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EPiece MakePiece(EColor c, EPieceType pt) 
        { 
            return (EPiece)(((int)c << 3) + pt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare FromSq(SMove m)
        {
            return (ESquare)(m.Raw >> 6 & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare ToSq(SMove m)
        {
            return (ESquare)(m.Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(SMove m)
        {
            return FromSq(m) != ToSq(m);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(ESquare s)
        { 
            return s >= ESquare.SQ_A1 && s <= ESquare.SQ_H8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EFile FileOf(ESquare s) 
        { 
            return (EFile)((int)s & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ERank RankOf(ESquare s) 
        {
            return (ERank)((int)s >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SKey MakeKey(ulong seed)
        {
            return seed * 6364136223846793005UL + 1442695040888963407UL;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EDirection PawnPush(EColor c) 
        { 
            return c == EColor.White ? EDirection.North : EDirection.South;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EPieceType TypeOf(EPiece pc)
        { 
            return (EPieceType)((int)pc & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EColor ColorOf(EPiece pc)
        {
            return (EColor)((int)pc >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare RelativeSquare(EColor c, ESquare s) 
        { 
            return (ESquare)((int)s ^ ((int)c * 56));
        }
    }
}