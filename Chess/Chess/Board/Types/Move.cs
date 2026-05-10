namespace Chess
{
    public interface IMoveType
    {
        static abstract MoveType Type { get; }
    }
    public enum MoveType : int
    {
        NORMAL,
        PROMOTION = 1 << 14,
        EN_PASSANT = 2 << 14,
        CASTLING = 3 << 14
    }
}
