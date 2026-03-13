using System;
namespace Chess
{
    public interface IGenType
    {
        static abstract GenType Type { get; }
    }
    public enum GenType : int
    {
        CAPTURE,
        QUIET,
        EVASION,
        NON_EVASION,
        LEGAL
    }
}
