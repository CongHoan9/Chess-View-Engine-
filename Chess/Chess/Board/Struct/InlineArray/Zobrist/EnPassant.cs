using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.MoveType;
using static Chess.File;
namespace Chess
{
    using Key = UInt64;
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EnPassant() : IMoveType
    {
        #pragma warning disable CS0649
        private static readonly Key* RawPtr;
        public static MoveType Type => EN_PASSANT;
        public readonly ref Key this[int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[s];
        }
        static EnPassant()
        {
            RawPtr = (Key*)NativeMemory.AllocZeroed((int)FILE_NB * sizeof(Key));
        }
    }
}
