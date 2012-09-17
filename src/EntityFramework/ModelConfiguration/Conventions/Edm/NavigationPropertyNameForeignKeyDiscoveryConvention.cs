// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;

    /// <summary>
    ///     Convention to discover foreign key properties whose names are a combination
    ///     of the dependent navigation property name and the principal type primary key property name(s).
    /// </summary>
    public class NavigationPropertyNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
    {
        protected override bool MatchDependentKeyProperty(
            EdmAssociationType associationType,
            EdmAssociationEnd dependentAssociationEnd,
            EdmProperty dependentProperty,
            EdmEntityType principalEntityType,
            EdmProperty principalKeyProperty)
        {
            var otherEnd = associationType.GetOtherEnd(dependentAssociationEnd);

            var navigationProperty
                = dependentAssociationEnd.EntityType.NavigationProperties
                    .SingleOrDefault(n => n.ResultEnd == otherEnd);

            if (navigationProperty == null)
            {
                return false;
            }

            return string.Equals(
                dependentProperty.Name, navigationProperty.Name + principalKeyProperty.Name,
                StringComparison.OrdinalIgnoreCase);
        }

        protected override bool SupportsMultipleAssociations
        {
            get { return true; }
        }
    }
}
