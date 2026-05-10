using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Color;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pinners()
    {
        private readonly Pinners_Data Raw = default;
        public readonly ref Bitboard this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.As<Pinners_Data, Bitboard>(ref Unsafe.AsRef(in Raw)), c);
        }
    }
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Pinners_Data
    {
        private Bitboard Raw;
    }
}
