// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Convention to discover foreign key properties whose names are a combination
    ///     of the principal type name and the principal type primary key property name(s).
    /// </summary>
    public class TypeNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
    {
        protected override bool MatchDependentKeyProperty(
            AssociationType associationType,
            AssociationEndMember dependentAssociationEnd,
            EdmProperty dependentProperty,
            EntityType principalEntityType,
            EdmProperty principalKeyProperty)
        {
            return string.Equals(
                dependentProperty.Name, principalEntityType.Name + principalKeyProperty.Name,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
