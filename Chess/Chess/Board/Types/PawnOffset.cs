namespace Chess
{
    public interface IPawnOffset
    {
        public static abstract EDirection Offset { get; }
        public static abstract SBitBoard Shift(SBitBoard bb);
    }
}
