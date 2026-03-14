namespace Chess
{
    public enum CastlingRights : int
    {
        NO_CASTLING = 0,
        WHITE_OO = 1 << 0,
        WHITE_OOO = 1 << 1,
        BLACK_OO = 1 << 2,
        BLACK_OOO = 1 << 3,
        KING_SIDE = WHITE_OO | BLACK_OO,
        QUEEN_SIDE = WHITE_OOO | BLACK_OOO,
        WHITE_CASLING = WHITE_OO | WHITE_OOO,
        BLACK_CASLING = BLACK_OO | BLACK_OOO,
        ANY_CASTLING = WHITE_CASLING | BLACK_CASLING,
        CASTLING_RIGHT_NB = 16
    }
}
