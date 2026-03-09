namespace Chess
{
    public enum EMoveType
    {
        Normal = 0,
        Promotion = 1 << 14,
        EnPassant = 2 << 14,
        Castling = 3 << 14
    }
}
