using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Chess.Types;

namespace Chess
{
    using Depth = Int32;
    public static class Benchmark
    {
        private static Stopwatch Watch { get; set; } = new();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static ulong Perft<B, C>(Position pos, Depth depth) where B : struct, IBool where C : struct, IColor
        {
            ulong nodes = 0;
            bool leaf = depth == 2;
            var moves = new MoveList<Legal, C>(pos);
            double lastTime = Watch.Elapsed.TotalSeconds;
            foreach (Move m in moves)
            {
                StateInfo st = new();
                ulong cnt;
                if (depth == 1)
                {
                    cnt = 1;
                }
                else
                {
                    //Console.WriteLine("Do_Move start");
                    pos.Do_Move<C>(m, ref st);
                    //Console.WriteLine("Do_Move complete");
                    if (leaf)
                    {
                        cnt = (ulong)new MoveList<Legal, C>(pos).Size();
                    }
                    else
                    {
                        cnt = C.Us == WHITE ? Perft<False, Black>(pos, depth - 1) : Perft<False, White>(pos, depth - 1);
                    }
                    pos.Undo_Move<C>(m);
                }
                nodes += cnt;
                if (B.Value)
                {
                    double now = Watch.Elapsed.TotalSeconds;
                    double delta = now - lastTime;
                    lastTime = now;
                    Console.WriteLine($"{m}\t{cnt:N0}\t{delta:F3}\t{cnt / delta:N0}\t");
                }
            }
            return nodes;
        }
        unsafe public static void Perft(string fen, Depth depth, bool isChess960)
        {
            StateInfo st = new();
            Position p = new();
            p.Set(fen, isChess960, &st);
            Watch = Stopwatch.StartNew();
            Console.WriteLine($"Move\tNodes\t\tTime\tNodes/s");
            ulong nodes = Perft<True, White>(p, depth);
            Watch.Stop();
            double seconds = Watch.Elapsed.TotalSeconds;
            Console.WriteLine($"\nNodes: {nodes:N0}");
            Console.WriteLine($"Time: {seconds:F3} s");
            Console.WriteLine($"Speed: {nodes / seconds:N0} nodes/s");
        }
    }
}
