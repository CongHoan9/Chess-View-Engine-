using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.GenType;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Captures : IGenType
    {
        public static GenType Type => CAPTURE; 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return pos.Get_Pieces(N.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quiets : IGenType
    {
        public static GenType Type => QUIET;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return pos.Get_Pieces(N.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Evasions : IGenType
    {
        public static GenType Type => EVASION;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return pos.Checkers();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NON_EVASIONs : IGenType
    {
        public static GenType Type => NON_EVASION;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return pos.Get_Pieces(N.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Legal : IGenType
    {
        public static GenType Type => LEGAL;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return pos.Get_Pieces(N.Value);
        }
    }
}
