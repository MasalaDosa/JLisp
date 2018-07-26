using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JLisp.Values;
using QDP;
using QDP.AST;
using QDP.Grammar;

namespace JLisp
{
    /// <summary>
    /// JLisp environment.
    /// </summary>
    public class Env 
    {
        internal int Level()
        {
            if(Parent == null)
            {
                return 0;
            }
            else
            {
                return 1 + Parent.Level();
            }
        }
        /// <summary>
        /// Optional parent environment.
        /// </summary>
        /// <value>The parent.</value>
        internal Env Parent { get; set; }

        /// <summary>
        /// Public constructor for root environment.
        /// </summary>
        public static Env New()
        {
            return new Env(true);
        }

        Dictionary<string, Value> _envDict;

        /// <summary>
        /// Internal constructor for child environments.
        /// </summary>
        internal Env(bool addBuiltins) : base()
        {
            _envDict = new Dictionary<string, Value>();
            if (addBuiltins)
            {
                AddBuiltins();
            }
        }

        internal IEnumerable<string> Keys(bool localOnly)
        {
            foreach (var k in _envDict.Keys)
            {
                yield return k;
            }
            if(!localOnly && Parent != null)
            {
                foreach (var k in Parent.Keys(true))
                {
                    yield return k;
                }
            }
            yield break;
        }

        /// <summary>
        /// Registers a built in function.
        /// </summary>
        void AddBuiltin(string name, Builtin func)
        {
            Value v = new BuiltinFuncValue(func);
            _envDict[name] = v;
        }

        /// <summary>
        /// Registers all built in functions
        /// </summary>
        internal void AddBuiltins()
        {
            //List Functions
            AddBuiltin("list", Builtins.List);
            AddBuiltin("head", Builtins.Head);
            AddBuiltin("tail", Builtins.Tail);
            AddBuiltin("eval", Builtins.Eval);
            AddBuiltin("join", Builtins.Join);

            // Function Declaration
            AddBuiltin("\\", Builtins.Lambda);

            // Mathematical Functions
            AddBuiltin("+", Builtins.Add);
            AddBuiltin("-", Builtins.Subtract);
            AddBuiltin("*", Builtins.Multiply);
            AddBuiltin("/", Builtins.Divide);
            AddBuiltin("%", Builtins.Modulus);
            AddBuiltin("^", Builtins.Power);

            // Ordinals / Comparisons
            AddBuiltin(">", Builtins.GreaterThan);
            AddBuiltin("<", Builtins.LessThan);
            AddBuiltin(">=", Builtins.GreaterThanOrEqual);
            AddBuiltin("<=", Builtins.LessThanOrEqual);
            AddBuiltin("==", Builtins.Equals);
            AddBuiltin("!=", Builtins.NotEquals);
            AddBuiltin("if", Builtins.If);

            // Environment
            AddBuiltin("def", Builtins.Def);
            AddBuiltin("=", Builtins.Put);
            AddBuiltin("list-env", Builtins.ListEnv);

            AddBuiltin("load", Builtins.Load);
            AddBuiltin("print", Builtins.Print);
            AddBuiltin("show", Builtins.Show);
            AddBuiltin("error", Builtins.Error);

        }

        /// <summary>
        /// Copy this environment.
        /// </summary>
        internal Env Copy()
        {
            var res = new Env(false);
            foreach(var k in _envDict.Keys)
            {
                res.Put(k, _envDict[k]);
            }
            // Note, we do not copy the parent env if present.
            return res;
        }

        /// <summary>
        /// Get a copy of a value from the environment
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="k">K.</param>
        internal Value Get(string k)
        {
            if(_envDict.ContainsKey(k))
            {
                return _envDict[k].Copy(); 
            }

            if (Parent != null)
            {
                return Parent.Get(k);
            }
                
            return new ErrorValue($"Unbound symbol '{k}'.");
        }

        /// <summary>
        /// Places a copy of the supplied JLispValue in to the environment
        /// </summary>
        /// <param name="k">K.</param>
        /// <param name="v">V.</param>
        internal void Put(string k, Value v)
        {
            _envDict[k] = v.Copy();
        }

        /// <summary>
        /// Places a copy of the supplied JLispValue in to the root environment
        /// </summary>
        /// <param name="k">K.</param>
        /// <param name="v">V.</param>
        internal void Def(string k, Value v)
        {
            // Find ultimate parent.
            var rootEnv = this;
            while(rootEnv.Parent != null)
            {
                rootEnv = rootEnv.Parent;
            }

            rootEnv.Put(k, v);
        }

