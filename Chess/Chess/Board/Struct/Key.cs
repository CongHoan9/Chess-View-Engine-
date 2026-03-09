using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SKey(ulong k)
    {
        public ulong Raw { get; } = k;
        public static implicit operator ulong(SKey k) => k.Raw;
        public static implicit operator SKey(ulong k) => new(k);
    }
}
