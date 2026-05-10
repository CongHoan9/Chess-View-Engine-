using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct RookTable()
    {
        private static readonly Bitboard* RawPtr;
        public readonly ref Bitboard this[int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static RookTable()
        {
            RawPtr = (Bitboard*)NativeMemory.AllocZeroed(0x19000, (uint)sizeof(Bitboard));
        }
    }
}
