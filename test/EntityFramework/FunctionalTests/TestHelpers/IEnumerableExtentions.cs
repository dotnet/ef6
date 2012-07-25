// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class IEnumerableExtentions
    {
        #region ToLists

        public static List<T> ToList<T>(this IEnumerable source)
        {
            var list = new List<T>();
            var enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    list.Add((T)enumerator.Current);
                }
            }
            finally
            {
                var asDisposable = enumerator as IDisposable;
                if (asDisposable != null)
                {
                    asDisposable.Dispose();
                }
            }
            return list;
        }

        #endregion
    }
}
