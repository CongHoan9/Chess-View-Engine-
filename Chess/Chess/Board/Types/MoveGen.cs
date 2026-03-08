using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public enum GenType
    {
        Captures,
        Quiets,
        NonEvasions,
        Evasions,
        Legal,
    }
    public interface IMoveGen
    {
        static abstract GenType Type { get; }
    }
}
