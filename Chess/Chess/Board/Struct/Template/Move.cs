using System.Runtime.InteropServices;
namespace Chess
{
    public interface IMoveType
    {
        static abstract EMoveType Type { get; }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Normal : IMoveType
    {
        public static EMoveType Type => EMoveType.Normal;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Promotion : IMoveType
    {
        public static EMoveType Type => EMoveType.Promotion;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EnPassant : IMoveType
    {
        public static EMoveType Type => EMoveType.EnPassant;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Castling : IMoveType
    {
        public static EMoveType Type => EMoveType.Castling;
    }
}
