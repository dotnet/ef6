// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class DbModelTests : TestBase
    {
        [Fact]
        public void Can_retrieve_entity_container_mapping()
        {
            var model = new DbModel(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var containerMapping = new EntityContainerMapping();

            Assert.Null(model.ConceptualToStoreMapping);

            model.DatabaseMapping.AddEntityContainerMapping(containerMapping);

            Assert.Same(containerMapping, model.ConceptualToStoreMapping);
        }

        [Fact]
        public void Compile_builds_populated_DbCompiledModel()
        {
            var defaultSchema = "mySchema";
            var modelBuilder = new DbModelBuilder();
            modelBuilder.ModelConfiguration.DefaultSchema = defaultSchema;
            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);
            
            var compiled = model.Compile();

            Assert.IsType<DbCompiledModel>(compiled);
            Assert.Same(model.CachedModelBuilder, compiled.CachedModelBuilder);
            Assert.Equal(defaultSchema, compiled.DefaultSchema);
            Assert.Equal(ProviderRegistry.Sql2008_ProviderInfo, compiled.ProviderInfo);
        }
    }
}
