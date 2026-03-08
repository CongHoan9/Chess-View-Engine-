using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DirtyThreat
    {
        private readonly uint data;
        private const int PcSqOffset = 0;
        private const int ThreatenedSqOffset = 8;
        private const int ThreatenedPcOffset = 16;
        private const int PcOffset = 20;
        private const int AddOffset = 31;
        public DirtyThreat(uint raw)
        {
            data = raw;
        }
        public DirtyThreat(Piece pc, Piece threatenedPc, Square pcSq, Square threatenedSq, bool add)
        {
            data = ((uint)(add ? 1 : 0) << AddOffset) | ((uint)pc << PcOffset) | ((uint)threatenedPc << ThreatenedPcOffset) | ((uint)threatenedSq << ThreatenedSqOffset) | ((uint)pcSq << PcSqOffset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece Pc()
        {
            return (Piece)((data >> PcOffset) & 0xF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece ThreatenedPc()
        {
            return (Piece)((data >> ThreatenedPcOffset) & 0xF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square ThreatenedSq()
        {
            return (Square)((data >> ThreatenedSqOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square PcSq()
        {
            return (Square)((data >> PcSqOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add()
        {
            return (data >> AddOffset) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Raw()
        {
            return data;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreats
    {
        public DirtyThreatList List;
        public Color Us;
        public Square PrevKsq;
        public Square Ksq;
        public BitBoard ThreatenedSqs;
        public BitBoard ThreateningSqs;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            List.Clear();
            Us = Color.White;
            PrevKsq = Square.SquareNone;
            Ksq = Square.SquareNone;
            ThreatenedSqs = 0;
            ThreateningSqs = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreatList
    {
        private DirtyThreatValues values;
        private int _count;
        public readonly int Count => _count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(DirtyThreat t)
        {
            values[_count++] = t;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
        }
        public ref DirtyThreat this[int index] => ref MemoryMarshal.CreateSpan(ref values.Raw, 32)[index];
    }
    [InlineArray(32)]
    public struct DirtyThreatValues
    {
        public DirtyThreat Raw;
    }
}
