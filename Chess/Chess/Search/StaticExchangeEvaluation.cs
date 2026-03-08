using System.Numerics;
using System.Runtime.CompilerServices;

namespace Chess
{
    public static class StaticExchangeEvaluation
    {
        private static readonly int[] PieceOrder = [1, 2, 3, 4, 5]; // 1=pawn, 2=knight, 3=bishop, 4=rook, 5=queen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SEE(Board_Bit board, Move move, int threshold = 0)
        {
            // Promotion luôn winning mạnh (queen/rook/etc - pawn + capture nếu có)
            if (move.IsPromotion)
            {
                int promValue = Search.PieceValue(move.Promotion); // hoặc Evaluation.PieceValue
                int pawnValue = Search.PieceValue(board[move.From]); // pawn đang promote
                int gain = promValue - pawnValue;
                if (move.IsCapture)
                {
                    gain += Search.PieceValue(board[move.To]);
                }
                return gain;
            }
            int to = move.To;
            int from = move.From;
            // Victim đầu tiên
            Piece_Bit victim = move.IsEnPassant ? (board.Curent == Piece_Color.White ? Piece_Bit.BPawn : Piece_Bit.WPawn) : board[to];
            if (victim == Piece_Bit.None)
            {
                return 0;

            }
            Span<int> gains = stackalloc int[32];
            int depth = 0;
            gains[0] = Search.PieceValue(victim);
            Piece_Color side = board.Curent ^ Piece_Color.Black;
            ulong occ = board.Occupied;
            occ ^= (1UL << from);
            if (move.IsEnPassant)
            {
                int epVictimSq = to + (board.Curent == Piece_Color.White ? -8 : 8);
                occ ^= (1UL << epVictimSq);
            }
            else if(!move.IsPromotion)
            {
                occ ^= (1UL << to);
            }
            while (true)
            {
                depth++;
                gains[depth] = -gains[depth - 1]; 
                bool found = false;
                foreach (int pt in PieceOrder)
                {
                    Piece_Bit piece = side == Piece_Color.White ? (Piece_Bit)pt : (Piece_Bit)(pt + 6);
                    ulong attackers = 0;
                    if (pt == 1) // pawn
                    {
                        attackers = Attacks.GetPawnAttacks(side ^ Piece_Color.Black, to) & board.GetPieceBB(piece);
                    }
                    else if (pt == 2) // knight
                    {
                        attackers = Attacks.GetKnightAttacks(to) & board.GetPieceBB(piece);
                    }
                    else if (pt == 3) // bishop + queen (diagonal xray)
                    {
                        attackers = Magic.GetBishopAttacks(to, occ) & (board.GetPieceBB(piece) | board.GetPieceBB(Generate.GetQueen(side)));
                    }
                    else if (pt == 4) // rook + queen (straight xray)
                    {
                        attackers = Magic.GetRookAttacks(to, occ) & (board.GetPieceBB(piece) | board.GetPieceBB(Generate.GetQueen(side)));
                    }
                    else if (pt == 5) // queen thuần (cả 2 hướng)
                    {
                        attackers = Magic.GetQueenAttacks(to, occ) & board.GetPieceBB(piece);
                    }
                    if (attackers != 0)
                    {
                        int attackerSq = BitOperations.TrailingZeroCount(attackers);
                        occ ^= (1UL << attackerSq);
                        gains[depth] = Search.PieceValue(piece) - gains[depth - 1];
                        found = true;
                        side ^= Piece_Color.Black;
                        break;
                    }
                }
                if (!found)
                {
                    break;
                }
                // Return sớm cực đoan: nếu defender không recapture được lợi → stop
                if (-gains[depth] >= threshold)
                {
                    return gains[depth - 1];
                }
            }
            // Min-max stand-pat (defender từ chối khi thua)
            while (depth-- > 0)
            {
                gains[depth] = System.Math.Max(-gains[depth + 1], gains[depth]);
            }
            return gains[0];
        }
    }
}
