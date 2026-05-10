using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.FuncBit;
using static Chess.MoveType;
using static Chess.PieceType;
using static Chess.Types;

namespace Chess
{
    using Depth = Int32;
    using Value = Int32;

    enum MovePick_Stage
    {
        // generate main search moves
        MAIN_TT,
        CAPTURE_INIT,
        GOOD_CAPTURE,
        QUIET_INIT,
        GOOD_QUIET,
        BAD_CAPTURE,
        BAD_QUIET,

        // generate evasion moves
        EVASION_TT,
        EVASION_INIT,
        EVASION,

        // generate qsearch moves
        QSEARCH_TT,
        QCAPTURE_INIT,
        QCAPTURE
    }

    // The MovePick class is used to pick one pseudo-legal move at a time from the
    // current position. The most important method is Next_Move(), which emits one
    // new pseudo-legal move on every call, until there are no moves left, when
    // Move::none() is returned. In order to improve the efficiency of the alpha-beta
    // algorithm, MovePick attempts to return the moves which are most likely to get
    // a cut-off first.
    public unsafe ref struct MovePick<C, N> where C : struct, IColor<C, N> where N : struct, IColor<N, C>
    {
        private readonly ref Position Pos;
        private readonly History MainHistory;
        private readonly Move TTMove;
        private readonly Move Killer0;
        private readonly Move Killer1;
        private readonly Depth Depth;

        private MoveList_Data Moves;
        private MoveScore_Date Scores;

        private int Cur, EndCur, EndBadCaptures, EndCaptures, EndGenerated;
        private MovePick_Stage Stage;
        private bool SkipQuiets;

        // MovePick constructor for the main search and for the quiescence search
        public MovePick(ref Position pos, History history, Move ttMove, Move killer0, Move killer1, Depth depth)
        {
            Pos = ref pos;
            MainHistory = history;
            TTMove = ttMove;
            Killer0 = killer0;
            Killer1 = killer1;
            Depth = depth;

            Moves = default;
            Scores = default;
            Cur = EndCur = EndBadCaptures = EndCaptures = EndGenerated = 0;
            SkipQuiets = false;

            if (pos.Checkers() != 0)
            {
                Stage = Move_In_List<Evasions>() ? MovePick_Stage.EVASION_TT : MovePick_Stage.EVASION_INIT;
            }
            else
            {
                Stage = Depth > 0
                    ? (Move_In_List<NON_EVASIONs>() ? MovePick_Stage.MAIN_TT : MovePick_Stage.CAPTURE_INIT)
                    : (Move_In_List<Captures>() ? MovePick_Stage.QSEARCH_TT : MovePick_Stage.QCAPTURE_INIT);
            }
        }

        public bool Try_Next(out Move move)
        {
            move = Next_Move();
            return move != Move.None();
        }

        public void Skip_Quiet_Moves()
        {
            SkipQuiets = true;
        }

        // Sort moves in descending order up to and including a given limit.
        // The order of moves smaller than the limit is left unspecified.
        private void Partial_Insertion_Sort(int begin, int end, int limit)
        {
            fixed (Move* moves = &Moves[0])
            fixed (Value* scores = &Scores[0])
            {
                int sortedEnd = begin;

                for (int p = begin + 1; p < end; ++p)
                {
                    if (*(scores + p) >= limit)
                    {
                        Move tmpMove = *(moves + p);
                        Value tmpValue = *(scores + p);

                        *(moves + p) = *(moves + ++sortedEnd);
                        *(scores + p) = *(scores + sortedEnd);

                        int q = sortedEnd;
                        while (q != begin && *(scores + q - 1) < tmpValue)
                        {
                            *(moves + q) = *(moves + q - 1);
                            *(scores + q) = *(scores + q - 1);
                            --q;
                        }

                        *(moves + q) = tmpMove;
                        *(scores + q) = tmpValue;
                    }
                }
            }
        }

        // Assigns a numerical value to each move in a list, used for sorting.
        // Captures are ordered by Most Valuable Victim (MVV), preferring captures
        // with a good history. Quiets moves are ordered using the history tables.
        private int Score<T>(MoveList<T, C, N> moveList) where T : struct, IGenType
        {
            int it = Cur;

            fixed (Move* moves = &Moves[0])
            fixed (Value* scores = &Scores[0])
            {
                foreach (Move move in moveList)
                {
                    *(moves + it) = move;
                    *(scores + it) = Score_Move<T>(move);
                    ++it;
                }
            }

            return it;
        }

        private readonly Value Score_Move<T>(Move move) where T : struct, IGenType
        {
            Square from = From_Sq(move);
            Square to = To_Sq(move);
            Piece piece = Pos.Piece_On(from);
            Piece capturedPiece = Captured_Piece(move, to);

            if (T.Type == GenType.CAPTURE)
            {
                return MainHistory.Get_Capture(piece, to, capturedPiece) + 7 * Piece_Value(capturedPiece);
            }

            if (T.Type == GenType.QUIET)
            {
                Value value = 2 * MainHistory.Get_Quiet(C.Value, move);
                value += MainHistory.Get_Piece_To(piece, to);

                if (move == Killer0)
                {
                    value += 8000;
                }
                else if (move == Killer1)
                {
                    value += 4000;
                }

                if (Type_Of(piece) == KING && Type_Of(move) == CASTLING)
                {
                    value += 1500;
                }

                return value;
            }

            // Type == EVASION
            if (Pos.Capture_Stage(move))
            {
                return Piece_Value(capturedPiece) + (1 << 28);
            }

            return MainHistory.Get_Quiet(C.Value, move) + MainHistory.Get_Piece_To(piece, to);
        }

        // Returns the next move. This never returns the TT move,
        // as it was emitted before.
        private Move Select_Any()
        {
            fixed (Move* moves = &Moves[0])
            {
                for (; Cur < EndCur; ++Cur)
                {
                    Move move = *(moves + Cur);
                    if (move != TTMove)
                    {
                        ++Cur;
                        return move;
                    }
                }
            }

            return Move.None();
        }

        private Move Select_Good_Capture()
        {
            fixed (Move* moves = &Moves[0])
            fixed (Value* scores = &Scores[0])
            {
                for (; Cur < EndCur; ++Cur)
                {
                    Move move = *(moves + Cur);
                    if (move == TTMove)
                    {
                        continue;
                    }

                    if (Pos.See_Ge(move, -(*(scores + Cur)) / 18))
                    {
                        ++Cur;
                        return move;
                    }

                    Swap_Entries(moves, scores, EndBadCaptures++, Cur);
                }
            }

            return Move.None();
        }

        private Move Select_Good_Quiet(int threshold)
        {
            fixed (Move* moves = &Moves[0])
            fixed (Value* scores = &Scores[0])
            {
                for (; Cur < EndCur; ++Cur)
                {
                    Move move = *(moves + Cur);
                    if (move != TTMove && *(scores + Cur) > threshold)
                    {
                        ++Cur;
                        return move;
                    }
                }
            }

            return Move.None();
        }

        private Move Select_Bad_Quiet(int threshold)
        {
            fixed (Move* moves = &Moves[0])
            fixed (Value* scores = &Scores[0])
            {
                for (; Cur < EndCur; ++Cur)
                {
                    Move move = *(moves + Cur);
                    if (move != TTMove && *(scores + Cur) <= threshold)
                    {
                        ++Cur;
                        return move;
                    }
                }
            }

            return Move.None();
        }

        // This is the most important method of the MovePick class. We emit one
        // new pseudo-legal move on every call until there are no more moves left,
        // picking the move with the highest score from a list of generated moves.
        private Move Next_Move()
        {
            const int GoodQuietThreshold = -14000;

        Top:
            switch (Stage)
            {
                case MovePick_Stage.MAIN_TT:
                case MovePick_Stage.EVASION_TT:
                case MovePick_Stage.QSEARCH_TT:
                    ++Stage;
                    return TTMove;

                case MovePick_Stage.CAPTURE_INIT:
                case MovePick_Stage.QCAPTURE_INIT:
                {
                    MoveList<Captures, C, N> moveList = new(ref Pos);

                    Cur = EndBadCaptures = 0;
                    EndCur = EndCaptures = Score(moveList);

                    Partial_Insertion_Sort(Cur, EndCur, int.MinValue);
                    ++Stage;
                    goto Top;
                }

                case MovePick_Stage.GOOD_CAPTURE:
                {
                    Move move = Select_Good_Capture();

                    if (move != Move.None())
                    {
                        return move;
                    }

                    ++Stage;
                    goto Top;
                }

                case MovePick_Stage.QUIET_INIT:
                    if (!SkipQuiets)
                    {
                        MoveList<Quiets, C, N> moveList = new(ref Pos);
                        EndCur = EndGenerated = Score(moveList);
                        Partial_Insertion_Sort(Cur, EndCur, -3560 * Depth);
                    }

                    ++Stage;
                    goto Top;

                case MovePick_Stage.GOOD_QUIET:
                {
                    if (!SkipQuiets)
                    {
                        Move move = Select_Good_Quiet(GoodQuietThreshold);
                        if (move != Move.None())
                        {
                            return move;
                        }
                    }

                    // Prepare the pointers to loop over the bad captures
                    Cur = 0;
                    EndCur = EndBadCaptures;

                    ++Stage;
                    goto Top;
                }

                case MovePick_Stage.BAD_CAPTURE:
                {
                    Move move = Select_Any();
                    if (move != Move.None())
                    {
                        return move;
                    }

                    // Prepare the pointers to loop over quiets again
                    Cur = EndCaptures;
                    EndCur = EndGenerated;

                    ++Stage;
                    goto Top;
                }

                case MovePick_Stage.BAD_QUIET:
                    if (!SkipQuiets)
                    {
                        return Select_Bad_Quiet(GoodQuietThreshold);
                    }

                    return Move.None();

                case MovePick_Stage.EVASION_INIT:
                {
                    MoveList<Evasions, C, N> moveList = new(ref Pos);

                    Cur = 0;
                    EndCur = EndGenerated = Score<Evasions>(moveList);

                    Partial_Insertion_Sort(Cur, EndCur, int.MinValue);
                    ++Stage;
                    goto Top;
                }

                case MovePick_Stage.EVASION:
                case MovePick_Stage.QCAPTURE:
                    return Select_Any();
            }

            return Move.None();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Piece Captured_Piece(Move move, Square to)
        {
            return Type_Of(move) == EN_PASSANT ? Make_Piece<Pawn>(N.Value) : Pos.Piece_On(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap_Entries(Move* moves, Value* scores, int i, int j)
        {
            (Move move, Value score) = (*(moves + i), *(scores + i));
            *(moves + i) = *(moves + j);
            *(scores + i) = *(scores + j);
            *(moves + j) = move;
            *(scores + j) = score;
        }

        private readonly bool Move_In_List<T>() where T : struct, IGenType
        {
            if (TTMove == Move.None())
            {
                return false;
            }

            MoveList<T, C, N> moveList = new(ref Pos);
            foreach (Move move in moveList)
            {
                if (move == TTMove)
                {
                    return true;
                }
            }

            return false;
        }
    }
    [InlineArray(MAX_MOVES)]
    [StructLayout(LayoutKind.Sequential)]
    public struct MoveScore_Date
    {
        private Value Raw;

    }
}
