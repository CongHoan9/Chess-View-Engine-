using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess
{
    public static class Evaluation
    {
        public static readonly int[] PieceValues =
        [
            0,100,300,300,500,900,0,
            100,300,300,500,900,0
        ];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Evaluate(Board_Bit b)
        {
            int s = 0;
            s += 100 * b.Count(Piece_Bit.WPawn);
            s += 300 * b.Count(Piece_Bit.WKnight);
            s += 300 * b.Count(Piece_Bit.WBishop);
            s += 500 * b.Count(Piece_Bit.WRook);
            s += 900 * b.Count(Piece_Bit.WQueen);
            s -= 100 * b.Count(Piece_Bit.BPawn);
            s -= 300 * b.Count(Piece_Bit.BKnight);
            s -= 300 * b.Count(Piece_Bit.BBishop);
            s -= 500 * b.Count(Piece_Bit.BRook);
            s -= 900 * b.Count(Piece_Bit.BQueen);
            return b.Curent == Piece_Color.White ? s : -s;
        }
    }
}
