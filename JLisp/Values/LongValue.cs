using System.Globalization;

namespace JLisp.Values
{
    /// <summary>
    /// An Int64 value.
    /// </summary>
    public class LongValue : Value
    {
        public long Value { get; set; }

        public LongValue(long value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        internal override Value Copy()
        {
            return new LongValue(Value);
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<LongValue>();
            if (typed == null)
            {
                return false;
            }

            return Value == typed.Value;
        }

        internal override string Type => TYPE;
        internal const string TYPE = "long";
    }
}
