namespace JLisp.Values
{
    /// <summary>
    /// A symbol value in JLisp
    /// </summary>
    public class SymbolValue : Value
    {
        public string Value { get; private set; }

        public SymbolValue(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        internal override Value Copy()
        {
            return new SymbolValue(Value);
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<SymbolValue>();
            if (typed == null)
            {
                return false;
            }

            return Value.Equals(typed.Value, System.StringComparison.Ordinal);
        }

        internal override string Type => TYPE;
        internal const string TYPE = "symbol";
    }
}
