// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ModelHashCalculatorTests : TestBase
    {
        [Fact]
        public void ModelHashCalculator_clones_the_model_builder()
        {
            var mockBuilder = new Mock<DbModelBuilder>
                                  {
                                      CallBase = true
                                  };
            var mockModel = CreateMockCompiledModel(mockBuilder, new DbModelBuilder());

            new ModelHashCalculator().Calculate(mockModel.Object);

            mockBuilder.Verify(m => m.Clone());
        }

        [Fact]
        public void ModelHashCalculator_includes_EdmMetadata_in_the_hash()
        {
            var clone = new DbModelBuilder();
            var mockBuilder = new Mock<DbModelBuilder>
                                  {
                                      CallBase = true
                                  };
            var mockModel = CreateMockCompiledModel(mockBuilder, clone);

            new ModelHashCalculator().Calculate(mockModel.Object);

#pragma warning disable 612,618
            Assert.True(clone.ModelConfiguration.Entities.Contains(typeof(EdmMetadata)));
            Assert.False(mockBuilder.Object.ModelConfiguration.Entities.Contains(typeof(EdmMetadata)));
#pragma warning restore 612,618
        }

        private Mock<DbCompiledModel> CreateMockCompiledModel(Mock<DbModelBuilder> mockBuilder, DbModelBuilder clone)
        {
            mockBuilder.Setup(m => m.Clone()).Returns(clone);

            var mockModel = new Mock<DbCompiledModel>();
            mockModel.Setup(m => m.ProviderInfo).Returns(ProviderRegistry.Sql2008_ProviderInfo);
            mockModel.Setup(m => m.CachedModelBuilder).Returns(mockBuilder.Object);
            return mockModel;
        }

        [Fact]
        public void ModelHashCalculator_creates_expected_model_hash_for_empty_builder()
        {
            // If this hash changes then it may mean that people upgrading from a previous version
            // will be forced to update their databases when in reality they have not changed anything.
            var builder = new DbModelBuilder();
            Assert.Equal(
                "B8F62AABEADFA7D4C6855DFBCEA0BCD540A7BF1D3226CF8DF9421CC06FD1ABD6",
                new ModelHashCalculator().Calculate(
                    new DbModel(builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping, builder).Compile()));
        }
    }
}
