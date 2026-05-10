using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Square;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct LineBB()
    {
        private static readonly Bitboard* RawPtr;
        public readonly ref Bitboard this[int s1, int s2]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[(s1 << 6) | s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static LineBB()
        {
            RawPtr = (Bitboard*)NativeMemory.AllocZeroed(((int)SQ_NB * (int)SQ_NB), (uint)sizeof(Bitboard));
        }
    }
}
