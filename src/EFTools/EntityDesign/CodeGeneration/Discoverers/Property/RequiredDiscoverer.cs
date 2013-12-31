// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class RequiredDiscoverer : IPropertyConfigurationDiscoverer
    {
        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            if (property.Nullable
                || property.PrimitiveType.ClrEquivalentType.IsValueType
                || property.IsKey()
                || model.GetColumn(property).IsTimestamp())
            {
                // By convention
                return null;
            }

            return new RequiredConfiguration();
        }
    }
}

