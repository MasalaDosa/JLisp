namespace JLisp.Values
{
    /// <summary>
    /// A JLisp error value
    /// </summary>
    public class ErrorValue : Value
    {
        public string Value { get; private set; }

        public ErrorValue(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "Error: " + Value;
        }

        internal override Value Copy()
        {
            return new ErrorValue(Value);
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<ErrorValue>();
            if(typed == null)
            {
                return false;
            }

            return Value.Equals(typed.Value, System.StringComparison.Ordinal);
        }

        internal override string Type => TYPE;
        internal const string TYPE = "error";
    }
}
