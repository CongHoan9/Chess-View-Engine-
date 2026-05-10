using static Chess.Color;
using static Chess.Types;

namespace Chess
{
    using Value = Int32;
    public sealed class NnueFeatureTransformer
    {
        public void Transform(ref Position pos, bool usesmall, List<int> activeindices, out Value psqt)
        {
            if (pos.SideToMove == WHITE)
            {
                Transform<White, Black>(ref pos, usesmall, activeindices, out psqt);
            }
            else
            {
                Transform<Black, White>(ref pos, usesmall, activeindices, out psqt);
            }
        }

        public static void Transform<C, N>(ref Position pos, bool usesmall, List<int> activeindices, out Value psqt) where C : struct, IColor<C, N> where N : struct, IColor<N, C>
        {
            activeindices.Clear();
            Color us = C.Value;
            Color them = N.Value;
            int offset = 0;
            HalfKaV2Hm.Append_Active_Indices(us, pos, activeindices, offset);
            offset += HalfKaV2Hm.Dimensions;
            HalfKaV2Hm.Append_Active_Indices(them, pos, activeindices, offset);
            offset += HalfKaV2Hm.Dimensions;
            if (!usesmall)
            {
                FullThreats.Append_Active_Indices(us, pos, activeindices, offset);
                offset += FullThreats.Dimensions;
                FullThreats.Append_Active_Indices(them, pos, activeindices, offset);
            }
            psqt = Calculate_Psqt(ref pos, us);
        }

        private static unsafe Value Calculate_Psqt(ref Position pos, Color us)
        {
            Value whitematerial = 0;
            Value blackmaterial = 0;
            fixed (Piece* pieceStart = &Types.Pieces[0])
            {
                for (Piece* piecePtr = pieceStart, pieceEnd = pieceStart + PieceArray12.Length; piecePtr != pieceEnd; ++piecePtr)
                {
                    Piece piece = *piecePtr;
                    Value score = pos.PieceCount[(int)piece] * Piece_Value(piece);
                    if (FuncBit.Color_Of(piece) == WHITE)
                    {
                        whitematerial += score;
                    }
                    else
                    {
                        blackmaterial += score;
                    }
                }
            }
            Value value = (whitematerial - blackmaterial) / NnueCommon.OutputScale;
            return us == WHITE ? value : -value;
        }
    }
}
