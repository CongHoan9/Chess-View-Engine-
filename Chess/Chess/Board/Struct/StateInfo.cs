using System.Runtime.InteropServices;
using static Chess.Color;
namespace Chess
{
    using Key = UInt64;
    using Value = Int32;
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct StateInfo()
    {
        public Key MaterialKey;
        public Key PawnKey;
        public Key MinorPieceKey;
        public fixed Key NonPawnKey[(int)COLOR_NB];
        public fixed Value NonPawnMaterial[(int)COLOR_NB];
        public int CastlingRights;
        public int Rule50;
        public int PliesFromNull;
        public Square EpSquare;
        public Key Key;

        public Bitboard CheckersBB;
        public StateInfo* Previous;
        public BlockersForKing BlockersForKing;
        public Pinners Pinners;
        public CheckSquares CheckSquares;
        public Piece CapturedPiece;
        public int Repetition;
    }
}
