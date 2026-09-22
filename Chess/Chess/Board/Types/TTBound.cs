// file TTBound.cs
namespace Chess
{
    public enum TTBound : byte
    {
        None,
        Upper,
        Lower,
        Exact = Upper | Lower
    }
}
