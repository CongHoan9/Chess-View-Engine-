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

    public readonly struct Search_Result(Move bestmove, Value score, Depth depth, Nodes nodes, TimeSpan time)
    {
        public readonly Move BestMove = bestmove;
        public readonly Value Score = score;
        public readonly Depth Depth = depth;
        public readonly Nodes Nodes = nodes;
        public readonly TimeSpan Time = time;
    }

    unsafe public static class Search
    {
        private const Value QS_FUTILITY_MARGIN = 100;
        private const Value QS_DELTA_MARGIN = 200;
        public const int MaxPly = 512;
        private const Value MateScore = 100000;
        public const int MaxHistory = 16384;

        public static Search_Result Root_Search(ref Position pos, Search_Limits limits, Search_Thread thread, NnueNetworks networks)
        {
            return pos.SideToMove == WHITE ? Root_Search<White, Black>(ref pos, limits, thread, networks)
                                           : Root_Search<Black, White>(ref pos, limits, thread, networks);
        }
        public static Search_Result Root_Search<C, N>(ref Position pos, Search_Limits limits, Search_Thread thread, NnueNetworks networks)
            where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Stopwatch watch = Stopwatch.StartNew();
            thread.Begin_Search(limits.Nodes > 0 ? limits.Nodes : 0);
            TranspositionTable.New_Search();
            Depth targetdepth = limits.Depth > 0 ? limits.Depth : (limits.Infinite ? 8 : 6);
            Move bestmove = Move.None();
            Value bestscore = 0;
            Depth completeddepth = 0;
            for (int depth = 1; depth <= targetdepth; ++depth)
            {
                if (Should_Stop(thread))
                {
                    break;
                }
                Move iterationbestmove = Move.None();
                Value iterationscore = Root_Search<C, N>(ref pos, depth, ref iterationbestmove, thread, networks);
                if (iterationbestmove != Move.None())
                {
                    bestmove = iterationbestmove;
                    bestscore = iterationscore;
                    completeddepth = depth;
                }
                if (Should_Stop(thread))
                {
                    break;
                }
                if (!limits.Infinite && !thread.TimeMan.Can_Start_Next_Depth())
                {
                    break;
                }
            }
            watch.Stop();
            return new Search_Result(bestmove, bestscore, completeddepth, thread.Nodes, watch.Elapsed);
        }
        private static Value Root_Search<C, N>(ref Position pos, Depth depth, ref Move bestmove, Search_Thread thread, NnueNetworks networks)
            where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Move ttmove = TranspositionTable.Probe_Best(pos.st->Key);
            MovePick<C, N> movepick = new(ref pos, thread.History, ttmove, thread.Killer_0[0], thread.Killer_1[0], depth);
            Value alpha = -VALUE_INFINITE;
            Value beta = VALUE_INFINITE;
            Value alphaorig = alpha;
            Value bestscore = -VALUE_INFINITE;
            bool hasmove = false;
            while (movepick.Try_Next(out Move move))
            {
                if (Should_Stop(thread))
                {
                    break;
                }
                hasmove = true;
                Piece movingpiece = pos.Piece_On(From_Sq(move));
                Piece capturedpiece = Capture_Of<C, N>(ref pos, move);
                StateInfo newstate = new();
                //pos.Do_Move<C, N>(move, newstate);
                Value score = -Alpha_Beta<N, C>(ref pos, depth - 1, -beta, -alpha, 1, thread, networks);
                pos.Undo_Move<C, N>(move);
                if (score > bestscore)
                {
                    bestscore = score;
                    bestmove = move;
                    if (score > alpha)
                    {
                        alpha = score;
                    }
                }
                if (score >= beta)
                {
                    Update_History(thread, C.Value, 0, move, movingpiece, capturedpiece, depth);
                    TranspositionTable.Store(pos.st->Key, (sbyte)depth, To_TT_Score(score, 0), TranspositionTable.TT_FLAG_LOWER, move);
                    return score;
                }
            }
            if (!hasmove)
            {
                return pos.Checkers() != 0 ? -VALUE_MATE + 1 : VALUE_DRAW;
            }
            byte flag = bestscore <= alphaorig ? TranspositionTable.TT_FLAG_UPPER : TranspositionTable.TT_FLAG_EXACT;
            TranspositionTable.Store(pos.st->Key, (sbyte)depth, To_TT_Score(bestscore, 0), flag, bestmove);
            return bestscore;
        }

        private static Value Alpha_Beta<C, N>(ref Position pos, Depth depth, Value alpha, Value beta, int ply, Search_Thread thread, NnueNetworks networks)
            where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            if (ply >= MaxPly - 1)
            {
                return Evaluate.Evaluate_Position<C, N>(ref pos, networks);
            }
            if ((thread.Nodes & 2047UL) == 0 && Should_Stop(thread))
            {
                thread.Stop = true;
                return alpha;
            }
            thread.Nodes++;
            thread.SelDepth = Math.Max(thread.SelDepth, ply);
            if (pos.st->Repetition != 0 || pos.st->Rule50 >= 100)
            {
                return VALUE_DRAW;
            }
            if (depth <= 0)
            {
                return Quiescence<C, N>(ref pos, alpha, beta, ply, thread, networks);
            }
            bool incheck = pos.Checkers() != 0;
            Move ttmove = Move.None();
            if (TranspositionTable.Probe(pos.st->Key, out TTEntry entry))
            {
                ttmove = entry.BestMove;
                Value ttscore = From_TT_Score(entry.Score, ply);
                if (entry.Depth >= depth)
                {
                    if (entry.Flag == TranspositionTable.TT_FLAG_EXACT)
                    {
                        return ttscore;
                    }
                    if (entry.Flag == TranspositionTable.TT_FLAG_LOWER && ttscore >= beta)
                    {
                        return ttscore;
                    }
                    if (entry.Flag == TranspositionTable.TT_FLAG_UPPER && ttscore <= alpha)
                    {
                        return ttscore;
                    }
                }
            }
            Value alphaorig = alpha;
            Value bestscore = -VALUE_INFINITE;
            Move bestmove = Move.None();
            MovePick<C, N> movepick = new(ref pos, thread.History, ttmove, thread.Killer_0[ply], thread.Killer_1[ply], depth);
            bool hasmove = false;
            while (movepick.Try_Next(out Move move))
            {
                if (Should_Stop(thread))
                {
                    thread.Stop = true;
                    break;
                }
                hasmove = true;
                Piece movingpiece = pos.Piece_On(From_Sq(move));
                Piece capturedpiece = Capture_Of<C, N>(ref pos, move);
                StateInfo newstate = new();
                //pos.Do_Move<C, N>(move, ref newstate);
                Value score = -Alpha_Beta<N, C>(ref pos, depth - 1, -beta, -alpha, ply + 1, thread, networks);
                pos.Undo_Move<C, N>(move);
                if (score > bestscore)
                {
                    bestscore = score;
                    bestmove = move;
                    if (score > alpha)
                    {
                        alpha = score;
                    }
                }
                if (score >= beta)
                {
                    Update_History(thread, C.Value, ply, move, movingpiece, capturedpiece, depth);
                    TranspositionTable.Store(pos.st->Key, (sbyte)depth, To_TT_Score(score, ply), TranspositionTable.TT_FLAG_LOWER, move);
                    return score;
                }
            }
            if (!hasmove)
            {
                return incheck ? -VALUE_MATE + ply : VALUE_DRAW;
            }
            byte flag = bestscore <= alphaorig ? TranspositionTable.TT_FLAG_UPPER : TranspositionTable.TT_FLAG_EXACT;
            TranspositionTable.Store(pos.st->Key, (sbyte)depth, To_TT_Score(bestscore, ply), flag, bestmove);
            return bestscore;
        }
        private static Value Quiescence<C, N>(ref Position pos, Value alpha, Value beta, int ply, Search_Thread thread, NnueNetworks networks)
            where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Value standpat = Evaluate.Evaluate_Position<C, N>(ref pos, networks);
            if (standpat >= beta)
            {
                return standpat;
            }
            if (standpat + QS_DELTA_MARGIN < alpha)
            {
                return alpha;
            }
            alpha = Math.Max(alpha, standpat);
            MoveList<Legal, C, N> movelist = new(ref pos);
            foreach (Move move in movelist)
            {
                Piece capturedpiece = Capture_Of<C, N>(ref pos, move);
                bool tactical = capturedpiece != NO_PIECE || Type_Of(move) == PROMOTION;
                if (!tactical)
                {
                    continue;
                }
                if (!Should_See_Move<C, N>(ref pos, move, capturedpiece, alpha, standpat))
                {
                    continue;
                }
                StateInfo newstate = new();
                //pos.Do_Move<C, N>(move, ref newstate);
                Value score = -Quiescence<N, C>(ref pos, -beta, -alpha, ply + 1, thread, networks);
                pos.Undo_Move<C, N>(move);
                if (score >= beta)
                {
                    return score;
                }
                alpha = Math.Max(alpha, score);
            }
            return alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Should_Stop(Search_Thread thread)
        {
            return thread.Stop || (thread.NodeLimit != 0 && thread.Nodes >= thread.NodeLimit) || thread.TimeMan.Should_Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Should_See_Move<C, N>(ref Position pos, Move move, Piece capturedpiece, Value alpha, Value standpat) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            if (capturedpiece == NO_PIECE)  
            {
                return Type_Of(move) == PROMOTION;
            }
            Piece promotionpiece = Type_Of(move) == PROMOTION ? Make_Piece(C.Value, Promotion_Type(move)) : NO_PIECE;
            Value gain = Piece_Value(capturedpiece) + (promotionpiece != NO_PIECE ? Piece_Value(promotionpiece) - PawnValue : 0);
            return standpat + gain + QS_FUTILITY_MARGIN >= alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Piece Capture_Of<C, N>(ref Position pos, Move move) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return Type_Of(move) == EN_PASSANT ? Make_Piece<Pawn>(N.Value) : pos.Piece_On(To_Sq(move));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Update_History(Search_Thread thread, Color us, int ply, Move move, Piece movingpiece, Piece capturedpiece, int depth)
        {
            int bonus = History_Bonus(depth);
            if (capturedpiece != NO_PIECE || Type_Of(move) == PROMOTION)
            {
                thread.History.Update_Capture(movingpiece, To_Sq(move), capturedpiece, bonus);
            }
            else
            {
                thread.Add_Killer(ply, move);
                thread.History.Update_Quiet(us, move, movingpiece, bonus);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int History_Bonus(int depth)
        {
            return Math.Min(1200, depth * depth * 16 + 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value To_TT_Score(Value score, int ply)
        {
            if (score >= VALUE_MATE_IN_MAX_PLY)
            {
                return score + ply;
            }
            if (score <= VALUE_MATED_IN_MAX_PLY)
            {
                return score - ply;
            }
            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value From_TT_Score(Value score, int ply)
        {
            if (score >= VALUE_MATE_IN_MAX_PLY)
            {
                return score - ply;
            }
            if (score <= VALUE_MATED_IN_MAX_PLY)
            {
                return score + ply;
            }
            return score;
        }
    }
}
