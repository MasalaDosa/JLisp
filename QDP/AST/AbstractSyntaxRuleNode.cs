using System;
using System.Diagnostics;
using System.Collections.Generic;
using QDP.Grammar;

namespace QDP.AST
{
    /// <summary>
    /// A rule (non terminal) node in our abstract syntax tree.
    /// </summary>
    public class AbstractSyntaxRuleNode : AbstractSyntaxNode
    {
        /// <summary>
        /// The Production Rule that was evaluated.
        /// </summary>
        /// <value>The production rule.</value>
        public ProductionRule ProductionRule { get; private set; }

        /// <summary>
        /// Index of the part of the rule that successfully evaluated
        /// e.g FooBar.Produces("Bar", "Foo").Or("Foo", "Bar") could either match on index 0:
        /// "Bar", "Foo"
        /// or on index 1:
        /// "Foo", "Bar"
        /// </summary>
        /// <value>The index of the rule part.</value>
        public int RulePartIndex { get; private set; }

        public override string ToString()
        {
            return ProductionRule.ToString() +
                                 (ProductionRule.Rhs.Count > 1 ?
                                  $"( {ProductionRule.Rhs[RulePartIndex].ToString()} )" : string.Empty) +
                                  $" {Position}" +
                                  $" :{Content}";
        }

        internal AbstractSyntaxRuleNode(ProductionRule productionRule, int rulePartIndex, Position position, string content) : base(position, content)
        {
            Debug.Assert(productionRule != null, "Production Rule is required.");
            if (rulePartIndex >= productionRule.Rhs.Count)
            {
                throw new ArgumentException("Invalid rule part index.", nameof(rulePartIndex));
            }
            ProductionRule = productionRule;
            RulePartIndex = rulePartIndex;
        }
    }
}
