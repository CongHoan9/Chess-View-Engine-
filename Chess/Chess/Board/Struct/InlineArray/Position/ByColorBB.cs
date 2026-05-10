using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Color;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ByColorBB()
    {
        private readonly ByColorBB_Data Raw = default;
        public readonly ref Bitboard this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef(in Raw.Raw), c);
        }
    }
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ByColorBB_Data
    {
        public Bitboard Raw;
    }
}
