using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public static class Zobrist
    {
        public static Key[][] Psq { get; } = [.. Enumerable.Range(0, (int)Piece.PieceNB).Select(_ => new Key[(int)Square.SquareNB])];
        public static Key[] EnPassant { get; } = new Key[(int)File.FileNB];
        public static Key[] Castling { get; } = new Key[(int)CastlingRights.CastlingRightNB];
        public static Key Side, NoPawns;
    }
}
