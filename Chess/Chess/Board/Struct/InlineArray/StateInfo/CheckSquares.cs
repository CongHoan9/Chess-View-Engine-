using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.PieceType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CheckSquares()
    {
        private readonly CheckSquares_Data Raw = default;
        public readonly ref Bitboard this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.As<CheckSquares_Data, Bitboard>(ref Unsafe.AsRef(in Raw)), c);
        }
    }
    [InlineArray((int)PIECE_TYPE_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CheckSquares_Data
    {
        private Bitboard Raw;
    }
}
