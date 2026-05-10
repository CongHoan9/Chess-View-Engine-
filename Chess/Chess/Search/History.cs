using System.Runtime.CompilerServices;
using static Chess.Piece;
using static Chess.PieceType;

namespace Chess
{
    // ButterflyHistory records how often quiet moves have been successful or unsuccessful
    // during the current search, and is used for reduction and move ordering decisions.
    //
    // CapturePieceToHistory is addressed by a move's [piece][to][captured piece type]
    //
    // PieceToHistory is like ButterflyHistory but is addressed by a move's [piece][to]
    public sealed class History
    {
        private readonly StatsEntry<QuietHistoryLimit>[,] QuietHistory =
          new StatsEntry<QuietHistoryLimit>[(int) Color.COLOR_NB, 1 << 16];

        private readonly StatsEntry<PieceToHistoryLimit>[,] PieceToHistory =
          new StatsEntry<PieceToHistoryLimit>[(int) PIECE_NB, (int) Square.SQ_NB];

        private readonly StatsEntry<CaptureHistoryLimit>[,,] CaptureHistory =
          new StatsEntry<CaptureHistoryLimit>[(int) PIECE_NB, (int) Square.SQ_NB, (int) PIECE_TYPE_NB];

        public void Clear()
        {
            Array.Clear(QuietHistory, 0, QuietHistory.Length);
            Array.Clear(PieceToHistory, 0, PieceToHistory.Length);
            Array.Clear(CaptureHistory, 0, CaptureHistory.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Get_Quiet(Color us, Move move)
        {
            return QuietHistory[(int) us, move.Raw];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Get_Piece_To(Piece piece, Square to)
        {
            return PieceToHistory[(int) piece, (int) to];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Get_Capture(Piece piece, Square to, Piece capturedpiece)
        {
            return capturedpiece == NO_PIECE
                ? (short) 0
                : CaptureHistory[(int) piece, (int) to, (int) FuncBit.Type_Of(capturedpiece)];
        }

        public void Update_Quiet(Color us, Move move, Piece piece, int bonus)
        {
            Update_Entry(ref QuietHistory[(int) us, move.Raw], bonus);
            Update_Entry(ref PieceToHistory[(int) piece, (int) FuncBit.To_Sq(move)], bonus);
        }

        public void Update_Capture(Piece piece, Square to, Piece capturedpiece, int bonus)
        {
            if (capturedpiece == NO_PIECE)
                return;

            int capturedtype = (int) FuncBit.Type_Of(capturedpiece);
            Update_Entry(ref CaptureHistory[(int) piece, (int) to, capturedtype], bonus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Update_Entry<TLimit>(ref StatsEntry<TLimit> entry, int bonus)
            where TLimit : struct, IStatsLimit
        {
            entry.Update(bonus);
        }
    }
}
