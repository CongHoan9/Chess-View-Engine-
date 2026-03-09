using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SMove
    {
        public ushort Raw { get; }
        public static readonly SMove MoveNone = new(0);
        public static readonly SMove MoveNull = new(65);
        public static implicit operator ushort(SMove m) => m.Raw;
        public static implicit operator SMove(ushort m) => new(m);
        public static bool operator ==(SMove a, SMove b) => a.Raw == b.Raw;
        public static bool operator !=(SMove a, SMove b) => a.Raw != b.Raw;
        public SMove(ushort m)
        {
            Raw = m;
        }
        public SMove(ESquare from, ESquare to)
        {
            Raw = (ushort)(((int)from << 6) | (int)to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EMoveType TypeOf()
        {
            return (EMoveType)(Raw & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare FromSq()
        {
            return (ESquare)(Raw >> 6 & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare ToSq()
        {
            return (ESquare)(Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPieceType PromotionType()
        {
            return ((Raw >> 12) & 3) + EPieceType.Knight;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SMove Make<M>(ESquare from, ESquare to) where M : struct, IMoveType
        {
            return (ushort)((int)from | ((int)to << 6) | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SMove Make<M, P>(ESquare from, ESquare to) where M : struct, IMoveType where P : struct, IPieceType
        {
            return (ushort)((int)from | ((int)to << 6) | ((int)P.Type << 12) | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is SMove move && Raw == move.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
