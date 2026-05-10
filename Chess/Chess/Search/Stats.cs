using System.Runtime.CompilerServices;

namespace Chess
{
    using Value = Int32;

    public interface IStatsLimit
    {
        public static abstract int Value { get; }
    }

    public readonly struct QuietHistoryLimit : IStatsLimit
    {
        public static int Value => 7183;
    }

    public readonly struct CaptureHistoryLimit : IStatsLimit
    {
        public static int Value => 10692;
    }

    public readonly struct PieceToHistoryLimit : IStatsLimit
    {
        public static int Value => 30000;
    }

    // StatsEntry is the container of various numerical statistics. We use a struct
    // instead of a naked value to directly call history update logic on the entry.
    public struct StatsEntry<TLimit> where TLimit : struct, IStatsLimit
    {
        private short Entry;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set_Value(short value)
        {
            Entry = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Value()
        {
            return Entry;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(int bonus)
        {
            // Make sure that bonus is in range [-D, D]
            int clampedBonus = Misc.Clamp(bonus, -TLimit.Value, TLimit.Value);
            int value = Entry;
            Entry = (short)(value + clampedBonus - value * Math.Abs(clampedBonus) / TLimit.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator short(StatsEntry<TLimit> entry)
        {
            return entry.Entry;
        }
    }
}
