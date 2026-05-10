using System.Numerics;
using System.Runtime.CompilerServices;
using static Chess.Color;
using static Chess.FuncBit;
using static Chess.PieceType;
using static Chess.Square;
using static Chess.Types;
namespace Chess
{
    unsafe public static class Bitboards
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
        public static readonly SquareDistance SquareDistance;
        public static readonly BetweenBB BetweenBB;
        public static readonly LineBB LineBB;
        public static readonly RayPassBB RayPassBB;
        public static readonly Magics Magics;
        public static readonly RookTable RookTable;
        public static readonly BishopTable BishopTable;
        public static readonly PseudoAttacks PseudoAttacks;    
        static Bitboards()
        {
            for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
            {
                for (Square s2 = SQ_A1; s2 <= SQ_H8; ++s2)
                {
                    SquareDistance[(int) s1, (int)s2] = (byte)Math.Max(File_Distance(s1, s2), Rank_Distance(s1, s2));
                }
            }
            Init_Magics<Rook>((Bitboard*)Unsafe.AsPointer(ref Unsafe.AsRef(in RookTable[0])), Magics);
            Init_Magics<Bishop>((Bitboard*)Unsafe.AsPointer(ref Unsafe.AsRef(in BishopTable[0])), Magics);
            for (Square s1 = SQ_A1; s1 <= SQ_H8; ++s1)
            {
                fixed (PieceType* sliderStart = &Types.Slider[0])
                {
                    for (PieceType* slider = sliderStart, sliderEnd = sliderStart + PieceTypeArray2.Length; slider != sliderEnd; ++slider)
                    {
                        PieceType pt = *slider;
                        for (Square s2 = SQ_A1; s2 <= SQ_H8; ++s2)
                        {
                            if ((PseudoAttacks[(int)pt, (int)s1] & s2) != 0)
                            {
                                LineBB[(int)s1, (int)s2] = (Bitboard)(Attacks_BB(pt, s1, 0) & Attacks_BB(pt, s2, 0)) | s1 | s2;
                                BetweenBB[(int)s1, (int)s2] = (Attacks_BB(pt, s1, Square_BB(s2)) & Attacks_BB(pt, s2, Square_BB(s1)));
                                RayPassBB[(int)s1, (int)s2] = Attacks_BB(pt, s1, 0) & (Attacks_BB(pt, s2, Square_BB(s1)) | s2);
                            }
                            BetweenBB[(int)s1, (int)s2] |= s2;
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Init_Magics<T>(Bitboard* table, Magics magics) where T : struct, IPieceType
        {
            Bitboard[] occupancy = new Bitboard[4096];
            Bitboard[] reference = new Bitboard[4096];
            int[] epoch = new int[4096];
            int size = 0;
            int cnt = 0;
            for (Square s = SQ_A1; s <= SQ_H8; ++s)
            {
                Bitboard edges = ((Rank_1BB | Rank_8BB) & ~Rank_BB(s)) | ((File_ABB | File_HBB) & ~File_BB(s));
                ref Magic m = ref magics[(int)s, T.Type - BISHOP];
                m.mask = Sliding_Attack<T>(s, 0) & ~edges;
                m.Shift = 64 - BitOperations.PopCount(m.mask);
                m.attacks = s == SQ_A1 ? table : magics[(int)s - 1, T.Type - BISHOP].attacks + size;
                size = 0;
                Bitboard b = 0;
                do
                {
                    occupancy[size] = b;
                    reference[size] = Sliding_Attack<T>(s, b);
                    size++;
                    b = (b - m.mask) & m.mask;
                }
                while (b != 0);
                PRNG rng = new((ulong)Seed_Of(Rank_Of(s)));
                for (int i = 0; i < size;)
                {
                    for (m.magic = 0; BitOperations.PopCount((m.magic * m.mask) >> 56) < 6;)
                    {
                        m.magic = rng.Sparse_Rand<Bitboard>();
                    }
                    for (++cnt, i = 0; i < size; ++i)
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
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Safe_Destination(Square s, int step)
        {
            Square to = s + step;
            return IsOk(to) && File_Distance(s, to) <= 2 ? Square_BB(to) : 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Sliding_Attack<T>(Square sq, Bitboard occupied) where T : struct, IPieceType
        {
            Bitboard attacks = 0;

            if (T.Type == ROOK)
            {
                fixed (Direction* directionStart = &RookDirections[0])
                {
                    for (Direction* direction = directionStart, directionEnd = directionStart + DirectionArray4.Length; direction != directionEnd; ++direction)
                    {
                        Direction d = *direction;
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
                }
            }
            else
            {
                fixed (Direction* directionStart = &BishopDirections[0])
                {
                    for (Direction* direction = directionStart, directionEnd = directionStart + DirectionArray4.Length; direction != directionEnd; ++direction)
                    {
                        Direction d = *direction;
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
                }
            }
            return attacks;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard Knight_Attack(Square sq)
        {
            Bitboard b = 0;
            fixed (int* stepStart = &KnightSteps[0])
            {
                for (int* step = stepStart, stepEnd = stepStart + IntArray8.Length; step != stepEnd; ++step)
                {
                    b |= Safe_Destination(sq, *step);
                }
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Bitboard King_Attack(Square sq)
        {
            Bitboard b = 0;
            fixed (int* stepStart = &KingSteps[0])
            {
                for (int* step = stepStart, stepEnd = stepStart + IntArray8.Length; step != stepEnd; ++step)
                {
                    b |= Safe_Destination(sq, *step);
                }
            }
            return b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pseudo_Attacks<T>(Square sq) where T : struct, IPieceType
        {
            return T.Type switch
            {
                ROOK or BISHOP => Sliding_Attack<T>(sq, 0),
                QUEEN => Sliding_Attack<Rook>(sq, 0) | Sliding_Attack<Bishop>(sq, 0),
                KNIGHT => Knight_Attack(sq),
                KING => King_Attack(sq),
                _ => 0,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB<P>(Square s, Color c = COLOR_NB) where P : struct, IPieceType
        {
            return P.Type == PAWN ? PseudoAttacks[(int)c, (int)s] : PseudoAttacks[(int)P.Type, (int)s];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB<P>(Square s, Bitboard occupied) where P : struct, IPieceType, IPieceTypes
        {
            return P.Type switch
            {
                BISHOP or ROOK => Magics[(int)s, P.Type - BISHOP].Attacks_BB(occupied),
                QUEEN => Attacks_BB<Bishop>(s, occupied) | Attacks_BB<Rook>(s, occupied),
                _ => PseudoAttacks[(int)P.Type, (int)s],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB(PieceType pt, Square sq, Bitboard occupied)
        {
            return pt switch
            {
                BISHOP or ROOK => Magics[(int)sq, pt - BISHOP].Attacks_BB(occupied),
                QUEEN => Attacks_BB<Bishop>(sq, occupied) | Attacks_BB<Rook>(sq, occupied),
                _ => PseudoAttacks[(int)pt, (int)sq],
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Attacks_BB(Piece pc, Square s, Bitboard occupied)
        {
            return Type_Of(pc) == PAWN ? PseudoAttacks[(int)Color_Of(pc), (int)s] : Attacks_BB(Type_Of(pc), s, occupied);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Line_BB(Square s1, Square s2)
        {
            return LineBB[(int)s1, (int)s2];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Between_BB(Square s1, Square s2)
        {
            return BetweenBB[(int)s1, (int)s2];
        }
    }
}
