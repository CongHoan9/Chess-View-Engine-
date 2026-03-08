using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PawnUp<C> : IPawnOffset where C : struct, IColor
    {
        public static Direction Value => C.Up;
        public static ulong Mask => ulong.MaxValue;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PawnUpLeft<C> : IPawnOffset where C : struct, IColor
    {
        public static Direction Value => C.Left;
        public static ulong Mask => BitBoards.NotFileABB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PawnUpRight<C> : IPawnOffset where C : struct, IColor
    {
        public static Direction Value => C.Right;
        public static ulong Mask => BitBoards.NotFileHBB;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PawnDoubleUp<C> : IPawnOffset where C : struct, IColor
    {
        public static Direction Value => (int)C.Up + C.Up;
        public static ulong Mask => ulong.MaxValue;
    }
}
