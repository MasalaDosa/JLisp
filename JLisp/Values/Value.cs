using System;
using System.Linq;
using System.Collections.Generic;
using QDP.AST;
using System.Diagnostics;
using System.IO;

namespace JLisp.Values
{
    /// <summary>
    /// Base class for values in JLisp.
    /// </summary>
    public abstract class Value
    {
        /// <summary>
        /// Copy this instance.
        /// </summary>
        /// <returns>The copy.</returns>
        internal abstract Value Copy();

        /// <summary>
        /// Is this value equivalent to another.
        /// This is not a strict value equality but is firly close. 
        /// </summary>
        /// <returns><c>true</c>, if eqivalent was ised, <c>false</c> otherwise.</returns>
        /// <param name="other">Other.</param>
        internal abstract bool IsEqivalent(Value other);

        /// <summary>
        /// Returns a friendly type name
        /// </summary>
        /// <value>The type.</value>
        internal abstract string Type { get; }

        /// <summary>
        /// Consumes an ASTNode + Child elements to produce a JListValue node + Child elements.
        /// </summary>
        /// <returns>The ASTN ode.</returns>
        /// <param name="astNode">Ast node.</param>
        internal static Value FromASTNode(AbstractSyntaxNode astNode)
        {
            AbstractSyntaxTerminalNode terminalNode = astNode.As<AbstractSyntaxTerminalNode>();
            AbstractSyntaxRuleNode ruleNode = astNode.As<AbstractSyntaxRuleNode>();

            if (terminalNode != null)
            {
                if (terminalNode.TerminalSymbol.Name.Equals("NUMBER", StringComparison.Ordinal))
                {
                    long l;
                    if (long.TryParse(terminalNode.Value, out l))
                    {
                        return new LongValue(l);
                    }
                    else
                    {
                        return new ErrorValue($"Invalid number '{terminalNode.Value}'.");
                    }
                }
                else if (terminalNode.TerminalSymbol.Name.Equals("SYMBOL", StringComparison.Ordinal))
                {
                    return new SymbolValue(terminalNode.Value);
                }
                else if (terminalNode.TerminalSymbol.Name.Equals("STRING",StringComparison.Ordinal))
                {
                    return StringValue.ReadRegexString(terminalNode.Value);
                }
                else if (terminalNode.TerminalSymbol.Name.Equals("COMMENT", StringComparison.Ordinal))
                {
                    return new SExprValue();
                }

            }
            else if (ruleNode != null)
            {
                if (new string[] { "JLisp", "SExpr" }.Contains(ruleNode.ProductionRule.Name))
                {
                    var s = new SExprValue();
                    BuildJLispExprValueChildren(ruleNode, s.Cell);
                    return s;
                }
                else if (ruleNode.ProductionRule.Name.Equals("QExpr", StringComparison.Ordinal))
                {
                    var q = new QExprValue();
                    BuildJLispExprValueChildren(ruleNode, q.Cell);
                    return q;
                }
                else if (ruleNode.ProductionRule.Name.Equals("Expr", StringComparison.Ordinal))
                {
                    return FromASTNode(ruleNode.Children[0]); // Should only be one !!
                }
            }

            return new ErrorValue("No handling for astNode: " + astNode);
        }

        static void BuildJLispExprValueChildren(AbstractSyntaxRuleNode ruleNode, List<Value> children)
        {
            foreach (var astChild in ruleNode.Children)
            {
                if ((astChild is AbstractSyntaxTerminalNode) &&
                    (new List<string> { "OPENPARENS", "CLOSEPARENS", "OPENBRACE", "CLOSEBRACE" }.
                     Contains((astChild.As<AbstractSyntaxTerminalNode>()).TerminalSymbol.Name)))
                {
                    continue;
                }
                else if(astChild is AbstractSyntaxRuleNode && 
                        astChild.As<AbstractSyntaxRuleNode>().ProductionRule.Name.Equals("Expr", StringComparison.Ordinal ) &&
                        astChild.As<AbstractSyntaxRuleNode>().Children.Count == 1 &&
                        astChild.As<AbstractSyntaxRuleNode>().Children[0] is AbstractSyntaxTerminalNode &&
                        astChild.As<AbstractSyntaxRuleNode>().Children[0].As<AbstractSyntaxTerminalNode>().TerminalSymbol.Name.Equals("COMMENT", StringComparison.Ordinal))
                {
                    continue;
                }
                    
                else
                {
                    children.Add(FromASTNode(astChild));
                }
            }
        }

