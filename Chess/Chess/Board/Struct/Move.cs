using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Move
    {
        public ushort Raw { get; }
        public static readonly Move MoveNone = new(0);
        public static readonly Move MoveNull = new(65);
        public static implicit operator ushort(Move m) => m.Raw;
        public static implicit operator Move(ushort m) => new(m);
        public static bool operator ==(Move a, Move b) => a.Raw == b.Raw;
        public static bool operator !=(Move a, Move b) => a.Raw != b.Raw;
        public Move(ushort m)
        {
            Raw = m;
        }
        public Move(Square from, Square to)
        {
            Raw = (ushort)(((int)from << 6) | (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveType TypeOf()
        {
            return (MoveType)(Raw & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square FromSq()
        {
            return (Square)(Raw >> 6 & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square ToSq()
        {
            return (Square)(Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceType PromotionType()
        {
            return ((Raw >> 12) & 3) + PieceType.Knight;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make<M>(Square from, Square to) where M : struct, IMoveType
        {
            return (ushort)((int)from | ((int)to << 6) | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make<M, P>(Square from, Square to) where M : struct, IMoveType where P : struct, IPieceType
        {
            return (ushort)((int)from | ((int)to << 6) | ((int)P.Type << 12) | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Move move && Raw == move.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
