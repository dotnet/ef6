// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class SqlCePropertyMaxLengthConvention : IEdmConvention<EdmEntityType>, IEdmConvention<EdmComplexType>
    {
        private const int DefaultLength = 4000;

        public void Apply(EdmEntityType edmDataModelItem, EdmModel model)
        {
            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(edmDataModelItem.DeclaredProperties);
            }
        }

        public void Apply(EdmComplexType edmDataModelItem, EdmModel model)
        {
            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(edmDataModelItem.DeclaredProperties);
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
