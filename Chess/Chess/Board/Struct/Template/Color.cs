using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SWhite : IColor
    {
        public static EColor Us => EColor.White;
        public static EColor Them => EColor.Black;
        public static EDirection Up => EDirection.North;
        public static SBitBoard Rank3BB => BitBoard.Rank3BB;
        public static SBitBoard Rank7BB => BitBoard.Rank7BB;
        public static EDirection UpLeft => EDirection.NorthWest;
        public static EDirection UpRight => EDirection.NorthEast;
        public static ECastlingRights KingSide => ECastlingRights.WhiteOO;
        public static ECastlingRights QueenSide => ECastlingRights.WhiteOOO;
        public static ECastlingRights CastlingRights => ECastlingRights.WhiteCastling;
        public static ECastlingRights[] AllCastlingRights => [KingSide, QueenSide];
        // func
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUp(SBitBoard bb) => bb << 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUpLeft(SBitBoard bb) => (bb & BitBoard.NotFileABB) << 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUpRight(SBitBoard bb) => (bb & BitBoard.NotFileHBB) << 9;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnDoubleUp(SBitBoard bb) => bb << 16;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SBlack : IColor
    {
        public static EColor Us => EColor.Black;
        public static EColor Them => EColor.White;
        public static EDirection Up => EDirection.South;
        public static SBitBoard Rank3BB => BitBoard.Rank6BB;
        public static SBitBoard Rank7BB => BitBoard.Rank2BB;
        public static EDirection UpLeft => EDirection.SouthWest;
        public static EDirection UpRight => EDirection.SouthEast;
        public static ECastlingRights KingSide => ECastlingRights.BlackOO;
        public static ECastlingRights QueenSide => ECastlingRights.BlackOOO;
        public static ECastlingRights CastlingRights => ECastlingRights.BlackCastling;
        public static ECastlingRights[] AllCastlingRights => [KingSide, QueenSide];
        // func
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUp(SBitBoard bb) => bb >> 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUpLeft(SBitBoard bb) => (bb & BitBoard.NotFileABB) >> 9;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnUpRight(SBitBoard bb) => (bb & BitBoard.NotFileHBB) >> 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnDoubleUp(SBitBoard bb) => bb >> 16;
    }
}

