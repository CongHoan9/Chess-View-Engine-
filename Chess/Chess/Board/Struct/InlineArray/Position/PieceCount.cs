using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Piece;
namespace Chess
{
    public readonly struct PieceCount()
    {
        private readonly PieceCount_Data Raw = default;
        public readonly ref int this[int p]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef(in Raw.Raw), p);
        }
    }
    [InlineArray((int)PIECE_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PieceCount_Data
    {
        public int Raw;
    }
}
