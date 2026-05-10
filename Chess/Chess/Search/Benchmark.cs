using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Chess.Color;
namespace Chess
{
    using Depth = Int32;
    using Nodes = UInt64;
    public static class Benchmark
    {
        private static readonly Stopwatch Watch = new();
        public static void Perft(string fen, Depth depth, bool isChess960)
        {
            StateInfo st = new();
            Position p = new();
            unsafe { p.Set(fen, isChess960, &st); }
            Console.WriteLine();
            Watch.Restart();
            Nodes totalNodes = p.SideToMove == WHITE
                ? RunPerft<True, White, Black>(ref p, depth, 0)
                : RunPerft<True, Black, White>(ref p, depth, 0);
            Watch.Stop();
            double sec = Math.Max(0.001, Watch.Elapsed.TotalSeconds);
            Console.WriteLine();
            Console.WriteLine($"Nodes: {totalNodes}");
            Console.WriteLine($"Time : {sec:F3} s");
            Console.WriteLine($"Speed: {totalNodes / sec:F0} nps\n");
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Nodes RunPerft<Root, C, N>(ref Position pos, Depth depth, double lastTime) where Root : struct, IBool where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Nodes nodes = 0;
            MoveList<Legal, C, N> moves = new(ref pos);
            foreach (Move move in moves)
            {
                Nodes count;
                if (depth <= 1)
                {
                    count = 1;
                }
                else
                {
                    StateInfo st = new();
                    pos.Do_Move<C, N>(move, ref st);
                    count = (depth == 2)
                        ? (Nodes)new MoveList<Legal, N, C>(ref pos).Size()
                        : RunPerft<False, N, C>(ref pos, depth - 1, 0);

                    pos.Undo_Move<C, N>(move);
                }
                nodes += count;
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
    }
}