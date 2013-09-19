// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    internal class ConventionSet
    {
        public ConventionSet()
        {
            ConfigurationConventions = new IConvention[0];
            ConceptualModelConventions = new IConvention[0];
            ConceptualToStoreMappingConventions = new IConvention[0];
            StoreModelConventions = new IConvention[0];
        }

        public ConventionSet(
            IEnumerable<IConvention> configurationConventions,
            IEnumerable<IConvention> entityModelConventions,
            IEnumerable<IConvention> dbMappingConventions,
            IEnumerable<IConvention> dbModelConventions)
        {
            DebugCheck.NotNull(configurationConventions);
            DebugCheck.NotNull(entityModelConventions);
            DebugCheck.NotNull(dbMappingConventions);
            DebugCheck.NotNull(dbModelConventions);

            ConfigurationConventions = configurationConventions;
            ConceptualModelConventions = entityModelConventions;
            ConceptualToStoreMappingConventions = dbMappingConventions;
            StoreModelConventions = dbModelConventions;
        }

        public IEnumerable<IConvention> ConfigurationConventions { get; private set; }
        public IEnumerable<IConvention> ConceptualModelConventions { get; private set; }
        public IEnumerable<IConvention> ConceptualToStoreMappingConventions { get; private set; }
        public IEnumerable<IConvention> StoreModelConventions { get; private set; }
    }
}
