// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;

    /// <summary>
    ///     Convention to discover foreign key properties whose names match the principal type primary key property name(s).
    /// </summary>
    public sealed class PrimaryKeyNameForeignKeyDiscoveryConvention : IEdmConvention<EdmAssociationType>
    {
        private readonly IEdmConvention<EdmAssociationType> _impl =
            new PrimaryKeyNameForeignKeyDiscoveryConventionImpl();

        internal PrimaryKeyNameForeignKeyDiscoveryConvention()
        {
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            _impl.Apply(associationType, model);
        }

        // Nested impl. because ForeignKeyDiscoveryConvention needs to be internal for now
        private sealed class PrimaryKeyNameForeignKeyDiscoveryConventionImpl : ForeignKeyDiscoveryConvention
        {
            protected override bool MatchDependentKeyProperty(
                EdmAssociationType associationType,
                EdmAssociationEnd dependentAssociationEnd,
                EdmProperty dependentProperty,
                EdmEntityType principalEntityType,
                EdmProperty principalKeyProperty)
            {
                return string.Equals(
                    dependentProperty.Name, principalKeyProperty.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
