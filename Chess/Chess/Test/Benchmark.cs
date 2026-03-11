using System.Windows.Documents;
using Windows.Media.Ocr;

namespace Chess
{
    public static class Benchmark
    {
        public static ulong Perft<B>(Position pos, SDepth depth) where B : struct, IBool
        {
            ulong nodes = 0;
            bool leaf = depth == 2;
            using var moves = new MoveList<Legal>(pos);
            foreach (SMove m in moves)
            {
                StateInfo st = new();
                ulong cnt;
                if (depth == 1)
                {
                    cnt = 1;
                }
                else
                {
                    pos.DoMove(m, ref st);
                    cnt = leaf ? (ulong)new MoveList<Legal>(pos).Size() : Perft<SFalse>(pos, depth - 1);
                    pos.UndoMove(m);
                }
                nodes += cnt;
                if (B.Value)
                {
                    Console.WriteLine($"Move {m}: {cnt:N0} nodes");
                }
            }
            return nodes;
        }
        unsafe public static ulong Perft(string fen, SDepth depth, bool isChess960) 
        {
            StateInfo st = new();
            Position p = new();
            p.Set(fen, isChess960, &st);
            return Perft<STrue>(p, depth);
        }
    }
}
