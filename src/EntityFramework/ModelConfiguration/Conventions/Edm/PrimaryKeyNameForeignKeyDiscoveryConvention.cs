// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Convention to discover foreign key properties whose names match the principal type primary key property name(s).
    /// </summary>
    public class PrimaryKeyNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
    {
        protected override bool MatchDependentKeyProperty(
            AssociationType associationType,
            AssociationEndMember dependentAssociationEnd,
            EdmProperty dependentProperty,
            EntityType principalEntityType,
            EdmProperty principalKeyProperty)
        {
            return string.Equals(
                dependentProperty.Name, principalKeyProperty.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
