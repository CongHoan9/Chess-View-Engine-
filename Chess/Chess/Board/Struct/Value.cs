using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Value(int v)
    {
        public const int ValueZero = 0;
        public const int ValueDraw = 0;
        public const int ValueMate = 32000;
        public const int ValueInfinite = 32001;
        public const int ValueNone = 32002;
        public const int ValueMateInMaxPly = ValueMate - 2 * Search.MaxPly;
        public const int ValueMatedInMaxPly = -ValueMate + 2 * Search.MaxPly;
        public const int PawnValueMg = 124;
        public const int PawnValueEg = 206;
        public const int KnightValueMg = 781;
        public const int KnightValueEg = 854;
        public const int BishopValueMg = 825;
        public const int BishopValueEg = 915;
        public const int RookValueMg = 1280;
        public const int RookValueEg = 1371;
        public const int QueenValueMg = 2526;
        public const int QueenValueEg = 2646;
        public const int MidgameLimit = 15158;
        public const int EndgameLimit = 3915;
        public int Raw { get; } = v;
        public static implicit operator int(Value v) => v.Raw;
        public static implicit operator Value(int v) => new(v);
        public static Value operator +(Value a, Value b) => a.Raw + b.Raw;
        public static Value operator -(Value a, Value b) => a.Raw - b.Raw;
        public static Value operator *(Value a, int b) => a.Raw * b;
        public static Value operator /(Value a, int b) => a.Raw / b;
        public static Value operator -(Value a) => -a.Raw;
        public static bool operator ==(Value a, Value b) => a.Raw == b.Raw;
        public static bool operator !=(Value a, Value b) => a.Raw != b.Raw;
        public static bool operator >(Value a, Value b) => a.Raw > b.Raw;
        public static bool operator <(Value a, Value b) => a.Raw < b.Raw;
        public static bool operator >=(Value a, Value b) => a.Raw >= b.Raw;
        public static bool operator <=(Value a, Value b) => a.Raw <= b.Raw;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Value value && Raw == value.Raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }
    }
}
