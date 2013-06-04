// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Xunit;

    public class DbContextExtensionsTests
    {
        [Fact]
        public void GetDynamicUpdateModel_should_build_model_mapped_to_tables()
        {
            var context = new ShopContext_v1();

            var model
                = context
                    .InternalContext
                    .CodeFirstModel
                    .CachedModelBuilder
                    .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.NotNull(model);

            var entityContainerMapping = model.DatabaseMapping.EntityContainerMappings.Single();

            Assert.False(entityContainerMapping.EntitySetMappings.SelectMany(esm => esm.ModificationFunctionMappings).Any());
            Assert.False(entityContainerMapping.AssociationSetMappings.Any(asm => asm.ModificationFunctionMapping != null));
        }

        [Fact]
        public void Should_be_able_to_get_model_from_context()
        {
            var context = new ShopContext_v1();

            var edmX = context.GetModel();

            Assert.NotNull(edmX);
        }
    }
}
