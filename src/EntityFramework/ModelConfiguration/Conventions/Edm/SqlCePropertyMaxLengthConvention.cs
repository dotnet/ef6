// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to set a default maximum length of 4000 for properties whose type supports length facets when SqlCe is the provider.
    /// </summary>
    public class SqlCePropertyMaxLengthConvention : IEdmConvention<EntityType>, IEdmConvention<ComplexType>
    {
        private const int DefaultLength = 4000;

        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(edmDataModelItem.DeclaredProperties);
            }
        }

        public void Apply(ComplexType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            var providerInfo = model.GetProviderInfo();

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(edmDataModelItem.Properties);
            }
        }

        private static void SetLength(IEnumerable<EdmProperty> properties)
        {
            foreach (var property in properties)
            {
                if (!property.IsPrimitiveType)
                {
                    continue;
                }

                if ((property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    || (property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)))
                {
                    SetDefaults(property);
                }
            }
        }

        private static void SetDefaults(EdmProperty property)
        {
            DebugCheck.NotNull(property);

            if ((property.MaxLength == null)
                && (!property.IsMaxLength))
            {
                property.MaxLength = DefaultLength;
            }
        }
    }
}
