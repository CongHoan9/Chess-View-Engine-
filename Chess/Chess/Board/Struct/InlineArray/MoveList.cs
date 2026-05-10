using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.GenType;
using static Chess.MoveGen;
using static Chess.Types;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public ref struct MoveList<T, C, N> where T : struct, IGenType where C : struct, IColor<C, N> where N : struct, IColor<N, C>
    {
        private MoveList_Data Raw;
        private readonly long Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList(ref Position pos)
        {
            fixed (Move* ptr = &Raw[0])
            {
                Move* end = T.Type == LEGAL ? Generate_Legal<C, N>(ref pos, ptr) : Generate<T, C, N>(ref pos, ptr);
                Count = end - ptr;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long Size()
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
