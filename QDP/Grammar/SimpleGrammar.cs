using System;
using System.Collections.Generic;
using System.Linq;

namespace QDP.Grammar
{
    /// <summary>
    /// Simple grammar - one or more terminals and one or more rules.
    /// </summary>
    public class SimpleGrammar
    {
        public ScannerMatchMode ScannerMatchMode { get; private set; }
        public List<TerminalSymbol> TerminalSymbols { get; private set; }
        public List<ProductionRule> ProductionRules { get; private set; }

        public SimpleGrammar(ScannerMatchMode scannerMatchMode, List<TerminalSymbol> terminals, List<ProductionRule> rules)
        {
            Validation.Requires<ArgumentNullException>(() => terminals != null, $"{nameof(terminals)} is required.");
            Validation.Requires<ArgumentException>(() => terminals.Any(), $"{nameof(terminals)} cannot be empty.");

            Validation.Requires<ArgumentNullException>(() => rules != null, $"{nameof(rules)} is required.");
            Validation.Requires<ArgumentException>(() => rules.Any(), $"{nameof(rules)} cannot be empty.");

            ScannerMatchMode = scannerMatchMode;
            TerminalSymbols = terminals;
            ProductionRules = rules;

            string failReason;
            if(!CheckGrammar(out failReason))
            {
                throw new InvalidOperationException(failReason);
            }
        }

        bool CheckGrammar(out string reason)
        {
            reason = string.Empty;

            // First Rule cannot be intermediate.
            if(ProductionRules.First().IsIntermediate)
            {
                reason = "First rule cannot be marked as intermediate.";
                return false;
            }

            // All Rule.Name UNION Terminal.Name Must be unique
            var allNames = ProductionRules.Select(pr => pr.Name).Union(TerminalSymbols.Select(ts => ts.Name));
            var duplicateNames = allNames.GroupBy(s => s).Where(g => g.Count() > 1);
            if(duplicateNames.Any())
            {
                reason = "Duplicate names: " + string.Join(", ", duplicateNames.Select(duplicateName => duplicateName.Key));
                return false;
            }

            // Each Rule should have a Non Null "Group" as the RHS - ie Produces has been called
            var nullRhsProductionRuleNames = ProductionRules.Where(pr => pr.Rhs == null || pr.Rhs.Count == 0).Select(pr => pr.Name);
            if(nullRhsProductionRuleNames.Any())
            {
                reason = "Rules which don't produce anything: " + string.Join(", ", nullRhsProductionRuleNames);
                return false;
            }

            // A 'target' (i.e element of a RulePart) must exist in the list of Rule.Name UNION Terminal.Name
            var badTargetRuleNames = ProductionRules.Where(pr => pr.Rhs.Any(rp => rp.AsEnumerable().Any(target => !allNames.Contains(target.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).First())))).Select(r=> r.Name);;
            if(badTargetRuleNames.Any())
            {
                reason = "Rules which produce unknown targets: " + string.Join(", ", badTargetRuleNames);
                return false;
            }

            // A RuleOrTerminal should not have a RulePart where the first RuleOrTerminal is the rule.
            var directLeftRecursion = ProductionRules.Where(r => r.Rhs.AsEnumerable().Where(rp => rp.Count() > 0 && rp[0] == r.Name).Any());
            if (directLeftRecursion.Any())
            {
                reason = "Direct left recursion: " + string.Join(", ", directLeftRecursion);
                return false;
            }

            return true;
        }
    }
}
