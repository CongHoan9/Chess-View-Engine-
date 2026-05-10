using System.Text;

namespace Chess
{
    using Value = Int32;
    public sealed class NnueEvalFile(string defaultname)
    {
        public string DefaultName { get; set; } = defaultname;
        public string Current { get; set; } = defaultname;
        public string NetDescription { get; set; } = string.Empty;
    }

    public readonly struct NnueEvalTrace(Value psqt, Value positional, bool usesmall, string description)
    {
        public readonly Value Psqt = psqt;
        public readonly Value Positional = positional;
        public readonly bool UseSmall = usesmall;
        public readonly string Description = description ?? string.Empty;

        public Value Total()
        {
            return Psqt + Positional;
        }
    }

    public static class NnueMisc
    {
        public static string Format_Trace(NnueEvalTrace trace)
        {
            StringBuilder builder = new();
            builder.AppendLine($"Network: {(trace.UseSmall ? "Small" : "Big")}");
            builder.AppendLine($"Description: {trace.Description}");
            builder.AppendLine($"PSQT: {trace.Psqt}");
            builder.AppendLine($"Positional: {trace.Positional}");
            builder.Append($"Total: {trace.Total()}");
            return builder.ToString();
        }
    }
}
