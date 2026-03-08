using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    public interface IMagic
    {
        static abstract int Index { get; }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Magic
    {
        public ulong mask;
        public BitBoard[] attacks;
        public ulong magic;
        public int Shift;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Index(ulong occupied)
        {
            return (int)(((occupied & mask) * magic) >> Shift);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong AttacksBB(ulong occupied)
        {
            return attacks[Index(occupied)];
        }
    }
}
