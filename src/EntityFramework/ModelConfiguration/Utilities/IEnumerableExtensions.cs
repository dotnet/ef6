namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

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

        public static void Each<T, S>(this IEnumerable<T> ts, Func<T, S> action)
        {
            Contract.Requires(ts != null);
            Contract.Requires(action != null);

            foreach (var t in ts)
            {
                action(t);
            }
        }
    }
}
