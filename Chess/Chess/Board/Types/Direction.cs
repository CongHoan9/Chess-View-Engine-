namespace Chess
{
    public interface IDirection
    {
        static abstract EDirection Offset { get; }
        static abstract SBitBoard Mask { get; }
    }
    public enum EDirection : int
    {
        North = 8,
        East = 1,
        South = -8,
        West = -1,
        NorthEast = 9,
        NorthWest = 7,
        SouthWest = -9,
        SouthEast = -7
    }
}
