// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    internal class KeyDiscoverer : ITypeConfigurationDiscoverer
    {
        public IConfiguration Discover(EntitySet entitySet, DbModel model)
        {
            Debug.Assert(entitySet != null, "entitySet is null.");
            Debug.Assert(model != null, "model is null.");

            var entityType = entitySet.ElementType;
            var keyProperties = entityType.KeyProperties;
            Debug.Assert(keyProperties.Count != 0, "keyProperties is empty.");

            if (keyProperties.Count == 1 && keyProperties.First().HasConventionalKeyName())
            {
                // By convention
                return null;
            }

            var configuration = new KeyConfiguration();

            foreach (var keyProperty in keyProperties)
            {
                configuration.KeyProperties.Add(keyProperty);
            }

            return configuration;
        }
    }
}

