// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal class NonUnicodeDiscoverer : IPropertyConfigurationDiscoverer
    {
        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            if (property.PrimitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
            {
                // Doesn't apply
                return null;
            }

            if (property.IsUnicode == true)
            {
                // By convention
                return null;
            }

            return new NonUnicodeConfiguration();
        }
    }
}

