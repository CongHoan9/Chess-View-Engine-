using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct Magic
    {
        public Bitboard mask;
        public Bitboard* attacks;
        public Bitboard magic;
        public int Shift;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Index(Bitboard occupied)
        {
            return (int)(((occupied & mask) * magic).Raw >> Shift);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Bitboard Attacks_BB(Bitboard occupied)
        {
            return attacks[Index(occupied)];
        }
    }
}
