using static Chess.Benchmark;
using static Chess.Color;

namespace Chess
{
    using Depth = Int32;
    using Nodes = UInt64;
    public static class Perft
    {
        public static void Report(string fen, Depth depth, bool isChess960 = false)
        {
            Perft(fen, depth, true);
        }
    }
}
