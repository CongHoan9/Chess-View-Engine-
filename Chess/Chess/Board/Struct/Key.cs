using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Key(ulong k)
    {
        public ulong Raw { get; } = k;
        public static implicit operator ulong(Key k) => k.Raw;
        public static implicit operator Key(ulong k) => new(k);
    }
}
