using System;
namespace Chess
{
    public interface IGenType
    {
        public static abstract GenType Type { get; }
        public static abstract Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>;
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
