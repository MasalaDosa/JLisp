using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QDP.AST;
using QDP.Grammar;

namespace QDP
{
    public class Parser
    {
        // Verbose mode
        bool _verbose;

        // The grammar we are parsing.
        SimpleGrammar _grammar;

        // The source code
        string _source;

        // The tokens we have scanned
        List<Token> _tokens;

        // Stack indicating which rules we are currently evaluating.
        Stack<string> _rulesStack;

        // A list of errors - includes the error message and a string list representing the rulesStack at the point of error
        List<ParserError> _errors;

        // A stack used to build up the tree.
        Stack<AbstractSyntaxNode> _treeNodeStack;

        public Parser(SimpleGrammar grammar, bool verbose = false)
        {
            _verbose = verbose;
            _grammar = grammar;
        }

        /// <summary>
        /// Uses the supplied grammar to parse the source.
        /// Throws ArguymentException if not successful
        /// </summary>
        /// <returns>An abstract syntax tree.</returns>
        /// <param name="source">Source.</param>
        public AbstractSyntaxNode Parse(string source)
        {
            _source = source;
            _errors = new List<ParserError>();

            _tokens = new Scanner(_grammar.ScannerMatchMode, _grammar.TerminalSymbols).Scan(source).ToList();

            _treeNodeStack = new Stack<AbstractSyntaxNode>();
            _rulesStack = new Stack<string>();

            var rule = _grammar.ProductionRules[0];
            int tokenIndex = 0;
            var success = ApplyRule(rule, ref tokenIndex); 
            if(success && tokenIndex != _tokens.Count)
            {
                //If the rules parsed but we didn't consume all the tokens then this must count as a failure,
                _errors.Add(new ParserError("Not all tokens were parsed.", tokenIndex, new List<string>()));
               success = false;
            }
            if(success)
            {
                var tree = BuildTree();
                PostProcessTree(tree);
                return tree;
            }
            else
            {
                string message = Errors.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Unknown parse error.";
                }
                else if(Errors.Count() > 1)
                {
                    message += Environment.NewLine + "(The most likely error is described.  See the Errors property for further possibilities.)";
                }
                throw new ArgumentException(message, nameof(source));
            }
        }

        /// <summary>
        /// Applies the suppled production rule starting at token index
        /// This will recursively call further production rules
        /// </summary>
        /// <returns><c>true</c>, if rule was applied, <c>false</c> otherwise.</returns>
        /// <param name="r">The red component.</param>
        /// <param name="tokenIndex">Token index.</param>
        /// <param name="indent">Indent.</param>
        bool ApplyRule(ProductionRule r, ref int tokenIndex, int indent = 0)
        {
            Log("Applying " + r.ToString() + " to " + string.Join(", ", _tokens.GetRange(tokenIndex, _tokens.Count - tokenIndex).Select(t => $"{t.TokenType.Name} ({t.Value})")), indent);
            _rulesStack.Push(r.Name);

            var incomingTokenIndex = tokenIndex;

            // In order to pass this rule one of the RuleParts must evaluate
            bool evaluatedRuleSuccessfully = false;
            ProductionRulePart successfullyEvaluatedRulePart = null;

            for (int i = 0; i < r.Rhs.Count; i++)
            {
                // Take a copy of tokens.
                if (ApplyRulePart(r.Rhs[i], ref tokenIndex, indent))
                {
                    // We have passed - so can break out.
                    // Otherwise, try the next rule part
                    evaluatedRuleSuccessfully = true;
                    successfullyEvaluatedRulePart = r.Rhs[i];
                    break;
                }
            }
            if (evaluatedRuleSuccessfully)
            {
                var nodePosition = GetRuleNodePosition(tokenIndex, incomingTokenIndex);
                _treeNodeStack.Push(
                    new AbstractSyntaxRuleNode(
                        r,
                        r.Rhs.IndexOf(successfullyEvaluatedRulePart),
                        nodePosition,
                        _source.Substring(nodePosition.Index, nodePosition.Length)
                    )
                );
            }
            else
            {
                // restore the index - we need to try a different rule
                tokenIndex = incomingTokenIndex;
            }
            _rulesStack.Pop();
            return evaluatedRuleSuccessfully;
        }

