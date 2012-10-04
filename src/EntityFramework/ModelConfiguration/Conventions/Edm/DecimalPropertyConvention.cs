// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Convention to set precision to 18 and scale to 2 for decimal properties.
    /// </summary>
    public class DecimalPropertyConvention : IEdmConvention<EdmProperty>
    {
        public void Apply(EdmProperty edmDataModelItem, EdmModel model)
        {
            if (edmDataModelItem.PrimitiveType
                == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal))
            {
                if (edmDataModelItem.Precision == null)
                {
                    edmDataModelItem.Precision = 18;
                }

                if (edmDataModelItem.Scale == null)
                {
                    edmDataModelItem.Scale = 2;
                }
            }
        }
    }
}
