// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils
{
    using System.Collections.Generic;

    // <summary>
    // Comparer that treats two strings as equivalent if they differ only by trailing
    // spaces, e.g. 'A' eq 'A   '. Useful when determining if a set of values is unique
    // even given the possibility of padding (consider SQL Server char and nchar columns)
    // or to lookup values when the set of values is known to honor this uniqueness constraint.
    // </summary>
    internal class TrailingSpaceComparer : IEqualityComparer<object>
    {
        private TrailingSpaceComparer()
        {
        }

        internal static readonly TrailingSpaceComparer Instance = new TrailingSpaceComparer();
        private static readonly IEqualityComparer<object> _template = EqualityComparer<object>.Default;

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            var xAsString = x as string;
            if (null != xAsString)
            {
                var yAsString = y as string;
                if (null != yAsString)
                {
                    return TrailingSpaceStringComparer.Instance.Equals(xAsString, yAsString);
                }
            }
            return _template.Equals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            var value = obj as string;
            if (null != value)
            {
                return TrailingSpaceStringComparer.Instance.GetHashCode(value);
            }
            return _template.GetHashCode(obj);
        }
    }
}
