// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class MaxLengthDiscoverer : LengthDiscovererBase
    {
        public override IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            if (!_lengthTypes.Contains(property.PrimitiveType.PrimitiveTypeKind))
            {
                // Doesn't apply
                return null;
            }

            if (property.IsMaxLength
                || !property.MaxLength.HasValue
                || (property.MaxLength.Value == 128 && property.IsKey()))
            {
                // By convention
                return null;
            }

            var configuration = property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String
                ? new MaxLengthStringConfiguration()
                : new MaxLengthConfiguration();
            configuration.MaxLength = property.MaxLength.Value;

            return configuration;
        }
    }
}

