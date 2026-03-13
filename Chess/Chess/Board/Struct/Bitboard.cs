using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.FuncBit;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Bitboard(ulong bb) : IEquatable<Bitboard>
    {
        public ulong Raw { get; } = bb;
        public static implicit operator ulong(Bitboard bb) => bb.Raw;
        public static implicit operator Bitboard(ulong bb) => new(bb);
        public static Bitboard operator &(Bitboard a, Square b) => a & Square_BB(b);
        public static Bitboard operator &(Bitboard a, int b) => a & (1UL << b);
        public static Bitboard operator |(Bitboard a, Square b) => a.Raw | Square_BB(b); 
        public static Bitboard operator |(Bitboard a, int b) => a.Raw | (1UL << b); 
        public static Bitboard operator ^(Bitboard a, Square b) => a.Raw ^ Square_BB(b);
        public static Bitboard operator ^(Bitboard a, int b) => a.Raw ^ (1UL << b);
        public static Bitboard operator ~(Bitboard a) => ~a.Raw;
        public static Bitboard operator <<(Bitboard a, int b) => a.Raw << b;
        public static Bitboard operator >>(Bitboard a, int b) => a.Raw >> b;
        public static Bitboard operator *(Bitboard a, ulong b) => a.Raw * b;
        public static bool operator ==(Bitboard a, Bitboard b) => a.Raw == b.Raw;
        public static bool operator !=(Bitboard a, Bitboard b) => a.Raw != b.Raw;
        public static bool operator %(Bitboard a, Square b) => (a & Square_BB(b)) != 0;
        public static bool operator %(Bitboard a, int b) => (a & (1UL << b)) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bitboard b)
        {
            return Raw == b.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Bitboard b && Equals(b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
