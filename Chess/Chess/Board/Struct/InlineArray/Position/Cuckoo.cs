using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
namespace Chess
{
    using Key = UInt64;
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Cuckoo()
    {
        private static readonly Key* RawPtr;
        public readonly ref Key this[int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[s];
        }
        static Cuckoo()
        {
            RawPtr = (Key*)NativeMemory.AllocZeroed(8192, sizeof(Key));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear()
        {
            Unsafe.InitBlock(RawPtr, 0, 8192 * sizeof(Key));
        }
    }
}
