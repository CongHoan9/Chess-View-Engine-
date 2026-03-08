using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BitBoard(ulong bb)
    {
        public ulong Raw { get; } = bb;
        public static implicit operator ulong(BitBoard bb) => bb.Raw;
        public static implicit operator BitBoard(ulong bb) => new(bb);
        public static BitBoard operator &(BitBoard a, BitBoard b) => a.Raw & b.Raw;
        public static BitBoard operator &(BitBoard a, Square b) => a & 1UL << (int)b;
        public static BitBoard operator &(BitBoard a, int b) => a & 1UL << b;
        public static BitBoard operator |(BitBoard a, BitBoard b) => a.Raw | b.Raw; 
        public static BitBoard operator |(BitBoard a, Square b) => a.Raw | (1UL << (int)b);
        public static BitBoard operator |(BitBoard a, int b) => a.Raw | (1UL << b); 
        public static BitBoard operator ^(BitBoard a, BitBoard b) => a.Raw ^ b.Raw;
        public static BitBoard operator ^(BitBoard a, Square b) => a.Raw ^ (1UL << (int)b);
        public static BitBoard operator ^(BitBoard a, int b) => a.Raw ^ (1UL << b);
        public static BitBoard operator ~(BitBoard a) => ~a.Raw;
        public static BitBoard operator <<(BitBoard a, int b) => a.Raw << b;
        public static BitBoard operator <<(BitBoard a, File b) => a.Raw << (int)b;
        public static BitBoard operator <<(BitBoard a, Rank b) => a.Raw << (int)b;
        public static BitBoard operator >>(BitBoard a, int b) => a.Raw >> b;
        public static BitBoard operator >>(BitBoard a, File b) => a.Raw >> (int)b;
        public static BitBoard operator >>(BitBoard a, Rank b) => a.Raw >> (int)b;
        public static BitBoard operator *(BitBoard a, ulong b) => a.Raw * b;
        public static bool operator ==(BitBoard a, BitBoard b) => a.Raw == b.Raw;
        public static bool operator !=(BitBoard a, BitBoard b) => a.Raw != b.Raw;
        public static bool operator %(BitBoard a, Square b) => (a & (1UL << (int)b)) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is BitBoard board && Raw == board.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
