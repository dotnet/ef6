// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class Net40DefaultDbProviderFactoryServiceTests : DefaultDbProviderFactoryServiceTests
    {
        [Fact]
        public void GetProviderFactory_throws_for_null_connection_on_net40()
        {
            Assert.Equal(
                "connection",
                Assert.Throws<ArgumentNullException>(() => new Net40DefaultDbProviderFactoryService().GetProviderFactory(null)).ParamName);
        }

        [Fact]
        public void GetProviderFactory_for_SqlConnection_should_return_SqlClientFactory_on_net40()
        {
            Assert.Equal(SqlClientFactory.Instance, new Net40DefaultDbProviderFactoryService().GetProviderFactory(new SqlConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_EntityConnection_should_return_EntityProviderFactory_on_net40()
        {
            Assert.Equal(
                EntityProviderFactory.Instance, new Net40DefaultDbProviderFactoryService().GetProviderFactory(new EntityConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_generic_connection_should_return_correct_generic_factory_on_net40()
        {
            Assert.NotNull(GenericProviderFactory<DbProviderFactory>.Instance);
            Assert.Equal(
                GenericProviderFactory<DbProviderFactory>.Instance,
                new Net40DefaultDbProviderFactoryService(
                    new ProviderRowFinder()).GetProviderFactory(
                        new GenericConnection<DbProviderFactory>(), DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>()));
        }

        [Fact]
        public void GetProviderFactory_throws_for_unknown_provider_on_net40()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.ToString()).Returns("Disco 2000");

            Assert.Equal(
                Strings.ProviderNotFound("Disco 2000"),
                Assert.Throws<NotSupportedException>(
                    () => new Net40DefaultDbProviderFactoryService().GetProviderFactory(mockConnection.Object)).Message);
        }

        [Fact]
        public void GetProviderFactory_caches_factory_instances_on_net40()
        {
            var mockFinder = new Mock<ProviderRowFinder>
                {
                    CallBase = true
                };

            var service = new Net40DefaultDbProviderFactoryService(mockFinder.Object);

            Assert.Equal(SqlClientFactory.Instance, service.GetProviderFactory(new SqlConnection()));
            mockFinder.Verify(
                m => m.FindRow(It.IsAny<Type>(), It.IsAny<Func<DataRow, bool>>(), It.IsAny<IEnumerable<DataRow>>()), Times.Once());

            Assert.Equal(SqlClientFactory.Instance, service.GetProviderFactory(new SqlConnection()));
            // Finder not called again
            mockFinder.Verify(
                m => m.FindRow(It.IsAny<Type>(), It.IsAny<Func<DataRow, bool>>(), It.IsAny<IEnumerable<DataRow>>()), Times.Once());
        }

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_same_assembly_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow("Row1", "Row.1", WeakProviderType2.AssemblyQualifiedName),
                    CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                    CreateProviderRow("Row3", "Row.3", WeakProviderType1.AssemblyQualifiedName),
                    CreateProviderRow("Row4", "Row.4", typeof(FakeSqlProviderFactory).AssemblyQualifiedName),
                };

            Assert.Same(
                FakeSqlProviderFactory.Instance,
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder()).GetProviderFactory(new FakeSqlConnection(), rows));
        }

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_different_assembly_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow("Row1", "Row.1", WeakProviderType2.AssemblyQualifiedName),
                    CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                    CreateProviderRow("Row3", "Row.3", WeakProviderType1.AssemblyQualifiedName),
                };

            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder())
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1)), rows));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_same_assembly_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow("Row1", "Row.1", WeakProviderType2.AssemblyQualifiedName),
                    CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                };

            Assert.Same(
                FakeProviderFactory2.Instance,
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder())
                    .GetProviderFactory(new FakeSqlConnection("2008", FakeProviderFactory2.Instance), rows));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_different_assembly_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow("Row1", "Row.1", WeakProviderType2.AssemblyQualifiedName),
                };

            Assert.Same(
                GetFactoryInstance(WeakProviderType2),
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder())
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType2)), rows));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_without_version_or_key_in_name_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow("ProviderFactory", "Provider.Factory", "WeakProviderFactory, ProviderAssembly1"),
                };

            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder())
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1)), rows));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_with_non_standard_spacing_in_name_on_net40()
        {
            var rows = new[]
                {
                    CreateProviderRow(
                        "ProviderFactory", "Provider.Factory",
                        "WeakProviderFactory,ProviderAssembly1,   Version=0.0.0.0,    Culture=neutral,PublicKeyToken=null"),
                };

            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new Net40DefaultDbProviderFactoryService(new ProviderRowFinder())
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1)), rows));
        }
    }
}
