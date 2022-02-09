using System.Collections.Generic;

namespace System.Data.Entity
{
    public static class StringSplitContainsStringExtensions
    {
        public static bool InStringSplitCommaSeparated<T>(this string str, T value)
            where T : struct
            => throw new Exception($"{nameof(InStringSplitCommaSeparated)} should not be called directly - only referred to in an expression.");
    }
}
