using System;

namespace QDP
{
    public static class Validation
    {
        public static void Requires<E>(Func<bool> valid, string errorMessage) where E : Exception
        {
            if (!valid())
            {
                throw Activator.CreateInstance(typeof(E), errorMessage) as E;
            }
        }
    }
}
