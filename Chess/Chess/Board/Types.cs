using System.Runtime.CompilerServices;
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

        public const PieceType PIECE_TYPE_NB = PieceType.PIECE_TYPE_NB;
        public const PieceType ALL_PIECE = PieceType.ALL_PIECES;
        public const PieceType PAWN = PieceType.PAWN;
        public const PieceType KNIGHT = PieceType.KNIGHT;
        public const PieceType BISHOP = PieceType.BISHOP;
        public const PieceType ROOK = PieceType.ROOK;
        public const PieceType QUEEN = PieceType.QUEEN;
        public const PieceType KING = PieceType.KING;
        public static readonly PieceType[] PieceTypes = [PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING];
        public static readonly PieceType[] Slider = [BISHOP, ROOK];

        public const Piece PIECE_NB = Piece.PIECE_NB;
        public const Piece NO_PIECE = Piece.NO_PIECE;
        public const Piece W_PAWN = Piece.W_PAWN;
        public const Piece W_KNIGHT = Piece.W_KNIGHT;
        public const Piece W_BISHOP = Piece.W_BISHOP;
        public const Piece W_ROOK = Piece.W_ROOK;
        public const Piece W_QUEEN = Piece.W_QUEEN;
        public const Piece W_KING = Piece.W_KING;
        public const Piece B_PAWN = Piece.B_PAWN;
        public const Piece B_KNIGHT = Piece.B_KNIGHT;
        public const Piece B_BISHOP = Piece.B_BISHOP;
        public const Piece B_ROOK = Piece.B_ROOK;
        public const Piece B_QUEEN = Piece.B_QUEEN;
        public const Piece B_KING = Piece.B_KING;
        public static readonly Piece[] Pieces = 
        [
            W_PAWN, W_KNIGHT, W_BISHOP, W_ROOK, W_QUEEN, W_KING,
            B_PAWN, B_KNIGHT, B_BISHOP, B_ROOK, B_QUEEN, B_KING
        ];

        public const Color COLOR_NB = Color.COLOR_NB;
        public const Color WHITE = Color.WHITE;
        public const Color BLACK = Color.BLACK;
        public static readonly Color[] Colors = [WHITE, BLACK];

        public const Direction NORTH = Direction.NORTH;
        public const Direction SOUTH = Direction.SOUTH;
        public const Direction EAST = Direction.EAST;
        public const Direction WEST = Direction.WEST;
        public const Direction NORTH_EAST = Direction.NORTH_EAST;
        public const Direction NORTH_WEST = Direction.NORTH_WEST;
        public const Direction SOUTH_EAST = Direction.SOUTH_EAST;
        public const Direction SOUTH_WEST = Direction.SOUTH_WEST;
        public static readonly Direction[] Directions = [NORTH, SOUTH, EAST, WEST, NORTH_EAST, NORTH_WEST, SOUTH_EAST, SOUTH_WEST];
        
        public const MoveType NORMAL = MoveType.NORMAL;
        public const MoveType PROMOTION = MoveType.PROMOTION;
        public const MoveType EN_PASSANT = MoveType.EN_PASSANT;
        public const MoveType CASTLING = MoveType.CASTLING;
        public static readonly MoveType[] MoveTypes = [NORMAL, PROMOTION, EN_PASSANT, CASTLING];
        
        public const GenType CAPTURE = GenType.CAPTURE;
        public const GenType QUIET = GenType.QUIET;
        public const GenType EVASION = GenType.EVASION;
        public const GenType NON_EVASION = GenType.NON_EVASION;
        public const GenType LEGAL = GenType.LEGAL;
        public static readonly GenType[] GenTypes = [CAPTURE, QUIET, NON_EVASION, EVASION];
        
        public const CastlingRights NO_CASTLING = CastlingRights.NO_CASTLING;
        public const CastlingRights WHITE_OO = CastlingRights.WHITE_OO;
        public const CastlingRights WHITE_OOO = CastlingRights.WHITE_OOO;
        public const CastlingRights BLACK_OO = CastlingRights.BLACK_OO;
        public const CastlingRights BLACK_OOO = CastlingRights.BLACK_OOO;
        public const CastlingRights KING_SIDE = CastlingRights.KING_SIDE;
        public const CastlingRights QUEEN_SIDE = CastlingRights.QUEEN_SIDE;
        public const CastlingRights WHITE_CASLING = CastlingRights.WHITE_CASLING;
        public const CastlingRights BLACK_CASLING = CastlingRights.BLACK_CASLING;
        public const CastlingRights ANY_CASTLING = CastlingRights.ANY_CASTLING;
        public const CastlingRights CASTLING_RIGHR_NB = CastlingRights.CASTLING_RIGHR_NB;
        
        public const Square SQ_NONE = Square.SQ_NONE;
        public const Square SQ_NB = Square.SQ_NB;
        public const Square SQ_A1 = Square.SQ_A1;
        public const Square SQ_A2 = Square.SQ_A2;
        public const Square SQ_A3 = Square.SQ_A3;
        public const Square SQ_A4 = Square.SQ_A4;
        public const Square SQ_A5 = Square.SQ_A5;
        public const Square SQ_A6 = Square.SQ_A6;
        public const Square SQ_A7 = Square.SQ_A7;
        public const Square SQ_A8 = Square.SQ_A8;
        public const Square SQ_B1 = Square.SQ_B1;
        public const Square SQ_B2 = Square.SQ_B2;
        public const Square SQ_B3 = Square.SQ_B3;
        public const Square SQ_B4 = Square.SQ_B4;
        public const Square SQ_B5 = Square.SQ_B5;
        public const Square SQ_B6 = Square.SQ_B6;
        public const Square SQ_B7 = Square.SQ_B7;
        public const Square SQ_B8 = Square.SQ_B8;
        public const Square SQ_C1 = Square.SQ_C1;
        public const Square SQ_C2 = Square.SQ_C2;
        public const Square SQ_C3 = Square.SQ_C3;
        public const Square SQ_C4 = Square.SQ_C4;
        public const Square SQ_C5 = Square.SQ_C5;
        public const Square SQ_C6 = Square.SQ_C6;
        public const Square SQ_C7 = Square.SQ_C7;
        public const Square SQ_C8 = Square.SQ_C8;
        public const Square SQ_D1 = Square.SQ_D1;
        public const Square SQ_D2 = Square.SQ_D2;
        public const Square SQ_D3 = Square.SQ_D3;
        public const Square SQ_D4 = Square.SQ_D4;
        public const Square SQ_D5 = Square.SQ_D5;
        public const Square SQ_D6 = Square.SQ_D6;
        public const Square SQ_D7 = Square.SQ_D7;
        public const Square SQ_D8 = Square.SQ_D8;
        public const Square SQ_E1 = Square.SQ_E1;
        public const Square SQ_E2 = Square.SQ_E2;
        public const Square SQ_E3 = Square.SQ_E3;
        public const Square SQ_E4 = Square.SQ_E4;
        public const Square SQ_E5 = Square.SQ_E5;
        public const Square SQ_E6 = Square.SQ_E6;
        public const Square SQ_E7 = Square.SQ_E7;
        public const Square SQ_E8 = Square.SQ_E8;
        public const Square SQ_F1 = Square.SQ_F1;
        public const Square SQ_F2 = Square.SQ_F2;
        public const Square SQ_F3 = Square.SQ_F3;
        public const Square SQ_F4 = Square.SQ_F4;
        public const Square SQ_F5 = Square.SQ_F5;
        public const Square SQ_F6 = Square.SQ_F6;
        public const Square SQ_F7 = Square.SQ_F7;
        public const Square SQ_F8 = Square.SQ_F8;
        public const Square SQ_G1 = Square.SQ_G1;
        public const Square SQ_G2 = Square.SQ_G2;
        public const Square SQ_G3 = Square.SQ_G3;
        public const Square SQ_G4 = Square.SQ_G4;
        public const Square SQ_G5 = Square.SQ_G5;
        public const Square SQ_G6 = Square.SQ_G6;
        public const Square SQ_G7 = Square.SQ_G7;
        public const Square SQ_G8 = Square.SQ_G8;
        public const Square SQ_H1 = Square.SQ_H1;
        public const Square SQ_H2 = Square.SQ_H2;
        public const Square SQ_H3 = Square.SQ_H3;
        public const Square SQ_H4 = Square.SQ_H4;
        public const Square SQ_H5 = Square.SQ_H5;
        public const Square SQ_H6 = Square.SQ_H6;
        public const Square SQ_H7 = Square.SQ_H7;
        public const Square SQ_H8 = Square.SQ_H8;
        
        public const File FILE_A = File.FILE_A;
        public const File FILE_B = File.FILE_B;
        public const File FILE_C = File.FILE_C;
        public const File FILE_D = File.FILE_D;
        public const File FILE_E = File.FILE_E;
        public const File FILE_F = File.FILE_F;
        public const File FILE_G = File.FILE_G;
        public const File FILE_H = File.FILE_H;
        public const File FILE_NB = File.FILE_NB;
        
        public const Rank RANK_1 = Rank.RANK_1;
        public const Rank RANK_2 = Rank.RANK_2;
        public const Rank RANK_3 = Rank.RANK_3;
        public const Rank RANK_4 = Rank.RANK_4;
        public const Rank RANK_5 = Rank.RANK_5;
        public const Rank RANK_6 = Rank.RANK_6;
        public const Rank RANK_7 = Rank.RANK_7;
        public const Rank RANK_8 = Rank.RANK_8;
        public const Rank RANK_NB = Rank.RANK_NB;

        public static readonly int[] KnightSteps = [-17, -15, -10, -6, 6, 10, 15, 17];
        public static readonly int[] KingSteps = [-9, -8, -7, -1, 1, 7, 8, 9];
    }
}