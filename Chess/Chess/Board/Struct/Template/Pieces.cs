using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.PieceType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pieces<P1, P2> : IPieceTypes where P1 : struct, IPieceTypes where P2 : struct, IPieceTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return P1.Get(ref bb) | P2.Get(ref bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn : IPieceType, IPieceTypes
    {
        public static PieceType Type => PAWN;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)PAWN];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Knight : IPieceType, IPieceTypes
    {
        public static PieceType Type => KNIGHT;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)KNIGHT];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Bishop : IPieceType, IPieceTypes
    {
        public static PieceType Type => BISHOP;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)BISHOP];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rook : IPieceType, IPieceTypes
    {
        public static PieceType Type => ROOK;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)ROOK];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Queen : IPieceTypes, IPieceType
    {
        public static PieceType Type => QUEEN;
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)QUEEN];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct King : IPieceType, IPieceTypes
    {
        public static PieceType Type => KING;
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)KING];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AllPiece : IPieceType, IPieceTypes
    {
        public static PieceType Type => ALL_PIECES;
        public static Bitboard Get(ref ByTypeBB bb)
        {
            return bb[(int)ALL_PIECES];
        }
    }

}
