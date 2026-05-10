using static Chess.Benchmark;
using static Chess.Color;

namespace Chess
{
    using Depth = Int32;
    using Nodes = UInt64;
    public static class Perft
    {
        public static Nodes Count(ref Position pos, Depth depth)
        {
            return pos.SideToMove == WHITE ? Count<White, Black>(ref pos, depth) : Count<Black, White>(ref pos, depth);
        }

        public static Nodes Count<C, N>(ref Position pos, Depth depth) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return RunPerft<True, C, N>(ref pos, depth, 0);
        }

        unsafe public static Nodes Run(string fen, Depth depth, bool isChess960 = false)
        {
            StateInfo state = new();
            Position pos = new();
            pos.Set(fen, isChess960, &state);
            return Count(ref pos, depth);
        }

        public static void Report(string fen, Depth depth, bool isChess960 = false)
        {
            Perft(fen, depth, true);
        }
    }
}
