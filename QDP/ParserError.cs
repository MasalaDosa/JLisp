using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QDP
{
    internal class ParserError
    {
        public string Message { get; private set; }
        public int NumberOfTokensParsed { get; private set; }
        public List<string> Rules { get; private set; }

        internal ParserError(string message, int numberOfTokensParsed, List<string> rules)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(message), "null or empty message.");
            Debug.Assert(rules != null, "null rules");

            Message = message;
            NumberOfTokensParsed = numberOfTokensParsed;
            Rules = rules;
        }
    }
}
