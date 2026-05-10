using System.IO;

namespace Chess
{
    public static class NnueCommon
    {
        public const uint Version = 0x7AF32F20u;
        public const int OutputScale = 16;
        public const int WeightScaleBits = 6;
        public const int CacheLineSize = 64;
        public const int TransformedFeatureDimensionsBig = 1024;
        public const int TransformedFeatureDimensionsSmall = 128;
        public const int L2Big = 32;
        public const int L2Small = 16;
        public const int L3 = 32;
        public const int PSQTBuckets = 8;
        public const int LayerStacks = 8;

        public static float Hashed_Value(int seed, int a, int b = 0, int c = 0)
        {
            unchecked
            {
                uint value = (uint)(seed * 0x9E3779B9) ^ (uint)(a * 0x85EBCA6B) ^ (uint)(b * 0xC2B2AE35) ^ (uint)(c * 0x27D4EB2F);
                value ^= value >> 16;
                value *= 0x7FEB352D;
                value ^= value >> 15;
                value *= 0x846CA68B;
                value ^= value >> 16;
                return (((value >> 11) & 2047) - 1024) / 4096f;
            }
        }

        public static void Write_String(BinaryWriter writer, string value)
        {
            writer.Write(value ?? string.Empty);
        }

        public static string Read_String(BinaryReader reader)
        {
            return reader.ReadString();
        }
    }
}
