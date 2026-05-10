using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Square;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Board()
    {
        private readonly Board_Data Raw;
        public readonly ref Piece this[int c]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.As<Board_Data, Piece>(ref Unsafe.AsRef(in Raw)), c);
        }
    }
    [InlineArray((int)SQ_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Board_Data
    {
        public Piece Raw;
    }
}
