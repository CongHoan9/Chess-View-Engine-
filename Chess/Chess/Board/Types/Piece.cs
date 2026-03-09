namespace Chess
{
    public interface IPieceType
    {
        public static abstract EPieceType Type { get; }
    }
    public interface IPieceTypes
    {
        public static abstract SBitBoard Get(SBitBoard[] bb);
    }
    public enum EPieceType : int
    {
        NoPieceType,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
        AllPieces,
        PieceTypeNB = 8
    }
    public enum EPiece : int
    {
        NoPiece,
        WPawn = 1, WKnight, WBishop, WRook, WQueen, WKing,
        BPawn = 9, BKnight, BBishop, BRook, BQueen, BKing,
        PieceNB = 16
    }
}
