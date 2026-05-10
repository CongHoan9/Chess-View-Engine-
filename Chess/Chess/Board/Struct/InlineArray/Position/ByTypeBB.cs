using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.PieceType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ByTypeBB()
    {
        private readonly ByTypeBB_Data Raw = default;
        public readonly ref Bitboard this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef(in Raw.Raw), c);
        }
    }
    [InlineArray((int)PIECE_TYPE_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ByTypeBB_Data
    {
        public Bitboard Raw;
    }
}
