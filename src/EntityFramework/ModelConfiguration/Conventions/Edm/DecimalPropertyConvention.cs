// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;

    /// <summary>
    ///     Convention to set precision to 18 and scale to 2 for decimal properties.
    /// </summary>
    public sealed class DecimalPropertyConvention : IEdmConvention<EdmProperty>
    {
        internal DecimalPropertyConvention()
        {
        }

        void IEdmConvention<EdmProperty>.Apply(EdmProperty property, EdmModel model)
        {
            if (property.PropertyType.PrimitiveType
                == EdmPrimitiveType.Decimal)
            {
                var facets = property.PropertyType.PrimitiveTypeFacets;

                if (facets.Precision == null)
                {
                    facets.Precision = 18;
                }

                if (facets.Scale == null)
                {
                    facets.Scale = 2;
                }
            }
        }
    }
}
