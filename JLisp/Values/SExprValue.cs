using System.Linq;

namespace JLisp.Values
{
    /// <summary>
    /// An S-Expression value.
    /// S-Expressions - unlike Q-Expressions - are evaluated.
    /// </summary>
    public class SExprValue : ExprValue
    {
        public SExprValue() : base()
        { }

        public override string ToString()
        {
            return "(" + string.Join(" ", Cell.Select(c => c.ToString())) + ")";
        }

        internal override Value Copy()
        {
            var res = new SExprValue();
            foreach (var c in Cell)
            {
                res.Cell.Add(c.Copy());
            }
            return res;
        }

        internal override string Type => TYPE;
        internal const string TYPE = "s-expr";

        /// <summary>
        /// Creates an Q-Expr from the S-Expr
        /// </summary>
        /// <returns>The SE xpression.</returns>
        internal QExprValue CreateQExpression()
        {
            var res = new QExprValue();
            foreach (var c in Cell)
            {
                res.Cell.Add(c.Copy());
            }
            return res;
        }
    }
}
