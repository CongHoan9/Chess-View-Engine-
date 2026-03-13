using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyPiece
    {
        public Piece Pc;
        public Square From, To;
        public Square Remove_Sq, Add_Sq;
        public Piece Remove_Pc, Add_Pc;
    }
}
