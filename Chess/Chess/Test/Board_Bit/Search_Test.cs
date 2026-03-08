using System.Diagnostics;

namespace Chess
{
    public static class Search_Test
    {
        public static string FEN => "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private readonly static Stopwatch stopwatch = new();
        public static void Run_All_Tests(string fen, int perft = 6, int alphabeta = 14, int root = 12)
        {
            if (perft > 0)
            {
                Perft_Test(fen, perft, true);
            }
            if (alphabeta > 0)
            {
                AlphaBeta_Test(fen, alphabeta);
            }
            if (root > 0)
            {
                Root_Test(fen, root);
            }
        }
        public static void Perft_Test(string fen, int depth, bool divide = false)
        {
            Console.WriteLine($"Perft - depth: {depth}");
            stopwatch.Restart();
            ulong perftnodes = Search.Perft(new Board_Bit(fen), depth, divide);
            stopwatch.Stop();
            double perftseconds = stopwatch.Elapsed.TotalSeconds;
            double perftnps = perftnodes / perftseconds;
            Console.WriteLine($"\nNodes: {perftnodes:N0}");
            Console.WriteLine($"Time: {perftseconds} s");
            Console.WriteLine($"Perft Speed: {perftnps:N0} nodes/s");
        }
        public static void AlphaBeta_Test(string fen, int depth)
        {
            Console.WriteLine($"\nAlphaBeta - depth: {depth}");
            stopwatch.Restart();
            int bestscore = Search.AlphaBeta(new Board_Bit(fen), depth);
            stopwatch.Stop();
            double abseconds = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Best score: {bestscore}");
            Console.WriteLine($"Time: {abseconds} s");
        }
        public static void Root_Test(string fen, int depth)
        {
            Console.WriteLine($"\nRoot - depth: {depth}");
            stopwatch.Restart();
            Move bestmove = Search.Root(new Board_Bit(fen), depth);
            stopwatch.Stop();
            double rootSeconds = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Best move: {bestmove}");
            Console.WriteLine($"Time: {rootSeconds} s");
        }
    }
}
