using System.Collections.Generic;

namespace JLisp.Values
{
    /// <summary>
    /// Base class for s-expression and q-expression values
    /// </summary>
    public abstract class ExprValue : Value
    {
        public List<Value> Cell { get; private set; }

        internal ExprValue()
        {
            Cell = new List<Value>();
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<ExprValue>();
            if (typed == null)
            {
                return false;
            }

            if (Cell.Count != typed.Cell.Count)
            {
                return false;
            }

            for (int i = 0; i < Cell.Count; i++)
            {
                if (!Cell[i].IsEqivalent(typed.Cell[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
