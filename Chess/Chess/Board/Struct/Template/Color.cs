using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct White : IColor
    {
        public static Color Us => Color.White;
        public static Color Them => Color.Black;
        public static BitBoard Rank2BB => BitBoards.Rank2BB;
        public static BitBoard Rank3BB => BitBoards.Rank3BB;
        public static BitBoard Rank4BB => BitBoards.Rank4BB;
        public static BitBoard Rank5BB => BitBoards.Rank5BB;
        public static BitBoard Rank6BB => BitBoards.Rank6BB;
        public static BitBoard Rank7BB => BitBoards.Rank7BB;
        public static Direction Up => Direction.North;
        public static Direction Left => Direction.NorthWest;
        public static Direction Right => Direction.NorthEast;
        public static Direction DoubleUp => (int)Up + Up;
        public static CastlingRights CastlingRights => CastlingRights.WhiteCastling;
        public static CastlingRights KingSide => CastlingRights.WhiteOO;
        public static CastlingRights QueenSide => CastlingRights.WhiteOOO;
        public static Rank RelativeRank(Rank r) => r;
        public static Square RelativeSquare(Square s) => s;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Black : IColor
    {
        public static Color Us => Color.Black;
        public static Color Them => Color.White;
        public static BitBoard Rank2BB => BitBoards.Rank7BB;
        public static BitBoard Rank3BB => BitBoards.Rank6BB;
        public static BitBoard Rank4BB => BitBoards.Rank5BB;
        public static BitBoard Rank5BB => BitBoards.Rank4BB;
        public static BitBoard Rank6BB => BitBoards.Rank3BB;
        public static BitBoard Rank7BB => BitBoards.Rank2BB;
        public static Direction Up => Direction.South;
        public static Direction Left => Direction.SouthWest;
        public static Direction Right => Direction.SouthEast;
        public static CastlingRights CastlingRights => CastlingRights.BlackCastling;
        public static CastlingRights KingSide => CastlingRights.BlackOO;
        public static CastlingRights QueenSide => CastlingRights.BlackOOO;
        public static Direction DoubleUp => (int)Up + Up;
        public static Rank RelativeRank(Rank r) => (Rank)((int)r ^ 7);
        public static Square RelativeSquare(Square s) => (Square)((int)s ^ 56);
    }
}

