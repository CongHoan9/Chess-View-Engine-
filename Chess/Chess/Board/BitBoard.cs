using System.Numerics;
using System.Runtime.CompilerServices;
using static Chess.PieceType;
using static Chess.Square;
using static Chess.Color;
using static Chess.FuncBit;
using static Chess.Types;
namespace Chess
{
    public static class Bitboards
    {
        public static readonly Bitboard File_ABB = 0x0101010101010101UL;
        public static readonly Bitboard File_BBB = File_ABB << 1;
        public static readonly Bitboard File_CBB = File_ABB << 2;
        public static readonly Bitboard File_DBB = File_ABB << 3;
        public static readonly Bitboard File_EBB = File_ABB << 4;
        public static readonly Bitboard File_FBB = File_ABB << 5;
        public static readonly Bitboard File_GBB = File_ABB << 6;
        public static readonly Bitboard File_HBB = File_ABB << 7;
        public static readonly Bitboard Rank_1BB = 0x00000000000000FFUL;
        public static readonly Bitboard Rank_2BB = Rank_1BB << (8 * 1);
        public static readonly Bitboard Rank_3BB = Rank_1BB << (8 * 2);
        public static readonly Bitboard Rank_4BB = Rank_1BB << (8 * 3);
        public static readonly Bitboard Rank_5BB = Rank_1BB << (8 * 4);
        public static readonly Bitboard Rank_6BB = Rank_1BB << (8 * 5);
        public static readonly Bitboard Rank_7BB = Rank_1BB << (8 * 6);
        public static readonly Bitboard Rank_8BB = Rank_1BB << (8 * 7);
        public static readonly Bitboard Not_File_ABB = ~File_ABB;
        public static readonly Bitboard Not_File_HBB = ~File_HBB;
        public static readonly byte[] PopCnt16 = new byte[1 << 16];
        public static readonly byte[][] SquareDistance = [.. Enumerable.Range(0, (int)SQ_NB).Select(_ => new byte[(int)SQ_NB])];
        public static readonly Bitboard[][] BetweenBB = [.. Enumerable.Range(0, (int)SQ_NB).Select(_ => new Bitboard[(int)SQ_NB])];
        public static readonly Bitboard[][] LineBB = [.. Enumerable.Range(0, (int)SQ_NB).Select(_ => new Bitboard[(int)SQ_NB])];
        public static readonly Bitboard[][] RayPassBB = [.. Enumerable.Range(0, (int)SQ_NB).Select(_ => new Bitboard[(int)SQ_NB])];
        public static readonly Magic[][] Magics = [.. Enumerable.Range(0, (int)SQ_NB).Select(_ => new Magic[2])];
        public static readonly Bitboard[] RookTable = new Bitboard[0x19000];
        public static readonly Bitboard[] BishopTable = new Bitboard[0x1480];
        public static readonly Bitboard[][] PseudoAttacks = Init_Pseudo_Attacks();
        static Bitboards()
        {
            for (int i = 0; i < (1 << 16); ++i)
            {
                PopCnt16[i] = (byte)BitOperations.PopCount((uint)i);
            }
            for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
            {
                for (Square s2 = SQ_A1; s2 <= SQ_H8; ++s2)
                {
                    SquareDistance[(int)s1][(int)s2] = (byte)Math.Max(File_Distance(s1, s2), Rank_Distance(s1, s2));
                }
            }
            Init_Magics(ROOK, RookTable, Magics);
            Init_Magics(BISHOP, BishopTable, Magics);
            for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
            {
                foreach (PieceType pt in Slider)
                {
                    for (Square s2 = SQ_A1; s2 <= SQ_H8; ++s2)
                    {
                        if ((PseudoAttacks[(int)pt][(int)s1] & s2) != 0)
                        {
                            LineBB[(int)s1][(int)s2] = (Bitboard)(Attacks_BB(pt, s1, 0) & Attacks_BB(pt, s2, 0)) | s1 | s2;
                            BetweenBB[(int)s1][(int)s2] = (Attacks_BB(pt, s1, Square_BB(s2)) & Attacks_BB(pt, s2, Square_BB(s1)));
                            RayPassBB[(int)s1][(int)s2] = Attacks_BB(pt, s1, 0) & (Attacks_BB(pt, s2, Square_BB(s1)) | s2);
                        }
                        BetweenBB[(int)s1][(int)s2] |= s2;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private static void Init_Magics(PieceType pt, Bitboard[] table, Magic[][] magics)
        {
            fixed (Bitboard* tb = table)
            {
                int[][] seeds = 
                [ 
                    [8977,44560,54343,38998,5731,95205,104912,17020],
                    [728,10316,55013,32803,12281,15100,16645,255] 
                ];
                Bitboard[] occupancy = new Bitboard[4096];
                Bitboard[] reference = new Bitboard[4096];
                int[] epoch = new int[4096];
                int size = 0;
                int cnt = 0;
                for (Square s = SQ_A1; s <= SQ_H8; ++s)
                {
                    Bitboard edges = ((Rank_1BB | Rank_8BB) & ~Rank_BB(s)) | ((File_ABB | File_HBB) & ~File_BB(s));
                    fixed (Magic* m = &magics[(int)s][pt - BISHOP])
                    {
                        m->mask = Sliding_Attack(pt, s, 0) & ~edges;
                        m->Shift = 64 - BitOperations.PopCount(m->mask);
                        m->attacks = s == SQ_A1 ? tb : magics[(int)s - 1][pt - BISHOP].attacks + size;
                        size = 0;
                        Bitboard b = 0;
                        do
                        {
                            occupancy[size] = b;
                            reference[size] = Sliding_Attack(pt, s, b);
                            size++;
                            b = (b - m->mask) & m->mask;
                        }
                        while (b != 0);
                        PRNG rng = new((ulong)seeds[1][(int)Rank_Of(s)]);
                        for (int i = 0; i < size;)
                        {
                            for (m->magic = 0; BitOperations.PopCount((m->magic * m->mask) >> 56) < 6;)
                            {
                                m->magic = rng.Sparse_Rand<Bitboard>();
                            }
                            for (++cnt, i = 0; i < size; ++i)
                            {
                                int idx = m->Index(occupancy[i]);
                                if (epoch[idx] < cnt)
                                {
                                    epoch[idx] = cnt;
                                    m->attacks[idx] = reference[i];
                                }
                                else if (m->attacks[idx] != reference[i])
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard[][] Init_Pseudo_Attacks()
        {
            Bitboard[][] attacks = [.. Enumerable.Range(0, (int)PIECE_TYPE_NB).Select(_ => new Bitboard[(int)SQ_NB])];
            for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
            {
                attacks[(int)WHITE][(int)s1] = Pawn_Attacks_BB<White>(Square_BB(s1));
                attacks[(int)BLACK][(int)s1] = Pawn_Attacks_BB<Black>(Square_BB(s1));
                attacks[(int)KNIGHT][(int)s1] = Pseudo_Attacks(KNIGHT, s1);
                attacks[(int)QUEEN][(int)s1] = attacks[(int)BISHOP][(int)s1] = Pseudo_Attacks(BISHOP, s1);
                attacks[(int)QUEEN][(int)s1] |= attacks[(int)ROOK][(int)s1] = Pseudo_Attacks(ROOK, s1);
                attacks[(int)KING][(int)s1] = Pseudo_Attacks(KING, s1);
            }
            return attacks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Safe_Destination(Square s, int step)
        {
            Square to = s + step;
            return IsOk(to) && File_Distance(s, to) <= 2 ? Square_BB(to) : 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Sliding_Attack(PieceType pt, Square sq, Bitboard occupied)
        {
            Bitboard attacks = 0;
            foreach (Direction d in pt == ROOK ? RookDirections : BishopDirections)
            {
                Square s = sq;
                while (Safe_Destination(s, (int)d) != 0)
                {
                    attacks |= s += (int)d;
                    if ((occupied & s) != 0)
                    {
                        break;
                    }
                }
            }
            return attacks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Knight_Attack(Square sq)
        {
            Bitboard b = 0;
            foreach (int step in KnightSteps)
            {
                b |= Safe_Destination(sq, step);
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard King_Attack(Square sq)
        {
            Bitboard b = 0;
            foreach (int step in KingSteps)
            {
                b |= Safe_Destination(sq, step);
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Pseudo_Attacks(PieceType pt, Square sq)
        {
            return pt switch
            {
                ROOK or BISHOP => Sliding_Attack(pt, sq, 0),
                QUEEN => Sliding_Attack(ROOK, sq, 0) | Sliding_Attack(BISHOP, sq, 0),
                KNIGHT => Knight_Attack(sq),
                KING => King_Attack(sq),
                _ => 0,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB<P>(Square s, Color c = COLOR_NB) where P : struct, IPieceType
        {
            return P.Type == PAWN ? PseudoAttacks[(int)c][(int)s] : PseudoAttacks[(int)P.Type][(int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB<P>(Square s, Bitboard occupied) where P : struct, IPieceType, IPieceTypes
        {
            return P.Type switch
            {
                BISHOP or ROOK => Magics[(int)s][P.Type - BISHOP].Attacks_BB(occupied),
                QUEEN => Attacks_BB<Bishop>(s, occupied) | Attacks_BB<Rook>(s, occupied),
                _ => PseudoAttacks[(int)P.Type][(int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB(PieceType pt, Square sq, Bitboard occupied)
        {
            return pt switch
            {
                BISHOP or ROOK => Magics[(int)sq][pt - BISHOP].Attacks_BB(occupied),
                QUEEN => Attacks_BB<Bishop>(sq, occupied) | Attacks_BB<Rook>(sq, occupied),
                _ => PseudoAttacks[(int)pt][(int)sq],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB(Piece pc, Square s, Bitboard occupied)
        {
            return Type_Of(pc) == PAWN ? PseudoAttacks[(int)Color_Of(pc)][(int)s] : Attacks_BB(Type_Of(pc), s, occupied);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Line_BB(Square s1, Square s2)
        {
            return LineBB[(int)s1][(int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Between_BB(Square s1, Square s2)
        {
            return BetweenBB[(int)s1][(int)s2];
        }
    }
}
