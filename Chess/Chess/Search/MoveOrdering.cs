using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess
{
    public static class MoveOrdering
    {
        private static readonly int[] MVVLVA = new int[13 * 13];
        public static readonly int[,] History = new int[2, 64 * 64];
        public static readonly Move[,] Killer = new Move[Search.MaxPly, 2];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MoveOrdering()
        {
            for (int a = 0; a < 13; a++)
            {
                for (int v = 0; v < 13; v++)
                {
                    MVVLVA[a * 13 + v] = Evaluation.PieceValues[v] * 32 - Evaluation.PieceValues[a];
                }
            }
        }
        public ref struct Staged_Move_Picker
        {
            private enum PickerPhase
            {
                Hash,
                Captures,
                Killers,
                Quiets,
                Done
            }
            private readonly bool CapturesOnly;
            private readonly Board_Bit Board;
            private readonly Move HashMove;
            private readonly Move Killer1;
            private readonly Move Killer2;
            private readonly int Count;
            private PickerPhase Phase;
            private Span<Move> Moves;
            private Span<int> Scores;
            private int CaptureStart;
            private int CaptureEnd;
            private int QuietStart;
            private int QuietEnd;
            private int Current;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Staged_Move_Picker(Board_Bit board, Span<Move> moves, int count, Span<int> scores, Move hash, Move killer1, Move killer2, bool capturesonly = false)
            {
                Current = 0;
                QuietEnd = 0;
                Board = board;
                Moves = moves;
                Count = count;
                CaptureEnd = 0;
                QuietStart = 0;
                Scores = scores;
                HashMove = hash;
                CaptureStart = 0;
                Killer1 = killer1;
                Killer2 = killer2;
                Phase = PickerPhase.Hash;
                CapturesOnly = capturesonly;
                ScoreAllMoves();
                PartitionTacticals();
                InsertionSort(CaptureStart, CaptureEnd);
                if (!CapturesOnly)
                {
                    InsertionSort(QuietStart, QuietEnd);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void PartitionTacticals()
            {
                CaptureEnd = 0;
                for (int i = 0; i < Count; i++)
                {
                    if (Moves[i].IsCapture || Moves[i].IsEnPassant || Moves[i].IsPromotion)
                    {
                        Swap(i, CaptureEnd);
                        CaptureEnd++;
                    }
                }
                CaptureStart = 0;
                QuietStart = CaptureEnd;
                QuietEnd = Count;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void InsertionSort(int start, int end)
            {
                for (int i = start + 1; i < end; ++i)
                {
                    int score = Scores[i];
                    Move move = Moves[i];
                    int j = i - 1;
                    while (j >= start && Scores[j] < score)
                    {
                        Scores[j + 1] = Scores[j];
                        Moves[j + 1] = Moves[j];
                        --j;
                    }
                    Scores[j + 1] = score;
                    Moves[j + 1] = move;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ScoreAllMoves()
            {
                for (int i = 0; i < Count; i++)
                {
                    Move move = Moves[i];
                    int score;
                    if (move.IsCapture || move.IsEnPassant || move.IsPromotion)
                    {
                        int attacker = (int)Board[move.From];
                        int victim = Search.GetVictimValue(move, Board);
                        score = MVVLVA[attacker * 13 + victim] + 10_000_000;
                        if (move.IsPromotion)
                        {
                            score += Search.PieceValue(move.Promotion);
                        }
                    }
                    else
                    {
                        score = History[(int)Board.Curent, move.From * 64 + move.To];
                    }
                    Scores[i] = score;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Swap(int i, int j)
            {
                (Moves[i], Moves[j]) = (Moves[j], Moves[i]);
                (Scores[i], Scores[j]) = (Scores[j], Scores[i]);
            }
            public bool Next(out Move move)
            {
                move = default;
                switch (Phase)
                {
                    case PickerPhase.Hash:
                        if (!HashMove.IsNull)
                        {
                            bool istactical = HashMove.IsCapture || HashMove.IsEnPassant || HashMove.IsPromotion;
                            int start = istactical ? CaptureStart : QuietStart;
                            int end = istactical ? CaptureEnd : QuietEnd;
                            for (int i = start; i < end; i++)
                            {
                                if (Moves[i] == HashMove)
                                {
                                    Swap(i, start);
                                    move = Moves[start];
                                    Current = start + 1;
                                    Phase = istactical ? PickerPhase.Captures : PickerPhase.Quiets;
                                    return true;
                                }
                            }
                        }
                        Phase = PickerPhase.Captures;
                        Current = CaptureStart;
                        goto case PickerPhase.Captures;
                    case PickerPhase.Captures:
                        if (Current < CaptureEnd)
                        {
                            move = Moves[Current++];
                            return true;
                        }
                        if (CapturesOnly)
                        {
                            Phase = PickerPhase.Done;
                            return false;
                        }
                        Phase = PickerPhase.Killers;
                        Current = QuietStart; // Chuẩn bị cho killers và quiets
                        goto case PickerPhase.Killers;
                    case PickerPhase.Killers:
                        // Thử Killer1 (chỉ nếu là quiet)
                        if (!Killer1.IsNull && !Killer1.IsCapture && !Killer1.IsEnPassant && !Killer1.IsPromotion)
                        {
                            for (int i = Current; i < QuietEnd; i++)
                            {
                                if (Moves[i].Value == Killer1.Value)
                                {
                                    Swap(i, Current);
                                    move = Moves[Current++];
                                    return true;
                                }
                            }
                        }
                        // Thử Killer2 (chỉ nếu là quiet)
                        if (!Killer2.IsNull && !Killer2.IsCapture && !Killer2.IsEnPassant && !Killer2.IsPromotion)
                        {
                            for (int i = Current; i < QuietEnd; i++)
                            {
                                if (Moves[i].Value == Killer2.Value)
                                {
                                    Swap(i, Current);
                                    move = Moves[Current++];
                                    return true;
                                }
                            }
                        }
                        Phase = PickerPhase.Quiets;
                        goto case PickerPhase.Quiets;
                    case PickerPhase.Quiets:
                        if (Current < QuietEnd)
                        {
                            move = Moves[Current++];
                            return true;
                        }
                        Phase = PickerPhase.Done;
                        return false;
                    default:
                        return false;
                }
            }
        }
    }
}
