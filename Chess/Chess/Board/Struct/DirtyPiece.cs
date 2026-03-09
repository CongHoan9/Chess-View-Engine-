using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirtyPiece
    {
        public EPiece Pc;
        public ESquare From, To;
        public ESquare RemoveSq, AddSq;
        public EPiece RemovePc, AddPc;
    }
}
