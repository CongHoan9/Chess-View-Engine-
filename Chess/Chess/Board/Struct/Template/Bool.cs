namespace Chess
{
    public readonly struct True : IBool
    {
        public static bool Value => true;
    }
    public readonly struct False : IBool
    {
        public static bool Value => false;
    }
    public readonly struct UnBool<B> : IBool where B : IBool
    {
        public static bool Value => !B.Value;
    }
}
