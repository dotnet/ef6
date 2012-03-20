namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(KeyDiscoveryConventionContracts))]
    internal abstract class KeyDiscoveryConvention : IEdmConvention<EdmEntityType>
    {
        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            if ((entityType.DeclaredKeyProperties.Count > 0)
                || (entityType.BaseType != null))
            {
                return;
            }

            var keyProperty = MatchKeyProperty(entityType, entityType.GetDeclaredPrimitiveProperties());

            if (keyProperty != null)
            {
                keyProperty.PropertyType.IsNullable = false;
                entityType.DeclaredKeyProperties.Add(keyProperty);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        protected abstract EdmProperty MatchKeyProperty(EdmEntityType entityType, IEnumerable<EdmProperty> primitiveProperties);

        #region Base Member Contracts

        [ContractClassFor(typeof(KeyDiscoveryConvention))]
        private abstract class KeyDiscoveryConventionContracts : KeyDiscoveryConvention
        {
            protected override EdmProperty MatchKeyProperty(EdmEntityType entityType, IEnumerable<EdmProperty> primitiveProperties)
            {
                Contract.Requires(entityType != null);
                Contract.Requires(primitiveProperties != null);

                return null;
            }
        }

        #endregion
    }
}