        /// <summary>
        /// Static instance of our parser.
        /// </summary>
        static Parser _parser = new Parser
              (
                  new SimpleGrammar
                  (
                      ScannerMatchMode.LongestMatch,
                      new List<TerminalSymbol>
                      {
                                    // TODO - Support decimals as well?
                                    new TerminalSymbol("WHITESPACE", UsefulRegex.WhiteSpace, true),
                                    new TerminalSymbol("NUMBER", UsefulRegex.Number),
                                    new TerminalSymbol("SYMBOL", UsefulRegex.Symbol),
                                    new TerminalSymbol("STRING", UsefulRegex.String),
                                    new TerminalSymbol("COMMENT", UsefulRegex.SemiColonComment),
                                    new TerminalSymbol("OPENBRACE",new Regex("{")),
                                    new TerminalSymbol("CLOSEBRACE",new Regex("}")),
                                    new TerminalSymbol("OPENPARENS",new Regex("[(]")),
                                    new TerminalSymbol("CLOSEPARENS",new Regex("[)]")),
                      },
                      new List<ProductionRule>
                      {
                                    new ProductionRule("JLisp").Produces("ExprTail"), // Want zero or more expr
                                    new ProductionRule("Expr").Produces("NUMBER").Or("SYMBOL").Or("STRING").Or("COMMENT").Or("SExpr").Or("QExpr"),
                                    // We mark such rules as "intermediate" so they don't contribute to the AST (actually they are removed)
                                    new ProductionRule("ExprTail", true).Produces("Expr", "ExprTail").Or(),
                                    new ProductionRule("SExpr").Produces("OPENPARENS", "ExprTail", "CLOSEPARENS"),
                                    new ProductionRule("QExpr").Produces("OPENBRACE", "ExprTail", "CLOSEBRACE")
                      }
                  )
              );

        /// <summary>
        /// Processes a JLisp value directly.
        /// </summary>
        public Value Evaluate(Value value)
        {
            return Value.Eval(this, value);
        }

        /// <summary>
        /// Process an ast which could lead to zero or many Value Trees..
        /// </summary>
        public IEnumerable<Value> Evaluate(AbstractSyntaxNode asTree)
        {
            if (IsMultiExpression(asTree))
            {
                foreach (var subTree in asTree.As<AbstractSyntaxRuleNode>().Children)
                {
                    var valueTree = Value.FromASTNode(subTree);
                    yield return Evaluate(valueTree);
                }
            }
            else
            {
                var valueTree = Value.FromASTNode(asTree);
                yield return Evaluate(valueTree);
            }
            yield break;
        }

        /// <summary>
        /// Processes a string.
        /// </summary>
        public IEnumerable<Value> Evaluate(string input)
        {
            AbstractSyntaxNode tree = null;
            ArgumentException argumentEx = null;
            try
            {
                tree = _parser.Parse(input);
                Debug.Assert(tree != null, "null parse tree with no exception.");
            }
            catch (ArgumentException e)
            {
                argumentEx = e;
            }
            if (argumentEx != null)
            {
                yield return (new ErrorValue("Unable to parse: " + argumentEx.Message));
            }
            else
            {
                foreach (Value v in Evaluate(tree))
                {
                    yield return v;
                }
            }
            yield break;
        }

        /// <summary>
        /// Processes a file.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="fileName">File name.</param>
        public Value EvaluateFile(string fileName)
        {
            var sexpr = new SExprValue();
            sexpr.Cell.Add(new BuiltinFuncValue(Builtins.Load));
            sexpr.Cell.Add(StringValue.ReadEscapedString(fileName));
            return Evaluate(sexpr);
        }

        /// <summary>
        /// Is the tree a single expression, or multiple expressions to be evaluated in turn.
        /// </summary>
        static bool IsMultiExpression(AbstractSyntaxNode node)
        {
            // if this is a Root Node "JLisp"
            // And each child node is an Expr by virtue of being an SExpr then we consider this to be multiple expression
            // This is typically of use when dealing with files.
            // Each expression should be fully formed i.e "(+ 1 2 3)" not "+ 1 2 3"
            if (node is AbstractSyntaxRuleNode &&
                node.As<AbstractSyntaxRuleNode>().ProductionRule.Name.Equals("JLisp") &&
                node.As<AbstractSyntaxRuleNode>().Children.TrueForAll(
                    c =>

                        c is AbstractSyntaxRuleNode &&
                        c.As<AbstractSyntaxRuleNode>().ProductionRule.Name.Equals("Expr") &&
                        (c.As<AbstractSyntaxRuleNode>().RulePartIndex == 4 || // S-Expr
                         c.As<AbstractSyntaxRuleNode>().RulePartIndex == 3) // Comment
                   )
               )
            {
                return true;
            }

            return false;
        }
    }
}
