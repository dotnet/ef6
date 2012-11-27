// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     This class is used as a Comparer for Types all through the PlanCompiler.
    ///     It has a pretty strict definition of type equality - which pretty much devolves
    ///     to equality of the "Identity" of the Type (not the TypeUsage).
    ///     NOTE: Unlike other parts of the query pipeline, record types follow
    ///     a much stricter equality condition here - the field names must be the same, and
    ///     the field types must be equal.
    ///     NOTE: Primitive types are considered equal, if their Identities are equal. This doesn't
    ///     take into account any of the facets that are represented external to the type (size, for instance).
    ///     Again, this is different from other parts of  the query pipeline; and we're much stricter here
    /// </summary>
    internal sealed class TypeUsageEqualityComparer : IEqualityComparer<TypeUsage>
    {
        private TypeUsageEqualityComparer()
        {
        }

        internal static readonly TypeUsageEqualityComparer Instance = new TypeUsageEqualityComparer();

        #region IEqualityComparer<TypeUsage> Members

        public bool Equals(TypeUsage x, TypeUsage y)
        {
            if (x == null
                || y == null)
            {
                return false;
            }

            return Equals(x.EdmType, y.EdmType);
        }

        public int GetHashCode(TypeUsage obj)
        {
            return obj.EdmType.Identity.GetHashCode();
        }

        #endregion

        internal static bool Equals(EdmType x, EdmType y)
        {
            return x.Identity.Equals(y.Identity);
        }
    }
}
