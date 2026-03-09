namespace Chess
{
    public enum EColor : int
    {
        White,
        Black,
        NoColor,
        ColorNB = 2
    }
    public interface IColor
    {
        public static abstract EColor Us { get; }
        public static abstract EColor Them { get; }
        public static abstract EDirection Up { get; }
        public static abstract SBitBoard Rank3BB { get; }
        public static abstract SBitBoard Rank7BB { get; }
        public static abstract EDirection UpLeft { get; }
        public static abstract EDirection UpRight { get; }
        public static abstract ECastlingRights KingSide { get; }
        public static abstract ECastlingRights QueenSide { get; }
        public static abstract ECastlingRights CastlingRights { get; }
        public static abstract ECastlingRights[] AllCastlingRights { get; }
        public static abstract SBitBoard PawnUp(SBitBoard bb);
        public static abstract SBitBoard PawnUpRight(SBitBoard bb);
        public static abstract SBitBoard PawnUpLeft(SBitBoard bb);
        public static abstract SBitBoard PawnDoubleUp(SBitBoard bb);
    }
}
