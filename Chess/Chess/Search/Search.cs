using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace Chess
{
    public static class Search
    {
        private const int QS_FUTILITY_MARGIN = 100; // pawn + margin
        private const int QS_DELTA_MARGIN = 200;    // queen value
        public const int MaxPly = 512;
        private const int MateScore = 100000;
        private const int MaxHistory = 16384;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PieceValue(Piece_Bit piece)
        {
            int index = (int)piece;
            if ((uint)index >= Evaluation.PieceValues.Length)
            {
                return 0;
            }
            return Evaluation.PieceValues[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Root(Board_Bit board, int depth)
        {
            Span<Move> moves = stackalloc Move[218];
            int count = Generate.GenerateMoves(board, moves);
            if (count == 0)
            {
                return default;
            }
            Span<int> scores = stackalloc int[218];
            MoveOrdering.Staged_Move_Picker picker = new(board, moves, count, scores, default, default, default);
            int alpha = -MateScore;
            int beta = MateScore;
            int bestscore = -MateScore;
            Move bestmove = default;
            while (picker.Next(out Move move))
            {
                board.MakeMove(move);
                int score = -AlphaBeta(board, depth - 1, -beta, -alpha, 1);
                board.UnMakeMove();
                if (score > bestscore)
                {
                    bestscore = score;
                    bestmove = move;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
            }
            return bestmove;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlphaBeta(Board_Bit board, int depth, int alpha = -1_000_000, int beta = 1_000_000, int ply = 0)
        {
            if (depth <= 0 || ply >= MaxPly)
            {
                return Quiescence(board, alpha, beta, ply);
            }
            ulong key = board.Zobrist_Instant;
            uint index = (uint)key ^ (uint)(key >> 32);
            index &= TranspositionTable.TableMask;
            ref TTEntry entry = ref TranspositionTable.TT[index];
            Move ttMove = default;
            if (entry.Key == key)
            {
                ttMove = entry.bestmove;
                if (entry.Depth >= depth)
                {
                    if (entry.Flag == 0)
                    {
                        return entry.Score;
                    }
                    if (entry.Flag == 1 && entry.Score >= beta)
                    {
                        return entry.Score;
                    }
                    if (entry.Flag == 2 && entry.Score <= alpha)
                    {
                        return entry.Score;
                    }
                }
            }
            Span<Move> moves = stackalloc Move[218];
            int count = Generate.GenerateMoves(board, moves);
            if (count == 0)
            {
                return board.InCheck(board.Curent) ? -MateScore + ply : 0;
            }
            Span<int> scores = stackalloc int[218];
            MoveOrdering.Staged_Move_Picker picker = new(board, moves, count, scores, ttMove, MoveOrdering.Killer[ply, 0], MoveOrdering.Killer[ply, 1]);
            int originalAlpha = alpha;
            int bestscore = -MateScore;
            Move bestmove = default;
            int movesTried = 0;
            bool first = true;
            while (picker.Next(out Move move))
            {
                //if (move.IsCapture)
                //{
                //    if (StaticExchangeEvaluation.SEE(board, move, 0) < 0)
                //        continue;
                //}
                movesTried++;
                board.MakeMove(move);
                bool quiet = !move.IsCapture && !move.IsPromotion;
                int reduction = 0;
                if (!first && depth >= 3 && movesTried >= 4 && quiet && !board.InCheck(board.Curent))
                {
                    reduction = 1;
                }
                int score;
                if (first)
                {
                    score = -AlphaBeta(board, depth - 1, -beta, -alpha, ply + 1);
                }
                else
                {
                    score = -AlphaBeta(board, depth - 1 - reduction, -alpha - 1, -alpha, ply + 1);
                    if (score > alpha)
                    {
                        score = -AlphaBeta(board, depth - 1, -beta, -alpha, ply + 1);
                    }
                }
                board.UnMakeMove();
                if (score >= beta)
                {
                    if (quiet)
                    {
                        MoveOrdering.Killer[ply, 1] = MoveOrdering.Killer[ply, 0];
                        MoveOrdering.Killer[ply, 0] = move;
                        int idx = move.From * 64 + move.To;
                        int c = (int)board.Curent;
                        MoveOrdering.History[c, idx] += depth * depth;
                        if (MoveOrdering.History[c, idx] > MaxHistory)
                        {
                            MoveOrdering.History[c, idx] = MaxHistory;
                        }
                    }
                    TranspositionTable.Store(key, (sbyte)depth, beta, 1, move);
                    return beta;
                }
                if (score > bestscore)
                {
                    bestscore = score;
                    bestmove = move;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
                first = false;
            }
            byte flag; 
            if (bestscore <= originalAlpha)
            {
                flag = 2; // UpperBound
            } 
            else if (bestscore >= beta) 
            { 
                flag = 1; // LowerBound
            } 
            else 
            { 
                flag = 0; // Exact
            }
            TranspositionTable.Store(key, (sbyte)depth, bestscore, flag, bestmove);
            return bestscore;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Quiescence(Board_Bit board, int alpha, int beta, int ply)
        {
            if (ply >= MaxPly)
            {
                return Evaluation.Evaluate(board);
            }
            int standPat = Evaluation.Evaluate(board);
            if (standPat >= beta)
            {
                return beta;
            }
            int originalAlpha = alpha;
            if (standPat > alpha)
            {
                alpha = standPat;
            }
            if (standPat + QS_DELTA_MARGIN < alpha)
            {
                return standPat;
            }
            ulong key = board.Zobrist_Instant;
            uint index = (uint)key ^ (uint)(key >> 32);
            index &= TranspositionTable.TableMask;
            ref TTEntry entry = ref TranspositionTable.TT[index];
            Move ttMove = default;
            if (entry.Key == key && (entry.bestmove.IsCapture || entry.bestmove.IsPromotion || entry.bestmove.IsEnPassant))
            {
                ttMove = entry.bestmove;
            }
            Span<Move> moves = stackalloc Move[218];
            int count = Generate.GenerateMoves(board, moves, capturesonly: true);
            if (count == 0)
            {
                return standPat;
            }
            Span<int> scores = stackalloc int[218];
            MoveOrdering.Staged_Move_Picker picker = new(board, moves, count, scores, ttMove, default, default);
            int bestScore = standPat;
            Move bestMove = default;
            while (picker.Next(out Move move))
            {
                //if (StaticExchangeEvaluation.SEE(board, move, 0) < 0)
                //    continue;
                board.MakeMove(move);
                int score = -Quiescence(board, -beta, -alpha, ply + 1);
                board.UnMakeMove();
                if (score >= beta)
                {
                    TranspositionTable.Store(key, 0, beta, 1, move);
                    return score;
                }
                if (score > alpha)
                {
                    alpha = score;
                    bestScore = score;
                    bestMove = move;
                }
            }
            byte flag = (alpha > originalAlpha) ? (byte)0 : (byte)2;    
            TranspositionTable.Store(key, 0, alpha, flag, bestMove);
            return alpha;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetVictimValue(Move m, Board_Bit board)
        {
            if (m.IsCapture)
            {
                return (int)board[m.To];
            }
            if (m.IsEnPassant)
            {
                return (int)(board.Curent == Piece_Color.White ? Piece_Bit.BPawn : Piece_Bit.WPawn);
            }
            return 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Perft(Board_Bit board, int depth, bool divide = false)
        {
            if (depth == 0)
            {
                return 1;
            }
            Span<Move> moves = stackalloc Move[218];
            int moveCount = Generate.GenerateMoves(board, moves);
            ulong nodes = 0;
            for (int i = 0; i < moveCount; i++)
            {
                Move move = moves[i];
                board.MakeMove(move);
                ulong count = Perft(board, depth - 1, false);
                nodes += count;
                if (divide)
                {
                    Console.WriteLine($"{i + 1}. {move}: {count:N0} nodes");
                }
                board.UnMakeMove();
            }
            return nodes;
        }
    }
}
