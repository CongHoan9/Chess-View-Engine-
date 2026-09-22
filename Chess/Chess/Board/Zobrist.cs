namespace Chess
{
    using Key = UInt64;
    public static class Zobrist
    {
        public static Psq Psq { get; } = default;
        public static EnPassant EnPassant { get; } = default;
        public static Castling Castling { get; } = default;
        public static Key Side { get; set; }
        public static Key NoPawns { get; set; }
    }
}
