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
    public static class JLisp
    {
     
        /// <summary>
        /// Returns the specified o as T.
        /// </summary>
        internal static T As<T>(this object o) where T : class
        {
            return o as T;
        }
    }
}
