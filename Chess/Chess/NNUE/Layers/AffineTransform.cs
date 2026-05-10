using System.IO;

namespace Chess
{
    public sealed class AffineTransform(int inputdimensions, int outputdimensions, int seed)
    {
        private readonly float[] Weights = Create_Weights(inputdimensions, outputdimensions, seed + 31);
        private readonly float[] Biases = Create_Biases(outputdimensions, seed);

        public int InputDimensions { get; } = inputdimensions;
        public int OutputDimensions { get; } = outputdimensions;

        private static float[] Create_Biases(int outputdimensions, int seed)
        {
            float[] biases = new float[outputdimensions];
            for (int o = 0; o < outputdimensions; ++o)
            {
                biases[o] = NnueCommon.Hashed_Value(seed, o) / 4f;
            }
            return biases;
        }

        private static float[] Create_Weights(int inputdimensions, int outputdimensions, int seed)
        {
            float[] weights = new float[inputdimensions * outputdimensions];
            for (int o = 0; o < outputdimensions; ++o)
            {
                for (int i = 0; i < inputdimensions; ++i)
                {
                    weights[o * inputdimensions + i] = NnueCommon.Hashed_Value(seed, o, i);
                }
            }
            return weights;
        }

        public void Propagate(ReadOnlySpan<float> input, Span<float> output)
        {
            for (int o = 0; o < OutputDimensions; ++o)
            {
                float sum = Biases[o];
                sum += Simd.Dot(input, Weights.AsSpan(o * InputDimensions, InputDimensions));
                output[o] = sum;
            }
        }

        public void Load(BinaryReader reader)
        {
            for (int o = 0; o < OutputDimensions; ++o)
            {
                Biases[o] = reader.ReadSingle();
            }
            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = reader.ReadSingle();
            }
        }

        public void Save(BinaryWriter writer)
        {
            for (int o = 0; o < OutputDimensions; ++o)
            {
                writer.Write(Biases[o]);
            }
            for (int i = 0; i < Weights.Length; ++i)
            {
                writer.Write(Weights[i]);
            }
        }
    }
}
