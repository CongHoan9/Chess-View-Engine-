using System.Formats.Tar;
using System.Runtime.CompilerServices;
using Windows.UI.StartScreen;

namespace Chess
{
    public struct TTEntry
    {
        public ulong Key;
        public int Score;
        public byte Flag;
        public sbyte Depth;
        public Move bestmove;
    }
    public static class TranspositionTable
    {
        private const int TableSize = 1 << 22;
        public static readonly uint TableMask = TableSize - 1;  
        public static readonly TTEntry[] TT = new TTEntry[TableSize];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ulong key, sbyte depth, int score, byte flag, Move best)
        {
            uint index = (uint)key ^ (uint)(key >> 32);
            index &= TableMask;
            ref TTEntry e = ref TT[index];
            if (depth >= e.Depth || e.Key != key)
            {
                e.Key = key;
                e.Flag = flag;
                e.Depth = depth;
                e.Score = score;
                e.bestmove = best;
            }
        }
    }
}
