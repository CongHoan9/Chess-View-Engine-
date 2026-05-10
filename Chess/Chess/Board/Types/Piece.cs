using static Chess.PieceType;
namespace Chess
{
    public interface IPieceType
    {
        public static abstract PieceType Type { get; }
    }
    public interface IPieceTypes
    {
        public static abstract Bitboard Get(ref ByTypeBB bb);
    }
    public enum PieceType : int
    {
        NO_PIECE_TYPE, 
        PAWN, 
        KNIGHT, 
        BISHOP, 
        ROOK, 
        QUEEN, 
        KING,
        ALL_PIECES = 0,
        PIECE_TYPE_NB = 8,
    }
    public enum Piece : int
    {
        NO_PIECE,
        W_PAWN = PAWN    , W_KNIGHT, W_BISHOP, W_ROOK, W_QUEEN, W_KING,
        B_PAWN = PAWN + 8, B_KNIGHT, B_BISHOP, B_ROOK, B_QUEEN, B_KING,
        PIECE_NB = 16,
    }
}
