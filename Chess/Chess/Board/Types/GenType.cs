using System;
namespace Chess
{
    public interface IGenType
    {
        public static abstract GenType Type { get; }
        public static abstract Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>;
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
