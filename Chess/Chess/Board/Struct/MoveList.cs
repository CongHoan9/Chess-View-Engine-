using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Types;

namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MoveList<T> : IDisposable where T : struct, IGenType
    {
        private SMove* begin;
        private SMove* last;
        public MoveList(Position pos)
        {
            begin = (SMove*)NativeMemory.Alloc(MAX_MOVES, (nuint)sizeof(SMove));
            last = T.Type == EGenType.Legal ? MoveGen.GenerateLegal(pos, begin) : MoveGen.Generate<T>(pos, begin);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly SMove* Begin() => begin;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly SMove* End() => last;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Size() => (int)(last - begin);
        public void Dispose()
        {
            if (begin != null)
            {
                NativeMemory.Free(begin);
                begin = null;
                last = null;
            }
        }
        public readonly Enumerator GetEnumerator() 
        { 
            return new Enumerator(Begin(), End());
        }
    }
    [StructLayout(LayoutKind.Sequential)] 
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