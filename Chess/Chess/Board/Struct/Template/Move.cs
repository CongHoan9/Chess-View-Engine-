using System.Runtime.InteropServices;
namespace Chess
{
    public interface IMoveType
    {
        static abstract MoveType Type { get; }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Normal : IMoveType
    {
        public static MoveType Type => MoveType.Normal;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Promotion : IMoveType
    {
        public static MoveType Type => MoveType.Promotion;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EnPassant : IMoveType
    {
        public static MoveType Type => MoveType.EnPassant;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Castling : IMoveType
    {
        public static MoveType Type => MoveType.Castling;
    }
}
