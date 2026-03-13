using System.Runtime.InteropServices;
using static Chess.Types;
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
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EnPassant : IMoveType
    {
        public static MoveType Type => EN_PASSANT;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Castling : IMoveType
    {
        public static MoveType Type => CASTLING;
    }
}
