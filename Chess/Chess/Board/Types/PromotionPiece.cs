using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public interface IPromotionPiece
    {
        public static abstract PieceType Piece { get; } 
    }
}
