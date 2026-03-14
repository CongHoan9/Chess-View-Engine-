using static Chess.PieceType;
using static Chess.Direction;
using static Chess.MoveType;
using static Chess.GenType;
using static Chess.Piece;
using static Chess.Color;
namespace Chess
{
    using Value = Int32;
    public static class Types
    {
        public const Value MAX_MOVES = 256;
        public const Value MAX_PLY = 246;
        public const Value VALUE_ZERO = 0;
        public const Value VALUE_DRAW = 0;
        public const Value VALUE_NONE = 32002;
        public const Value VALUE_INFINITE = 32001;
        public const Value VALUE_MATE = 32000;
        public const Value VALUE_MATE_IN_MAX_PLY = VALUE_MATE - MAX_PLY;
        public const Value VALUE_MATED_IN_MAX_PLY = -VALUE_MATE_IN_MAX_PLY;
        public const Value VALUE_TB = VALUE_MATE_IN_MAX_PLY - 1;
        public const Value VALUE_TB_WIN_IN_MAX_PLY = VALUE_TB - MAX_PLY;
        public const Value VALUE_TB_LOSS_IN_MAX_PLY = -VALUE_TB_WIN_IN_MAX_PLY;
        public const Value PawnValue = 208;
        public const Value KnightValue = 781;
        public const Value BishopValue = 825;
        public const Value RookValue = 1276;
        public const Value QueenValue = 2538;
        public static readonly Value[] PieceValue =
        [
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, VALUE_ZERO, VALUE_ZERO
        ];
        public static readonly Direction[] RookDirections = [NORTH, SOUTH, EAST, WEST];
        public static readonly Direction[] BishopDirections = [NORTH_EAST, SOUTH_EAST, SOUTH_WEST, NORTH_WEST];
        public static readonly PieceType[] PieceTypes = [PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING];
        public static readonly PieceType[] Slider = [BISHOP, ROOK];
        public static readonly Piece[] Pieces = 
        [
            W_PAWN, W_KNIGHT, W_BISHOP, W_ROOK, W_QUEEN, W_KING,
            B_PAWN, B_KNIGHT, B_BISHOP, B_ROOK, B_QUEEN, B_KING
        ];
        public static readonly Color[] Colors = [WHITE, BLACK];
        public static readonly Direction[] Directions = [NORTH, SOUTH, EAST, WEST, NORTH_EAST, NORTH_WEST, SOUTH_EAST, SOUTH_WEST];
        public static readonly MoveType[] MoveTypes = [NORMAL, PROMOTION, EN_PASSANT, CASTLING];
        public static readonly GenType[] GenTypes = [CAPTURE, QUIET, NON_EVASION, EVASION];
        public static readonly int[] KnightSteps = [-17, -15, -10, -6, 6, 10, 15, 17];
        public static readonly int[] KingSteps = [-9, -8, -7, -1, 1, 7, 8, 9];
    }
}