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
        public DirtyThreat(EPiece pc, EPiece threatenedPc, ESquare pcSq, ESquare threatenedSq, bool add)
        {
            data = ((uint)(add ? 1 : 0) << AddOffset) | ((uint)pc << PcOffset) | ((uint)threatenedPc << ThreatenedPcOffset) | ((uint)threatenedSq << ThreatenedSqOffset) | ((uint)pcSq << PcSqOffset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPiece Pc()
        {
            return (EPiece)((data >> PcOffset) & 0xF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPiece ThreatenedPc()
        {
            return (EPiece)((data >> ThreatenedPcOffset) & 0xF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare ThreatenedSq()
        {
            return (ESquare)((data >> ThreatenedSqOffset) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ESquare PcSq()
        {
            return (ESquare)((data >> PcSqOffset) & 0xFF);
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
        public EColor Us;
        public ESquare PrevKsq;
        public ESquare Ksq;
        public SBitBoard ThreatenedSqs;
        public SBitBoard ThreateningSqs;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            List.Clear();
            Us = EColor.White;
            PrevKsq = ESquare.SquareNone;
            Ksq = ESquare.SquareNone;
            ThreatenedSqs = 0;
            ThreateningSqs = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreatList
    {
        private DirtyThreatValues Raw;
        private int count;
        public readonly int Count => count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(DirtyThreat t)
        {
            Raw[count++] = t;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            count = 0;
        }
        public ref DirtyThreat this[int index] => ref MemoryMarshal.CreateSpan(ref Raw.Raw, 32)[index];
    }
    [InlineArray(96)]
    public struct DirtyThreatValues
    {
        public DirtyThreat Raw;
    }
}
