using System.Numerics;
using System.Runtime.CompilerServices;
namespace Chess
{
    public static class BitBoards
    {
        public static readonly BitBoard Board = 0xFFFFFFFFFFFFFFFFUL;
        public static readonly BitBoard FileABB = 0x0101010101010101UL;
        public static readonly BitBoard FileBBB = FileABB << 1;
        public static readonly BitBoard FileCBB = FileABB << 2;
        public static readonly BitBoard FileDBB = FileABB << 3;
        public static readonly BitBoard FileEBB = FileABB << 4;
        public static readonly BitBoard FileFBB = FileABB << 5;
        public static readonly BitBoard FileGBB = FileABB << 6;
        public static readonly BitBoard FileHBB = FileABB << 7;
        public static readonly BitBoard Rank1BB = 0x00000000000000FFUL;
        public static readonly BitBoard Rank2BB = Rank1BB << (8 * 1);
        public static readonly BitBoard Rank3BB = Rank1BB << (8 * 2);
        public static readonly BitBoard Rank4BB = Rank1BB << (8 * 3);
        public static readonly BitBoard Rank5BB = Rank1BB << (8 * 4);
        public static readonly BitBoard Rank6BB = Rank1BB << (8 * 5);
        public static readonly BitBoard Rank7BB = Rank1BB << (8 * 6);
        public static readonly BitBoard Rank8BB = Rank1BB << (8 * 7);
        public static readonly BitBoard NotFileABB = ~FileABB;
        public static readonly BitBoard NotFileHBB = ~FileHBB;
        public static readonly byte[] PopCnt16 = new byte[1 << 16];
        public static readonly byte[][] SquareDistance = new byte[64][];
        public static readonly BitBoard[][] betweenBB = new BitBoard[64][];
        public static readonly BitBoard[][] lineBB = new BitBoard[64][];
        public static readonly BitBoard[][] RayPassBB = new BitBoard[64][];
        public static readonly Magic[][] Magics = new Magic[64][];
        public static readonly BitBoard[] RookTable = new BitBoard[0x19000];
        public static readonly BitBoard[] BishopTable = new BitBoard[0x1480];
        public static readonly BitBoard[][] pseudoAttacks = InitPseudoAttacks();
        private static readonly PieceType[] Slider = [PieceType.Bishop, PieceType.Rook];
        static BitBoards()
        {
            for (int i = 0; i < (1 << 16); ++i)
            {
                PopCnt16[i] = (byte)BitOperations.PopCount((uint)i);
            }
            for (int s1 = (int)Square.SQ_A1; s1 <= (int)Square.SQ_H8; ++s1)
            {
                SquareDistance[s1] = new byte[64];
                betweenBB[s1] = new BitBoard[64];
                lineBB[s1] = new BitBoard[64];
                RayPassBB[s1] = new BitBoard[64];
                Magics[s1] = new Magic[2];
                for (int s2 = (int)Square.SQ_A1; s2 <= (int)Square.SQ_H8; ++s2)
                {
                    int fileDistance = Math.Abs(Types.FileOf((Square)s1) - Types.FileOf((Square)s2));
                    int rankDistance = Math.Abs(Types.RankOf((Square)s1) - Types.RankOf((Square)s2));
                    SquareDistance[s1][s2] = (byte)Math.Max(fileDistance, rankDistance);
                }
            }
            InitMagics(PieceType.Rook, RookTable, Magics);
            InitMagics(PieceType.Bishop, BishopTable, Magics);
            for (Square s1 = Square.SQ_A1; s1 <= Square.SQ_H8; ++s1)
            {
                foreach (PieceType pt in Slider)
                {
                    for (Square s2 = Square.SQ_A1; s2 <= Square.SQ_H8; ++s2)
                    {
                        int index1 = (int)s1;
                        int index2 = (int)s2;
                        if ((pseudoAttacks[(int)pt][index1] & s2) != 0)
                        {
                            lineBB[index1][index2] = (AttacksBB(pt, s1, 0) & AttacksBB(pt, s2, 0)) | s1 | s2;
                            betweenBB[index1][index2] = (AttacksBB(pt, s1, SquareBB(s2)) & AttacksBB(pt, s2, SquareBB(s1)));
                            RayPassBB[index1][index2] = AttacksBB(pt, s1, 0) & (AttacksBB(pt, s2, SquareBB(s1)) | s2);
                        }
                        betweenBB[index1][index2] |= s2;
                    }
                }
            }
        }
        private static void InitMagics(PieceType pt, BitBoard[] table, Magic[][] magics)
        {
            int[,] seeds = 
            {
                {8977,44560,54343,38998,5731,95205,104912,17020},
                {728,10316,55013,32803,12281,15100,16645,255}
            };
            BitBoard[] occupancy = new BitBoard[4096];
            int[] epoch = new int[4096];
            int cnt = 0;
            BitBoard[] reference = new BitBoard[4096];
            for (Square s = Square.SQ_A1; s <= Square.SQ_H8; s++)
            {
                BitBoard edges = ((Rank1BB | Rank8BB) & ~RankBB(s)) | ((FileABB | FileHBB) & ~FileBB(s));
                Magic m = magics[(int)s][(int)pt - (int)PieceType.Bishop];
                m.mask = SlidingAttack(pt, s, 0) & ~edges;
                m.Shift = 64 - BitOperations.PopCount(m.mask);
                if (s == Square.SQ_A1)
                {
                    m.attacks = table;
                }
                else
                {
                    m.attacks = magics[(int)s - 1][(int)pt - (int)PieceType.Bishop].attacks;
                }
                int size = 0;
                BitBoard b = 0;
                do
                {
                    occupancy[size] = b;
                    reference[size] = SlidingAttack(pt, s, b);
                    size++;
                    b = (b - m.mask) & m.mask;
                } 
                while (b != 0);
                Random rng = new(seeds[1, (int)Types.RankOf(s)]);
                for (int i = 0; i < size;)
                {
                    do
                    {
                        m.magic = rng.SparseRand();
                    }
                    while (BitOperations.PopCount((m.magic * m.mask) >> 56) < 6);
                    cnt++;
                    for (i = 0; i < size; i++)
                    {
                        int idx = m.Index(occupancy[i]);
                        if (epoch[idx] < cnt)
                        {
                            epoch[idx] = cnt;
                            m.attacks[idx] = reference[i];
                        }
                        else if (m.attacks[idx] != reference[i])
                        {
                            break;
                        }
                    }
                }
                magics[(int)s][(int)pt - (int)PieceType.Bishop] = m;
            }
        }
        private static BitBoard[][] InitPseudoAttacks()
        {
            int PieceTypeNB = (int)PieceType.PieceTypeNB;
            var attacks = new BitBoard[PieceTypeNB][];
            for (Square s = 0; s < Square.SquareNB; s++)
            {
                int index = (int)s;
                attacks[(int)Color.White][index] = PawnAttacksBB<White>(SquareBB(s));
                attacks[(int)Color.Black][index] = PawnAttacksBB<Black>(SquareBB(s));
                attacks[(int)PieceType.King][index] = PseudoAttacks(PieceType.King, s);
                attacks[(int)PieceType.Knight][index] = PseudoAttacks(PieceType.Knight, s);
                attacks[(int)PieceType.Bishop][index] = PseudoAttacks(PieceType.Bishop, s);
                attacks[(int)PieceType.Rook][index] = PseudoAttacks(PieceType.Rook, s);
                attacks[(int)PieceType.Queen][index] = attacks[(int)PieceType.Bishop][index] | attacks[(int)PieceType.Rook][index];
            }
            return attacks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard SafeDestination(Square s, int step)
        {
            Square to = (Square)((int)s + step);
            return Types.IsOk(to) && Math.Abs((int)Types.FileOf(s) - (int)Types.FileOf(to)) <= 2 ? SquareBB(to) : SquareBB(0);
        }
        private static readonly Direction[] RookDirections = [Direction.North, Direction.South, Direction.East, Direction.West];
        private static readonly Direction[] BishopDirections = [Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard SlidingAttack(PieceType pt, Square sq, BitBoard occupied)
        {
            BitBoard attacks = 0;
            foreach (Direction d in pt == PieceType.Rook ? RookDirections : BishopDirections)
            {
                Square s = sq;
                while (SafeDestination(s, (int)d) != 0)
                {
                    attacks |= (s += (int)d);
                    if ((occupied & s) != 0)
                    {
                        break;
                    }
                }
            }
            return attacks;
        }
        private static readonly int[] KnightSteps = [-17, -15, -10, -6, 6, 10, 15, 17];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard KnightAttack(Square sq)
        {
            BitBoard b = 0;
            foreach (int step in KnightSteps)
            {
                b |= SafeDestination(sq, step);
            }
            return b;
        }
        private static readonly int[] KingSteps = [-9, -8, -7, -1, 1, 7, 8, 9];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard KingAttack(Square sq)
        {
            BitBoard b = 0;
            foreach (int step in KingSteps)
            {
                b |= SafeDestination(sq, step);
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard PseudoAttacks(PieceType pt, Square sq)
        {
            return pt switch
            {
                PieceType.Rook or PieceType.Bishop => SlidingAttack(pt, sq, 0),
                PieceType.Queen => SlidingAttack(PieceType.Rook, sq, 0) | SlidingAttack(PieceType.Bishop, sq, 0),
                PieceType.Knight => KnightAttack(sq),
                PieceType.King => KingAttack(sq),
                _ => 0,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AttacksBB<P>(Square s, Color c = Color.ColorNB) where P : struct, IPieceType
        {
            return P.Type == PieceType.Pawn ? pseudoAttacks[(int)c][(int)s] : pseudoAttacks[(int)P.Type][(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AttacksBB<P>(Square s, BitBoard occupied) where P : struct, IPieceType, IPieceTypes
        {
            return P.Type switch
            {
                PieceType.Bishop or PieceType.Rook => Magics[(int)s][P.Type - PieceType.Bishop].AttacksBB(occupied),
                PieceType.Queen => AttacksBB<Bishop>(s, occupied) | AttacksBB<Rook>(s, occupied),
                _ => pseudoAttacks[(int)P.Type][(int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AttacksBB(PieceType pt, Square s, BitBoard occupied)
        {
            return pt switch
            {
                PieceType.Bishop or PieceType.Rook => Magics[(int)s][pt - PieceType.Bishop].AttacksBB(occupied),
                PieceType.Queen => AttacksBB<Bishop>(s, occupied) | AttacksBB<Rook>(s, occupied),
                _ => pseudoAttacks[(int)pt][(int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AttacksBB(Piece pc, Square s, BitBoard occupied)
        {
            return Types.TypeOf(pc) == PieceType.Pawn ? pseudoAttacks[(int)Types.ColorOf(pc)][(int)s] : AttacksBB(Types.TypeOf(pc), s, occupied);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MoreThanOne(BitBoard b)
        {
            return (b & (b - 1)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PawnAttacksBB<C>(BitBoard b) where C : struct, IColor
        {
            return Shift<PawnUpLeft<C>>(b) | Shift<PawnUpRight<C>>(b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard LineBB(Square s1, Square s2)
        {
            return lineBB[(int)s1][(int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BetweenBB(Square s1, Square s2)
        {
            return betweenBB[(int)s1][(int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard SquareBB(Square s)
        {
            return 1UL << (int)s;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard RankBB(Rank r)
        { 
            return Rank1BB << (8 * (int)r);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard RankBB(Square s) 
        { 
            return RankBB(Types.RankOf(s));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard FileBB(File f) 
        { 
            return FileABB << f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard FileBB(Square s) 
        { 
            return FileBB(Types.FileOf(s)); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Shift<O>(BitBoard bb) where O : struct, IPawnOffset
        {
            bb &= O.Mask;
            int offset = (int)O.Value;
            return offset > 0 ? bb << offset : bb >> -offset;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Lsb(BitBoard b)
        {
            return (Square)BitOperations.TrailingZeroCount(b.Raw);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square PopLsb(ref BitBoard bb)
        {
            int sq = BitOperations.TrailingZeroCount(bb);
            bb &= bb - 1;
            return (Square)sq;
        }
    }
    public static class RandomExtensions
    {
        public static ulong Rand64(this Random rng)
        {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }
        public static ulong SparseRand(this Random rng)
        {
            return rng.Rand64() & rng.Rand64() & rng.Rand64();
        }
    }
}
