// file TTEntry.cs
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TTEntry
    {
        public ushort Key16;
        public byte Depth8;
        public byte GenBound8;
        public Move Move16;
        public short Value16;
        public short Eval16;
    }
}
