using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
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
        public StateInfo* Previous;

        public Key Key;
        public BitBoard CheckersBB;
        public BlockersForKing BlockersForKing;
        public Pinners Pinners;
        public CheckSquares CheckSquares;
        public Piece CapturedPiece;
        public int Repetition;
    }
    [InlineArray((int)Color.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnKey
    {
        private Key Raw;
    }
    [InlineArray((int)Color.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnMaterial
    {
        private Value Raw;
    }
    [InlineArray((int)Color.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockersForKing
    {
        private BitBoard Raw;
    }
    [InlineArray((int)Color.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Pinners
    {
        private BitBoard Raw;
    }
    [InlineArray((int)PieceType.PieceTypeNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CheckSquares
    {
        private BitBoard Raw;
    }
}