using System.Numerics;
using static Chess.Bitboards;
using static Chess.Color;
using static Chess.FuncBit;
using static Chess.PieceType;
using static Chess.Types;

namespace Chess
{
    using Value = Int32;
    public static class Evaluate
    {
        private const Value Tempo = 18;

        public static Value Evaluate_Position(ref Position pos, NnueNetworks networks = null)
        {
            return pos.SideToMove == WHITE ? Evaluate_Position<White, Black>(ref pos, networks) : Evaluate_Position<Black, White>(ref pos, networks);
        }

        public static Value Evaluate_Position<C, N>(ref Position pos, NnueNetworks networks = null) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return networks != null && networks.Can_Evaluate ? EvaluateNNUE.Evaluate<C, N>(ref pos, networks) : Classical_Eval<C, N>(ref pos);
        }

        public static Value Classical_Eval(ref Position pos)
        {
            return pos.SideToMove == WHITE ? Classical_Eval<White, Black>(ref pos) : Classical_Eval<Black, White>(ref pos);
        }

        public static Value Classical_Eval<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            Value score = Evaluate_Side(ref pos, WHITE) - Evaluate_Side(ref pos, BLACK);
            score += Tempo * C.Sign;
            return score * C.Sign;
        }

        public static Value Simple_Eval(ref Position pos)
        {
            return pos.SideToMove == WHITE ? Simple_Eval<White, Black>(ref pos) : Simple_Eval<Black, White>(ref pos);
        }

        public static Value Simple_Eval<C, N>(ref Position pos) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            return Classical_Eval<C, N>(ref pos);
        }

        private static Value Evaluate_Side(ref Position pos, Color side)
        {
            return Evaluate_Material(ref pos, side)
                 + Evaluate_Pawn_Structure(ref pos, side)
                 + Evaluate_Mobility(ref pos, side)
                 + Evaluate_King_Safety(ref pos, side)
                 + Evaluate_Piece_Squares(ref pos, side);
        }

        private static unsafe Value Evaluate_Material(ref Position pos, Color side)
        {
            Value score = 0;
            fixed (PieceType* typeStart = &Types.PieceTypes[0])
            {
                for (PieceType* typePtr = typeStart, typeEnd = typeStart + PieceTypeArray6.Length; typePtr != typeEnd; ++typePtr)
                {
                    PieceType type = *typePtr;
                    if (type == KING)
                    {
                        continue;
                    }
                    Bitboard pieces = pos.Get_Pieces(side) & pos.ByTypeBB[(int)type];
                    score += BitOperations.PopCount((ulong)pieces) * Piece_Value(side, type);
                }
            }
            return score;
        }

        private static Value Evaluate_Pawn_Structure(ref Position pos, Color side)
        {
            Value score = 0;
            Color enemy = side == WHITE ? BLACK : WHITE;
            Bitboard pawns = pos.Get_Pieces<Pawn>(side);
            Bitboard ownpawns = pawns;
            Bitboard enemypawns = pos.Get_Pieces<Pawn>(enemy);
            while (pawns != 0)
            {
                Square sq = Pop_Lsb(ref pawns);
                File fileindex = File_Of(sq);
                Rank relativerank = Relativ_Rank(side, Rank_Of(sq));
                Bitboard samefile = ownpawns & File_BB(fileindex);
                if (BitOperations.PopCount((ulong)samefile) > 1)
                {
                    score -= 12;
                }
                Bitboard adjacentfiles = 0;
                if (fileindex > File.FILE_A)
                {
                    adjacentfiles |= File_BB((File)((int)fileindex - 1));
                }
                if (fileindex < File.FILE_H)
                {
                    adjacentfiles |= File_BB((File)((int)fileindex + 1));
                }
                if ((ownpawns & adjacentfiles) == 0)
                {
                    score -= 10;
                }
                bool passed = true;
                Bitboard enemyscan = enemypawns;
                while (enemyscan != 0)
                {
                    Square enemysq = Pop_Lsb(ref enemyscan);
                    if (Math.Abs((int)File_Of(enemysq) - (int)fileindex) <= 1)
                    {
                        bool ahead = side == WHITE ? enemysq > sq : enemysq < sq;
                        if (ahead)
                        {
                            passed = false;
                            break;
                        }
                    }
                }
                if (passed)
                {
                    score += 16 + 12 * (int)relativerank;
                }
            }
            return score;
        }

        private static Value Evaluate_Mobility(ref Position pos, Color side)
        {
            Value score = 0;
            Bitboard occupied = pos.Get_Pieces();
            score += Evaluate_Mobility<Knight>(ref pos, side, occupied, 4);
            score += Evaluate_Mobility<Bishop>(ref pos, side, occupied, 5);
            score += Evaluate_Mobility<Rook>(ref pos, side, occupied, 3);
            score += Evaluate_Mobility<Queen>(ref pos, side, occupied, 2);
            return score;
        }

        private static Value Evaluate_Mobility<P>(ref Position pos, Color side, Bitboard occupied, int weight) where P : struct, IPieceType, IPieceTypes
        {
            Value score = 0;
            Bitboard pieces = pos.Get_Pieces<P>(side);
            while (pieces != 0)
            {
                Square sq = Pop_Lsb(ref pieces);
                Bitboard moves = Attacks_BB<P>(sq, occupied) & ~pos.Get_Pieces(side);
                score += weight * BitOperations.PopCount((ulong)moves);
            }
            return score;
        }

        private static Value Evaluate_King_Safety(ref Position pos, Color side)
        {
            Color enemy = side == WHITE ? BLACK : WHITE;
            Square kingsq = pos.Get_Square<King>(side);
            Bitboard kingring = Attacks_BB<King>(kingsq);
            Bitboard pawnshield = side == WHITE
                ? Shift<Pawn_Up<White, Black>>(Square_BB(kingsq)) | Shift<Pawn_Up_Left<White, Black>>(Square_BB(kingsq)) | Shift<Pawn_Up_Right<White, Black>>(Square_BB(kingsq))
                : Shift<Pawn_Up<Black, White>>(Square_BB(kingsq)) | Shift<Pawn_Up_Left<Black, White>>(Square_BB(kingsq)) | Shift<Pawn_Up_Right<Black, White>>(Square_BB(kingsq));
            Value shield = BitOperations.PopCount((ulong)(pawnshield & pos.Get_Pieces<Pawn>(side)));
            Value attackers = BitOperations.PopCount((ulong)(kingring & Attacked_By(ref pos, enemy)));
            return shield * 12 - attackers * 18;
        }

        private static Value Evaluate_Piece_Squares(ref Position pos, Color side)
        {
            Value score = 0;
            Bitboard pieces = pos.Get_Pieces(side);
            while (pieces != 0)
            {
                Square sq = Pop_Lsb(ref pieces);
                Piece piece = pos.Piece_On(sq);
                Square orientedsq = side == WHITE ? sq : Rotate_180(sq);
                int filedistance = Math.Abs((int)File_Of(orientedsq) - 3);
                int rankdistance = Math.Abs((int)Rank_Of(orientedsq) - 3);
                int central = 6 - filedistance - rankdistance;
                score += Type_Of(piece) switch
                {
                    PAWN => 2 * (int)Rank_Of(orientedsq) + central,
                    KNIGHT => 7 * central,
                    BISHOP => 6 * central,
                    ROOK => 2 * central + 2 * (int)Rank_Of(orientedsq),
                    QUEEN => 3 * central,
                    KING => (7 - (int)Rank_Of(orientedsq)) * 3,
                    _ => 0,
                };
            }
            return score;
        }

        private static Bitboard Attacked_By(ref Position pos, Color side)
        {
            Bitboard occupied = pos.Get_Pieces();
            Bitboard attacks = 0;
            Bitboard pieces = pos.Get_Pieces(side);
            while (pieces != 0)
            {
                Square sq = Pop_Lsb(ref pieces);
                Piece piece = pos.Piece_On(sq);
                attacks |= Attacks_BB(piece, sq, occupied);
            }
            return attacks;
        }
    }
}
