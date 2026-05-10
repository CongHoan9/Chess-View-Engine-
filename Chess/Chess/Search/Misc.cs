using System.Globalization;

namespace Chess
{
    public static class Misc
    {
        public static string Engine_Name()
        {
            return "Chess";
        }

        public static string Engine_Info(bool ToUci = false)
        {
            return ToUci ? $"id name {Engine_Name()}\nid author 84352" : Engine_Name();
        }

        public static string[] Split_Tokens(string Line)
        {
            return Line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static string To_Lower(string Value)
        {
            return Value.ToLowerInvariant();
        }

        public static int Clamp(int Value, int Min, int Max)
        {
            return Math.Max(Min, Math.Min(Max, Value));
        }

        public static bool Try_Parse_Int(string Text, out int Value)
        {
            return int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out Value);
        }
    }
}
