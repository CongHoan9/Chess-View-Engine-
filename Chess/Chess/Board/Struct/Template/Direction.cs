using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct North : IDirection
    {
        public static EDirection Offset => EDirection.North;
        public static SBitBoard Mask => ulong.MaxValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct South : IDirection
    {
        public static EDirection Offset => EDirection.South;
        public static SBitBoard Mask => ulong.MaxValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct East : IDirection
    {
        public static EDirection Offset => EDirection.East;
        public static SBitBoard Mask => BitBoard.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct West : IDirection
    {
        public static EDirection Offset => EDirection.West;
        public static SBitBoard Mask => BitBoard.NotFileABB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthEast : IDirection
    {
        public static EDirection Offset => EDirection.NorthEast;
        public static SBitBoard Mask => BitBoard.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthWest : IDirection
    {
        public static EDirection Offset => EDirection.NorthWest;
        public static SBitBoard Mask => BitBoard.NotFileABB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthEast : IDirection
    {
        public static EDirection Offset => EDirection.SouthEast;
        public static SBitBoard Mask => BitBoard.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthWest : IDirection
    {
        public static EDirection Offset => EDirection.SouthWest;
        public static SBitBoard Mask => BitBoard.NotFileABB;
    }
}
