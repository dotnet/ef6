namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;

    internal class FixedLengthDiscoverer : LengthDiscovererBase
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

            if (// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

property.IsFixedLength != true)
            {
                // By convention
                return null;
            }

            return new FixedLengthConfiguration();
        }
    }
}

