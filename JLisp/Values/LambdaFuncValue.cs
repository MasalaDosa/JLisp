using System;
using System.Diagnostics;

namespace JLisp.Values
{
    /// <summary>
    /// A JLisp defined function.
    /// </summary>
    public class LambdaFuncValue : FuncValue
    {
        public Env Environment { get; private set; }
        public QExprValue Formals { get; private set; }
        public Value Body { get; private set; }

        public LambdaFuncValue(QExprValue formals, Value body)
        {
            Debug.Assert(formals != null, "Null formals.");
            Debug.Assert(body != null, "Null body.");

            // Build new environment
            Environment = new Env(false);

            // Set Formals and Body
            Formals = formals;
            Body = body;
        }

        public override string ToString()
        {
            return $"(\\ {Formals.ToString()} {Body.ToString()})";
        }

        internal override Value Copy()
        {
            var res = new LambdaFuncValue(Formals.Copy() as QExprValue, Body.Copy())
            {
                Environment = Environment.Copy()
            };
            return res;
        }

        internal override bool IsEqivalent(Value other)
        {
            var typed = other.As<LambdaFuncValue>();
            if (typed == null)
            {
                return false;
            }

            if (!Formals.IsEqivalent(typed.Formals))
            {
                return false;
            }

            if (!Body.IsEqivalent(typed.Body ))
            {
                return false;
            }

            // don't care about environment

            return true;
        }
    }
}
