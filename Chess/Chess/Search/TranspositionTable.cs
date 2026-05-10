using static Chess.Types;

namespace Chess
{
    using Depth = Int32;
    using Key = UInt64;
    using Value = Int32;

    public enum TTBound : byte
    {
        None,
        Upper,
        Lower,
        Exact = Upper | Lower
    }

    public readonly struct TTData
    {
        public readonly Move Move;
        public readonly Value Value;
        public readonly Value Eval;
        public readonly Depth Depth;
        public readonly TTBound Bound;
        public readonly bool IsPv;

        public TTData(Move move, Value value, Value eval, Depth depth, TTBound bound, bool isPv)
        {
            Move = move;
            Value = value;
            Eval = eval;
            Depth = depth;
            Bound = bound;
            IsPv = isPv;
        }
    }

    public struct TTEntry
    {
        public Key Key;
        public Move BestMove;
        public Value Score;
        public Value Eval;
        public sbyte Depth;
        public byte Flag;
        public bool IsPv;
    }

    public static class TranspositionTable
    {
        public const byte TT_FLAG_NONE = (byte) TTBound.None;
        public const byte TT_FLAG_UPPER = (byte) TTBound.Upper;
        public const byte TT_FLAG_LOWER = (byte) TTBound.Lower;
        public const byte TT_FLAG_EXACT = (byte) TTBound.Exact;

        private const int ClusterSize = 3;
        private const int ClusterByteSize = 32;
        private const int GenerationBits = 3;
        private const int GenerationDelta = 1 << GenerationBits;
        private const int GenerationCycle = 255 + GenerationDelta;
        private const int GenerationMask = (0xFF << GenerationBits) & 0xFF;
        private const int DepthEntryOffset = -3;

        private static TTSlot[] Table = [];
        private static int ClusterCount;
        private static byte Generation8;

        static TranspositionTable()
        {
            Resize(16);
        }

        public static void Resize(int mbSize)
        {
            long bytes = Math.Max(1, mbSize) * 1024L * 1024L;
            ClusterCount = (int) Math.Max(1L, bytes / ClusterByteSize);
            Table = new TTSlot[checked(ClusterCount * ClusterSize)];
            Clear();
        }

        public static void Clear()
        {
            Generation8 = 0;
            Array.Clear(Table, 0, Table.Length);
        }

        public static int HashFull(int maxAge = 0)
        {
            if (ClusterCount == 0)
                return 0;

            int maxAgeInternal = maxAge << GenerationBits;
            int sampledClusters = Math.Min(1000, ClusterCount);
            int occupied = 0;

            for (int i = 0; i < sampledClusters; ++i)
            {
                int baseIndex = i * ClusterSize;
                for (int j = 0; j < ClusterSize; ++j)
                {
                    ref readonly TTSlot slot = ref Table[baseIndex + j];
                    if (slot.IsOccupied() && slot.RelativeAge(Generation8) <= maxAgeInternal)
                        occupied++;
                }
            }

            return sampledClusters == 0 ? 0 : occupied * 1000 / (sampledClusters * ClusterSize);
        }

        public static void New_Search()
        {
            Generation8 += GenerationDelta;
        }

        public static bool Probe(Key key, out TTEntry entry)
        {
            int    baseIndex = FirstEntryIndex(key);
            ushort key16     = (ushort) key;

            for (int i = 0; i < ClusterSize; ++i)
            {
                ref readonly TTSlot slot = ref Table[baseIndex + i];
                if (slot.Key16 == key16)
                {
                    entry = To_Public_Entry(key, slot.Read());
                    return slot.IsOccupied();
                }
            }

            entry = default;
            return false;
        }

        public static Move Probe_Best(Key key)
        {
            return Probe(key, out TTEntry entry) ? entry.BestMove : Move.None();
        }

        public static void Store(Key key, sbyte depth, Value score, byte flag, Move bestmove)
        {
            Store(key, depth, score, flag, bestmove, VALUE_NONE, false);
        }

        public static void Store(Key key,
                                 sbyte depth,
                                 Value score,
                                 byte flag,
                                 Move bestmove,
                                 Value eval,
                                 bool isPv)
        {
            int    baseIndex = FirstEntryIndex(key);
            ushort key16     = (ushort) key;
            TTBound bound    = (TTBound) (flag & 0x3);

            for (int i = 0; i < ClusterSize; ++i)
            {
                ref TTSlot slot = ref Table[baseIndex + i];
                if (slot.Key16 == key16)
                {
                    slot.Save(key, score, isPv, bound, depth, bestmove, eval, Generation8);
                    return;
                }
            }

            int replaceIndex = baseIndex;
            for (int i = 1; i < ClusterSize; ++i)
            {
                ref readonly TTSlot best = ref Table[replaceIndex];
                ref readonly TTSlot cur  = ref Table[baseIndex + i];

                int bestValue = best.Depth8 - best.RelativeAge(Generation8);
                int curValue  = cur.Depth8 - cur.RelativeAge(Generation8);
                if (bestValue > curValue)
                    replaceIndex = baseIndex + i;
            }

            Table[replaceIndex].Save(key, score, isPv, bound, depth, bestmove, eval, Generation8);
        }

        private static TTEntry To_Public_Entry(Key key, TTData data)
        {
            return new TTEntry
            {
                Key = key,
                BestMove = data.Move,
                Score = data.Value,
                Eval = data.Eval,
                Depth = (sbyte) data.Depth,
                Flag = (byte) data.Bound,
                IsPv = data.IsPv
            };
        }

        private static int FirstEntryIndex(Key key)
        {
            return checked((int) (Mul_Hi64(key, (ulong) ClusterCount) * ClusterSize));
        }

        private static ulong Mul_Hi64(ulong a, ulong b)
        {
            return (ulong) (((UInt128) a * b) >> 64);
        }

        private struct TTSlot
        {
            public ushort Key16;
            public byte Depth8;
            public byte GenBound8;
            public Move Move16;
            public short Value16;
            public short Eval16;

            public readonly TTData Read()
            {
                return new TTData(Move16,
                                  Value16,
                                  Eval16,
                                  Depth8 + DepthEntryOffset,
                                  (TTBound) (GenBound8 & 0x3),
                                  (GenBound8 & 0x4) != 0);
            }

            public readonly bool IsOccupied()
            {
                return Depth8 != 0;
            }

            public void Save(Key key,
                             Value value,
                             bool pv,
                             TTBound bound,
                             Depth depth,
                             Move move,
                             Value eval,
                             byte generation8)
            {
                if (move != Move.None() || (ushort) key != Key16)
                    Move16 = move;

                if (bound == TTBound.Exact
                    || (ushort) key != Key16
                    || depth - DepthEntryOffset + (pv ? 2 : 0) > Depth8 - 4
                    || RelativeAge(generation8) != 0)
                {
                    depth = Math.Clamp(depth, DepthEntryOffset + 1, 255 + DepthEntryOffset);

                    Key16 = (ushort) key;
                    Depth8 = (byte) (depth - DepthEntryOffset);
                    GenBound8 = (byte) (generation8 | ((pv ? 1 : 0) << 2) | (byte) bound);
                    Value16 = (short) value;
                    Eval16 = (short) eval;
                }
            }

            public readonly byte RelativeAge(byte generation8)
            {
                return (byte) ((GenerationCycle + generation8 - GenBound8) & GenerationMask);
            }
        }
    }
}
