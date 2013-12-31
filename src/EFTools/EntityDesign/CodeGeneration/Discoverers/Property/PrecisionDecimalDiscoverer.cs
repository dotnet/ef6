// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal class PrecisionDecimalDiscoverer : IPropertyConfigurationDiscoverer
    {
        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            if (property.PrimitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                // Doesn't apply
                return null;
            }

            var precision = property.Precision ?? 18;
            var scale = property.Scale ?? 2;

            if (precision == 18 && scale == 2)
            {
                // By convention
                return null;
            }

            return new PrecisionDecimalConfiguration { Precision = precision, Scale = scale };
        }
    }
}

