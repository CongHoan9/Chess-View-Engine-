using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Captures : IMoveGen
    {
        public static EGenType Type => EGenType.Captures;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quiets : IMoveGen
    {
        public static EGenType Type => EGenType.Quiets;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Evasions : IMoveGen
    {
        public static EGenType Type => EGenType.Evasions;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NonEvasions : IMoveGen
    {
        public static EGenType Type => EGenType.NonEvasions;
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Legal : IMoveGen
    {
        public static EGenType Type => EGenType.Legal;
    }
}
