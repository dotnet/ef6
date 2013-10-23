// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    internal static class SchemaFilterEntryExtensions
    {
        internal static EntityStoreSchemaFilterEffect GetEffectViaFilter(
            this EntityStoreSchemaFilterEntry entryToTest, IEnumerable<EntityStoreSchemaFilterEntry> filterEntries)
        {
            var effect = EntityStoreSchemaFilterEffect.Exclude;

            // Look for the all filter for specific type of object; this includes the 'All' types filter
            foreach (var entry in filterEntries.Where(e => e.Name == "%" && (e.Types & entryToTest.Types) == entryToTest.Types))
            {
                effect = entry.Effect;
                break;
            }

            // Look for the specific type of object
            foreach (var entry in filterEntries.Where(
                e =>
                e.Catalog.Equals(entryToTest.Catalog ?? String.Empty, StringComparison.CurrentCulture) &&
                e.Schema.Equals(entryToTest.Schema ?? String.Empty, StringComparison.CurrentCulture) &&
                e.Name.Equals(entryToTest.Name ?? String.Empty, StringComparison.CurrentCulture)))
            {
                effect = entry.Effect;
                break;
            }

            return effect;
        }
    }
}
