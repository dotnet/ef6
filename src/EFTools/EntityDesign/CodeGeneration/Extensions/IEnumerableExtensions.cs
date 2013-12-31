// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal static class IEnumerableExtensions
    {
        public static bool MoreThan<TSource>(this IEnumerable<TSource> source, int count)
        {
            Debug.Assert(source != null, "source is null.");

            var genericCollection = source as ICollection<TSource>;
            if (genericCollection != null)
            {
                return genericCollection.Count > count;
            }

            var collection = source as ICollection;
            if (collection != null)
            {
                return collection.Count > count;
            }

            using (var enumerator = source.GetEnumerator())
            {
                var elementCount = 0;
                while (enumerator.MoveNext())
                {
                    elementCount++;

                    if (elementCount > count)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
