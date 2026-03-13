using System.Runtime.InteropServices;
using static Chess.Types;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Captures : IGenType
    {
        public static GenType Type => CAPTURE;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quiets : IGenType
    {
        public static GenType Type => QUIET;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Evasions : IGenType
    {
        public static GenType Type => EVASION;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NON_EVASIONs : IGenType
    {
        public static GenType Type => NON_EVASION;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Legal : IGenType
    {
        public static GenType Type => LEGAL;
    }
}
