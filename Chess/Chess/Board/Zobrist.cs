namespace Chess
{
    public static class Zobrist
    {
        public static SKey[][] Psq { get; } = [.. Enumerable.Range(0, (int)EPiece.PieceNB).Select(_ => new SKey[(int)ESquare.SquareNB])];
        public static SKey[] EnPassant { get; } = new SKey[(int)EFile.FileNB];
        public static SKey[] Castling { get; } = new SKey[(int)ECastlingRights.CastlingRightNB];
        public static SKey Side { get; set; }
        public static SKey NoPawns { get; set; }
        static Zobrist()
        {
            PRNG rng = new(1070372);
            for (EPiece pc = 0; pc < EPiece.PieceNB; pc++)
            {
                for (ESquare sq = 0; sq < ESquare.SquareNB; sq++)
                {
                    Psq[(int)pc][(int)sq] = rng.Rand64();
                }    
            }
            for (EFile f = 0; f < EFile.FileNB; f++)
            {
                EnPassant[(int)f] = rng.Rand64();
            }
            for (ECastlingRights c = 0; c < ECastlingRights.CastlingRightNB; c++)
            {
                Castling[(int)c] = rng.Rand64();
            }
            Side = rng.Rand64();
            NoPawns = rng.Rand64();
        }
    }
}
