using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.CastlingRights;
using static Chess.Direction;
using static Chess.Color;
using static Chess.Bitboards;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct White : IColor
    {
        public static Color Us => WHITE;
        public static Color Them => BLACK;
        public static Bitboard Rank3 => Rank_3BB;
        public static Bitboard Rank7 => Rank_7BB;
        public static Direction Up => NORTH;
        public static Direction UpLeft => NORTH_WEST;
        public static Direction UpRight => NORTH_EAST;
        public static Direction Double => NORTH_NORTH;
        public static CastlingRights KingSide => WHITE_OO;
        public static CastlingRights QueenSide => WHITE_OOO;
        public static CastlingRights CastlingRights => WHITE_CASLING;
        public static CastlingRights[] AllCastlingRights => [KingSide, QueenSide];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up(Bitboard bb) => bb << 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up_Left(Bitboard bb) => (bb & Not_File_ABB) << 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up_Right(Bitboard bb) => (bb & Not_File_HBB) << 9;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Double_Up(Bitboard bb) => bb << 16;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Black : IColor
    {
        public static Color Us => BLACK;
        public static Color Them => WHITE;
        public static Bitboard Rank3 => Rank_6BB;
        public static Bitboard Rank7 => Rank_2BB;
        public static Direction Up => SOUTH;
        public static Direction UpLeft => SOUTH_EAST;
        public static Direction UpRight => SOUTH_WEST;
        public static Direction Double => SOUTH_SOUTH;
        public static CastlingRights KingSide => BLACK_OO;
        public static CastlingRights QueenSide => BLACK_OOO;
        public static CastlingRights CastlingRights => BLACK_CASLING;
        public static CastlingRights[] AllCastlingRights => [KingSide, QueenSide];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up(Bitboard bb) => bb >> 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up_Left(Bitboard bb) => (bb & Not_File_ABB) >> 9;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Up_Right(Bitboard bb) => (bb & Not_File_HBB) >> 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Double_Up(Bitboard bb) => bb >> 16;
    }
}

