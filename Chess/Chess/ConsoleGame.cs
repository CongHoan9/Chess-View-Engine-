using System.IO;
using System.Text;
using static Chess.Color;

namespace Chess
{
    public static class ConsoleGame
    {
        private const int DefaultPlayDepth = 8;
        private const int DefaultMatchDepth = 8;
        private const int DefaultMatchPlies = 80;
        private const int MaxMatchPlies = 400;
        public static void Run()
        {
            using Engine engine = new();
            UCI uci = new(engine);

            Try_Clear_Console();
            Print_Banner();

            while (true)
            {
                Console.Write("uci> ");
                string line = Console.ReadLine();
                if (line == null)
                {
                    return;
                }

                string trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (Try_Execute_Local_Command(trimmed, engine, out bool shouldQuit, out string response))
                {
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        Console.WriteLine(response);
                    }

                    if (shouldQuit)
                    {
                        return;
                    }

                    continue;
                }
                string uciResponse = uci.Execute(trimmed);
                if (string.Equals(uciResponse, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if (!string.IsNullOrWhiteSpace(uciResponse))
                {
                    Console.WriteLine(uciResponse);
                }
            }
        }

        private static bool Try_Execute_Local_Command(string line, Engine engine, out bool shouldQuit, out string response)
        {
            shouldQuit = false;
            response = "";
            string[] tokens = Misc.Split_Tokens(line);
            if (tokens.Length == 0)
            {
                return false;
            }

            switch (tokens[0].ToLowerInvariant())
            {
                case "help":
                case "?":
                case "commands":
                    response = Get_Help_Text();
                    return true;
                case "cls":
                case "clear":
                    Try_Clear_Console();
                    Print_Banner();
                    return true;
                case "play":
                    Run_Play_Command(engine, tokens);
                    return true;
                case "match":
                    Run_Match_Command(engine, tokens);
                    return true;
                default:
                    return false;
            }
        }

        private static void Run_Play_Command(Engine engine, string[] tokens)
        {
            PlaySettings settings = Parse_Play_Settings(tokens);
            Console.WriteLine("Play mode");
            Console.WriteLine($"Ban cam {Color_Name(settings.HumanColor)} | {Format_Limit_Summary(settings.EngineLimits)}");
            Console.WriteLine("Dung 'position ...' truoc neu muon doi vi tri.");
            Console.WriteLine("Trong luc choi co the nhap: nuoc UCI, d, fen, eval, hint, help, end.");
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(engine.pos.Show(engine.pos.SideToMove));
                if (Try_End_Game(engine, "YOU", "ENGINE"))
                {
                    Console.WriteLine($"Fen end: {engine.Fen()}");
                    return;
                }
                if (engine.pos.SideToMove == settings.HumanColor)
                {
                    if (!Try_Read_Player_Move(engine, settings.EngineLimits, out Move playerMove))
                    {
                        Console.WriteLine("Da thoat play mode.");
                        return;
                    }
                    engine.Play_Move(playerMove);
                    continue;
                }
                Console.WriteLine("Engine dang tim nuoc di...");
                Search_Result result = engine.Go(settings.EngineLimits);
                Move bestMove = result.BestMove;
                if (bestMove == Move.None())
                {
                    Console.WriteLine("Engine khong tim thay nuoc di hop le.");
                    return;
                }
                string bestMoveText = UCI.Move_To_String(ref engine.pos, bestMove);
                Console.WriteLine($"Engine moved: {bestMoveText}");
                Console.WriteLine(Format_Search_Result(result));
                engine.Play_Move(bestMove);
            }
        }
        private static void Run_Match_Command(Engine sourceEngine, string[] tokens)
        {
            MatchSettings settings = Parse_Match_Settings(tokens);
            using Engine whiteEngine = Create_Snapshot_Engine(sourceEngine);
            using Engine blackEngine = Create_Snapshot_Engine(sourceEngine);
            Console.WriteLine("Match mode");
            Console.WriteLine($"WHITE: {Format_Limit_Summary(settings.WhiteLimits)}");
            Console.WriteLine($"BLACK: {Format_Limit_Summary(settings.BlackLimits)}");
            Console.WriteLine($"max plies {settings.MaxPlies}");
            Console.WriteLine($"FEN: {sourceEngine.Fen()}");

            for (int ply = 1; ply <= settings.MaxPlies; ++ply)
            {
                Engine current = whiteEngine.pos.SideToMove == WHITE ? whiteEngine : blackEngine;
                Search_Limits limits = current.pos.SideToMove == WHITE ? settings.WhiteLimits : settings.BlackLimits;
                string sideLabel = current.pos.SideToMove == WHITE ? "WHITE" : "BLACK";
                Console.WriteLine();
                Console.WriteLine($"Ply {ply} | {sideLabel}");
                Console.WriteLine(current.pos.Show(current.pos.SideToMove));

                if (Try_End_Game(current, "WHITE", "BLACK"))
                {
                    Console.WriteLine($"Fen end: {current.Fen()}");
                    return;
                }

                Search_Result result = current.Go(limits);
                Move bestMove = result.BestMove;
                if (bestMove == Move.None())
                {
                    Console.WriteLine($"{sideLabel} khong tim thay nuoc di hop le.");
                    Console.WriteLine($"Fen cuoi: {current.Fen()}");
                    return;
                }

                string moveText = UCI.Move_To_String(ref current.pos, bestMove);
                Console.WriteLine($"{sideLabel} di: {moveText}");
                Console.WriteLine(Format_Search_Result(result));

                whiteEngine.Play_Move(bestMove);
                blackEngine.Play_Move(bestMove);
            }

            Console.WriteLine();
            Console.WriteLine($"Da dat gioi han {settings.MaxPlies} ply, tran dau tam dung.");
            Console.WriteLine($"Fen cuoi: {whiteEngine.Fen()}");
        }

