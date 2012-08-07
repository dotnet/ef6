// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Collections.Generic;

    /// <summary>
    ///     Compares two BaseTypeForLinq objects.
    /// </summary>
    public class BaseTypeForLinqComparer : IEqualityComparer<BaseTypeForLinq>
    {
        public bool Equals(BaseTypeForLinq left, BaseTypeForLinq right)
        {
            return (left == null && right == null) ||
                   (left.GetType() == right.GetType() && left.EntityEquals(right));
        }

        public int GetHashCode(BaseTypeForLinq entity)
        {
            return entity.EntityHashCode;
        }
    }
}
