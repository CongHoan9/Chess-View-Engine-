using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Types;
namespace Chess
{
    using Value = Int32;
    using Key = UInt64;
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct StateInfo()
    {
        public Key MaterialKey;
        public Key PawnKey;
        public Key MinorPieceKey;
        public NonPawnKey NonPawnKey;
        public NonPawnMaterial NonPawnMaterial;
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
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnKey
    {
        private Key Raw;
    }
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnMaterial
    {
        private Value Raw;
    }
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockersForKing
    {
        private Bitboard Raw;
    }
    [InlineArray((int)COLOR_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Pinners
    {
        private Bitboard Raw;
    }
    [InlineArray((int)PIECE_TYPE_NB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CheckSquares
    {
        private Bitboard Raw;
    }
}