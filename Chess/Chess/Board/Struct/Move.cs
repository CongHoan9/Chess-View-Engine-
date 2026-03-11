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
        public static SMove None()
        {
            return new SMove(0);
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
            return (SMove)(((int)from << 6) | (int)to | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SMove Make<M, P>(ESquare from, ESquare to) where M : struct, IMoveType where P : struct, IPieceType
        {
            return (SMove)(((int)from << 6) | (int)to | (((int)P.Type - (int)EPieceType.Knight) << 12) | (ushort)M.Type);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            int from = Raw & 0x3F;
            int to = (Raw >> 6) & 0x3F;
            int promo = (Raw >> 12) & 0x3;
            Span<char> s = stackalloc char[5];
            s[0] = (char)('a' + (from & 7));
            s[1] = (char)('1' + (from >> 3));
            s[2] = (char)('a' + (to & 7));
            s[3] = (char)('1' + (to >> 3));
            if (promo != 0)
            {
                s[4] = promo switch
                {
                    1 => 'n',
                    2 => 'b',
                    3 => 'r',
                    _ => 'q'
                };
                return new string(s);
            }
            return new string(s[..4]);
        }
    }
}
