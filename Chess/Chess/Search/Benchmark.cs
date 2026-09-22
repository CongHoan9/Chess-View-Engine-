using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Chess.Color;
namespace Chess
{
    using Depth = Int32;
    public static class Benchmark
    {
        private static readonly Stopwatch Watch = new();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static ulong Perft<Root, Us, Next>(ref Position pos, Depth depth, double lastTime) where Root : struct, IBool where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            StateInfo st = default;
            ulong count, nodes = 0;
            bool leaf = (depth == 2);
            foreach (Move move in new MoveList<Legal, Us, Next>(ref pos))
            {
                if (Root.Value && depth <= 1)
                {
                    count = 1; nodes++;
                }
                else
                {
                    pos.Do_Move<Us, Next>(move, ref st);
                    count = leaf ? new MoveList<Legal, Next, Us>(ref pos).Size() : Perft<False, Next, Us>(ref pos, depth - 1, 0);
                    nodes += count;
                    pos.Undo_Move<Us, Next>(move);
                }
                if (Root.Value)
                {
                    double now = Watch.Elapsed.TotalSeconds;
                    double delta = Math.Max(0.000001, now - lastTime);
                    lastTime = now;
                    string moveStr = UCI.Move_To_String(ref pos, move);
                    Console.WriteLine($"{moveStr} \t {count} \t {delta:F3} \t {count / delta:F0} nps");
                }
            }
            return nodes;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Perft(string fen, Depth depth, bool isChess960)
        {
            StateInfo st = default;
            Position p = default;
            unsafe { p.Set(fen, isChess960, &st); }
            Console.WriteLine();
            Watch.Restart();
            ulong totalNodes = p.SideToMove == WHITE ? Perft<True, White, Black>(ref p, depth, 0) : Perft<True, Black, White>(ref p, depth, 0);
            Watch.Stop();
            double sec = Math.Max(0.001, Watch.Elapsed.TotalSeconds);
            Console.WriteLine();
            Console.WriteLine($"Nodes: {totalNodes}");
            Console.WriteLine($"Time : {sec:F3} s");
            Console.WriteLine($"Speed: {totalNodes / sec:F0} nps\n");
        }
    }
}