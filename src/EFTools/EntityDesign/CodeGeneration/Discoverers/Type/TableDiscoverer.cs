// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class TableDiscoverer : ITypeConfigurationDiscoverer
    {
        private readonly CodeHelper _code;
        private readonly IPluralizationService _pluralizationService;

        public TableDiscoverer(CodeHelper code)
            : this(code, DependencyResolver.GetService<IPluralizationService>())
        {
            Debug.Assert(code != null, "code is null.");
        }

        // Internal for testing
        internal TableDiscoverer(CodeHelper code, IPluralizationService pluralizationService)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(pluralizationService != null, "pluralizationService is null.");

            _code = code;
            _pluralizationService = pluralizationService;
        }

        public IConfiguration Discover(EntitySet entitySet, DbModel model)
        {
            Debug.Assert(entitySet != null, "entitySet is null.");
            Debug.Assert(model != null, "model is null.");

            var storeEntitySet = model.ConceptualToStoreMapping
                .EntitySetMappings.First(m => m.EntitySet == entitySet)
                .EntityTypeMappings.First()
                .Fragments.First()
                .StoreEntitySet;
            var tableName = storeEntitySet.GetStoreModelBuilderMetadataProperty("Name")
                ?? storeEntitySet.Table
                ?? storeEntitySet.Name;
            var schemaName = storeEntitySet.GetStoreModelBuilderMetadataProperty("Schema") ?? storeEntitySet.Schema;

            if (_pluralizationService.Pluralize(_code.Type(entitySet.ElementType)) == tableName
                && (string.IsNullOrEmpty(schemaName) || schemaName == "dbo"))
            {
                // By convention
                return null;
            }

            return new TableConfiguration { Table = tableName, Schema = schemaName != "dbo" ? schemaName : null };
        }
    }
}

