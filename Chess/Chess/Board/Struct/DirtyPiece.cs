using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyPiece
    {
        public Piece Pc;
        public Square From, To;
        public Square RemoveSq, AddSq;
        public Piece RemovePc, AddPc;
    }
}
