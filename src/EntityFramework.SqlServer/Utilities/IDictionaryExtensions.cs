// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections.Generic;

    internal static class IDictionaryExtensions
    {
        internal static void Add<TKey, TValue>(this IDictionary<TKey, IList<TValue>> map, TKey key, TValue value)
        {
            DebugCheck.NotNull(map);
            DebugCheck.NotNull((object)key);

            IList<TValue> valueList;
            if (!map.TryGetValue(key, out valueList))
            {
                valueList = new List<TValue>();
                map[key] = valueList;
            }
            valueList.Add(value);
        }
    }
}
