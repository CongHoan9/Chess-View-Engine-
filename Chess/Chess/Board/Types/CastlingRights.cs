namespace Chess
{
    public enum ECastlingRights : int
    {
        NoCastling = 0,
        WhiteOO = 1 << 0,
        WhiteOOO = 1 << 1,
        BlackOO = 1 << 2,
        BlackOOO = 1 << 3,
        KingSide = WhiteOO | BlackOO,
        QueenSide = WhiteOOO | BlackOOO,
        WhiteCastling = WhiteOO | WhiteOOO,
        BlackCastling = BlackOO | BlackOOO,
        AnyCastling = WhiteCastling | BlackCastling,
        CastlingRightNB = 16
    }
}
