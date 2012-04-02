namespace System.Data.Entity.Migrations.Extensions
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    [DebuggerStepThrough]
    internal static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> ts, Action<T, int> action)
        {
            Contract.Requires(ts != null);
            Contract.Requires(action != null);

            var i = 0;
            foreach (var t in ts)
            {
                action(t, i++);
            }
        }

        public static void Each<T>(this IEnumerable<T> ts, Action<T> action)
        {
            Contract.Requires(ts != null);
            Contract.Requires(action != null);

            foreach (var t in ts)
            {
                action(t);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void Each<T, S>(this IEnumerable<T> ts, Func<T, S> action)
        {
            Contract.Requires(ts != null);
            Contract.Requires(action != null);

            foreach (var t in ts)
            {
                action(t);
            }
        }

        public static string Join<T>(this IEnumerable<T> ts, Func<T, string> selector = null, string separator = ", ")
        {
            Contract.Requires(ts != null);

            selector = selector ?? (t => t.ToString());

            return string.Join(separator, ts.Select(selector));
        }
    }
}
