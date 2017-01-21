// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal static class IndexAttributeExtensions
    {
        internal static CompatibilityResult IsCompatibleWith(this IndexAttribute me, IndexAttribute other, bool ignoreOrder = false)
        {
            DebugCheck.NotNull(me);

            if (ReferenceEquals(me, other)
                || other == null)
            {
                return new CompatibilityResult(true, null);
            }

            string errorMessage = null;

            if (me.Name != other.Name)
            {
                errorMessage = Strings.ConflictingIndexAttributeProperty("Name", me.Name, other.Name);
            }

            if (!ignoreOrder
                && me.Order != -1
                && other.Order != -1
                && me.Order != other.Order)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("Order", me.Order, other.Order);
            }

            if (me.IsClusteredConfigured
                && other.IsClusteredConfigured
                && me.IsClustered != other.IsClustered)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("IsClustered", me.IsClustered, other.IsClustered);
            }

            if (me.IsUniqueConfigured
                && other.IsUniqueConfigured
                && me.IsUnique != other.IsUnique)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("IsUnique", me.IsUnique, other.IsUnique);
            }

            return new CompatibilityResult(errorMessage == null, errorMessage);
        }

        internal static IndexAttribute MergeWith(this IndexAttribute me, IndexAttribute other, bool ignoreOrder = false)
        {
            DebugCheck.NotNull(me);

            if (ReferenceEquals(me, other)
                || other == null)
            {
                return me;
            }

            var isCompatible = me.IsCompatibleWith(other, ignoreOrder);
            if (!isCompatible)
            {
                throw new InvalidOperationException(
                    Strings.ConflictingIndexAttribute(me.Name, Environment.NewLine + "\t" + isCompatible.ErrorMessage));
            }

            var merged = me.Name != null
                ? new IndexAttribute(me.Name)
                : other.Name != null ? new IndexAttribute(other.Name) : new IndexAttribute();

            if (!ignoreOrder)
            {
                if (me.Order != -1)
                {
                    merged.Order = me.Order;
                }
                else if (other.Order != -1)
                {
                    merged.Order = other.Order;
                }
            }

            if (me.IsClusteredConfigured)
            {
                merged.IsClustered = me.IsClustered;
            }
            else if (other.IsClusteredConfigured)
            {
                merged.IsClustered = other.IsClustered;
            }

            if (me.IsUniqueConfigured)
            {
                merged.IsUnique = me.IsUnique;
            }
            else if (other.IsUniqueConfigured)
            {
                merged.IsUnique = other.IsUnique;
            }

            return merged;
        }
    }
}
