namespace Chess
{
    public sealed class PRNG(ulong seed)
    {
        private ulong seed = seed;
        public ulong Rand64()
        {
            seed ^= seed >> 12;
            seed ^= seed << 25;
            seed ^= seed >> 27;
            return seed * 2685821657736338717UL;
        }
        public ulong SparseRand()
        {
            return Rand64() & Rand64() & Rand64();
        }
    }
}
