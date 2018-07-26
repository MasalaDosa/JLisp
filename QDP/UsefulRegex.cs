using System;
using System.Text.RegularExpressions;

namespace QDP
{
    /// <summary>
    /// Various useful regex
    /// </summary>
    public static class UsefulRegex
    {
        public static readonly Regex WhiteSpace = new Regex( @"\s+");
        public static readonly Regex Symbol = new Regex(@"[a-zA-Z0-9_+\-*\/\\=<>!&%^]+");
        public static readonly Regex String = new Regex("\"(\\\\.|[^\"])*\"");
        public static readonly Regex Number = new Regex(@"[0-9]+");
        public static readonly Regex PlusMinus = new Regex(@"\+|-");
        public static readonly Regex MultDiv = new Regex(@"/|\*");
        public static readonly Regex SemiColonComment = new Regex(";[^\\r\\n]*");
        public static readonly Regex NewLine = new Regex("[\r\n|\r|\n]");
    }
}
