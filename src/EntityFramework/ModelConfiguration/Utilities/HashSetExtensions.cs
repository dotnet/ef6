namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;

    internal static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var i in items)
            {
                set.Add(i);
            }
        }
    }
}