        private static PlaySettings Parse_Play_Settings(string[] tokens)
        {
            Color humanColor = WHITE;
            foreach (string token in tokens[1..])
            {
                switch (token.ToLowerInvariant())
                {
                    case "white":
                    case "w":
                        humanColor = WHITE;
                        break;
                    case "black":
                    case "b":
                        humanColor = BLACK;
                        break;
                    case "random":
                    case "r":
                        humanColor = Random.Shared.Next(2) == 0 ? WHITE : BLACK;
                        break;
                }
            }

            Search_Limits limits = UCI.Parse_Limits(tokens);
            limits = Ensure_Search_Limit(limits, DefaultPlayDepth);
            return new PlaySettings(humanColor, limits);
        }

        private static MatchSettings Parse_Match_Settings(string[] tokens)
        {
            List<string> sharedArgs = [];
            List<string> whiteArgs = [];
            List<string> blackArgs = [];
            int maxPlies = DefaultMatchPlies;
            MatchSegment segment = MatchSegment.Shared;

            for (int i = 1; i < tokens.Length; ++i)
            {
                string token = tokens[i].ToLowerInvariant();
                switch (token)
                {
                    case "white":
                        segment = MatchSegment.White;
                        continue;
                    case "black":
                        segment = MatchSegment.Black;
                        continue;
                    case "plies":
                        if (i + 1 < tokens.Length && int.TryParse(tokens[i + 1], out int parsedPlies))
                        {
                            maxPlies = Math.Clamp(parsedPlies, 1, MaxMatchPlies);
                            ++i;
                        }
                        continue;
                }
                switch (segment)
                {
                    case MatchSegment.White:
                        whiteArgs.Add(tokens[i]);
                        break;
                    case MatchSegment.Black:
                        blackArgs.Add(tokens[i]);
                        break;
                    default:
                        sharedArgs.Add(tokens[i]);
                        break;
                }
            }
            Search_Limits sharedLimits = Ensure_Search_Limit(Parse_Limits_From_Args(sharedArgs), DefaultMatchDepth);
            Search_Limits whiteLimits = Ensure_Search_Limit(Merge_Limits(sharedLimits, Parse_Limits_From_Args(whiteArgs)), DefaultMatchDepth);
            Search_Limits blackLimits = Ensure_Search_Limit(Merge_Limits(sharedLimits, Parse_Limits_From_Args(blackArgs)), DefaultMatchDepth);
            return new MatchSettings(whiteLimits, blackLimits, maxPlies);
        }

