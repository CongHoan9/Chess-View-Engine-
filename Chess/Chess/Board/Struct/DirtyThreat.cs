using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DirtyThreat
    {
        private readonly int data;
        private const int PcSqOffset = 0;
        private const int ThreatenedSqOffset = 8;
        private const int ThreatenedPcOffset = 16;
        private const int PcOffset = 20;
        public DirtyThreat(int raw)
        {
            data = raw;
        }
        public DirtyThreat(Piece pc, Piece threatenedPc, Square pcSq, Square threatenedSq, bool add)
        {
            data = ((add ? 1 : 0) << 31) 
                 | ((int)pc << PcOffset) 
                 | ((int)threatenedPc << ThreatenedPcOffset) 
                 | ((int)threatenedSq << ThreatenedSqOffset) 
                 | ((int)pcSq << PcSqOffset);
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
            return (data >> 31) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Raw()
        {
            return data;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreats
    {
        public DirtyThreatList List;
        public Color Us;
        public Square PrevKsq, Ksq;
        public Bitboard ThreatenedSqs, ThreateningSqs;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreatList
    {
        private ValueList Raw;
        private int count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push_Back(DirtyThreat t)
        {
            Raw[count++] = t;
        }
    }
    [InlineArray(96)]
    public struct ValueList
    {
        public DirtyThreat Raw;
    }
}
