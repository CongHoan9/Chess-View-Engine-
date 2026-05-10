using System.Runtime.CompilerServices;

namespace Chess
{
    using Depth = Int32;
    using Nodes = UInt64;
    unsafe public sealed class Engine : IDisposable
    {
        private const int StateCapacity = Search.MaxPly + 256;
        private readonly StateInfo* StateList;
        private int StateIndex;
        private bool Disposed;
        public Position pos = new();
        public Search_Thread MainThread { get; } = new();
        public UCIOption_Map Options { get; } = [];
        public NnueNetworks Networks { get; } = new();
        public bool Chess960 { get; private set; }
        public string StartFen { get; private set; } = Fens.Defaults[0];

        public Engine()
        {
            StateList = Memory.Allocate_Array<StateInfo>(StateCapacity);
            Init_Default_Options();
            TranspositionTable.Resize(Options.Get_Int("Hash", 64));
            Load_Big_Network(Options.Get_String("EvalFileBig"));
            Load_Small_Network(Options.Get_String("EvalFileSmall"));
            Set_Position(StartFen);
        }
        ~Engine()
        {
            Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref StateInfo State_At(int index)
        {
            return ref StateList[index];
        }
        public void Set_Position(string fen, IReadOnlyList<string> moves = null, bool isChess960 = false)
        {
            Chess960 = isChess960;
            StartFen = string.IsNullOrWhiteSpace(fen) ? Fens.Defaults[0] : fen;
            StateIndex = 0;
            pos.Set(StartFen, Chess960, StateList);
            if (moves != null)
            {
                foreach (string movetext in moves)
                {
                    if (Try_Parse_Move(movetext, out Move move))
                    {
                        Play_Move(move);
                    }
                }
            }
        }
        public void New_Game()
        {
            MainThread.Clear();
            TranspositionTable.Clear();
            Set_Position(Fens.Defaults[0], null, Options.Get_Bool("UCI_Chess960"));
        }
        public bool Try_Parse_Move(string movetext, out Move move)
        {
            return UCI.Try_Parse_Move(ref pos, movetext, out move);
        }
        public void Play_Move(Move move)
        {
            if (StateIndex + 1 >= StateCapacity)
            {
                throw new InvalidOperationException("State list overflow");
            }
            ++StateIndex;
            if (pos.SideToMove == Color.WHITE)
            {
                Play_Move<White, Black>(move);
            }
            else
            {
                Play_Move<Black, White>(move);
            }
        }
        public Search_Result Go(Search_Limits limits)
        {
            if (limits.Depth <= 0 && limits.MoveTime <= 0 && limits.WhiteTime <= 0 && limits.BlackTime <= 0 && limits.Nodes <= 0)
            {
                limits.Depth = 6;
            }
            return pos.SideToMove == Color.WHITE ? Go<White, Black>(limits) : Go<Black, White>(limits);
        }
        public void Stop()
        {
            MainThread.Stop = true;
        }
        public Nodes Perft(Depth depth)
        {
            return Chess.Perft.Count(ref pos, depth);
        }
        public static Nodes Perft(string fen, Depth depth, bool isChess960 = false)
        {
            return Chess.Perft.Run(fen, depth, isChess960);
        }
        public void Load_Big_Network(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                Networks.Big.Load(filename);
            }
        }
        public void Load_Small_Network(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                Networks.Small.Load(filename);
            }
        }
        public string Trace_Eval()
        {
            return EvaluateNNUE.Trace(ref pos, Networks);
        }
        public string Fen()
        {
            return pos.Fen(pos.SideToMove);
        }
        private void Play_Move<C, N>(Move move) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            pos.Do_Move<C, N>(move, ref State_At(StateIndex));
        }
        private Search_Result Go<C, N>(Search_Limits limits) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            MainThread.TimeMan.Init<C, N>(limits, pos.gamePly, Options);
            return Search.Root_Search<C, N>(ref pos, limits, MainThread, Options.Get_Bool("Use NNUE", true) ? Networks : null);
        }
        private void Init_Default_Options()
        {
            Options.Add(new UCIOption("Ponder", UCIOption_Type.CHECK, "false"));
            Options.Add(new UCIOption("Threads", UCIOption_Type.SPIN, "1", 1, 1));
            Options.Add(new UCIOption("Hash",
                                      UCIOption_Type.SPIN,
                                      "64",
                                      1,
                                      2048,
                                      on_change: option => TranspositionTable.Resize(option.Int_Value())));
            Options.Add(new UCIOption("Clear Hash", UCIOption_Type.BUTTON, string.Empty, on_change: _ => TranspositionTable.Clear()));
            Options.Add(new UCIOption("Move Overhead", UCIOption_Type.SPIN, "10", 0, 1000));
            Options.Add(new UCIOption("UCI_Chess960", UCIOption_Type.CHECK, "false", on_change: option => Chess960 = option.Bool_Value()));
            Options.Add(new UCIOption("Use NNUE", UCIOption_Type.CHECK, "true"));
            Options.Add(new UCIOption("EvalFileBig", UCIOption_Type.STRING, EvaluateNNUE.EvalFileDefaultNameBig, on_change: option => Load_Big_Network(option.Current_Value())));
            Options.Add(new UCIOption("EvalFileSmall", UCIOption_Type.STRING, EvaluateNNUE.EvalFileDefaultNameSmall, on_change: option => Load_Small_Network(option.Current_Value())));
        }
        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            Memory.Free(StateList);
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
