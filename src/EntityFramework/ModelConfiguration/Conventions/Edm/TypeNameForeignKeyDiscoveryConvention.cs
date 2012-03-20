namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to discover foreign key properties whose names are a combination
    ///     of the principal type name and the principal type primary key property name(s).
    /// </summary>
    public sealed class TypeNameForeignKeyDiscoveryConvention : IEdmConvention<EdmAssociationType>
    {
        private readonly IEdmConvention<EdmAssociationType> _impl = new TypeNameForeignKeyDiscoveryConventionImpl();

        internal TypeNameForeignKeyDiscoveryConvention()
        {
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            _impl.Apply(associationType, model);
        }

        // Nested impl. because ForeignKeyDiscoveryConvention needs to be internal for now
        private sealed class TypeNameForeignKeyDiscoveryConventionImpl : ForeignKeyDiscoveryConvention
        {
            protected override bool MatchDependentKeyProperty(
                EdmAssociationType associationType,
                EdmAssociationEnd dependentAssociationEnd,
                EdmProperty dependentProperty,
                EdmEntityType principalEntityType,
                EdmProperty principalKeyProperty)
            {
                return string.Equals(
                    dependentProperty.Name, principalEntityType.Name + principalKeyProperty.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}