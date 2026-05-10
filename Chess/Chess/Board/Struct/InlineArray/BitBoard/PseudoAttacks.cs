using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Chess.Bitboards;
using static Chess.FuncBit;
using static Chess.PieceType;
using static Chess.Color;
namespace Chess
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct PseudoAttacks
    {
        private static readonly Bitboard* RawPtr;
        public ref readonly Bitboard this[int pc, int sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref RawPtr[(pc << 6) | sq];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static PseudoAttacks()
        {
            RawPtr = (Bitboard*)NativeMemory.AllocZeroed(((int)PIECE_TYPE_NB * 64), (uint)sizeof(Bitboard));
            for (int s = 0; s < 64; s++)
            {
                Square sq = (Square)s;
                Bitboard b = Square_BB(sq);
                RawPtr[((int)WHITE << 6) | s] = Pawn_Attacks_BB<White, Black>(b);
                RawPtr[((int)BLACK << 6) | s] = Pawn_Attacks_BB<Black, White>(b);
                RawPtr[((int)KNIGHT << 6) | s] = Pseudo_Attacks<Knight>(sq);
                RawPtr[((int)QUEEN << 6) | s] = RawPtr[((int)BISHOP << 6) | s] = Pseudo_Attacks<Bishop>(sq);
                RawPtr[((int)QUEEN << 6) | s] |= RawPtr[((int)ROOK << 6) | s] = Pseudo_Attacks<Rook>(sq);
                RawPtr[((int)KING << 6) | s] = Pseudo_Attacks<King>(sq);
            }
        }
    }
}