        static Value EvalSexpr(Env environment, SExprValue sexpr)
        {
            // Evaluate Children 
            for (int i = 0; i < sexpr.Cell.Count; i++)
            {
                sexpr.Cell[i] = Eval(environment, sexpr.Cell[i]);
            }

            // Check for errors - and return the first encountered
            var errored = sexpr.Cell.Where(c => c is ErrorValue).FirstOrDefault();
            if (errored != null)
            {
                return errored;
            }

            // Empty Expression
            if (sexpr.Cell.Count == 0)
            {
                return sexpr;
            }

            // Single Expression
            if (sexpr.Cell.Count == 1)
            {
                return sexpr.Cell[0];
            }

            // Ensure first element is a function after evaluation
            Value first = sexpr.Cell[0];
            sexpr.Cell.RemoveAt(0);
            if (!(first is FuncValue))
            {
                return new ErrorValue($"s-expr Does not start with a function.  Encountered '{first.Type}'.");
            }

            // Call the function
            return Call(environment, first.As<FuncValue>(), sexpr);
        }

        internal static Value Eval(Env environment, Value value)
        {
            if (value is SymbolValue)
            {
                return environment.Get(value.As<SymbolValue>().Value);
            }

            // Evaluate Sexpressions
            if (value is SExprValue)
            {
                return EvalSexpr(environment, value.As<SExprValue>());
            }
            // All other lval types remain the same
            return value;
        }

        internal static Value Call(Env environment, FuncValue function, SExprValue arguments)
        {
            BuiltinFuncValue builtin = function.As<BuiltinFuncValue>();
            LambdaFuncValue lambda = function.As<LambdaFuncValue>();

            // If Builtin then simply call that
            if(builtin != null)
            {
                return builtin.BuiltinFunction(environment, arguments);
            }
           
            // Record Argument Counts
            int given = arguments.Cell.Count;
            int total = lambda.Formals.Cell.Count;

            // While arguments still remain to be processed
            while (arguments.Cell.Count > 0)
            {

                // If we've ran out of formal arguments to bind
                if (lambda.Formals.Cell.Count == 0)
                {
                    return new ErrorValue($"Function passed too many arguments. Got {given}, Expected {total}.");
                }

                // Pop the first symbol from the formals
                SymbolValue symbol = lambda.Formals.Cell[0].As<SymbolValue>();
                lambda.Formals.Cell.RemoveAt(0);

                // Special Case to deal with '&'
                if (symbol.Value == "&")
                {
                    // Ensure '&' is followed by another symbol
                    if (lambda.Formals.Cell.Count != 1)
                    {
                        return new ErrorValue("Function format invalid.  Symbol '&' not followed by single symbol.");
                    }

                    // Next formal should be bound to remaining arguments.
                    SymbolValue nsym = lambda.Formals.Cell[0].As<SymbolValue>();
                    lambda.Formals.Cell.RemoveAt(0);
                    lambda.Environment.Put(nsym.Value, Builtins.List(environment, arguments));
                    break;
                }

                // Pop the next argument from the list
                Value val = arguments.Cell[0];
                arguments.Cell.RemoveAt(0);

                // Bind a copy into the function's environment
                lambda.Environment.Put(symbol.Value, val);
            }

            // Argument list is now bound.

            // if '&' remains in formal list after we've bound arguments then bind to empty list 
            if (lambda.Formals.Cell.Count > 0 &&
                lambda.Formals.Cell[0].As<SymbolValue>().Value == "&")
            {
                // check to ensure that & is not passed invalidly.
                if (lambda.Formals.Cell.Count != 2)
                {
                    return new ErrorValue("Function format invalid.  Symbol '&' not followed by single symbol.");
                }

                // Pop and throw away '&' symbol
                lambda.Formals.Cell.RemoveAt(0);

                // Pop next symbol and create empty list 
                SymbolValue symbolValue = lambda.Formals.Cell[0].As<SymbolValue>();
                lambda.Formals.Cell.RemoveAt(0);

                QExprValue val = new QExprValue();

                // Bind to environment and delete 
                lambda.Environment.Put(symbolValue.Value, val);
            }
           
            // If all formals have been bound evaluate 
            if (lambda.Formals.Cell.Count == 0)
            {

                // Set environment parent to evaluation environment
                lambda.Environment.Parent  = environment;

                // Evaluate and return
                var sexpr = new SExprValue();
                sexpr.Cell.Add(lambda.Body.Copy());
                return Builtins.Eval(lambda.Environment, sexpr);
            }
            else
            {
                // Otherwise return partially evaluated function
                return lambda.Copy();
            }
        }
    }
}
