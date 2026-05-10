using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Square;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Magics()
    {
        private static readonly Magic* RawPtr;
        public readonly ref Magic this[int s, int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[(s << 1) | i];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Magics()
        {
            RawPtr = (Magic*)NativeMemory.AllocZeroed(((int)SQ_NB * 2), (uint)sizeof(Magic));
        }
    }
}
