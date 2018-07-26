using System;
using System.Diagnostics;
using QDP.Grammar;

namespace QDP
{
    /// <summary>
    /// A token.
    /// </summary>
    public class Token
    {
        public TerminalSymbol TokenType { get; private set; }
        public string Value { get; private set; }
        public Position Position { get; private set; }

        internal Token(TerminalSymbol tokenType, string value, Position position)
        {
            Debug.Assert(tokenType != null, "Token Type Null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(value), "Empty Token Value");
            Debug.Assert(position != null, "Null position.");

            TokenType = tokenType;
            Value = value;
            Position = position;
        }
    }
}
