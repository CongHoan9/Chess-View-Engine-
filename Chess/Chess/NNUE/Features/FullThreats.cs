using static Chess.Color;
using static Chess.File;
using static Chess.Bitboards;
using static Chess.FuncBit;
using static Chess.PieceType;

namespace Chess
{
    public static class FullThreats
    {
        public const int Dimensions = 60144;

        public static int Make_Index(Color perspective, Piece attacker, Square from, Square to, Piece threatened, Square kingsq)
        {
            int attackerindex = Threat_Index(Type_Of(attacker));
            int threatenedindex = Threat_Index(Type_Of(threatened));
            if (attackerindex < 0 || threatenedindex < 0)
            {
                return -1;
            }
            Square orientedking = Orient_Square(perspective, kingsq, kingsq);
            Square orientedfrom = Orient_Square(perspective, from, kingsq);
            Square orientedto = Orient_Square(perspective, to, kingsq);
            long key = (((long)attackerindex * 5 + threatenedindex) * 64 + (int)orientedfrom) * 64 + (int)orientedto;
            key += ((long)File_Of(orientedking) << 8) ^ (int)Rank_Of(orientedking);
            return (int)(Math.Abs(key) % Dimensions);
        }
        public static void Append_Active_Indices(Color perspective, Position pos, List<int> active, int baseoffset = 0)
        {
            Color enemy = perspective == WHITE ? BLACK : WHITE;
            Square kingsq = pos.Get_Square<King>(perspective);
            Bitboard pieces = pos.Get_Pieces(perspective);
            Bitboard occupied = pos.Get_Pieces();
            while (pieces != 0)
            {
                Square from = Pop_Lsb(ref pieces);
                Piece attacker = pos.Piece_On(from);
                if (Type_Of(attacker) == KING)
                {
                    continue;
                }
                Bitboard targets = Attacks_BB(attacker, from, occupied) & pos.Get_Pieces(enemy);
                while (targets != 0)
                {
                    Square to = Pop_Lsb(ref targets);
                    Piece threatened = pos.Piece_On(to);
                    int index = Make_Index(perspective, attacker, from, to, threatened, kingsq);
                    if (index >= 0)
                    {
                        active.Add(baseoffset + index);
                    }
                }
            }
        }

        public static void Append_Changed_Indices(Color perspective, List<int> removed, List<int> added, DirtyThreats diff, int baseoffset = 0)
        {
            removed.Clear();
            added.Clear();
        }

        public static bool Requires_Refresh(DirtyThreats diff, Color perspective)
        {
            return diff.PrevKsq != diff.Ksq || diff.Us == perspective;
        }

        private static int Threat_Index(PieceType type)
        {
            return type switch
            {
                PAWN => 0,
                KNIGHT => 1,
                BISHOP => 2,
                ROOK => 3,
                QUEEN => 4,
                _ => -1,
            };
        }

        private static Square Orient_Square(Color perspective, Square sq, Square kingsq)
        {
            int oriented = perspective == WHITE ? (int)sq : (int)Rotate_180(sq);
            int orientedking = perspective == WHITE ? (int)kingsq : (int)Rotate_180(kingsq);
            if (File_Of((Square)orientedking) >= FILE_E)
            {
                oriented ^= 7;
            }
            return (Square)oriented;
        }
    }
}