        /// <summary>
        /// Applies the rule part starting at the given token index
        /// </summary>
        /// <returns><c>true</c>, if rule part was applyed, <c>false</c> otherwise.</returns>
        /// <param name="rulePart">Rule part.</param>
        /// <param name="tokenIndex">Token index.</param>
        /// <param name="indent">Indent.</param>
        bool ApplyRulePart(ProductionRulePart rulePart, ref int tokenIndex, int indent)
        {
            Log("Testing " + rulePart.ToString() + " " + string.Join(", ", _tokens.GetRange(tokenIndex, _tokens.Count - tokenIndex).Select(t => $"{t.TokenType.Name} ({t.Value})")), indent);

            var incomingTokenIndex = tokenIndex;

            foreach (var target in rulePart)
            {
                // Get the current token at the start of each loop - index may have changed.
                var currentToken = GetToken(tokenIndex);

                if (IsTerminal(target))
                {
                    var symbolParts = target.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string terminalName = symbolParts.First();
                    string value = symbolParts.Length != 2 ?
                                    string.Empty :
                                    symbolParts.Last().Trim('\'');
                    if (currentToken != null && currentToken.TokenType.Name == terminalName)
                    {
                        Log($"- Matched  {target} to Token {currentToken.TokenType.Name} {currentToken.Value}", indent);

                        // TODO - These could be orphaned if this target succeeds but others fail
                        // Not a problem but consider tidying up?
                        _treeNodeStack.Push(
                            new AbstractSyntaxTerminalNode(
                                currentToken.TokenType,
                                currentToken.Value,
                                currentToken.Position,
                                _source.Substring(currentToken.Position.Index, currentToken.Position.Length)
                            )
                        );
                        tokenIndex++;
                    }
                    else if (currentToken != null)
                    {
                        Log($"- Failed Match Expected {target} Token {GetToken(tokenIndex).TokenType.Name} {GetToken(tokenIndex).Value}", indent);
                        // Since this was the wrong terminal then the RulePart has failed.
                        var rulesList = _rulesStack.ToList();
                        rulesList.Reverse();
                        _errors.Add(new ParserError($"Expected {target}.  Encountered {GetToken(tokenIndex).TokenType.Name} {GetToken(tokenIndex).Value} at {GetToken(tokenIndex).Position}", tokenIndex, rulesList));
                        tokenIndex = incomingTokenIndex;
                        return false;
                    }
                    else
                    {
                        Log($"- Failed Match Expected {target} Found nothing", indent);
                        // Since this was the wrong terminal then the RulePart has failed.
                        var rulesList = _rulesStack.ToList();
                        rulesList.Reverse();
                        _errors.Add(new ParserError($"Expected {target}.  Encountered EOF", tokenIndex, rulesList));
                        tokenIndex = incomingTokenIndex;
                        return false;
                    }
                }
                else
                {
                    var nextRule = _grammar.ProductionRules.Where(cr => cr.Name == target).First();

                    if (!ApplyRule(nextRule, ref tokenIndex, indent + 1))
                    {
                        // Hmm, this rule faile - reset token index
                        tokenIndex = incomingTokenIndex;
                        return false;
                    }
                    else
                    {
                        // This rule succeeded - but there may be more - we should use the current token index returned from this moving forwards
                    }
                }
            }
            // If we've got here then this rule passed
            return true;
        }

        /// <summary>
        /// Gets the current token
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="tokenIndex">Token index.</param>
        Token GetToken(int tokenIndex)
        {
            Debug.Assert(tokenIndex >= 0, "Negative token index.");
            return tokenIndex < _tokens.Count ? _tokens[tokenIndex] : null;
        }

