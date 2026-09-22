using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.GenType;
using static Chess.MoveGen;
using static Chess.Types;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public ref struct MoveList<T, Us, Next> where T : struct, IGenType where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
    {
        private MoveList_Data Raw;
        private readonly ulong Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList(ref Position pos)
        {
            fixed (Move* ptr = &Raw[0])
            {
                Move* end = T.Type == LEGAL ? Generate_Legal<Us, Next>(ref pos, ptr) : Generate<T, Us, Next>(ref pos, ptr);
                Count = (ulong)(end - ptr);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong Size()
        {
            return Count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator<Move> GetEnumerator()
        {
            fixed (Move* ptr = &Raw[0])
            {
                return new Enumerator<Move>(ptr, ptr + Count);
            }
        }
    }
    [InlineArray(MAX_MOVES)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MoveList_Data
    {
        public Move Raw;

    }
}
