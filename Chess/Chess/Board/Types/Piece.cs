namespace Chess
{
    public interface IPieceType
    {
        public static abstract PieceType Type { get; }
    }
    public interface IPieceTypes
    {
        public static abstract Bitboard Get(Bitboard[] bb);
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
        ALL_PIECES,
        PIECE_TYPE_NB = 8,
    }
    public enum Piece : int
    {
        NO_PIECE,
        W_PAWN = 1, W_KNIGHT, W_BISHOP, W_ROOK, W_QUEEN, W_KING,
        B_PAWN = 9, B_KNIGHT, B_BISHOP, B_ROOK, B_QUEEN, B_KING,
        PIECE_NB = 16,
    }
}
