using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPieces<P1, P2> : IPieceTypes where P1 : struct, IPieceTypes where P2 : struct, IPieceTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return P1.Get(bb) | P2.Get(bb);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SPawn : IPieceType, IPieceTypes
    {
        public static EPieceType Type => EPieceType.Pawn;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.Pawn];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SKnight : IPieceType, IPieceTypes
    {
        public static EPieceType Type => EPieceType.Knight;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.Knight];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SBishop : IPieceType, IPieceTypes, IMagic
    {
        public static EPieceType Type => EPieceType.Bishop;
        public static int Index => 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.Bishop];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SRook : IPieceType, IPieceTypes, IMagic
    {
        public static EPieceType Type => EPieceType.Rook;
        public static int Index => 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.Rook];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SQueen : IPieceTypes, IPieceType
    {
        public static EPieceType Type => EPieceType.Queen;
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.Queen];
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SKing : IPieceType, IPieceTypes
    {
        public static EPieceType Type => EPieceType.King;
        public static SBitBoard Get(SBitBoard[] bb)
        {
            return bb[(int)EPieceType.King];
        }
    }
}
