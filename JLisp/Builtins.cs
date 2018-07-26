using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JLisp.Values;

namespace JLisp
{
    /// <summary>
    /// Built-ins JLisp function
    /// </summary>
    internal static class Builtins
    {

        // Constants until implement a boolean type.
        internal const long FALSE = 0;
        internal const long TRUE = 1;

        #region "Argument Verification"

        static ErrorValue ArgumentsExactCount(SExprValue sExprValue, int n, string name)
        {
            if (sExprValue == null || sExprValue.Cell.Count != n)
            {
                return new ErrorValue($"Function '{name}' expects {n} arguments.  Got {sExprValue.Cell.Count}.");
            }
            return null;
        }

        static ErrorValue ArgumentsAtLeastCount(SExprValue sexprValue, int n, string name)
        {
            if (sexprValue == null || sexprValue.Cell.Count < n)
            {
                return new ErrorValue($"Function '{name}' expects at least {n} arguments.  Got {sexprValue.Cell.Count}.");
            }
            return null;
        }

        static ErrorValue ArgumentsIOfType(SExprValue sexprValue, int i, string type, string name)
        {
            Debug.Assert(sexprValue.Cell.Count > i, "i too high");
            if (sexprValue.Cell[i].Type != type)
            {
                return new ErrorValue($"Function '{name}' expects argument {i} to be of type '{type}'.  Got {sexprValue.Cell[i].Type}.");
            }
            return null;
        }

        static ErrorValue ArgumentsAllOfType(SExprValue sexprValue, string type, string name)
        {
            if (sexprValue.Cell.Any(c => c.Type != type))
            {
                var example = sexprValue.Cell.First(c => c.Type != type);
                var index = sexprValue.Cell.IndexOf(example);
                return new ErrorValue($"Function '{name}' expects all arguments to be of type '{type}'.  Got {example.Type} at {index}.");
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Add.
        /// </summary>
        internal static Value Add(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "+");
        }

        /// <summary>
        /// Subtract.
        /// </summary>
        internal static Value Subtract(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "-");
        }

        /// <summary>
        /// Multiply.
        /// </summary>
        internal static Value Multiply(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "*");
        }

