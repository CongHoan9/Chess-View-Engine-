using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SBitBoard(ulong bb)
    {
        public ulong Raw { get; } = bb;
        public static implicit operator ulong(SBitBoard bb) => bb.Raw;
        public static implicit operator SBitBoard(ulong bb) => new(bb);
        public static SBitBoard operator &(SBitBoard a, SBitBoard b) => a.Raw & b.Raw;
        public static SBitBoard operator &(SBitBoard a, ESquare b) => a & 1UL << (int)b;
        public static SBitBoard operator &(SBitBoard a, int b) => a & 1UL << b;
        public static SBitBoard operator |(SBitBoard a, SBitBoard b) => a.Raw | b.Raw; 
        public static SBitBoard operator |(SBitBoard a, ESquare b) => a.Raw | (1UL << (int)b);
        public static SBitBoard operator |(SBitBoard a, int b) => a.Raw | (1UL << b); 
        public static SBitBoard operator ^(SBitBoard a, SBitBoard b) => a.Raw ^ b.Raw;
        public static SBitBoard operator ^(SBitBoard a, ESquare b) => a.Raw ^ (1UL << (int)b);
        public static SBitBoard operator ^(SBitBoard a, int b) => a.Raw ^ (1UL << b);
        public static SBitBoard operator ~(SBitBoard a) => ~a.Raw;
        public static SBitBoard operator <<(SBitBoard a, int b) => a.Raw << b;
        public static SBitBoard operator <<(SBitBoard a, EFile b) => a.Raw << (int)b;
        public static SBitBoard operator <<(SBitBoard a, ERank b) => a.Raw << (int)b;
        public static SBitBoard operator >>(SBitBoard a, int b) => a.Raw >> b;
        public static SBitBoard operator >>(SBitBoard a, EFile b) => a.Raw >> (int)b;
        public static SBitBoard operator >>(SBitBoard a, ERank b) => a.Raw >> (int)b;
        public static SBitBoard operator *(SBitBoard a, ulong b) => a.Raw * b;
        public static bool operator ==(SBitBoard a, SBitBoard b) => a.Raw == b.Raw;
        public static bool operator !=(SBitBoard a, SBitBoard b) => a.Raw != b.Raw;
        public static bool operator %(SBitBoard a, ESquare b) => (a & (1UL << (int)b)) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is SBitBoard board && Raw == board.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
