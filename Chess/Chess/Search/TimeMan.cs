using System.Diagnostics;

namespace Chess
{
    // The TimeManagement class computes the optimal time to think depending on
    // the maximum available time, the game move number, and other parameters.
    public sealed class TimeMan
    {
        private readonly Stopwatch Watch = new();

        private long StartTime;
        private long OptimumTime;
        private long MaximumTime;

        private long AvailableNodes = -1;   // When in 'nodes as time' mode
        private bool UseNodesTime = false;  // True if we are in 'nodes as time' mode

        public void Init(Search_Limits limits, Color us, int ply, UCIOption_Map options)
        {
            double originalTimeAdjust = -1.0;

            if (us == Color.WHITE)
                Init_Core<White, Black>(limits, ply, options, ref originalTimeAdjust);
            else
                Init_Core<Black, White>(limits, ply, options, ref originalTimeAdjust);
        }

        public void Init<C, N>(Search_Limits limits, int ply, UCIOption_Map options) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            double originalTimeAdjust = -1.0;
            Init_Core<C, N>(limits, ply, options, ref originalTimeAdjust);
        }

        public long Optimum() { return OptimumTime; }
        public long Maximum() { return MaximumTime; }
        public long Elapsed() { return Elapsed_Time(); }
        public long Elapsed(Func<long> nodes) { return UseNodesTime ? nodes() : Elapsed_Time(); }
        public bool Can_Start_Next_Depth() { return OptimumTime == 0 || Elapsed_Time() < OptimumTime; }
        public bool Should_Stop() { return MaximumTime > 0 && Elapsed_Time() >= MaximumTime; }

        public long Elapsed_Time() {
            return Watch.IsRunning ? Watch.ElapsedMilliseconds : StartTime;
        }

        public void Clear() {
            Watch.Reset();
            StartTime = 0;
            OptimumTime = 0;
            MaximumTime = 0;
            AvailableNodes = -1;
            UseNodesTime = false;
        }

        public void Advance_Nodes_Time(long nodes) {
            if (UseNodesTime)
                AvailableNodes = Math.Max(0, AvailableNodes - nodes);
        }

        // Called at the beginning of the search and calculates
        // the bounds of time allowed for the current game ply. We currently support:
        //      1) x basetime (+ z increment)
        //      2) x moves in y seconds (+ z increment)
        private void Init_Core<C, N>(Search_Limits limits,
                                  int ply,
                                  UCIOption_Map options,
                                  ref double originalTimeAdjust) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            long npmsec = options?.Get_Int("nodestime", 0) ?? 0;

            // If we have no time, we don't need to fully initialize TM.
            // startTime is used by movetime and useNodesTime is used in elapsed calls.
            Watch.Restart();
            StartTime = 0;
            UseNodesTime = npmsec != 0;

            long time = C.Value == Color.WHITE ? limits.WhiteTime : limits.BlackTime;
            long inc = C.Value == Color.WHITE ? limits.WhiteInc : limits.BlackInc;

            if (limits.MoveTime > 0)
            {
                long moveOverhead = options?.Get_Int("Move Overhead", 10) ?? 10;
                OptimumTime = Math.Max(1, limits.MoveTime - moveOverhead);
                MaximumTime = OptimumTime;
                return;
            }

            if (time == 0)
            {
                OptimumTime = 0;
                MaximumTime = 0;
                return;
            }

            long moveOverheadScaled = options?.Get_Int("Move Overhead", 10) ?? 10;

            // If we have to play in 'nodes as time' mode, then convert from time
            // to nodes, and use resulting values in time management formulas.
            // WARNING: to avoid time losses, the given npmsec (nodes per millisecond)
            // must be much lower than the real engine speed.
            if (UseNodesTime)
            {
                if (AvailableNodes == -1)  // Only once at game start
                    AvailableNodes = npmsec * time;

                // Convert from milliseconds to nodes
                time = AvailableNodes;
                inc *= npmsec;
                moveOverheadScaled *= npmsec;
            }

            // These numbers are used where multiplications, divisions or comparisons
            // with constants are involved.
            long scaledTime = UseNodesTime ? time / Math.Max(1, npmsec) : time;

            // Maximum move horizon
            int centiMTG = limits.MovesToGo > 0 ? Math.Min(limits.MovesToGo * 100, 5000) : 5051;

            // If less than one second, gradually reduce mtg
            if (scaledTime < 1000)
                centiMTG = (int) (scaledTime * 5.051);

            // Make sure timeLeft is > 0 since we may use it as a divisor
            long timeLeft = Math.Max(
              1,
              time + (inc * (centiMTG - 100) - moveOverheadScaled * (200 + centiMTG)) / 100);

            // optScale is a percentage of available time to use for the current move.
            // maxScale is a multiplier applied to optimumTime.
            double optScale, maxScale;

            // x basetime (+ z increment)
            // If there is a healthy increment, timeLeft can exceed the actual available
            // game time for the current move, so also cap to a percentage of available game time.
            if (limits.MovesToGo == 0)
            {
                // Extra time according to timeLeft
                if (originalTimeAdjust < 0)
                    originalTimeAdjust = 0.3128 * Math.Log10(timeLeft) - 0.4354;

                // Calculate time constants based on current time left.
                double logTimeInSec = Math.Log10(Math.Max(1.0, scaledTime) / 1000.0);
                double optConstant  = Math.Min(0.0032116 + 0.000321123 * logTimeInSec, 0.00508017);
                double maxConstant  = Math.Max(3.3977 + 3.03950 * logTimeInSec, 2.94761);

                optScale = Math.Min(0.0121431 + Math.Pow(ply + 2.94693, 0.461073) * optConstant,
                                    0.213035 * time / (double) timeLeft)
                         * originalTimeAdjust;

                maxScale = Math.Min(6.67704, maxConstant + ply / 11.9847);
            }

            // x moves in y seconds (+ z increment)
            else
            {
                optScale = Math.Min((0.88 + ply / 116.4) / (centiMTG / 100.0),
                                    0.88 * time / (double) timeLeft);
                maxScale = 1.3 + 0.11 * (centiMTG / 100.0);
            }

            // Limit the maximum possible time for this move
            OptimumTime = (long) (optScale * timeLeft);
            MaximumTime =
              (long) Math.Min(0.825179 * time - moveOverheadScaled, maxScale * OptimumTime) - 10;

            if (options?.Get_Bool("Ponder") == true)
                OptimumTime += OptimumTime / 4;

            OptimumTime = Math.Max(1, OptimumTime);
            MaximumTime = Math.Max(OptimumTime, MaximumTime);
        }
    }
}
