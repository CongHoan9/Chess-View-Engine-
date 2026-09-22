// file TTData.cs
using System.Runtime.InteropServices;
namespace Chess 
{
    using Depth = Int32;
    using Value = Int32;
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TTData(Move move, Value value, Value eval, Depth depth, TTBound bound, bool isPv)
    {
        public readonly Move Move = move;
        public readonly Value Value = value;
        public readonly Value Eval = eval;
        public readonly Depth Depth = depth;
        public readonly TTBound Bound = bound;
        public readonly bool IsPv = isPv;
    }
}
