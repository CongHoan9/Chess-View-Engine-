using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct North : IDirection
    {
        public static int Offset => 8;
        public static ulong Mask => ulong.MaxValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct South : IDirection
    {
        public static int Offset => -8;
        public static ulong Mask => ulong.MaxValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct East : IDirection
    {
        public static int Offset => 1;
        public static ulong Mask => BitBoards.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct West : IDirection
    {
        public static int Offset => -1;
        public static ulong Mask => BitBoards.NotFileABB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthEast : IDirection
    {
        public static int Offset => 9;
        public static ulong Mask => BitBoards.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthWest : IDirection
    {
        public static int Offset => 7;
        public static ulong Mask => BitBoards.NotFileABB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthEast : IDirection
    {
        public static int Offset => -7;
        public static ulong Mask => BitBoards.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthWest : IDirection
    {
        public static int Offset => -9;
        public static ulong Mask => BitBoards.NotFileABB;
    }
}
