using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public enum EGenType
    {
        Captures,
        Quiets,
        NonEvasions,
        Evasions,
        Legal,
    }
    public interface IMoveGen
    {
        static abstract EGenType Type { get; }
    }
}
