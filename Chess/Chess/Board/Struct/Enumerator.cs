using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{

    [StructLayout(LayoutKind.Sequential)]
    unsafe public ref struct Enumerator<T>(T* begin, T* end) where T : unmanaged
    {
        private T* current = begin - 1;
        private readonly T* end = end;
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            current++;
            return current < end;
        }
        public T Current => *current;
    }
}
