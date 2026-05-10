namespace Chess
{
    public static class Simd
    {
        public static float Dot(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
        {
            float sum = 0;
            for (int i = 0; i < left.Length; ++i)
            {
                sum += left[i] * right[i];
            }
            return sum;
        }
    }
}
