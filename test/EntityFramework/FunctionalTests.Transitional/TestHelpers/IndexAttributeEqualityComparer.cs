// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class IndexAttributeEqualityComparer : IEqualityComparer<IndexAttribute>
    {
        public bool Equals(IndexAttribute x, IndexAttribute y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal)
                   && x.Order == y.Order
                   && Equals(x.ClusteredConfiguration, y.ClusteredConfiguration)
                   && Equals(x.UniqueConfiguration, y.UniqueConfiguration);
        }

        public int GetHashCode(IndexAttribute obj)
        {
            unchecked
            {
                var hashCode = obj.Name != null ? obj.Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ obj.Order;
                hashCode = (hashCode * 397) ^ obj.ClusteredConfiguration.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UniqueConfiguration.GetHashCode();
                return hashCode;
            }
        }
    }
}
