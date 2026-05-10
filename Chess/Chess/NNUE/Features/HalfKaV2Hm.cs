using static Chess.Color;
using static Chess.File;
using static Chess.FuncBit;
using static Chess.Piece;
using static Chess.PieceType;

namespace Chess
{
    public static class HalfKaV2Hm
    {
        private const int PieceSquareCount = 11 * 64;
        public const int Dimensions = 22528;

        private static readonly int[,] PieceSquareIndex =
        {
            { 0, 0, 128, 256, 384, 512, 640, 0, 0, 64, 192, 320, 448, 576, 640, 0 },
            { 0, 64, 192, 320, 448, 576, 640, 0, 0, 0, 128, 256, 384, 512, 640, 0 }
        };

        private static readonly int[] KingBucketIndex =
        [
            28, 29, 30, 31, 31, 30, 29, 28,
            24, 25, 26, 27, 27, 26, 25, 24,
            20, 21, 22, 23, 23, 22, 21, 20,
            16, 17, 18, 19, 19, 18, 17, 16,
            12, 13, 14, 15, 15, 14, 13, 12,
            8, 9, 10, 11, 11, 10, 9, 8,
            4, 5, 6, 7, 7, 6, 5, 4,
            0, 1, 2, 3, 3, 2, 1, 0
        ];

        public static int Make_Index(Color perspective, Square sq, Piece piece, Square kingsq)
        {
            if (piece == NO_PIECE)
            {
                return -1;
            }
            Square orientedking = Orient_Square(perspective, kingsq, kingsq);
            int pieceoffset = PieceSquareIndex[(int)perspective, (int)piece];
            int kingoffset = KingBucketIndex[(int)orientedking] * PieceSquareCount;
            int squareoffset = (int)Orient_Square(perspective, sq, kingsq);
            return (kingoffset + pieceoffset + squareoffset) % Dimensions;
        }

        public static void Append_Active_Indices(Color perspective, Position pos, List<int> active, int baseoffset = 0)
        {
            Square kingsq = pos.Get_Square<King>(perspective);
            Bitboard pieces = pos.Get_Pieces();
            while (pieces != 0)
            {
                Square sq = Pop_Lsb(ref pieces);
                Piece piece = pos.Piece_On(sq);
                int index = Make_Index(perspective, sq, piece, kingsq);
                if (index >= 0)
                {
                    active.Add(baseoffset + index);
                }
            }
        }

        public static void Append_Changed_Indices(Color perspective, Position pos, List<int> removed, List<int> added, DirtyPiece diff, int baseoffset = 0)
        {
            removed.Clear();
            added.Clear();
            Square kingsq = pos.Get_Square<King>(perspective);
            if (diff.From != Square.SQ_NONE && diff.Pc != NO_PIECE)
            {
                removed.Add(baseoffset + Make_Index(perspective, diff.From, diff.Pc, kingsq));
            }
            if (diff.To != Square.SQ_NONE && diff.Pc != NO_PIECE)
            {
                added.Add(baseoffset + Make_Index(perspective, diff.To, diff.Pc, kingsq));
            }
            if (diff.Remove_Sq != Square.SQ_NONE && diff.Remove_Pc != NO_PIECE)
            {
                removed.Add(baseoffset + Make_Index(perspective, diff.Remove_Sq, diff.Remove_Pc, kingsq));
            }
            if (diff.Add_Sq != Square.SQ_NONE && diff.Add_Pc != NO_PIECE)
            {
                added.Add(baseoffset + Make_Index(perspective, diff.Add_Sq, diff.Add_Pc, kingsq));
            }
        }

        public static bool Requires_Refresh(DirtyPiece diff, Color perspective)
        {
            return (diff.Pc != NO_PIECE && Type_Of(diff.Pc) == KING && Color_Of(diff.Pc) == perspective)
                || (diff.Remove_Pc != NO_PIECE && Type_Of(diff.Remove_Pc) == KING && Color_Of(diff.Remove_Pc) == perspective)
                || (diff.Add_Pc != NO_PIECE && Type_Of(diff.Add_Pc) == KING && Color_Of(diff.Add_Pc) == perspective);
        }

        private static Square Orient_Square(Color perspective, Square sq, Square kingsq)
        {
            int oriented = perspective == WHITE ? (int)sq : (int)Rotate_180(sq);
            int orientedking = perspective == WHITE ? (int)kingsq : (int)Rotate_180(kingsq);
            if (File_Of((Square)orientedking) < FILE_E)
            {
                oriented ^= 7;
            }
            return (Square)oriented;
        }
    }
}
