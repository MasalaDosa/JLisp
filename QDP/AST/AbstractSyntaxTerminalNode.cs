using System.Diagnostics;
using QDP.Grammar;

namespace QDP.AST
{
    /// <summary>
    /// A terminal (leaf) node in our abstract syntax tree.
    /// </summary>
    public class AbstractSyntaxTerminalNode : AbstractSyntaxNode
    {
        /// <summary>
        /// The Terminal Symbol that was encountered
        /// </summary>
        /// <value>The terminal symbol.</value>
        public TerminalSymbol TerminalSymbol { get; private set; }

        /// <summary>
        /// Its value, as a string
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; private set; }
        public override string ToString()
        {
            return TerminalSymbol.Name +
                                 (string.IsNullOrWhiteSpace(Value) ? string.Empty : $" '{Value}'") +
                                 $" {Position}" +
                                 $" :{Content}";
        }

        internal AbstractSyntaxTerminalNode(TerminalSymbol terminalSymbol, string value, Position position, string content) : base(position, content)
        {
            Debug.Assert(terminalSymbol != null, "Terminal Symbol is required.");
            TerminalSymbol = terminalSymbol;
            Value = value ?? string.Empty;
        }
    }
}
