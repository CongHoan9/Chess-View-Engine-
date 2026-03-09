using System.Numerics;
using System.Runtime.CompilerServices;
namespace Chess
{
    public static class BitBoard
    {
        public static readonly SBitBoard Board = 0xFFFFFFFFFFFFFFFFUL;
        public static readonly SBitBoard FileABB = 0x0101010101010101UL;
        public static readonly SBitBoard FileBBB = FileABB << 1;
        public static readonly SBitBoard FileCBB = FileABB << 2;
        public static readonly SBitBoard FileDBB = FileABB << 3;
        public static readonly SBitBoard FileEBB = FileABB << 4;
        public static readonly SBitBoard FileFBB = FileABB << 5;
        public static readonly SBitBoard FileGBB = FileABB << 6;
        public static readonly SBitBoard FileHBB = FileABB << 7;
        public static readonly SBitBoard Rank1BB = 0x00000000000000FFUL;
        public static readonly SBitBoard Rank2BB = Rank1BB << (8 * 1);
        public static readonly SBitBoard Rank3BB = Rank1BB << (8 * 2);
        public static readonly SBitBoard Rank4BB = Rank1BB << (8 * 3);
        public static readonly SBitBoard Rank5BB = Rank1BB << (8 * 4);
        public static readonly SBitBoard Rank6BB = Rank1BB << (8 * 5);
        public static readonly SBitBoard Rank7BB = Rank1BB << (8 * 6);
        public static readonly SBitBoard Rank8BB = Rank1BB << (8 * 7);
        public static readonly SBitBoard NotFileABB = ~FileABB;
        public static readonly SBitBoard NotFileHBB = ~FileHBB;
        public static readonly byte[] PopCnt16 = new byte[1 << 16];
        public static readonly byte[][] SquareDistance = new byte[64][];
        public static readonly SBitBoard[][] betweenBB = new SBitBoard[64][];
        public static readonly SBitBoard[][] lineBB = new SBitBoard[64][];
        public static readonly SBitBoard[][] RayPassBB = new SBitBoard[64][];
        public static readonly Magic[][] Magics = new Magic[64][];
        public static readonly SBitBoard[] RookTable = new SBitBoard[0x19000];
        public static readonly SBitBoard[] BishopTable = new SBitBoard[0x1480];
        public static readonly SBitBoard[][] pseudoAttacks = InitPseudoAttacks();
        private static readonly EPieceType[] Slider = [EPieceType.Bishop, EPieceType.Rook];
        static BitBoard()
        {
            for (int i = 0; i < (1 << 16); ++i)
            {
                PopCnt16[i] = (byte)BitOperations.PopCount((uint)i);
            }
            for (ESquare s1 = ESquare.SQ_A1; s1 <= ESquare.SQ_H8; ++s1)
            {
                int index1 = (int)s1;
                SquareDistance[index1] = new byte[64];
                betweenBB[index1] = new SBitBoard[64];
                lineBB[index1] = new SBitBoard[64];
                RayPassBB[index1] = new SBitBoard[64];
                Magics[index1] = new Magic[2];
                for (ESquare s2 = ESquare.SQ_A1; s2 <= ESquare.SQ_H8; ++s2)
                {
                    int fileDistance = Math.Abs(Types.FileOf(s1) - Types.FileOf(s2));
                    int rankDistance = Math.Abs(Types.RankOf(s1) - Types.RankOf(s2));
                    SquareDistance[index1][(int)s2] = (byte)Math.Max(fileDistance, rankDistance);
                }
            }
            InitMagics(EPieceType.Rook, RookTable, Magics);
            InitMagics(EPieceType.Bishop, BishopTable, Magics);
            for (ESquare s1 = ESquare.SQ_A1; s1 <= ESquare.SQ_H8; ++s1)
            {
                foreach (EPieceType pt in Slider)
                {
                    for (ESquare s2 = ESquare.SQ_A1; s2 <= ESquare.SQ_H8; ++s2)
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
        private static void InitMagics(EPieceType pt, SBitBoard[] table, Magic[][] magics)
        {
            int[,] seeds = 
            {
                {8977,44560,54343,38998,5731,95205,104912,17020},
                {728,10316,55013,32803,12281,15100,16645,255}
            };
            SBitBoard[] occupancy = new SBitBoard[4096];
            int[] epoch = new int[4096];
            int cnt = 0;
            SBitBoard[] reference = new SBitBoard[4096];
            for (ESquare s = ESquare.SQ_A1; s <= ESquare.SQ_H8; s++)
            {
                SBitBoard edges = ((Rank1BB | Rank8BB) & ~RankBB(s)) | ((FileABB | FileHBB) & ~FileBB(s));
                Magic m = magics[(int)s][(int)pt - (int)EPieceType.Bishop];
                m.mask = SlidingAttack(pt, s, 0) & ~edges;
                m.Shift = 64 - BitOperations.PopCount(m.mask);
                if (s == ESquare.SQ_A1)
                {
                    m.attacks = table;
                }
                else
                {
                    m.attacks = magics[(int)s - 1][(int)pt - (int)EPieceType.Bishop].attacks;
                }
                int size = 0;
                SBitBoard b = 0;
                do
                {
                    occupancy[size] = b;
                    reference[size] = SlidingAttack(pt, s, b);
                    size++;
                    b = (b - m.mask) & m.mask;
                } 
                while (b != 0);
                PRNG rng = new((ulong)seeds[1, (int)Types.RankOf(s)]);
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
                magics[(int)s][(int)pt - (int)EPieceType.Bishop] = m;
            }
        }
        private static SBitBoard[][] InitPseudoAttacks()
        {
            int PieceTypeNB = (int)EPieceType.PieceTypeNB;
            var attacks = new SBitBoard[PieceTypeNB][];
            for (ESquare s = 0; s < ESquare.SquareNB; s++)
            {
                int index = (int)s;
                attacks[(int)EColor.White][index] = PawnAttacksBB<SWhite>(SquareBB(s));
                attacks[(int)EColor.Black][index] = PawnAttacksBB<SBlack>(SquareBB(s));
                attacks[(int)EPieceType.King][index] = PseudoAttacks(EPieceType.King, s);
                attacks[(int)EPieceType.Knight][index] = PseudoAttacks(EPieceType.Knight, s);
                attacks[(int)EPieceType.Bishop][index] = PseudoAttacks(EPieceType.Bishop, s);
                attacks[(int)EPieceType.Rook][index] = PseudoAttacks(EPieceType.Rook, s);
                attacks[(int)EPieceType.Queen][index] = attacks[(int)EPieceType.Bishop][index] | attacks[(int)EPieceType.Rook][index];
            }
            return attacks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SBitBoard SafeDestination(ESquare s, int step)
        {
            ESquare to = (ESquare)((int)s + step);
            return Types.IsOk(to) && Math.Abs((int)Types.FileOf(s) - (int)Types.FileOf(to)) <= 2 ? SquareBB(to) : SquareBB(0);
        }
        private static readonly EDirection[] RookDirections = [EDirection.North, EDirection.South, EDirection.East, EDirection.West];
        private static readonly EDirection[] BishopDirections = [EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SBitBoard SlidingAttack(EPieceType pt, ESquare sq, SBitBoard occupied)
        {
            SBitBoard attacks = 0;
            foreach (EDirection d in pt == EPieceType.Rook ? RookDirections : BishopDirections)
            {
                ESquare s = sq;
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
        private static SBitBoard KnightAttack(ESquare sq)
        {
            SBitBoard b = 0;
            foreach (int step in KnightSteps)
            {
                b |= SafeDestination(sq, step);
            }
            return b;
        }
        private static readonly int[] KingSteps = [-9, -8, -7, -1, 1, 7, 8, 9];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SBitBoard KingAttack(ESquare sq)
        {
            SBitBoard b = 0;
            foreach (int step in KingSteps)
            {
                b |= SafeDestination(sq, step);
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SBitBoard PseudoAttacks(EPieceType pt, ESquare sq)
        {
            return pt switch
            {
                EPieceType.Rook or EPieceType.Bishop => SlidingAttack(pt, sq, 0),
                EPieceType.Queen => SlidingAttack(EPieceType.Rook, sq, 0) | SlidingAttack(EPieceType.Bishop, sq, 0),
                EPieceType.Knight => KnightAttack(sq),
                EPieceType.King => KingAttack(sq),
                _ => 0,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard AttacksBB<P>(ESquare s, EColor c = EColor.ColorNB) where P : struct, IPieceType
        {
            return P.Type == EPieceType.Pawn ? pseudoAttacks[(int)c][(int)s] : pseudoAttacks[(int)P.Type][(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard AttacksBB<P>(ESquare s, SBitBoard occupied) where P : struct, IPieceType, IPieceTypes
        {
            return P.Type switch
            {
                EPieceType.Bishop or EPieceType.Rook => Magics[(int)s][P.Type - EPieceType.Bishop].AttacksBB(occupied),
                EPieceType.Queen => AttacksBB<SBishop>(s, occupied) | AttacksBB<SRook>(s, occupied),
                _ => pseudoAttacks[(int)P.Type][(int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard AttacksBB(EPieceType pt, ESquare s, SBitBoard occupied)
        {
            return pt switch
            {
                EPieceType.Bishop or EPieceType.Rook => Magics[(int)s][pt - EPieceType.Bishop].AttacksBB(occupied),
                EPieceType.Queen => AttacksBB<SBishop>(s, occupied) | AttacksBB<SRook>(s, occupied),
                _ => pseudoAttacks[(int)pt][(int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard AttacksBB(EPiece pc, ESquare s, SBitBoard occupied)
        {
            return Types.TypeOf(pc) == EPieceType.Pawn ? pseudoAttacks[(int)Types.ColorOf(pc)][(int)s] : AttacksBB(Types.TypeOf(pc), s, occupied);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MoreThanOne(SBitBoard b)
        {
            return (b & (b - 1)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard PawnAttacksBB<C>(SBitBoard b) where C : struct, IColor
        {
            return Shift<SPawnUpLeft<C>>(b) | Shift<SPawnUpRight<C>>(b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard LineBB(ESquare s1, ESquare s2)
        {
            return lineBB[(int)s1][(int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard BetweenBB(ESquare s1, ESquare s2)
        {
            return betweenBB[(int)s1][(int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard SquareBB(ESquare s)
        {
            return 1UL << (int)s;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard RankBB(ERank r)
        { 
            return Rank1BB << (8 * (int)r);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard RankBB(ESquare s) 
        { 
            return RankBB(Types.RankOf(s));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard FileBB(EFile f) 
        { 
            return FileABB << f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard FileBB(ESquare s) 
        { 
            return FileBB(Types.FileOf(s)); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SBitBoard Shift<O>(SBitBoard bb) where O : struct, IPawnOffset
        {
            return O.Shift(bb);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare Lsb(SBitBoard b)
        {
            return (ESquare)BitOperations.TrailingZeroCount(b.Raw);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare PopLsb(ref SBitBoard bb)
        {
            int sq = BitOperations.TrailingZeroCount(bb);
            bb &= bb - 1;
            return (ESquare)sq;
        }
    }
}
