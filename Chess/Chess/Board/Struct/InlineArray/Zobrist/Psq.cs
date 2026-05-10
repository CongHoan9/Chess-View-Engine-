using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Square;
using static Chess.Piece;

namespace Chess
{
    using Key = UInt64;
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Psq()
    {
        private static readonly Key* RawPtr;
        public readonly ref Key this[int p, int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[(p << 6) | s];
        }
        static Psq()
        {
            RawPtr = (Key*)NativeMemory.AllocZeroed((uint)PIECE_NB * (uint)SQ_NB, sizeof(Key));
        }
    }
}
