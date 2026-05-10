using static Chess.CastlingRights;
using static Chess.Square;
using static Chess.Piece;
using static Chess.File;
namespace Chess
{
    using Key = UInt64;
    public static class Zobrist
    {
        public static Psq Psq { get; } = new();
        public static EnPassant EnPassant { get; } = new();
        public static Castling Castling { get; } = new();
        public static Key Side { get; set; }
        public static Key NoPawns { get; set; }
        static Zobrist()
        {
            PRNG rng = new(1070372);
            for (Piece pc = 0; pc < PIECE_NB; pc++)
            {
                for (Square sq = 0; sq < SQ_NB; sq++)
                {
                    Psq[(int)pc, (int)sq] = rng.Rand64();
                }    
            }
            for (File f = 0; f < FILE_NB; f++)
            {
                EnPassant[(int)f] = rng.Rand64();
            }
            for (CastlingRights c = 0; c < CASTLING_RIGHT_NB; c++)
            {
                Castling[(int)c] = rng.Rand64();
            }
            Side = rng.Rand64();
            NoPawns = rng.Rand64();
        }
    }
}
