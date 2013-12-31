// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class KeyPropertyDiscoverer : IPropertyConfigurationDiscoverer
    {
        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            var keyProperties = ((EntityType)property.DeclaringType).KeyProperties;

            if (!keyProperties.Contains(property))
            {
                // Doesn't apply
                return null;
            }

            if (keyProperties.Count == 1 && keyProperties.First().HasConventionalKeyName())
            {
                // By convention
                return null;
            }

            return new KeyPropertyConfiguration();
        }
    }
}

