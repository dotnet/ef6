// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class DatabaseGeneratedDiscoverer : IPropertyConfigurationDiscoverer
    {
        private static readonly PrimitiveTypeKind[] _identityKeyTypes = new[]
                {
                    PrimitiveTypeKind.Int32,
                    PrimitiveTypeKind.Int64,
                    PrimitiveTypeKind.Int16
                };

        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            var columnProperty = model.GetColumn(property);

            if (property.IsKey() && _identityKeyTypes.Contains(property.PrimitiveType.PrimitiveTypeKind))
            {
                if (columnProperty.IsStoreGeneratedIdentity)
                {
                    // By convention
                    return null;
                }
            }
            else if (columnProperty.IsTimestamp())
            {
                // By convention
                return null;
            }
            else if (columnProperty.StoreGeneratedPattern == StoreGeneratedPattern.None)
            {
                // Doesn't apply
                return null;
            }

            return new DatabaseGeneratedConfiguration { StoreGeneratedPattern = columnProperty.StoreGeneratedPattern };
        }
    }
}

