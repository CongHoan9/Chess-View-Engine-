namespace Chess
{
    using Value = Int32;
    public sealed class NnueAccumulator
    {
        public readonly List<int> BigActive = new(256);
        public readonly List<int> SmallActive = new(128);
        public Value BigPsqt;
        public Value SmallPsqt;
    }

    public sealed class NnueAccumulatorCaches
    {
        public static void Clear()
        {
        }
    }

    public sealed class NnueAccumulatorStack
    {
        private readonly NnueAccumulator CurrentAccumulator = new();

        public NnueAccumulator Current()
        {
            return CurrentAccumulator;
        }

        public NnueAccumulator Refresh(ref Position pos, NnueFeatureTransformer transformer)
        {
            transformer.Transform(ref pos, false, CurrentAccumulator.BigActive, out CurrentAccumulator.BigPsqt);
            transformer.Transform(ref pos, true, CurrentAccumulator.SmallActive, out CurrentAccumulator.SmallPsqt);
            return CurrentAccumulator;
        }

        public void Reset()
        {
            CurrentAccumulator.BigActive.Clear();
            CurrentAccumulator.SmallActive.Clear();
            CurrentAccumulator.BigPsqt = 0;
            CurrentAccumulator.SmallPsqt = 0;
        }
    }
}
