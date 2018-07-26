using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using QDP.Grammar;

namespace QDP
{
    /// <summary>
    /// Scan the supplied expression.
    /// </summary>
    internal class Scanner
    {
        readonly ScannerMatchMode _scannerMatchMode;
        readonly List<TerminalSymbol> _terminals;

        internal Scanner(ScannerMatchMode scannerMatchMode, List<TerminalSymbol> terminals)
        {
            Debug.Assert(terminals != null && terminals.Any(), "At least one terminal symbol is required.");
            _scannerMatchMode = scannerMatchMode;
            _terminals = terminals;
        }

        /// <summary>
        ///  Scans the source
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Token> Scan(string source)
        {
            int currentIndex = 0;
            int currentRow = 0;
            int currentColumn = 0;

            while (currentIndex < source.Length)
            {
                TerminalSymbol matched = null;
                int matchedLength = -1;
                foreach (var terminal in _terminals)
                {
                    var match = terminal.Regex.Match(source, currentIndex);

                    if (match.Success && (match.Index - currentIndex) == 0)
                    {
                        if (match.Length > matchedLength)
                        {
                            matched = terminal;
                            matchedLength = match.Length;
                            if (_scannerMatchMode == ScannerMatchMode.FirstMatch)
                            {
                                break;
                            }
                        }
                    }
                }

                if (matched == null)
                {
                    throw new ArgumentException($"Unrecognized symbol '{source[currentIndex]}' at {new Position(currentIndex, currentRow, currentColumn, 1)}.");
                }
                else
                {
                    var value = source.Substring(currentIndex, matchedLength);

                    if (!matched.Ignore)
                    {
                        yield return new Token(matched, value, new Position(currentIndex, currentRow, currentColumn, matchedLength));
                    }

                    var isEndOfLine = UsefulRegex.NewLine.Match(value);
                    if(isEndOfLine.Success && isEndOfLine.Index  == 0)
                    {
                        currentRow += 1;
                        currentColumn = 0;
                    }
                    else
                    {
                        currentColumn += matchedLength;
                    }
                    currentIndex += matchedLength;
                }
            }
            yield break;
        }
    }
}