        /// <summary>
        /// Divide.
        /// </summary>
        internal static Value Divide(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "/");
        }

        /// <summary>
        /// Modulus.
        /// </summary>
        internal static Value Modulus(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "%");
        }

        /// <summary>
        /// Power.
        /// </summary>
        internal static Value Power(Env environment, Value arguments)
        {
            return MathOp(environment, arguments, "^");
        }

        /// <summary>
        /// Mathematical functions
        /// </summary>
        static Value MathOp(Env environment, Value arguments, string op)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Ensure all numbers
            var check = ArgumentsAllOfType(sexprValue, LongValue.TYPE, op);
            if (check != null) return check;

            // Pop the first element
            Value xBase = sexprValue.Cell[0];
            LongValue x = xBase.As<LongValue>();
            sexprValue.Cell.RemoveAt(0);

            // If no arguments and sub then perform unary negation
            if (op.Equals ("-", StringComparison.Ordinal) && sexprValue.Cell.Count == 0)
            {
                x.Value = -x.Value;
            }

            // While there are still elements remaining
            while (sexprValue.Cell.Count > 0)
            {
                // Pop the next element
                LongValue y = sexprValue.Cell[0].As<LongValue>();
                sexprValue.Cell.RemoveAt(0);

                if (op.Equals("+", StringComparison.Ordinal))
                {
                    x.Value += y.Value;
                }
                else if (op.Equals("-", StringComparison.Ordinal))
                {
                    x.Value -= y.Value;
                }
                else if (op.Equals("*", StringComparison.Ordinal))
                {
                    x.Value *= y.Value;
                }
                else if (op.Equals("/", StringComparison.Ordinal))
                {
                    if (y.Value == 0)
                    {
                        xBase = new ErrorValue("Division By Zero.");
                        break;
                    }
                    x.Value /= y.Value;
                }
                else if (op.Equals("%", StringComparison.Ordinal))
                {
                    if (y.Value == 0)
                    {
                        xBase = new ErrorValue("Division By Zero.");
                        break;
                    }
                    x.Value %= y.Value;
                }
                else if (op.Equals("^", StringComparison.Ordinal))
                {
                    x.Value = (long)Math.Pow(x.Value, y.Value);
                }
            }

            return xBase;
        }

        /// <summary>
        /// Greater than
        /// </summary>
        internal static Value GreaterThan(Env environment, Value a)
        {
            return OrdinalOp(environment, a, ">");
        }

        /// <summary>
        /// Less than
        /// </summary>
        internal static Value LessThan(Env environment, Value a)
        {
            return OrdinalOp(environment, a, "<");
        }

        /// <summary>
        /// Greater than or equal
        /// </summary>
        internal static Value GreaterThanOrEqual(Env environment, Value a)
        {
            return OrdinalOp(environment, a, ">=");
        }

        /// <summary>
        /// Less than or equal
        /// </summary>
        internal static Value LessThanOrEqual(Env environment, Value a)
        {
            return OrdinalOp(environment, a, "<=");
        }

        /// <summary>
        /// Ordinal functions
        /// </summary>
        static Value OrdinalOp(Env environment, Value arguments, string op)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 2, op);
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, LongValue.TYPE, op);
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 1, LongValue.TYPE, op);
            if (check != null) return check;

            bool r = false;

            var n1 = sexprValue.Cell[0].As<LongValue>().Value;
            var n2 = sexprValue.Cell[1].As<LongValue>().Value;

            if (op.Equals(">", StringComparison.Ordinal))
            {
                r = n1 > n2;
            }
            if (op.Equals("<", StringComparison.Ordinal))
            {
                r = n1 < n2;
            }
            if (op.Equals(">=", StringComparison.Ordinal))
            {
                r = n1 >= n2;
            }
            if (op.Equals("<=", StringComparison.Ordinal))
            {
                r = n1 <= n2;
            }

            if (r == false)
            {
                return new LongValue(FALSE);
            }
            else
            {
                return new LongValue(TRUE);
            }
        }

        /// <summary>
        /// Equal.
        /// </summary>
        internal static Value Equals(Env environment, Value arguments)
        {
            return EqualityOp(environment, arguments, "==");
        }

        /// <summary>
        /// Not Equal.
        /// </summary>
        internal static Value NotEquals(Env environment, Value arguments)
        {
            return EqualityOp(environment, arguments, "!=");
        }

        /// <summary>
        /// Comparisons.
        /// </summary>
        static Value EqualityOp(Env environment, Value arguments, string op)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 2, op);
            if (check != null) return check;

            bool r = false;
            if (op.Equals("==", StringComparison.Ordinal))
            {
                r = sexprValue.Cell[0].IsEqivalent(sexprValue.Cell[1]);
            }
                if (op.Equals("!=", StringComparison.Ordinal))
            {
                r = !sexprValue.Cell[0].IsEqivalent(sexprValue.Cell[1]);
            }

            if (r == false)
            {
                return new LongValue(FALSE);
            }
            else
            {
                return new LongValue(TRUE);
            }
        }

        /// <summary>
        /// If 0th argument is FALSE, evaluate the 2nd argument, otherwise evaluate the 1st argument.
        /// Argument 0 should be LongValue.
        /// Arguments 1 and 2 should be Q-Expressions
        /// </summary>
        internal static Value If(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 3, "if");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, LongValue.TYPE, "if");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 1, QExprValue.TYPE, "if");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 2, QExprValue.TYPE, "if");
            if (check != null) return check;

            Value res;
            if (sexprValue.Cell[0].As<LongValue>().Value != FALSE)
            {
                var s = sexprValue.Cell[1].As<QExprValue>().CreateSExpression();
                res = environment.Evaluate(s);
            }
            else
            {
                var s = sexprValue.Cell[2].As<QExprValue>().CreateSExpression();
                res = environment.Evaluate(s);
            }
            return res; ;
        }

        /// <summary>
        /// Returns a list containing the head of supplied list.
        /// Argument 0 should be a Q-Expression representing the list
        /// </summary>
        internal static Value Head(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 1, "head");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, QExprValue.TYPE, "head");
            if (check != null) return check;

            if (sexprValue.Cell[0].As<QExprValue>().Cell.Count == 0)
            {
                return new ErrorValue("Function 'head' passed {}.");
            }

            // Otherwise take first argument
            var qexpr = sexprValue.Cell[0].As<QExprValue>();

            // Remove everything but the head
            while (qexpr.Cell.Count > 1)
            {
                qexpr.Cell.RemoveAt(1);
            }

            return qexpr; // And return
        }

        /// <summary>
        /// Returns the tail of a list
        /// Argument 0 should be a Q-Expression representing the list
        /// </summary>
        internal static Value Tail(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Check Error Conditions
            var check = ArgumentsExactCount(sexprValue, 1, "tail");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, QExprValue.TYPE, "tail");
            if (check != null) return check;

            if (sexprValue.Cell[0].As<QExprValue>().Cell.Count == 0)
            {
                return new ErrorValue("Function 'tail' passed {}.");
            }

            // Take first argument
            var qexpr = sexprValue.Cell[0].As<QExprValue>();

            // And remove the head
            qexpr.Cell.RemoveAt(0);

            return qexpr;
        }

        /// <summary>
        /// Converts an S-Expression into a Q-Expression
        /// </summary>
        /// <returns>The list.</returns>
        internal static Value List(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");
            return sexprValue.CreateQExpression();;
        }

        /// <summary>
        /// Evaluates the q-expression contained as first child of this s-expr
        /// </summary>
        /// <returns>The eval.</returns>
        internal static Value Eval(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 1, "eval");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, QExprValue.TYPE, "eval");
            if (check != null) return check;

            QExprValue x = sexprValue.Cell[0].As<QExprValue>();
            return environment.Evaluate(x.CreateSExpression());
        }

        /// <summary>
        /// Joins multiple q-expressions together
        /// </summary>
        /// <returns>The join.</returns>
        internal static Value Join(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsAllOfType(sexprValue, QExprValue.TYPE, "join");
            if (check != null) return check;

            QExprValue x = sexprValue.Cell[0].As<QExprValue>();
            sexprValue.Cell.RemoveAt(0);

            while (sexprValue.Cell.Count > 0)
            {
                QExprValue y = sexprValue.Cell[0].As<QExprValue>();
                sexprValue.Cell.RemoveAt(0);
                x = Join(environment, x, y);
            }

            return x;
        }

        /// <summary>
        /// Joins two q-expressions by copying all chilren of y into x
        /// </summary>
        /// <returns>The isp value join.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        static QExprValue Join(Env environment, QExprValue x, QExprValue y)
        {
            // For each child in 'y' add it to 'x'
            while (y.Cell.Count > 0)
            {
                var z = y.Cell[0];
                y.Cell.RemoveAt(0);
                x.Cell.Add(z);
            }

            return x;
        }

        /// <summary>
        /// Creates a lambda function consisting of two q-expressions
        /// representing the formal parameters (all must be symbols) and the body.
        /// </summary>
        internal static Value Lambda(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Check Error Conditions
            var check = ArgumentsExactCount(sexprValue, 2, "lambda");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, QExprValue.TYPE, "lambda (formals)");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 1, QExprValue.TYPE, "lambda (body)");
            if (check != null) return check;

            // Check first Q-Expression contains only Symbols
            QExprValue formals = sexprValue.Cell[0].As<QExprValue>();
            for (int i = 0; i < formals.Cell.Count; i++)
            {
                if (!(formals.Cell[i] is SymbolValue))
                {
                    return new ErrorValue($"Cannot define non-symbol.  Got {formals.Cell[i].Type} Expected {SymbolValue.TYPE}.");
                }
            }

            Value body = sexprValue.Cell[1];

            return new LambdaFuncValue(formals, body);
        }

        /// <summary>
        /// Defines a symbol in global environment.
        /// </summary>
        internal static Value Def(Env environment, Value arguments)
        {
            return Var(environment, arguments, "def");
        }

        /// <summary>
        /// Puts a symbol in local environment.
        /// </summary>
        internal static Value Put(Env environment, Value arguments)
        {
            return Var(environment, arguments, "=");
        }

        /// <summary>
        /// Variable put and def.
        /// </summary> 
        static Value Var(Env environment, Value arguments, string func)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsIOfType(sexprValue, 0, QExprValue.TYPE, func);
            if (check != null) return check;

            QExprValue syms = sexprValue.Cell[0].As<QExprValue>();

            for (int i = 0; i < syms.Cell.Count; i++)
            {
                if (!(syms.Cell[i] is SymbolValue))
                {
                    return new ErrorValue($"Function '{func}' cannot define non-symbol.  Got {syms.Cell[i].Type}. Expected {SymbolValue.TYPE}");
                }
            }

            if (syms.Cell.Count != sexprValue.Cell.Count - 1)
            {
                return new ErrorValue($"Function '{func}' passed wrong number of arguments.  Got {syms.Cell.Count} symbols and {sexprValue.Cell.Count - 1} arguments.");
            }

            for (int i = 0; i < syms.Cell.Count; i++)
            {
                // If 'def' define in globally. If 'put' define in locally
                if (func == "def")
                {
                    environment.Def(syms.Cell[i].As<SymbolValue>().Value, sexprValue.Cell[i + 1]);
                }

                if (func == "=")
                {
                    environment.Put(syms.Cell[i].As<SymbolValue>().Value, sexprValue.Cell[i + 1]);
                }
            }

            return new SExprValue();
        }
                
        /// <summary>
        /// Load the specified file and evaluates its contents
        /// </summary>
        internal static Value Load(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 1, "load");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, StringValue.TYPE, "load");
            if (check != null) return check;

            string fileContents;
            try
            {
                string fileName = sexprValue.Cell[0].As<StringValue>().Value;
                if(!Path.IsPathRooted(fileName))
                {
                    var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    fileName = Path.Combine(folder, fileName);
                }

                fileContents = File.ReadAllText(fileName);
            }
            catch (IOException e)
            {
                return new ErrorValue("Unable to load: " + e.Message);
            }

            foreach (var result in environment.Evaluate(fileContents))
            {
                // Dump error values only
                if (result is ErrorValue)
                {
                    Console.WriteLine(result);
                }
            }
            return new SExprValue(); // Empty S-Expr to finish
        }

        /// <summary>
        /// Print arguments to console.
        /// </summary>
        internal static Value Print(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Print each argument followed by a space
            Console.WriteLine(string.Join(" ", sexprValue.Cell));

            return new SExprValue();
        }

        /// <summary>
        /// Similar to print, but shows string values unescpaed.
        /// </summary>
        internal static Value Show(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Print each argument followed by a space
            Console.WriteLine(string.Join(" ", sexprValue.Cell.Select(c => c.Type == StringValue.TYPE ? c.As<StringValue>().Value : c.ToString())));

            return new SExprValue();
        }

        /// <summary>
        /// Generates an error value
        /// </summary>
        internal static Value Error(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            var check = ArgumentsExactCount(sexprValue, 1, "error");
            if (check != null) return check;

            check = ArgumentsIOfType(sexprValue, 0, StringValue.TYPE, "load");
            if (check != null) return check;

            // Construct Error from first argument
            return new ErrorValue(sexprValue.Cell[0].As<StringValue>().Value);
        }

        /// <summary>
        /// Lists the environment (and optionally parent environment) as key value pairs
        /// (A Q-expr of Q-exprs)
        /// </summary>
        internal static Value ListEnv(Env environment, Value arguments)
        {
            SExprValue sexprValue = arguments.As<SExprValue>();
            Debug.Assert(sexprValue != null, "Non s-expr");

            // Check Error Conditions
            var check = ArgumentsExactCount(sexprValue, 1, "printenv");
            if (check != null) return check;

            var result = new QExprValue();

            foreach(var k in environment.Keys(true))
            {
                var kvp = new QExprValue();
                kvp.Cell.Add(new SymbolValue(k));
                kvp.Cell.Add(environment.Get(k));
                result.Cell.Add(kvp);
            }

            return result;
        }
    }
}
