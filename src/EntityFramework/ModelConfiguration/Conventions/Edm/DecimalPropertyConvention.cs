// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;

    /// <summary>
    ///     Convention to set precision to 18 and scale to 2 for decimal properties.
    /// </summary>
    public class DecimalPropertyConvention : IEdmConvention<EdmProperty>
    {
        public void Apply(EdmProperty edmDataModelItem, EdmModel model)
        {
            if (edmDataModelItem.PropertyType.PrimitiveType
                == EdmPrimitiveType.Decimal)
            {
                var facets = edmDataModelItem.PropertyType.PrimitiveTypeFacets;

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
