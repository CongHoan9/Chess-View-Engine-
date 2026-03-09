using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace Chess
{
    public static class Search
    {
        private const int QS_FUTILITY_MARGIN = 100; // pawn + margin
        private const int QS_DELTA_MARGIN = 200;    // queen value
        public const int MaxPly = 512;
        private const int MateScore = 100000;
        private const int MaxHistory = 16384;
    }
}
