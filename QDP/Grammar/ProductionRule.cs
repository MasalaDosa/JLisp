using System;

namespace QDP.Grammar
{
    /// <summary>
    /// A Prodcution rule in our grammar.
    /// </summary>
    public class ProductionRule
    {
        public ProductionRule(string name, bool intermediate = false)
        {
            Validation.Requires<ArgumentNullException>(() => name != null, $"{nameof(name)} is required.");
            Validation.Requires<ArgumentException>(() => name != null, $"{nameof(name)} cannot be empty.");
            IsIntermediate = intermediate;
            Name = name;
        }
        public string Name { get; private set; }
        public ProductionRuleGroup Rhs { get; private set; }
        public bool IsIntermediate { get; private set; }

        public ProductionRule Produces(params string[] rhs)
        {
            Rhs = new ProductionRuleGroup();
            return Or(rhs);
        }

        public ProductionRule Or(params string[] rhs)
        {
            if (Rhs == null)
            {
                throw new InvalidOperationException("Call 'Produces' before 'Or'.");
            }
            Rhs.Add(new ProductionRulePart());
            {
                foreach (var s in rhs)
                {
                    Rhs[Rhs.Count - 1].Add(s);
                }
            }
            return this;
        }
        public override string ToString()
        {
            return Name + " => " + Rhs;
        }
    }
}
