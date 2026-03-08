using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public interface IDirection
    {
        static abstract int Offset { get; }
        static abstract ulong Mask { get; }
    }
    public enum Direction : int
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
