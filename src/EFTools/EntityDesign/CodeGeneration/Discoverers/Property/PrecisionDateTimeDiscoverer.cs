// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class PrecisionDateTimeDiscoverer : IPropertyConfigurationDiscoverer
    {
        private static readonly PrimitiveTypeKind[] _precisionTypes = new[]
                {
                    PrimitiveTypeKind.DateTime,
                    PrimitiveTypeKind.Time,
                    PrimitiveTypeKind.DateTimeOffset
                };

        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            if (!_precisionTypes.Contains(property.PrimitiveType.PrimitiveTypeKind))
            {
                // Doesn't apply
                return null;
            }

            var storeProperty = model.GetColumn(property);
            var defaultPrecision = (byte)storeProperty.PrimitiveType.FacetDescriptions
                .First(d => d.FacetName == DbProviderManifest.PrecisionFacetName).DefaultValue;

            // NOTE: This facet is not propagated to the conceptual side of the reverse
            //       engineered model.
            var precision = storeProperty.Precision ?? defaultPrecision;

            if (precision == defaultPrecision)
            {
                // By convention
                return null;
            }

            return new PrecisionDateTimeConfiguration { Precision = precision };
        }
    }
}

