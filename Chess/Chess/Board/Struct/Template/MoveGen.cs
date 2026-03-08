using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Captures : IMoveGen
    {
        public static GenType Type => GenType.Captures;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quiets : IMoveGen
    {
        public static GenType Type => GenType.Quiets;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Evasions : IMoveGen
    {
        public static GenType Type => GenType.Evasions;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NonEvasions : IMoveGen
    {
        public static GenType Type => GenType.NonEvasions;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Legal : IMoveGen
    {
        public static GenType Type => GenType.Legal;
    }
}
