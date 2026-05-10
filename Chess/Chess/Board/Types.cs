using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.CastlingRights;
using static Chess.Color;
using static Chess.Direction;
using static Chess.GenType;
using static Chess.MoveType;
using static Chess.Piece;
using static Chess.PieceType;
namespace Chess
{
    using Value = Int32;
    public static unsafe class Types
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
        public static readonly ValueArray16 PieceValue = Create_Piece_Value();
        public static readonly DirectionArray4 RookDirections = Create_Rook_Directions();
        public static readonly DirectionArray4 BishopDirections = Create_Bishop_Directions();
        public static readonly PieceTypeArray6 PieceTypes = Create_Piece_Types();
        public static readonly PieceTypeArray2 Slider = Create_Slider();
        public static readonly PieceArray12 Pieces = Create_Pieces();
        public static readonly ColorArray2 Colors = Create_Colors();
        public static readonly CastlingRightsArray2 AllWhiteCastlingRights = Create_All_White_Castling_Rights();
        public static readonly CastlingRightsArray2 AllBlackCastlingRights = Create_All_Black_Castling_Rights();
        public static readonly DirectionArray8 Directions = Create_Directions();
        public static readonly MoveTypeArray4 MoveTypes = Create_Move_Types();
        public static readonly GenTypeArray4 GenTypes = Create_Gen_Types();
        public static readonly IntArray8 KnightSteps = Create_Knight_Steps();
        public static readonly IntArray8 KingSteps = Create_King_Steps();
        public static readonly IntArray8 seeds = Create_Seeds();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Value Piece_Value(Piece piece)
        {
            return PieceValue[(int)piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Value Piece_Value(Color color, PieceType type)
        {
            return Piece_Value(FuncBit.Make_Piece(color, type));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Seed_Of(Rank rank)
        {
            return seeds[(int)rank];
        }

        private static ValueArray16 Create_Piece_Value()
        {
            ValueArray16 values = default;
            Value* value = (Value*)&values;
            *value++ = VALUE_ZERO;
            *value++ = PawnValue;
            *value++ = KnightValue;
            *value++ = BishopValue;
            *value++ = RookValue;
            *value++ = QueenValue;
            *value++ = VALUE_ZERO;
            *value++ = VALUE_ZERO;
            *value++ = VALUE_ZERO;
            *value++ = PawnValue;
            *value++ = KnightValue;
            *value++ = BishopValue;
            *value++ = RookValue;
            *value++ = QueenValue;
            *value++ = VALUE_ZERO;
            *value = VALUE_ZERO;
            return values;
        }

        private static DirectionArray4 Create_Rook_Directions()
        {
            DirectionArray4 values = default;
            Direction* value = (Direction*)&values;
            *value++ = NORTH;
            *value++ = SOUTH;
            *value++ = EAST;
            *value = WEST;
            return values;
        }

        private static DirectionArray4 Create_Bishop_Directions()
        {
            DirectionArray4 values = default;
            Direction* value = (Direction*)&values;
            *value++ = NORTH_EAST;
            *value++ = SOUTH_EAST;
            *value++ = SOUTH_WEST;
            *value = NORTH_WEST;
            return values;
        }

        private static PieceTypeArray6 Create_Piece_Types()
        {
            PieceTypeArray6 values = default;
            PieceType* value = (PieceType*)&values;
            *value++ = PAWN;
            *value++ = KNIGHT;
            *value++ = BISHOP;
            *value++ = ROOK;
            *value++ = QUEEN;
            *value = KING;
            return values;
        }

        private static PieceTypeArray2 Create_Slider()
        {
            PieceTypeArray2 values = default;
            PieceType* value = (PieceType*)&values;
            *value++ = BISHOP;
            *value = ROOK;
            return values;
        }

        private static PieceArray12 Create_Pieces()
        {
            PieceArray12 values = default;
            Piece* value = (Piece*)&values;
            *value++ = W_PAWN;
            *value++ = W_KNIGHT;
            *value++ = W_BISHOP;
            *value++ = W_ROOK;
            *value++ = W_QUEEN;
            *value++ = W_KING;
            *value++ = B_PAWN;
            *value++ = B_KNIGHT;
            *value++ = B_BISHOP;
            *value++ = B_ROOK;
            *value++ = B_QUEEN;
            *value = B_KING;
            return values;
        }

        private static ColorArray2 Create_Colors()
        {
            ColorArray2 values = default;
            Color* value = (Color*)&values;
            *value++ = WHITE;
            *value = BLACK;
            return values;
        }

        private static CastlingRightsArray2 Create_All_White_Castling_Rights()
        {
            CastlingRightsArray2 values = default;
            CastlingRights* value = (CastlingRights*)&values;
            *value++ = WHITE_OO;
            *value = WHITE_OOO;
            return values;
        }

        private static CastlingRightsArray2 Create_All_Black_Castling_Rights()
        {
            CastlingRightsArray2 values = default;
            CastlingRights* value = (CastlingRights*)&values;
            *value++ = BLACK_OO;
            *value = BLACK_OOO;
            return values;
        }

        private static DirectionArray8 Create_Directions()
        {
            DirectionArray8 values = default;
            Direction* value = (Direction*)&values;
            *value++ = NORTH;
            *value++ = SOUTH;
            *value++ = EAST;
            *value++ = WEST;
            *value++ = NORTH_EAST;
            *value++ = NORTH_WEST;
            *value++ = SOUTH_EAST;
            *value = SOUTH_WEST;
            return values;
        }

        private static MoveTypeArray4 Create_Move_Types()
        {
            MoveTypeArray4 values = default;
            MoveType* value = (MoveType*)&values;
            *value++ = NORMAL;
            *value++ = PROMOTION;
            *value++ = EN_PASSANT;
            *value = CASTLING;
            return values;
        }

        private static GenTypeArray4 Create_Gen_Types()
        {
            GenTypeArray4 values = default;
            GenType* value = (GenType*)&values;
            *value++ = CAPTURE;
            *value++ = QUIET;
            *value++ = NON_EVASION;
            *value = EVASION;
            return values;
        }

        private static IntArray8 Create_Knight_Steps()
        {
            IntArray8 values = default;
            int* value = (int*)&values;
            *value++ = -17;
            *value++ = -15;
            *value++ = -10;
            *value++ = -6;
            *value++ = 6;
            *value++ = 10;
            *value++ = 15;
            *value = 17;
            return values;
        }

        private static IntArray8 Create_King_Steps()
        {
            IntArray8 values = default;
            int* value = (int*)&values;
            *value++ = -9;
            *value++ = -8;
            *value++ = -7;
            *value++ = -1;
            *value++ = 1;
            *value++ = 7;
            *value++ = 8;
            *value = 9;
            return values;
        }

        private static IntArray8 Create_Seeds()
        {
            IntArray8 values = default;
            int* value = (int*)&values;
            *value++ = 728;
            *value++ = 10316;
            *value++ = 55013;
            *value++ = 32803;
            *value++ = 12281;
            *value++ = 15100;
            *value++ = 16645;
            *value = 255;
            return values;
        }
    }
    [InlineArray(16)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ValueArray16 
    {
        public const int Length = 16;
        private Value Raw;
    }
    [InlineArray(4)]
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionArray4
    {
        public const int Length = 4;
        private Direction Raw;
    }
    [InlineArray(4)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MoveTypeArray4
    {
        public const int Length = 4;
        private MoveType Raw;
    }
    [InlineArray(4)]
    [StructLayout(LayoutKind.Sequential)]
    public struct GenTypeArray4
    { 
        public const int Length = 4;
        private GenType Raw; 
    }
    [InlineArray(6)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PieceTypeArray6 
    { 
        public const int Length = 6;
        private PieceType Raw; 
    }
    [InlineArray(2)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PieceTypeArray2 
    { 
        public const int Length = 2;
        private PieceType Raw;
    }
    [InlineArray(12)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PieceArray12 
    { 
        public const int Length = 12;
        private Piece Raw; 
    }
    [InlineArray(2)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorArray2 
    {
        public const int Length = 2;
        private Color Raw; 
    }
    [InlineArray(2)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CastlingRightsArray2 
    { 
        public const int Length = 2;
        private CastlingRights Raw; 
    }
    [InlineArray(8)]
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionArray8
    { 
        public const int Length = 8;
        private Direction Raw; 
    }
    [InlineArray(8)]
    [StructLayout(LayoutKind.Sequential)]
    public struct IntArray8 
    { 
        public const int Length = 8;
        private int Raw;
    }
}
