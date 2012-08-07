// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DynamicEqualityComparerLinqIntegration
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> func)
            where T : class
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);

            return source.Distinct(new DynamicEqualityComparer<T>(func));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<IGrouping<TSource, TSource>> GroupBy<TSource>(
            this IEnumerable<TSource> source, Func<TSource, TSource, bool> func)
            where TSource : class
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);

            return source.GroupBy(t => t, new DynamicEqualityComparer<TSource>(func));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<T> Intersect<T>(
            this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> func)
            where T : class
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(func != null);

            return first.Intersect(second, new DynamicEqualityComparer<T>(func));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> func)
            where T : class
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(func != null);

            return first.Except(second, new DynamicEqualityComparer<T>(func));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
