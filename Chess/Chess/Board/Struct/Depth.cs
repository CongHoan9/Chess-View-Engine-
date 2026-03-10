using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SDepth(int d)
    {
        public int Raw { get; } = d;
        public static implicit operator int(SDepth d) => d.Raw;
        public static implicit operator SDepth(int d) => new(d);
    }
}
