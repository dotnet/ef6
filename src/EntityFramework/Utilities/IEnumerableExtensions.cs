// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerStepThrough]
    internal static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> ts, Action<T, int> action)
        {
            DebugCheck.NotNull(ts);
            DebugCheck.NotNull(action);

            var i = 0;
            foreach (var t in ts)
            {
                action(t, i++);
            }
        }

        public static void Each<T>(this IEnumerable<T> ts, Action<T> action)
        {
            DebugCheck.NotNull(ts);
            DebugCheck.NotNull(action);

            foreach (var t in ts)
            {
                action(t);
            }
        }

        public static void Each<T, S>(this IEnumerable<T> ts, Func<T, S> action)
        {
            DebugCheck.NotNull(ts);
            DebugCheck.NotNull(action);

            foreach (var t in ts)
            {
                action(t);
            }
        }

        public static string Join<T>(this IEnumerable<T> ts, Func<T, string> selector = null, string separator = ", ")
        {
            DebugCheck.NotNull(ts);

            selector = selector ?? (t => t.ToString());

            return string.Join(separator, ts.Where(t => !ReferenceEquals(t, null)).Select(selector));
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            DebugCheck.NotNull(source);

            yield return value;

            foreach (var element in source)
            {
                yield return element;
            }
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            DebugCheck.NotNull(source);

            foreach (var element in source)
            {
                yield return element;
            }

            yield return value;
        }
    }
}
