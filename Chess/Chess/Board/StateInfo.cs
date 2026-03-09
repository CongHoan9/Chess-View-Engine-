using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct StateInfo()
    {
        public SKey MaterialKey;
        public SKey PawnKey;
        public SKey MinorPieceKey;
        public NonPawnKey NonPawnKey;
        public NonPawnMaterial NonPawnMaterial;
        public int CastlingRights;
        public int Rule50;
        public int PliesFromNull;
        public ESquare EpSquare;
        public SKey Key;
        public SBitBoard CheckersBB;
        public StateInfo* Previous;
        public BlockersForKing BlockersForKing;
        public Pinners Pinners;
        public CheckSquares CheckSquares;
        public EPiece CapturedPiece;
        public int Repetition;
    }
    [InlineArray((int)EColor.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnKey
    {
        private SKey Raw;
    }
    [InlineArray((int)EColor.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct NonPawnMaterial
    {
        private SValue Raw;
    }
    [InlineArray((int)EColor.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockersForKing
    {
        private SBitBoard Raw;
    }
    [InlineArray((int)EColor.ColorNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Pinners
    {
        private SBitBoard Raw;
    }
    [InlineArray((int)EPieceType.PieceTypeNB)]
    [StructLayout(LayoutKind.Sequential)]
    public struct CheckSquares
    {
        private SBitBoard Raw;
    }
}