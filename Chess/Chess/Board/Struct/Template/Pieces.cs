using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.PieceType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pieces<P1, P2> : IPieceTypes where P1 : struct, IPieceTypes where P2 : struct, IPieceTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(Bitboard[] bb)
        {
            return P1.Get(bb) | P2.Get(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn : IPieceType, IPieceTypes
    {
        public static PieceType Type => PAWN;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)PAWN];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Knight : IPieceType, IPieceTypes
    {
        public static PieceType Type => KNIGHT;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)KNIGHT];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Bishop : IPieceType, IPieceTypes, IMagic
    {
        public static PieceType Type => BISHOP;
        public static int Index => 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)BISHOP];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rook : IPieceType, IPieceTypes, IMagic
    {
        public static PieceType Type => ROOK;
        public static int Index => 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)ROOK];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Queen : IPieceTypes, IPieceType
    {
        public static PieceType Type => QUEEN;
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)QUEEN];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct King : IPieceType, IPieceTypes
    {
        public static PieceType Type => KING;
        public static Bitboard Get(Bitboard[] bb)
        {
            return bb[(int)KING];
        }
    }
}
