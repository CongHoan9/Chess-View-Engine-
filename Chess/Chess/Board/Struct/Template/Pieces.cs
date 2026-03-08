using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pieces<P1, P2> : IPieceTypes where P1 : struct, IPieceTypes where P2 : struct, IPieceTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Get(BitBoard[] bb)
        {
            return P1.Get(bb) | P2.Get(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Pawn : IPieceType, IPieceTypes
    {
        public static PieceType Type => PieceType.Pawn;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.Pawn];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Knight : IPieceType, IPieceTypes
    {
        public static PieceType Type => PieceType.Knight;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.Knight];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Bishop : IPieceType, IPieceTypes, IMagic
    {
        public static PieceType Type => PieceType.Bishop;
        public static int Index => 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.Bishop];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Rook : IPieceType, IPieceTypes, IMagic
    {
        public static PieceType Type => PieceType.Rook;
        public static int Index => 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.Rook];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Queen : IPieceTypes, IPieceType
    {
        public static PieceType Type => PieceType.Queen;
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.Queen];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct King : IPieceType, IPieceTypes
    {
        public static PieceType Type => PieceType.King;
        public static BitBoard Get(BitBoard[] bb)
        {
            return bb[(int)PieceType.King];
        }
    }
}
