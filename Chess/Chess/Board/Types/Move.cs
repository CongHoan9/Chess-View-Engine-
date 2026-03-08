using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    public enum MoveType
    {
        Normal = 0,
        Promotion = 1 << 14,
        EnPassant = 2 << 14,
        Castling = 3 << 14
    }
}
