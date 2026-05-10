using System.Runtime.InteropServices;
namespace Chess
{

    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreats
    {
        public DirtyThreatList List;
        public Color Us;
        public Square PrevKsq, Ksq;
        public Bitboard ThreatenedSqs, ThreateningSqs;
    }
}
