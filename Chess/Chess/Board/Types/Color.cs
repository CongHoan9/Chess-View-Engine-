using System;
using System.Collections.Generic;
using System.Text;
namespace Chess
{
    public enum Color : int
    {
        White,
        Black,
        NoColor,
        ColorNB = 2
    }
    public interface IColor
    {
        static abstract Color Us { get; }
        static abstract Color Them { get; }
        static abstract BitBoard Rank2BB { get; }
        static abstract BitBoard Rank3BB { get; }
        static abstract BitBoard Rank4BB { get; }
        static abstract BitBoard Rank5BB { get; }
        static abstract BitBoard Rank6BB { get; }
        static abstract BitBoard Rank7BB { get; }
        static abstract Direction Up { get; }
        static abstract Direction Left { get; }
        static abstract Direction Right { get; }
        static abstract Direction DoubleUp { get; }
        static abstract CastlingRights CastlingRights { get; }
        static abstract CastlingRights KingSide { get; }
        static abstract CastlingRights QueenSide { get; }
        static abstract Rank RelativeRank(Rank r);
        static abstract Square RelativeSquare(Square s);
    }
}
