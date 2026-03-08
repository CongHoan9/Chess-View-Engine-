namespace Chess
{
    public interface IPawnOffset
    {
        static abstract Direction Value { get; }
        static abstract ulong Mask { get; }
    }
}
