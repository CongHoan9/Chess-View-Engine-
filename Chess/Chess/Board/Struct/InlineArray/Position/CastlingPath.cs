using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.CastlingRights;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CastlingPath()
    {
        private readonly CastlingPath_Data Raw = default;
        public readonly ref Bitboard this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef(in Raw.Raw), c);
        }
    }
    [InlineArray((int)CASTLING_RIGHT_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CastlingPath_Data
    {
        public Bitboard Raw;
    }
}
