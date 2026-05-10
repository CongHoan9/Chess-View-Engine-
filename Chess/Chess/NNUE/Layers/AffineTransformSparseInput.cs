using System.IO;

namespace Chess
{
    public sealed class AffineTransformSparseInput(int inputdimensions, int outputdimensions, int seed)
    {
        private readonly float[] Biases = Create_Biases(outputdimensions, seed);
        private int Seed = seed;

        public int InputDimensions { get; } = inputdimensions;
        public int OutputDimensions { get; } = outputdimensions;

        private static float[] Create_Biases(int outputdimensions, int seed)
        {
            float[] biases = new float[outputdimensions];
            for (int i = 0; i < outputdimensions; ++i)
            {
                biases[i] = NnueCommon.Hashed_Value(seed, i) / 8f;
            }
            return biases;
        }

        public void Propagate(IReadOnlyList<int> activeindices, Span<float> output)
        {
            Biases.AsSpan().CopyTo(output);
            foreach (int active in activeindices)
            {
                int feature = Math.Abs(active % Math.Max(InputDimensions, 1));
                for (int o = 0; o < OutputDimensions; ++o)
                {
                    output[o] += NnueCommon.Hashed_Value(Seed, feature, o) / 8f;
                }
            }
        }

        public void Load(BinaryReader reader)
        {
            Seed = reader.ReadInt32();
            for (int i = 0; i < Biases.Length; ++i)
            {
                Biases[i] = reader.ReadSingle();
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Seed);
            for (int i = 0; i < Biases.Length; ++i)
            {
                writer.Write(Biases[i]);
            }
        }
    }
}
