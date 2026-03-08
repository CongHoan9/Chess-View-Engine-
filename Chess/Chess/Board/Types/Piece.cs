using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public interface IPieceType
    {
        public static abstract PieceType Type { get; }
    }
    public interface IPieceTypes
    {
        public static abstract BitBoard Get(BitBoard[] bb);
    }
    public enum PieceType : int
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
    public enum Piece : int
    {
        NoPiece,
        WPawn = 1, WKnight, WBishop, WRook, WQueen, WKing,
        BPawn = 9, BKnight, BBishop, BRook, BQueen, BKing,
        PieceNB = 16
    }
}
