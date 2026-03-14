using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.PieceType;
using static Chess.Types;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Move : IEquatable<Move>
    {
        public ushort Raw { get; }
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
        public static Move None()
        {
            return new Move(0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveType Type_Of()
        {
            return (MoveType)(Raw & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square From_Sq()
        {
            return (Square)((Raw >> 6) & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square To_Sq()
        {
            return (Square)(Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceType Promotion_Type()
        {
            return (PieceType)(((Raw >> 12) & 3) + (int)KNIGHT);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make_Move<M>(Square from, Square to) where M : struct, IMoveType
        {
            return (Move)(((ushort)from << 6) | (ushort)to | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make_Move<M, P>(Square from, Square to) where M : struct, IMoveType where P : struct, IPieceType
        {
            return (Move)(((ushort)from << 6) | (ushort)to | ((P.Type - KNIGHT) << 12) | (ushort)M.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Move m)
        {
            return Raw == m.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Move m && Equals(m);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            int to = Raw & 0x3F;
            int from = (Raw >> 6) & 0x3F;
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
