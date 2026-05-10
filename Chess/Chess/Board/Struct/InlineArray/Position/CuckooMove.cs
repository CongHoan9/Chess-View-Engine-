using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct CuckooMove()
    {
        private static readonly Move* RawPtr;
        public readonly ref Move this[int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static CuckooMove()
        {
            RawPtr = (Move*)NativeMemory.AllocZeroed(8192, (uint)sizeof(Move));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear()
        {
            Unsafe.InitBlock(RawPtr, 0, (uint)(8192 * sizeof(Move)));
        }
    }
}