        /// <summary>
        /// Does this target represent a terminal symbol
        /// </summary>
        /// <returns><c>true</c>, if terminal was ised, <c>false</c> otherwise.</returns>
        /// <param name="target">Target.</param>
        bool IsTerminal(string target)
        {
            var symbolPart = target.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string terminalName = symbolPart.First();
            return _grammar.TerminalSymbols.Any(ts => ts.Name == terminalName);
        }

        /// <summary>
        /// Builds the tree from the stack we have generated.
        /// There may be orphans left on the stack once this has 
        /// completed due to partial successes on different rules / parts.
        /// These can be ignored since we build the tree from the root downwards.
        /// </summary>
        /// <returns>The tree.</returns>
        AbstractSyntaxNode BuildTree()
        {
            var current = _treeNodeStack.Pop();
            if (current is AbstractSyntaxRuleNode)
            {
                var asrn = (current as AbstractSyntaxRuleNode);
                var childrenCount = asrn.ProductionRule.Rhs[asrn.RulePartIndex].Count;
                if (childrenCount > 0)
                {
                    current.Children = new List<AbstractSyntaxNode>();
                }
                for (int i = 0; i < childrenCount; i++)
                {
                    current.Children.Insert(0, BuildTree());
                }
            }
            return current;
        }

        /// <summary>
        /// Removes intermediate AST nodes from the tree
        /// </summary>
        /// <param name="root">Root.</param>
        void PostProcessTree(AbstractSyntaxNode root)
        {
            // Assumes top level rule is not intermediate
            // Are any of my children intermediate
            if (root.Children != null)
            {
                foreach (var c in root.Children)
                {
                    PostProcessTree(c);
                }

                var intermediates = root.Children.Where(c => c is AbstractSyntaxRuleNode && ((AbstractSyntaxRuleNode)c).ProductionRule.IsIntermediate); //
                foreach (var c in intermediates.ToList())
                {
                    var index = root.Children.IndexOf(c);
                    // Remove it
                    root.Children.Remove(c);
                    // And adopt
                    if (c.Children != null)
                    {
                        root.Children.InsertRange(index, c.Children);
                    }
                }
            }
        }

        /// <summary>
        /// Determine position data for an AbstractSyntaxRuleNode
        /// </summary>
        /// <returns>The rule node position.</returns>
        /// <param name="tokenIndex">Token index.</param>
        /// <param name="incomingTokenIndex">Incoming token index.</param>
        Position GetRuleNodePosition(int tokenIndex, int incomingTokenIndex)
        {
            Position startPosition, nodePosition;
            int length;

            var startToken = GetToken(incomingTokenIndex);
            if (startToken != null)
            {
                startPosition = startToken.Position;
                var currentToken = GetToken(tokenIndex);
                length = currentToken != null ?
                    currentToken.Position.Index - startPosition.Index :
                                _source.Length - startPosition.Index;
            }
            else if (_tokens.Any())
            {
                
                startPosition = new Position(
                    _tokens.Last().Position.Index,
                    _tokens.Last().Position.Row,
                    _tokens.Last().Position.Column,
                    _tokens.Last().Position.Length);
                length = 0;
            }
            else
            {
                startPosition = new Position(
                    0,
                    0,
                    0,
                    0);
                length = 0;
            }
            nodePosition = new Position(
                        startPosition.Index,
                        startPosition.Row,
                        startPosition.Column,
                        length
            );
            return nodePosition;
        }

        /// <summary>
        /// Gets the errors ranked by number of tokens parsed at the point of error.
        /// </summary>
        /// <value>The errors.</value>
        public IEnumerable<string> Errors
        {
            get
            {
                if (_errors == null)
                {
                    return null;
                }
                return _errors.OrderBy(e => e.NumberOfTokensParsed).Reverse().Select(e => e.Message + " " + (string.Join(" => ", e.Rules)));
            }
        }

        /// <summary>
        /// Log an indented message to the console.
        /// </summary>
        /// <param name="m">M.</param>
        /// <param name="i">The index.</param>
        void Log(string m, int i)
        {
            if (_verbose)
            {
                Console.WriteLine(new string(' ', i) + m);
            }
        }
    }
}
