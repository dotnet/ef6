// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Extensions
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerStepThrough]
    internal static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> ts, Action<T> action)
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
    }
}
