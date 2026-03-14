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
        EAST = 1,
        SOUTH = -NORTH,
        WEST = -EAST,
        NORTH_EAST = NORTH + EAST,
        SOUTH_EAST = SOUTH + EAST,
        SOUTH_WEST = SOUTH + WEST,
        NORTH_WEST = NORTH + WEST,
        NORTH_NORTH = NORTH + NORTH,
        SOUTH_SOUTH = SOUTH + SOUTH,
    }
}
