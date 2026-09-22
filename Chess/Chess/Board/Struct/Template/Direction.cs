using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Direction;
using static Chess.Bitboards;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct North : IDirection
    {
        public static Direction Offset => NORTH;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return bb << 8;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct South : IDirection
    {
        public static Direction Offset => SOUTH;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return bb >> 8;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct East : IDirection
    {
        public static Direction Offset => EAST;
        public static Bitboard Mask => Not_File_HBB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_HBB) << 1;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct West : IDirection
    {
        public static Direction Offset => WEST;
        public static Bitboard Mask => Not_File_ABB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_ABB) >> 1;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthEast : IDirection
    {
        public static Direction Offset => NORTH_EAST;
        public static Bitboard Mask => Not_File_HBB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_HBB) << 9;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NorthWest : IDirection
    {
        public static Direction Offset => NORTH_WEST;
        public static Bitboard Mask => Not_File_ABB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_ABB) << 7;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthEast : IDirection
    {
        public static Direction Offset => SOUTH_EAST;
        public static Bitboard Mask => Not_File_HBB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_HBB) >> 7;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SouthWest : IDirection
    {
        public static Direction Offset => SOUTH_WEST;
        public static Bitboard Mask => Not_File_ABB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return (bb & Not_File_ABB) >> 9;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn_Up<Us, Next> : IDirection where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
    {
        public static Direction Offset => Us.Up;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return Us.Pawn_Up(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn_Up_Left<Us, Next> : IDirection where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
    {
        public static Direction Offset => Us.UpLeft;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return Us.Pawn_Up_Left(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn_Up_Right<Us, Next> : IDirection where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
    {
        public static Direction Offset => Us.UpRight;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return Us.Pawn_Up_Right(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn_Double_Up<Us, Next> : IDirection where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
    {
        public static Direction Offset => Us.Double;
        public static Bitboard Mask => ulong.MaxValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift(Bitboard bb)
        {
            return Us.Pawn_Double_Up(bb);
        }
    }
}
