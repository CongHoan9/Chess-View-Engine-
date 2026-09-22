using System.Numerics;
using System.Runtime.CompilerServices;
using static Chess.Bitboards;
using static Chess.PieceType;
using static Chess.Direction;
using static Chess.Square;
using static Chess.Color;
using System.Runtime.Intrinsics.X86;
namespace Chess
{
    using Key = UInt64;
    public static class FuncBit
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Shift<Direction>(Bitboard bb) where Direction : struct, IDirection
        {
            return Direction.Shift(bb);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Square_BB(Square s)
        {
            return 1UL << (int)s;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Rank_BB(Square s)
        {
            return Rank_BB(Rank_Of(s));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard File_BB(Square s)
        {
            return File_BB(File_Of(s));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool More_Than_One(Bitboard b)
        {
            if (Bmi1.X64.IsSupported)
            {
                return Bmi1.X64.ResetLowestSetBit(b.Raw) != 0;
            }
            return (b & (b - 1)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Pawn_Attacks_BB<Us, Next>(Bitboard b) where Us : struct, IColor<Us, Next> where Next : struct, IColor<Next, Us>
        {
            return Shift<Pawn_Up_Left<Us, Next>>(b) | Shift<Pawn_Up_Right<Us, Next>>(b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Lsb(Bitboard b)
        {
            return (Square)BitOperations.TrailingZeroCount(b.Raw);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Pop_Lsb(ref Bitboard bb)
        {
            if (Bmi1.X64.IsSupported)
            {
                ulong value = bb.Raw;
                int sq = BitOperations.TrailingZeroCount(value);
                bb = Bmi1.X64.ResetLowestSetBit(value);
                return (Square)sq;
            }
            else
            {
                int sq = BitOperations.TrailingZeroCount(bb);
                bb &= bb - 1;
                return (Square)sq;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Rotate_180(Square sq)
        {
            return (Square)((int)sq ^ 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int From_To(Move m)
        {
            return m & 0xFFF;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveType Type_Of(Move m)
        {
            return (MoveType)(m & (3 << 14));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType Promotion_Type(Move m)
        {
            return ((m >> 12) & 3) + KNIGHT;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make_Move(Square from, Square to)
        {
            return new(from, to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Make_Move<T, P>(Square from, Square to) where T : struct, IMoveType where P : struct, IPieceType
        {
            return Move.Make_Move<T, P>(from, to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece Make_Piece(Color c, PieceType pt)
        {
            return (Piece)(((int)c << 3) + pt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece Make_Piece<T>(Color c) where T : struct, IPieceType
        {
            return (Piece)(((int)c << 3) + T.Type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Make_Square(File f, Rank r)
        {
            return (Square)(((int)r << 3) + f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square From_Sq(Move m)
        {
            return (Square)((m.Raw >> 6) & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square To_Sq(Move m)
        {
            return (Square)(m.Raw & 0x3F);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(Move m)
        {
            return From_Sq(m) != To_Sq(m);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOk(Square s)
        {
            return s >= SQ_A1 && s <= SQ_H8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File File_Of(Square s)
        {
            return (File)((int)s & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank Rank_Of(Square s)
        {
            return (Rank)((int)s >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Rank_BB(Rank r)
        {
            return Rank_1BB << (8 * (int)r);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard File_BB(File f)
        {
            return File_ABB << (int)f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Key Make_Key(ulong seed)
        {
            return seed * 6364136223846793005UL + 1442695040888963407UL;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction Pawn_Push(Color c)
        {
            return c == WHITE ? NORTH : SOUTH;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType Type_Of(Piece pc)
        {
            return (PieceType)((int)pc & 7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Color_Of(Piece pc)
        {
            return (Color)((int)pc >> 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Relativ_Square(Color c, Square s)
        {
            return (Square)((int)s ^ ((int)c * 56));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank Relativ_Rank(Color c, Rank r)
        {
            return (Rank)((int)r ^ ((int)c * 7));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int H1(Key k)
        {
            return (int)(k & 0x1FFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int H2(Key k)
        {
            return (int)((k >> 16) & 0x1FFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int File_Distance(Square s1, Square s2)
        {
            return Math.Abs(File_Of(s1) - File_Of(s2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Rank_Distance(Square s1, Square s2)
        {
            return Math.Abs(Rank_Of(s1) - Rank_Of(s2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Square_To_String(Square sq)
        {
            char file = (char)('a' + ((int)sq & 7));
            char rank = (char)('1' + ((int)sq >> 3));
            return $"{file}{rank}";
        }
    }
}
