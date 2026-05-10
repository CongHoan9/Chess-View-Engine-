using System.IO;
using System.Text;
using static Chess.File;
using static Chess.FuncBit;
using static Chess.MoveType;

namespace Chess
{
    using Value = Int32;
    using Depth = Int32;
    using Nodes = UInt64;
    public sealed class UCI(Engine engine)
    {
        private readonly Engine Engine = engine;
        public void Loop(TextReader input, TextWriter output)
        {
            while (true)
            {
                string line = input.ReadLine();
                if (line == null)
                {
                    break;
                }
                string response = Execute(line);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    output.WriteLine(response);
                    output.Flush();
                }
                if (string.Equals(response, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
        }

        public string Execute(string line)
        {
            string[] tokens = Misc.Split_Tokens(line);
            if (tokens.Length == 0)
            {
                return string.Empty;
            }
            string command = Misc.To_Lower(tokens[0]);
            return command switch
            {
                "uci" => Print_UCI(),
                "isready" => "readyok",
                "ucinewgame" => Handle_New_Game(),
                "position" => Handle_Position(tokens),
                "setoption" => Handle_SetOption(tokens),
                "go" => Handle_Go(tokens),
                "stop" => Handle_Stop(),
                "d" => Engine.pos.Show(Engine.pos.SideToMove),
                "fen" => Engine.Fen(),
                "eval" => Engine.Trace_Eval(),
                "perft" => Handle_Perft(tokens),
                "quit" => "quit",
                _ => string.Empty,
            };
        }

        public static bool Try_Parse_Move(ref Position pos, string movetext, out Move move)
        {
            return pos.SideToMove == Color.WHITE ? Try_Parse_Move<White, Black>(ref pos, movetext, out move) : Try_Parse_Move<Black, White>(ref pos, movetext, out move);
        }
        public static string Move_To_String(ref Position pos, Move move)
        {
            if (move == Move.None())
            {
                return "(none)";
            }
            if (move == Move.MoveNull)
            {
                return "0000";
            }
            Square from = move.From_Sq();
            Square to = move.To_Sq();
            if (move.Type_Of() == CASTLING && !pos.IsChess960())
            {
                to = Make_Square(to > from ? FILE_G : FILE_C, Rank_Of(from));
            }
            Span<char> text = stackalloc char[5];
            text[0] = (char)('a' + (int)File_Of(from));
            text[1] = (char)('1' + (int)Rank_Of(from));
            text[2] = (char)('a' + (int)File_Of(to));
            text[3] = (char)('1' + (int)Rank_Of(to));
            if (move.Type_Of() == PROMOTION)
            {
                text[4] = move.Promotion_Type() switch
                {
                    PieceType.KNIGHT => 'n',
                    PieceType.BISHOP => 'b',
                    PieceType.ROOK => 'r',
                    _ => 'q'
                };
                return new string(text);
            }
            return new string(text[..4]);
        }

        private static bool Try_Parse_Move<C, N>(ref Position pos, string movetext, out Move move) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            string target = movetext.Trim().ToLowerInvariant();
            MoveList<Legal, C, N> moves = new(ref pos);
            foreach (Move candidate in moves)
            {
                if (Move_To_String(ref pos, candidate).Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    move = candidate;
                    return true;
                }
            }
            move = Move.None();
            return false;
        }

        private string Print_UCI()
        {
            StringBuilder builder = new();
            builder.AppendLine(Misc.Engine_Info(true));
            foreach (UCIOption option in Engine.Options)
            {
                builder.AppendLine(option.To_UCI());
            }
            builder.Append("uciok");
            return builder.ToString();
        }

        private string Handle_New_Game()
        {
            Engine.New_Game();
            return string.Empty;
        }

        private string Handle_SetOption(string[] tokens)
        {
            int nameindex = Array.FindIndex(tokens, token => token.Equals("name", StringComparison.OrdinalIgnoreCase));
            int valueindex = Array.FindIndex(tokens, token => token.Equals("value", StringComparison.OrdinalIgnoreCase));
            if (nameindex < 0)
            {
                return string.Empty;
            }
            string name = valueindex > nameindex ? string.Join(' ', tokens[(nameindex + 1)..valueindex]) : string.Join(' ', tokens[(nameindex + 1)..]);
            string value = valueindex >= 0 && valueindex + 1 < tokens.Length ? string.Join(' ', tokens[(valueindex + 1)..]) : string.Empty;
            Engine.Options.Set_Option(name, value);
            return string.Empty;
        }

        private string Handle_Position(string[] tokens)
        {
            string fen = Fens.Defaults[0];
            List<string> moves = [];
            if (tokens.Length >= 2 && tokens[1].Equals("startpos", StringComparison.OrdinalIgnoreCase))
            {
                fen = Fens.Defaults[0];
            }
            else if (tokens.Length >= 3 && tokens[1].Equals("fen", StringComparison.OrdinalIgnoreCase))
            {
                int movesindex = Array.FindIndex(tokens, token => token.Equals("moves", StringComparison.OrdinalIgnoreCase));
                int fenend = movesindex >= 0 ? movesindex : tokens.Length;
                fen = string.Join(' ', tokens[2..fenend]);
            }
            int movestokenindex = Array.FindIndex(tokens, token => token.Equals("moves", StringComparison.OrdinalIgnoreCase));
            if (movestokenindex >= 0)
            {
                for (int i = movestokenindex + 1; i < tokens.Length; ++i)
                {
                    moves.Add(tokens[i]);
                }
            }
            Engine.Set_Position(fen, moves, Engine.Options.Get_Bool("UCI_Chess960"));
            return string.Empty;
        }

        private string Handle_Go(string[] tokens)
        {
            Search_Limits limits = Parse_Limits(tokens);
            if (limits.Perft > 0)
            {
                Handle_Perft(limits.Perft);
            }
            Search_Result result = Engine.Go(limits);
            string bestmove = result.BestMove == Move.None() ? "0000" : Move_To_String(ref Engine.pos, result.BestMove);
            Console.WriteLine($"info depth {result.Depth} score {Format_Score(result.Score)} nodes {result.Nodes} time {(long)result.Time.TotalMilliseconds} pv {bestmove}\nbestmove {bestmove}");
            return "\n";
        }

        private string Handle_Stop()
        {
            Engine.Stop();
            return string.Empty;
        }

        private static string Handle_Perft(string[] tokens)
        {
            Depth depth = tokens.Length >= 2 && Misc.Try_Parse_Int(tokens[1], out int parseddepth) ? parseddepth : 1;
            Handle_Perft(depth);
            return "\n";
        }

        private static void Handle_Perft(Depth depth)
        {
            Perft.Report("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Math.Max(1, depth));
        }

        public static Search_Limits Parse_Limits(string[] tokens)
        {
            Search_Limits limits = new();
            for (int i = 1; i < tokens.Length; ++i)
            {
                string token = tokens[i].ToLowerInvariant();
                if (i + 1 < tokens.Length && Misc.Try_Parse_Int(tokens[i + 1], out int value))
                {
                    switch (token)
                    {
                        case "depth":
                            limits.Depth = value;
                            ++i;
                            continue;
                        case "wtime":
                            limits.WhiteTime = value;
                            ++i;
                            continue;
                        case "btime":
                            limits.BlackTime = value;
                            ++i;
                            continue;
                        case "winc":
                            limits.WhiteInc = value;
                            ++i;
                            continue;
                        case "binc":
                            limits.BlackInc = value;
                            ++i;
                            continue;
                        case "movestogo":
                            limits.MovesToGo = value;
                            ++i;
                            continue;
                        case "movetime":
                            limits.MoveTime = value;
                            ++i;
                            continue;
                        case "nodes":
                            limits.Nodes = (Nodes)Math.Max(0, value);
                            ++i;
                            continue;
                        case "perft":
                            limits.Perft = value;
                            ++i;
                            continue;
                    }
                }
                if (token == "infinite")
                {
                    limits.Infinite = true;
                }
            }
            return limits;
        }

        private static string Format_Score(Value value)
        {
            Score score = new(value);
            return score.Is_Mate() ? $"mate {score.Mate_In()}" : $"cp {score.To_Centipawns()}";
        }
    }
}
