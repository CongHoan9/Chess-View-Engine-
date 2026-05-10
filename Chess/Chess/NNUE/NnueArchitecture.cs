using System.IO;

namespace Chess
{
    using Value = Int32;
    public sealed class NnueArchitecture(string name, int transformedfeaturedimensions, int hidden0, int hidden1, bool usethreats, int seed)
    {
        public string Name { get; } = name;
        public int TransformedFeatureDimensions { get; } = transformedfeaturedimensions;
        public int Hidden0 { get; } = hidden0;
        public int Hidden1 { get; } = hidden1;
        public bool UseThreats { get; } = usethreats;
        public AffineTransformSparseInput Fc_0 { get; } = new(transformedfeaturedimensions, hidden0, seed + 1);
        public SqrClippedRelu Ac_Sqr_0 { get; } = new();
        public ClippedRelu Ac_0 { get; } = new();
        public AffineTransform Fc_1 { get; } = new(hidden0 * 2, hidden1, seed + 2);
        public ClippedRelu Ac_1 { get; } = new();
        public AffineTransform Fc_2 { get; } = new(hidden1, 1, seed + 3);

        public float Propagate(IReadOnlyList<int> activeindices, Value psqt)
        {
            float[] fc0 = new float[Hidden0];
            float[] sqr = new float[Hidden0];
            float[] relu = new float[Hidden0];
            float[] combined = new float[Hidden0 * 2];
            float[] fc1 = new float[Hidden1];
            float[] act1 = new float[Hidden1];
            float[] fc2 = new float[1];

            Fc_0.Propagate(activeindices, fc0);
            Ac_Sqr_0.Propagate(fc0, sqr);
            Ac_0.Propagate(fc0, relu);
            sqr.AsSpan().CopyTo(combined);
            relu.AsSpan().CopyTo(combined.AsSpan(Hidden0));
            Fc_1.Propagate(combined, fc1);
            Ac_1.Propagate(fc1, act1);
            Fc_2.Propagate(act1, fc2);

            return fc2[0] * 600f + psqt;
        }

        public void Load(BinaryReader reader)
        {
            Fc_0.Load(reader);
            Fc_1.Load(reader);
            Fc_2.Load(reader);
        }

        public void Save(BinaryWriter writer)
        {
            Fc_0.Save(writer);
            Fc_1.Save(writer);
            Fc_2.Save(writer);
        }

        public static NnueArchitecture Create_Big()
        {
            return new NnueArchitecture("Big", NnueCommon.TransformedFeatureDimensionsBig, NnueCommon.L2Big, NnueCommon.L3, true, 1031);
        }

        public static NnueArchitecture Create_Small()
        {
            return new NnueArchitecture("Small", NnueCommon.TransformedFeatureDimensionsSmall, NnueCommon.L2Small, NnueCommon.L3, false, 2063);
        }
    }
}
