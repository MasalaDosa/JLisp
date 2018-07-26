using System.Collections.Generic;

namespace QDP.Grammar
{
    /// <summary>
    /// Production rule part.
    /// </summary>
    public class ProductionRulePart : List<string>
    {
        public override string ToString()
        {
            return Count > 0 ? string.Join(" ", this) : "<nil>";
        }
    }
}
