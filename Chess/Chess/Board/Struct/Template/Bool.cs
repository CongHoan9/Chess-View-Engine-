namespace Chess
{
    public readonly struct STrue : IBool
    {
        public static bool Value => true;
    }
    public readonly struct SFalse : IBool
    {
        public static bool Value => false;
    }
    public readonly struct SUnBool<B> : IBool where B : IBool
    {
        public static bool Value => !B.Value;
    }
}
