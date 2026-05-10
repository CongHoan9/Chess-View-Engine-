using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Square;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct SquareDistance()
    {
        private static readonly byte* RawPtr;
        public readonly ref byte this[int s1, int s2]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[(s1 << 6) | s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SquareDistance()
        {
            RawPtr = (byte*)NativeMemory.AllocZeroed(((int)SQ_NB * (int)SQ_NB), sizeof(byte));
        }

    }
}
