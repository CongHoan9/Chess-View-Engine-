namespace Chess
{
    public sealed class ClippedRelu
    {
        public void Propagate(ReadOnlySpan<float> input, Span<float> output)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = Math.Clamp(input[i], 0f, 1f);
            }
        }
    }
}
