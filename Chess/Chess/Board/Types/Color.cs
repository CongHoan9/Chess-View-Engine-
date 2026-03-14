namespace Chess
{
    public interface IColor
    {
        public static abstract Color Us { get; }
        public static abstract Color Them { get; }
        public static abstract Bitboard Rank3 { get; }
        public static abstract Bitboard Rank7 { get; }
        public static abstract Direction Up { get; }
        public static abstract Direction UpLeft { get; }
        public static abstract Direction UpRight { get; }
        public static abstract Direction Double { get; }
        public static abstract CastlingRights KingSide { get; }
        public static abstract CastlingRights QueenSide { get; }
        public static abstract CastlingRights CastlingRights { get; }
        public static abstract CastlingRights[] AllCastlingRights { get; }
        public static abstract Bitboard Pawn_Up(Bitboard bb);
        public static abstract Bitboard Pawn_Up_Right(Bitboard bb);
        public static abstract Bitboard Pawn_Up_Left(Bitboard bb);
        public static abstract Bitboard Pawn_Double_Up(Bitboard bb);
    }
    public enum Color : int
    {
        WHITE,
        BLACK,
        NO_COLOR = -1,
        COLOR_NB = 2,
    }
}
