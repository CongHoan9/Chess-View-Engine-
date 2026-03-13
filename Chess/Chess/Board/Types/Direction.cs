namespace Chess
{
    public interface IDirection
    {
        public static abstract Direction Offset { get; }
        public static abstract Bitboard Mask { get; }
        public static abstract Bitboard Shift(Bitboard bb);
    }
    public enum Direction : int
    {
        NORTH = 8,
        SOUTH = -8,
        EAST = 1,
        WEST = -1,
        NORTH_EAST = 9,
        NORTH_WEST = 7,
        SOUTH_EAST = -7,
        SOUTH_WEST = -9,
        NORTH_NORTH = 16,
        SOUTH_SOUTH = -16
    }
}
