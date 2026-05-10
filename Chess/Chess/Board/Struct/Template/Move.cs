using System.Runtime.InteropServices;
using static Chess.MoveType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Normal : IMoveType
    {
        public static MoveType Type => NORMAL;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Promotion : IMoveType
    {
        public static MoveType Type => PROMOTION;
    }
    // EnPassant and Castling are defined in their respective files due to their unique data structures Chess/Board/Struct/InlineArray/Zobrist.
}
