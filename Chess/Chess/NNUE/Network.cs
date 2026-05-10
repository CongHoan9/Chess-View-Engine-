using System.IO;
using IOFile = System.IO.File;

namespace Chess
{
    using Value = Int32;
    public sealed class NnueNetwork(NnueEvalFile evalfile, NnueArchitecture architecture)
    {
        public NnueEvalFile EvalFile { get; } = evalfile;
        public NnueArchitecture Architecture { get; } = architecture;
        public bool Initialized { get; private set; }
        public bool Load(string filename)
        {
            if (!IOFile.Exists(filename))
            {
                EvalFile.Current = filename;
                return false;
            }
            using BinaryReader reader = new(IOFile.OpenRead(filename));
            uint version = reader.ReadUInt32();
            if (version != NnueCommon.Version)
            {
                return false;
            }
            EvalFile.Current = filename;
            EvalFile.NetDescription = NnueCommon.Read_String(reader);
            Architecture.Load(reader);
            Initialized = true;
            return true;
        }
        public bool Save(string filename)
        {
            string directoryname = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(directoryname))
            {
                Directory.CreateDirectory(directoryname);
            }
            using BinaryWriter writer = new(IOFile.Create(filename));
            writer.Write(NnueCommon.Version);
            NnueCommon.Write_String(writer, EvalFile.NetDescription);
            Architecture.Save(writer);
            EvalFile.Current = filename;
            return true;
        }
        public Value Evaluate(ref Position pos, NnueAccumulatorStack accumulators, NnueFeatureTransformer transformer)
        {
            NnueAccumulator accumulator = accumulators.Refresh(ref pos, transformer);
            bool usesmall = Architecture.TransformedFeatureDimensions == NnueCommon.TransformedFeatureDimensionsSmall;
            IReadOnlyList<int> active = usesmall ? accumulator.SmallActive : accumulator.BigActive;
            Value psqt = usesmall ? accumulator.SmallPsqt : accumulator.BigPsqt;
            float raw = Architecture.Propagate(active, psqt);
            return (int)Math.Round(raw);
        }
        public NnueEvalTrace Trace_Evaluate(ref Position pos, NnueAccumulatorStack accumulators, NnueFeatureTransformer transformer)
        {
            NnueAccumulator accumulator = accumulators.Refresh(ref pos, transformer);
            bool usesmall = Architecture.TransformedFeatureDimensions == NnueCommon.TransformedFeatureDimensionsSmall;
            IReadOnlyList<int> active = usesmall ? accumulator.SmallActive : accumulator.BigActive;
            Value psqt = usesmall ? accumulator.SmallPsqt : accumulator.BigPsqt;
            Value total = (Value)Math.Round(Architecture.Propagate(active, psqt));
            return new NnueEvalTrace(psqt, total - psqt, usesmall, EvalFile.NetDescription);
        }
    }

    public sealed class NnueNetworks
    {
        public NnueNetwork Big { get; }
        public NnueNetwork Small { get; }

        public bool Can_Evaluate => Big.Initialized || Small.Initialized;

        public NnueNetworks()
        {
            Big = new NnueNetwork(new NnueEvalFile(EvaluateNNUE.EvalFileDefaultNameBig), NnueArchitecture.Create_Big());
            Small = new NnueNetwork(new NnueEvalFile(EvaluateNNUE.EvalFileDefaultNameSmall), NnueArchitecture.Create_Small());
        }

        public NnueNetwork Get_Network(bool usesmall)
        {
            return usesmall && Small.Initialized ? Small : Big;
        }
    }
}
