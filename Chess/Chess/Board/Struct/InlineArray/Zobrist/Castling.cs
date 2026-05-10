using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.CastlingRights;
using static Chess.MoveType;
namespace Chess
{
    using Key = UInt64;
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Castling() : IMoveType
    {
        private readonly static Key* RawPtr;
        public static MoveType Type => CASTLING;
        public readonly ref Key this[int s]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[s];
        }
        static Castling()
        {
            RawPtr = (Key*)NativeMemory.AllocZeroed((int)CASTLING_RIGHT_NB * sizeof(Key));
        }
    }
}
