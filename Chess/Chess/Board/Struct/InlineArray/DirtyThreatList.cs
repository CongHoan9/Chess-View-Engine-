using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyThreatList
    {
        private DirtyThreatList_Data Raw;
        private int count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push_Back(DirtyThreat t)
        {
            Raw[count++] = t;
        }
    }
    [InlineArray(96)]
    public struct DirtyThreatList_Data
    {
        public DirtyThreat Raw;
    }
}
