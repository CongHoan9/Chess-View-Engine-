using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.MoveGen;
using static Chess.Types;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public ref struct MoveList<T, C> where T : struct, IGenType where C : struct, IColor
    {
        private MoveValues list;
        private readonly long size;
        public MoveList(Position pos)
        {
            fixed (Move* ptr = &list[0])
            {
                Move* end = T.Type == LEGAL ? Generate_Legal<C>(pos, ptr) : Generate<T, C>(pos, ptr);
                size = end - ptr;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long Size()
        {
            return size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            fixed (Move* ptr = &list[0])
            {
                return new Enumerator(ptr, ptr + size);
            }
        }
    }
    [InlineArray(MAX_MOVES)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MoveValues
    {
        public Move Raw;
    }
    [StructLayout(LayoutKind.Sequential)]
    unsafe public ref struct Enumerator(Move* begin, Move* end)
    {
        private Move* current = begin - 1;
        private readonly Move* end = end;
        public bool MoveNext()
        {
            current++;
            return current < end;
        }
        public Move Current => *current;
    }
}