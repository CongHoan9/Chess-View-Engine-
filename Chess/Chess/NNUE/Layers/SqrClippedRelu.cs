namespace Chess
{
    public sealed class SqrClippedRelu
    {
        public void Propagate(ReadOnlySpan<float> input, Span<float> output)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                float value = Math.Clamp(input[i], 0f, 1f);
                output[i] = value * value;
            }
        }
    }
}