        private static Search_Limits Parse_Limits_From_Args(List<string> args)
        {
            if (args.Count == 0)
            {
                return new Search_Limits();
            }
            string[] tokens = new string[args.Count + 1];
            tokens[0] = "go";
            for (int i = 0; i < args.Count; ++i)
            {
                tokens[i + 1] = args[i];
            }
            return UCI.Parse_Limits(tokens);
        }

        private static Search_Limits Ensure_Search_Limit(Search_Limits limits, int defaultDepth)
        {
            if (Has_Search_Budget(limits))
            {
                return limits;
            }
            limits.Depth = defaultDepth;
            return limits;
        }

        private static bool Has_Search_Budget(Search_Limits limits)
        {
            return limits.Depth > 0
                || limits.MoveTime > 0
                || limits.WhiteTime > 0
                || limits.BlackTime > 0
                || limits.Nodes > 0
                || limits.Infinite;
        }

        private static Search_Limits Merge_Limits(Search_Limits baseLimits, Search_Limits overrides)
        {
            Search_Limits merged = baseLimits;
            if (overrides.Depth > 0)
            {
                merged.Depth = overrides.Depth;
            }
            if (overrides.WhiteTime > 0)
            {
                merged.WhiteTime = overrides.WhiteTime;
            }
            if (overrides.BlackTime > 0)
            {
                merged.BlackTime = overrides.BlackTime;
            }
            if (overrides.WhiteInc > 0)
            {
                merged.WhiteInc = overrides.WhiteInc;
            }
            if (overrides.BlackInc > 0)
            {
                merged.BlackInc = overrides.BlackInc;
            }
            if (overrides.MovesToGo > 0)
            {
                merged.MovesToGo = overrides.MovesToGo;
            }
            if (overrides.MoveTime > 0)
            {
                merged.MoveTime = overrides.MoveTime;
            }
            if (overrides.Nodes > 0)
            {
                merged.Nodes = overrides.Nodes;
            }
            if (overrides.Perft > 0)
            {
                merged.Perft = overrides.Perft;
            }
            if (overrides.Infinite)
            {
                merged.Infinite = true;
            }

            return merged;
        }
        private static Engine Create_Snapshot_Engine(Engine source)
        {
            Engine snapshot = new();
            snapshot.Set_Position(source.Fen(), null, source.pos.IsChess960());
            return snapshot;
        }
        private static bool Try_Read_Player_Move(Engine engine, Search_Limits limits, out Move move)
        {
            while (true)
            {
                Console.Write("You> ");
                string input = Console.ReadLine();
                if (input == null)
                {
                    move = Move.None();
                    return false;
                }
                string trimmed = input.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                switch (trimmed.ToLowerInvariant())
                {
                    case "end":
                    case "exit":
                        move = Move.None();
                        return false;
                    case "d":
                    case "board":
                        Console.WriteLine(engine.pos.Show(engine.pos.SideToMove));
                        continue;
                    case "fen":
                        Console.WriteLine(engine.Fen());
                        continue;
                    case "eval":
                        Console.WriteLine(engine.Trace_Eval());
                        continue;
                    case "help":
                    case "?":
                        Console.WriteLine(Get_Play_Help_Text());
                        continue;
                    case "hint":
                        Search_Result hint = engine.Go(limits);
                        if (hint.BestMove == Move.None())
                        {
                            Console.WriteLine("Khong tim thay hint hop le.");
                        }
                        else
                        {
                            Console.WriteLine($"Hint: {UCI.Move_To_String(ref engine.pos, hint.BestMove)} | {Format_Search_Result(hint)}");
                        }
                        continue;
                }
                if (engine.Try_Parse_Move(trimmed, out move))
                {
                    return true;
                }
                Console.WriteLine("Nuoc di khong hop le. Hay nhap theo UCI, vi du: e2e4 hoac e7e8q.");
            }
        }

