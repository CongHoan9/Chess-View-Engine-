using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPawnUp<C> : IPawnOffset where C : struct, IColor
    {
        public static EDirection Offset => C.Up;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Shift(SBitBoard bb) => C.PawnUp(bb);
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPawnUpLeft<C> : IPawnOffset where C : struct, IColor
    {
        public static EDirection Offset => C.UpLeft;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Shift(SBitBoard bb) => C.PawnUpLeft(bb);
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPawnUpRight<C> : IPawnOffset where C : struct, IColor
    {
        public static EDirection Offset => C.UpRight;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Shift(SBitBoard bb) => C.PawnUpRight(bb);
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPawnDoubleUp<C> : IPawnOffset where C : struct, IColor
    {
        public static EDirection Offset => (int)C.Up + C.Up;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Shift(SBitBoard bb) => C.PawnDoubleUp(bb);
    }
}

