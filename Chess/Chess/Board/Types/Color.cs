#pragma warning disable CA1069 
namespace Chess
{
    public interface IColor<C, N> where C : struct, IColor<C, N> where N : struct, IColor<N, C>
    {
        public static abstract Color Value { get; }
        public static abstract N Them { get; }
        public static abstract int Sign { get; }
        public static abstract Bitboard Rank3 { get; }
        public static abstract Bitboard Rank7 { get; }
        public static abstract Direction Up { get; }
        public static abstract Direction UpLeft { get; }
        public static abstract Direction UpRight { get; }
        public static abstract Direction Double { get; }
        public static abstract CastlingRights KingSide { get; }
        public static abstract CastlingRights QueenSide { get; }
        public static abstract CastlingRights CastlingRights { get; }
        public static abstract ref readonly CastlingRightsArray2 AllCastlingRights { get; }
        public static abstract Bitboard Pawn_Up(Bitboard bb);
        public static abstract Bitboard Pawn_Up_Right(Bitboard bb);
        public static abstract Bitboard Pawn_Up_Left(Bitboard bb);
        public static abstract Bitboard Pawn_Double_Up(Bitboard bb);
    }
    public enum Color : int
    {
        WHITE,
        BLACK, 
        NO_COLOR = 2,
        COLOR_NB = 2,
    }
}
