// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    internal static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            Contract.Requires(set != null);
            Contract.Requires(items != null);

            foreach (var i in items)
            {
                set.Add(i);
            }
        }
    }
}
