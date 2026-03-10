using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    unsafe public struct MoveList<T> where T : struct, IGenType
    {
        public const int MAX_MOVES = 256;
        private MoveValues moveList;
        private readonly SMove* last;
        public static implicit operator MoveValues(MoveList<T> move) => move.moveList;
        public MoveList(Position pos)
        {
            fixed (SMove* ptr = &moveList[0])
            {
                last = MoveGen.Generate<T>(pos, ptr);
            }
        }
        public readonly SMove* Begin()
        {
            fixed (SMove* ptr = &moveList[0])
            {
                return ptr;
            }
        }
        public readonly SMove* End()
        {
            return last;
        }
        public readonly int Size()
        {
            fixed (SMove* ptr = &moveList[0])
            {
                return (int)(last - ptr);
            }    
        }
        public readonly bool Contains(int move)
        {
            fixed (SMove* ptr = &moveList[0])
            {
                int count = (int)(last - ptr);
                for (int i = 0; i < count; i++)
                {
                    if (ptr[i] == move)
                    {
                        return true;
                    }
                }    
                return false;
            }
        }
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(Begin(), End());
        }
    }
    [InlineArray(256)]
    public struct MoveValues
    {
        private SMove Raw;
    }
    unsafe public ref struct Enumerator(SMove* begin, SMove* end)
    {
        private SMove* cur = begin - 1;
        private readonly SMove* end = end;
        public bool MoveNext()
        {
            cur++;
            return cur < end;
        }
        public SMove Current => *cur;
    }
}
