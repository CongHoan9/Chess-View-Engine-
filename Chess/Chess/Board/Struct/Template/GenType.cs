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
        public static Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return pos.Get_Pieces(Next.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quiets : IGenType
    {
        public static GenType Type => QUIET;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return pos.Get_Pieces(Next.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Evasions : IGenType
    {
        public static GenType Type => EVASION;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return pos.Checkers();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Non_Evasions : IGenType
    {
        public static GenType Type => NON_EVASION;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return pos.Get_Pieces(Next.Value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Legal : IGenType
    {
        public static GenType Type => LEGAL;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Enemies<Us, Next>(ref Position pos) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return pos.Get_Pieces(Next.Value);
        }
    }
}
