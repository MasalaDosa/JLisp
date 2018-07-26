using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace JLisp.Values
{
    /// <summary>
    /// A JLisp string value.
    /// </summary>
	public class StringValue : Value
    {
        public string Value { get; private set; }

        public StringValue(string unescapedValue)
        {
            Value = unescapedValue;
        }

        public override string ToString()
        {
            // escape the stored string;
            string escaped = Regex.Escape(Value);
            return "\"" + escaped.Replace("\"", "\\\"").Replace("\\ "," ") + "\"";
        }

        internal override Value Copy()
        {
            return new StringValue(Value);
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<StringValue>();
            if (typed == null)
            {
                return false;
            }

            return Value.Equals(typed.Value, System.StringComparison.Ordinal);
        }

        internal override string Type => TYPE;
        internal const string TYPE = "string";

        /// <summary>
        /// Read a string as matched by the QDP.UsefulRegex.String.
        /// This should be an escaped string, surrounded by dbl-quotes.
        /// </summary>
        /// <returns>The regex string.</returns>
        /// <param name="regexMatchedString">Regex matched string.</param>
        public static StringValue ReadRegexString(string regexMatchedString) 
        {
            Debug.Assert(regexMatchedString.Length >= 2 &&
                         regexMatchedString.First() == '"' &&
                         regexMatchedString.Last() == '"',
                         "Invalid string format");
            // Remove the start and end quotes.
            string escaped = regexMatchedString.Substring(1);
            escaped = escaped.Substring(0, escaped.Length - 1);
            return ReadEscapedString(escaped);
        }

        /// <summary>
        /// Read an escaped string.
        /// </summary>
        /// <returns>The regex string.</returns>
        /// <param name="regexMatchedString">Regex matched string.</param>
        public static StringValue ReadEscapedString(string escaped)
        {
            // Unescape this
            string unescaped = Regex.Unescape(escaped);
            return new StringValue(unescaped);
        }
    }
}
