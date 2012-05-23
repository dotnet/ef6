namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to set a default maximum length of 4000 for properties whose type supports length facets when SqlCe is the provider.
    /// </summary>
    public sealed class SqlCePropertyMaxLengthConvention : IEdmConvention<EdmEntityType>, IEdmConvention<EdmComplexType>
    {
        private const int DefaultLength = 4000;

        internal SqlCePropertyMaxLengthConvention()
        {
        }

        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(entityType.DeclaredProperties);
            }
        }

        void IEdmConvention<EdmComplexType>.Apply(EdmComplexType complexType, EdmModel model)
        {
            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(complexType.DeclaredProperties);
            }
        }

        private static void SetLength(IEnumerable<EdmProperty> properties)
        {
            foreach (var property in properties)
            {
                if (!property.PropertyType.IsPrimitiveType)
                {
                    continue;
                }

                if ((property.PropertyType.PrimitiveType == EdmPrimitiveType.String)
                    || (property.PropertyType.PrimitiveType == EdmPrimitiveType.Binary))
                {
                    SetDefaults(property);
                }
            }
        }

        private static void SetDefaults(EdmProperty property)
        {
            Contract.Requires(property != null);

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            if ((primitiveTypeFacets.MaxLength == null)
                && (primitiveTypeFacets.IsMaxLength == null))
            {
                primitiveTypeFacets.MaxLength = DefaultLength;
            }
        }
    }
}
