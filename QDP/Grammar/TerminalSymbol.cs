using System;
using System.Text.RegularExpressions;

namespace QDP.Grammar
{
    /// <summary>
    /// A Terminal symbol in our grammar.
    /// </summary>
    public class TerminalSymbol
    {
        public string Name { get; private set; }
        public Regex Regex { get; private set; }
        public bool Ignore { get; private set; }

        public TerminalSymbol(string name, Regex regex, bool ignore = false)
        {
            Validation.Requires<ArgumentNullException>(() => name != null, $"{nameof(name)} is required.");
            Validation.Requires<ArgumentException>(() => name != null, $"{nameof(name)} cannot be empty.");
            Validation.Requires<ArgumentNullException>(() => regex != null, $"{nameof(regex)} is required.");

            Name = name;
            Regex = regex;
            Ignore = ignore;
        }
    }
}
