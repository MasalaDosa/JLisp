using System.Diagnostics;

namespace JLisp.Values
{
    /// <summary>
    /// A delegate for built-in functions.
    /// </summary>
    public delegate Value Builtin(Env e, Value v);

    /// <summary>
    /// A JLisp Built in function value.
    /// </summary>
    public class BuiltinFuncValue : FuncValue
    {
        public Builtin BuiltinFunction { get; private set; }

        public BuiltinFuncValue(Builtin function)
        {
            Debug.Assert(function != null, "Null function.");
            BuiltinFunction = function;
        }

        public override string ToString()
        {
            return $"<{BuiltinFunction.Method.Name}>";
        }

        internal override Value Copy()
        {
            return new BuiltinFuncValue(BuiltinFunction);
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<BuiltinFuncValue>();
            if (typed == null)
            {
                return false;
            };

            if(!BuiltinFunction.Equals(typed.BuiltinFunction))
            {
                return false;
            }

            return true;
        }
    }
}
