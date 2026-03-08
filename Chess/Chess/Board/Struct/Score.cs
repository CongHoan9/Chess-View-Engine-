using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Score(int s)
    {
        public int Raw { get; } = s;
        public static implicit operator int(Score s) => s.Raw;
        public static implicit operator Score(int s) => new(s);
    }
}
