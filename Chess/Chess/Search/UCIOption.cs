using System.Collections;

namespace Chess
{
    public enum UCIOption_Type
    {
        CHECK,
        SPIN,
        STRING,
        COMBO,
        BUTTON
    }

    public sealed class UCIOption(string name, UCIOption_Type type, string defaultvalue, int min = 0, int max = 0, string[] combovalues = null, Action<UCIOption> on_change = null)
    {
        private string CurrentValue = defaultvalue ?? string.Empty;
        public string Name { get; } = name;
        public UCIOption_Type Type { get; } = type;
        public string DefaultValue { get; } = defaultvalue ?? string.Empty;
        public int Min { get; } = min;
        public int Max { get; } = max;
        public string[] ComboValues { get; } = combovalues ?? [];
        public Action<UCIOption> On_Change { get; } = on_change;

        public string Current_Value()
        {
            return CurrentValue;
        }

        public bool Bool_Value()
        {
            return string.Equals(CurrentValue, "true", StringComparison.OrdinalIgnoreCase);
        }

        public int Int_Value()
        {
            return Misc.Try_Parse_Int(CurrentValue, out int value) ? value : 0;
        }

        public void Set_Value(string value)
        {
            switch (Type)
            {
                case UCIOption_Type.CHECK:
                    CurrentValue = string.IsNullOrWhiteSpace(value) || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
                    break;
                case UCIOption_Type.SPIN:
                    int spinvalue = Misc.Try_Parse_Int(value, out int parsedvalue) ? parsedvalue : Int_Value();
                    CurrentValue = Misc.Clamp(spinvalue, Min, Max).ToString();
                    break;
                case UCIOption_Type.COMBO:
                    if (ComboValues.Length == 0 || ComboValues.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        CurrentValue = string.IsNullOrWhiteSpace(value) ? DefaultValue : value;
                    }
                    break;
                case UCIOption_Type.BUTTON:
                    On_Change?.Invoke(this);
                    return;
                default:
                    CurrentValue = value ?? string.Empty;
                    break;
            }
            On_Change?.Invoke(this);
        }

        public string To_UCI()
        {
            return Type switch
            {
                UCIOption_Type.CHECK => $"option name {Name} type check default {DefaultValue}",
                UCIOption_Type.SPIN => $"option name {Name} type spin default {DefaultValue} min {Min} max {Max}",
                UCIOption_Type.COMBO => $"option name {Name} type combo default {DefaultValue} {string.Join(' ', ComboValues.Select(item => $"var {item}"))}",
                UCIOption_Type.BUTTON => $"option name {Name} type button",
                _ => $"option name {Name} type string default {DefaultValue}",
            };
        }
    }

    public sealed class UCIOption_Map : IEnumerable<UCIOption>
    {
        private readonly Dictionary<string, UCIOption> Options = new(StringComparer.OrdinalIgnoreCase);

        public void Add(UCIOption option)
        {
            Options[option.Name] = option;
        }

        public bool Contains(string name)
        {
            return Options.ContainsKey(name);
        }

        public UCIOption Get(string name)
        {
            return Options.TryGetValue(name, out UCIOption option) ? option : null;
        }

        public string Get_String(string name, string fallback = "")
        {
            UCIOption option = Get(name);
            return option == null ? fallback : option.Current_Value();
        }

        public int Get_Int(string name, int fallback = 0)
        {
            UCIOption option = Get(name);
            return option == null ? fallback : option.Int_Value();
        }

        public bool Get_Bool(string name, bool fallback = false)
        {
            UCIOption option = Get(name);
            return option == null ? fallback : option.Bool_Value();
        }

        public void Set_Option(string name, string value)
        {
            UCIOption option = Get(name);
            option?.Set_Value(value);
        }

        public IEnumerator<UCIOption> GetEnumerator()
        {
            return Options.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
