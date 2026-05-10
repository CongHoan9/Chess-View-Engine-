using static Chess.Types;

namespace Chess
{
    using Value = Int32;
    public readonly struct Score(Value value)
    {
        public readonly Value Value = value;

        public bool Is_Mate()
        {
            return Math.Abs(Value) >= VALUE_MATE_IN_MAX_PLY;
        }

        public bool Is_TableBase()
        {
            int AbsValue = Math.Abs(Value);
            return AbsValue >= VALUE_TB_WIN_IN_MAX_PLY && AbsValue < VALUE_MATE_IN_MAX_PLY;
        }

        public Value Mate_In()
        {
            Value AbsValue = Math.Abs(Value);
            Value Plies = VALUE_MATE - AbsValue;
            Value Moves = (Plies + 1) / 2;
            return Value >= 0 ? Moves : -Moves;
        }

        public Value To_Centipawns()
        {
            return Value;
        }

        public override string ToString()
        {
            return Is_Mate() ? $"mate {Mate_In()}" : $"cp {To_Centipawns()}";
        }

        public static implicit operator Score(Value value)
        {
            return new Score(value);
        }

        public static implicit operator Value(Score score)
        {
            return score.Value;
        }
    }
}
