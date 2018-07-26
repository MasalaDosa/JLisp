using System.Linq;

namespace JLisp.Values
{
    /// <summary>
    /// A Q-Expression value.
    /// Q-Expressions are not evaluated when encountered.
    /// </summary>
    public class QExprValue : ExprValue
    {
        public QExprValue() : base()
        { }

        public override string ToString()
        {
            return "{" + string.Join(" ", Cell.Select(c => c.ToString())) + "}";
        }

        internal override Value Copy()
        {
            var res = new QExprValue();
            foreach (var c in Cell)
            {
                res.Cell.Add(c.Copy());
            }
            return res;
        }

        internal override string Type => TYPE;
        internal const string TYPE = "q-expr";

        /// <summary>
        /// Creates an S-Expr from the Q-Expr
        /// </summary>
        /// <returns>The SE xpression.</returns>
        internal SExprValue CreateSExpression()
        {
            var res = new SExprValue();
            foreach (var c in Cell)
            {
                res.Cell.Add(c.Copy());
            }
            return res;
        }
    }
}
