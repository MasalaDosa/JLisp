using System.Collections.Generic;

namespace QDP.Grammar
{
    /// <summary>
    /// A Production Rule Group in our Grammar.
    /// Each production rule has a group.
    /// A group consists of 1 or more parts.
    /// In order for a rule to parse, one of its parts must parse.
    /// </summary>
    public class ProductionRuleGroup : List<ProductionRulePart>
    {
        public override string ToString()
        {
            return string.Join("|", this);
        }
    }
}
