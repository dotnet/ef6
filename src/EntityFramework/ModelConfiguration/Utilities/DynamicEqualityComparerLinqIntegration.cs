namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DynamicEqualityComparerLinqIntegration
    {
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> func)
            where T : class
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);

            return source.Distinct(new DynamicEqualityComparer<T>(func));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static IEnumerable<T> Intersect<T>(
            this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> func)
            where T : class
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(func != null);

            return first.Intersect(second, new DynamicEqualityComparer<T>(func));
        }

        public static IEnumerable<IGrouping<TSource, TSource>> GroupBy<TSource>(
            this IEnumerable<TSource> source, Func<TSource, TSource, bool> func)
            where TSource : class
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);

            return source.GroupBy(t => t, new DynamicEqualityComparer<TSource>(func));
        }

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> source, IEnumerable<TSource> other, Func<TSource, TSource, bool> func)
            where TSource : class
        {
            Contract.Requires(source != null);
            Contract.Requires(other != null);
            Contract.Requires(func != null);

            return source.SequenceEqual(other, new DynamicEqualityComparer<TSource>(func));
        }
    }
}
