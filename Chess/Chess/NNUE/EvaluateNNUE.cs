using static Chess.Color;
using static Chess.Types;

namespace Chess
{
    using Value = Int32;
    unsafe public static class EvaluateNNUE
    {
        public const string EvalFileDefaultNameBig = "nn-9a0cc2a62c52.nnue";
        public const string EvalFileDefaultNameSmall = "nn-47fc8b7fff06.nnue";

        private static readonly NnueFeatureTransformer Transformer = new();

        public static Value Simple_Eval(ref Position pos)
        {
            return pos.SideToMove == WHITE ? Simple_Eval<White, Black>(ref pos) : Simple_Eval<Black, White>(ref pos);
        }

        public static Value Simple_Eval<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return Chess.Evaluate.Simple_Eval<C, N>(ref pos);
        }

        public static bool Use_SmallNet(ref Position pos)
        {
            if (pos.st == null)
            {
                return false;
            }
            Value nonpawnmaterial = pos.st->NonPawnMaterial[(int)WHITE] + pos.st->NonPawnMaterial[(int)BLACK];
            return nonpawnmaterial <= QueenValue + RookValue + BishopValue;
        }

        public static Value Evaluate(ref Position pos, NnueNetworks networks, NnueAccumulatorStack accumulators = null)
        {
            return pos.SideToMove == WHITE ? Evaluate<White, Black>(ref pos, networks, accumulators) : Evaluate<Black, White>(ref pos, networks, accumulators);
        }

        public static Value Evaluate<C, N>(ref Position pos, NnueNetworks networks, NnueAccumulatorStack accumulators = null) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            if (networks == null || !networks.Can_Evaluate)
            {
                return Simple_Eval<C, N>(ref pos);
            }
            NnueAccumulatorStack stack = accumulators ?? new NnueAccumulatorStack();
            return networks.Get_Network(Use_SmallNet(ref pos)).Evaluate(ref pos, stack, Transformer);
        }

        public static string Trace(ref Position pos, NnueNetworks networks)
        {
            if (networks == null || !networks.Can_Evaluate)
            {
                return $"NNUE unavailable{Environment.NewLine}Fallback: {Simple_Eval(ref pos)}";
            }
            NnueAccumulatorStack stack = new();
            NnueEvalTrace trace = networks.Get_Network(Use_SmallNet(ref pos)).Trace_Evaluate(ref pos, stack, Transformer);
            return NnueMisc.Format_Trace(trace);
        }
    }
}
