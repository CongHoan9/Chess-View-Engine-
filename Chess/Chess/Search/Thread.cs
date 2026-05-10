namespace Chess
{
    using Nodes = UInt64;
    public sealed class Search_Thread
    {
        public readonly History History = new();
        public readonly TimeMan TimeMan = new();
        public readonly Move[] Killer_0 = new Move[Search.MaxPly];
        public readonly Move[] Killer_1 = new Move[Search.MaxPly];
        public Nodes Nodes;
        public Nodes NodeLimit;
        public int SelDepth;
        public bool Stop;

        public void Clear()
        {
            History.Clear();
            Array.Fill(Killer_0, Move.None());
            Array.Fill(Killer_1, Move.None());
            Begin_Search();
            TimeMan.Clear();
        }

        public void Begin_Search(Nodes nodelimit = 0)
        {
            Nodes = 0;
            NodeLimit = nodelimit;
            SelDepth = 0;
            Stop = false;
        }

        public void Add_Killer(int ply, Move move)
        {
            if (ply < 0 || ply >= Killer_0.Length || move == Move.None())
            {
                return;
            }
            if (Killer_0[ply] == move)
            {
                return;
            }
            Killer_1[ply] = Killer_0[ply];
            Killer_0[ply] = move;
        }
    }
}
