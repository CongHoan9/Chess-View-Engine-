using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.CastlingRights;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CastlingRookSquare()
    {
        private readonly CastlingRookSquare_Data Raw = default;
        public readonly ref Square this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef(in Raw.Raw), c);
        }
    }
    [InlineArray((int)CASTLING_RIGHT_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CastlingRookSquare_Data
    {
        public Square Raw;
    }
}
