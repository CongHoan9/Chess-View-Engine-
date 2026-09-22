using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Chess.Color;
using static Chess.FuncBit;
using static Chess.MoveType;
using static Chess.Piece;
using static Chess.Types;

namespace Chess
{
    using Value = Int32;
    using Depth = Int32;
    using Nodes = UInt64;
    public struct Search_Limits
    {
        public Depth Depth;
        public int WhiteTime;
        public int BlackTime;
        public int WhiteInc;
        public int BlackInc;
        public int MovesToGo;
        public int MoveTime;
        public Nodes Nodes;
        public Depth Perft;
        public bool Infinite;
    }
    unsafe public static class Search
    {
        public readonly struct InfoShort()
        {
            readonly int depth;
            //readonly Score score;
        }
        public readonly struct InfoFull()
        {
            readonly int depth;
            //readonly Score score;
            readonly int selDepth;
            readonly string multiPV;
            readonly string wdl;
            readonly string bound;
            readonly ulong timeMs;
            readonly ulong nodes;
            readonly ulong nps;
            readonly ulong tbHits;
            readonly string pv;
            readonly int hashfull;
        }
        public readonly struct InfoIteration()
        {
            readonly int depth;
            readonly string currmove;
            readonly ulong currmovenumber;
        }
    }
}