        private static bool Try_End_Game(Engine engine, string whiteLabel, string blackLabel)
        {
            if (Is_Draw(ref engine.pos))
            {
                Console.WriteLine(Get_Draw_Message(ref engine.pos));
                return true;
            }
            if (Has_Legal_Moves(ref engine.pos))
            {
                return false;
            }
            if (engine.pos.Checkers() != 0)
            {
                Console.WriteLine($"CHECKMATE. {Side_Label(engine.pos.SideToMove == WHITE ? BLACK : WHITE, whiteLabel, blackLabel)} WIN.");
            }
            else
            {
                Console.WriteLine("STALEMATE.");
            }
            return true;
        }
        private static string Side_Label(Color side, string whiteLabel, string blackLabel)
        {
            return side == WHITE ? whiteLabel : blackLabel;
        }
        private static string Format_Search_Result(Search_Result result)
        {
            return $"Danh gia: {new Score(result.Score)} | depth {result.Depth} | nodes {result.Nodes:N0} | time {result.Time.TotalMilliseconds:F0} ms";
        }
        private static string Format_Limit_Summary(Search_Limits limits)
        {
            if (limits.MoveTime > 0)
            {
                return $"movetime {limits.MoveTime} ms";
            }
            if (limits.Depth > 0)
            {
                return $"depth {limits.Depth}";
            }
            if (limits.WhiteTime > 0 || limits.BlackTime > 0)
            {
                StringBuilder builder = new();
                builder.Append($"wtime {limits.WhiteTime} btime {limits.BlackTime}");
                if (limits.WhiteInc > 0 || limits.BlackInc > 0)
                {
                    builder.Append($" winc {limits.WhiteInc} binc {limits.BlackInc}");
                }
                if (limits.MovesToGo > 0)
                {
                    builder.Append($" movestogo {limits.MovesToGo}");
                }
                return builder.ToString();
            }
            if (limits.Nodes > 0)
            {
                return $"nodes {limits.Nodes}";
            }
            if (limits.Infinite)
            {
                return "infinite";
            }
            return $"depth {DefaultPlayDepth}";
        }
        private static string Color_Name(Color color)
        {
            return color == WHITE ? "WHITE" : "BLACK";
        }
        private static void Print_Banner()
        {
            Console.WriteLine(Misc.Engine_Info());
            Console.WriteLine("UCI command shell");
            Console.WriteLine("Lenh nhanh: help, guide, position, go, perft, play, match, quit.");
            Console.WriteLine();
        }
        private static string Get_Help_Text()
        {
            return 
            """
            Lenh shell chinh:
                help
                guide
                clear
                play [white|black|random] [go-args]
                match [go-args chung] [white go-args] [black go-args] [plies N]

            Lenh UCI/chuan:
                uci
                isready
                ucinewgame
                setoption name Hash value 128
                position startpos
                position startpos moves e2e4 e7e5
                position fen <FEN>
                go depth 8
                go movetime 1000
                go wtime 300000 btime 300000 winc 0 binc 0
                perft 5
                go perft 5
                d
                fen
                eval
                quit

            Vi du nhanh:
                position startpos
                play white depth 8
                position startpos moves e2e4 e7e5
                match depth 6 plies 40
                match depth 8 white movetime 500 black movetime 1000 plies 60
            """;
        }

        private static string Get_Play_Help_Text()
        {
            return 
            """
            Play mode:
                Nhap nuoc di theo UCI, vi du: e2e4, g1f3, e7e8q
                d       : in ban co
                fen     : in FEN hien tai
                eval    : in trace eval
                hint    : engine goi y nuoc
                help    : in huong dan ngan
                end     : thoat play mode
            """;
        }
        private static void Try_Clear_Console()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
        private static bool Has_Legal_Moves(ref Position pos)
        {
            return pos.SideToMove == WHITE
                ? new MoveList<Legal, White, Black>(ref pos).Size() != 0
                : new MoveList<Legal, Black, White>(ref pos).Size() != 0;
        }

        private static unsafe bool Is_Draw(ref Position pos)
        {
            return pos.st->Rule50 >= 100 || pos.st->Repetition != 0;
        }

        private static unsafe string Get_Draw_Message(ref Position pos)
        {
            return pos.st->Rule50 >= 100 ? "Hoa do luat 50 nuoc." : "Hoa do lap lai vi tri.";
        }

        private enum MatchSegment
        {
            Shared,
            White,
            Black,
        }

        private readonly record struct PlaySettings(Color HumanColor, Search_Limits EngineLimits);

        private readonly record struct MatchSettings(Search_Limits WhiteLimits, Search_Limits BlackLimits, int MaxPlies);
    }
}
