using System;
namespace JLisp.Values
{
    /// <summary>
    /// Base class for built-in and JLisp functions.
    /// </summary>
    public abstract class FuncValue : Value
    {
        internal override string Type => TYPE;
        internal const string TYPE = "function";
    }
}
