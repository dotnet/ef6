// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Xunit;

#if NET40
    using System.Data.Entity.Resources;
    using Moq;
#endif

    public class DefaultDbProviderFactoryServiceTests : TestBase
    {
        [Fact]
        public void GetProviderFactory_throws_for_null_connection()
        {
            Assert.Equal(
                "connection",
                Assert.Throws<ArgumentNullException>(() => new DefaultDbProviderFactoryService().GetProviderFactory(null)).ParamName);
        }

        [Fact]
        public void GetProviderFactory_for_SqlConnection_should_return_SqlClientFactory()
        {
            Assert.Equal(SqlClientFactory.Instance, new DefaultDbProviderFactoryService().GetProviderFactory(new SqlConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_EntityConnection_should_return_EntityProviderFactory()
        {
            Assert.Equal(EntityProviderFactory.Instance, new DefaultDbProviderFactoryService().GetProviderFactory(new EntityConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_generic_connection_should_return_correct_generic_factory()
        {
            Assert.NotNull(GenericProviderFactory<DbProviderFactory>.Instance);
            Assert.Equal(
                GenericProviderFactory<DbProviderFactory>.Instance,
                new DefaultDbProviderFactoryService(
                    new ProviderRowFinder(DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>()))
                    .GetProviderFactory(new GenericConnection<DbProviderFactory>()));
        }

#if NET40
        [Fact]
        public void GetProviderFactory_throws_for_unknown_provider_on_net40()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.ToString()).Returns("Disco 2000");

            Assert.Equal(
                Strings.ProviderNotFound("Disco 2000"),
                Assert.Throws<NotSupportedException>(
                    () => new DefaultDbProviderFactoryService().GetProviderFactory(mockConnection.Object)).Message);
        }

        [Fact]
        public void GetProviderFactory_caches_factory_instances_on_net40()
        {
            var mockFinder = new Mock<ProviderRowFinder>(null)
                                 {
                                     CallBase = true
                                 };

            var service = new DefaultDbProviderFactoryService(mockFinder.Object);

            Assert.Equal(SqlClientFactory.Instance, service.GetProviderFactory(new SqlConnection()));
            mockFinder.Verify(m => m.FindRow(It.IsAny<Type>(), It.IsAny<Func<DataRow, bool>>()), Times.Once());

            Assert.Equal(SqlClientFactory.Instance, service.GetProviderFactory(new SqlConnection()));
            mockFinder.Verify(m => m.FindRow(It.IsAny<Type>(), It.IsAny<Func<DataRow, bool>>()), Times.Once()); // Finder not called again
        }
#endif

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_same_assembly()
        {
            var rows = new[]
                           {
                               CreateProviderRow("Row1", "Row.1", _weakProviderType2.AssemblyQualifiedName),
                               CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                               CreateProviderRow("Row3", "Row.3", _weakProviderType1.AssemblyQualifiedName),
                               CreateProviderRow("Row4", "Row.4", typeof(FakeSqlProviderFactory).AssemblyQualifiedName),
                           };

            Assert.Same(
                FakeSqlProviderFactory.Instance,
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows)).GetProviderFactory(new FakeSqlConnection()));
        }

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_different_assembly()
        {
            var rows = new[]
                           {
                               CreateProviderRow("Row1", "Row.1", _weakProviderType2.AssemblyQualifiedName),
                               CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                               CreateProviderRow("Row3", "Row.3", _weakProviderType1.AssemblyQualifiedName),
                           };

            Assert.Same(
                GetFactoryInstance(_weakProviderType1),
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows))
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(_weakProviderType1))));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_same_assembly()
        {
            var rows = new[]
                           {
                               CreateProviderRow("Row1", "Row.1", _weakProviderType2.AssemblyQualifiedName),
                               CreateProviderRow("Row2", "Row.2", typeof(FakeProviderFactory2).AssemblyQualifiedName),
                           };

            Assert.Same(
                FakeProviderFactory2.Instance,
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows))
                    .GetProviderFactory(new FakeSqlConnection("2008", FakeProviderFactory2.Instance)));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_different_assembly()
        {
            var rows = new[]
                           {
                               CreateProviderRow("Row1", "Row.1", _weakProviderType2.AssemblyQualifiedName),
                           };

            Assert.Same(
                GetFactoryInstance(_weakProviderType2),
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows))
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(_weakProviderType2))));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_without_version_or_key_in_name()
        {
            var rows = new[]
                           {
                               CreateProviderRow("ProviderFactory", "Provider.Factory", "WeakProviderFactory, ProviderAssembly1"),
                           };

            Assert.Same(
                GetFactoryInstance(_weakProviderType1),
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows))
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(_weakProviderType1))));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_with_non_standard_spacing_in_name()
        {
            var rows = new[]
                           {
                               CreateProviderRow(
                                   "ProviderFactory", "Provider.Factory",
                                   "WeakProviderFactory,ProviderAssembly1,   Version=0.0.0.0,    Culture=neutral,PublicKeyToken=null"),
                           };

            Assert.Same(
                GetFactoryInstance(_weakProviderType1),
                new DefaultDbProviderFactoryService(new ProviderRowFinder(rows))
                    .GetProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(_weakProviderType1))));
        }

        private static readonly Type _weakProviderType1 = CreateWeakProviderType(typeof(FakeProviderBase1), "ProviderAssembly1");
        private static readonly Type _weakProviderType2 = CreateWeakProviderType(typeof(FakeProviderBase2), "ProviderAssembly2");

        private static Type CreateWeakProviderType(Type baseProviderType, string assemblyName)
        {
            var assembly = new DynamicAssembly();
            var dynamicType =
                assembly.DynamicType("WeakProviderFactory").HasBaseClass(baseProviderType);
            dynamicType.CtorAccess = MemberAccess.Public;
            dynamicType.Field("Instance").HasType(dynamicType).IsStatic().IsInstance();
            var compiledAssembly = assembly.Compile(new AssemblyName(assemblyName));

            // We need this so that Type.GetType() used in DbProviderFactories.GetFactory will work for
            // the dynamic assembly. In other words, this is only needed for the test code to work.
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) => args.Name.StartsWith(assemblyName) ? compiledAssembly : null;

            return assembly.GetType("WeakProviderFactory");
        }

        private static DbProviderFactory GetFactoryInstance(Type factoryType)
        {
            return (DbProviderFactory)factoryType.GetField("Instance").GetValue(null);
        }

        public class FakeProviderBase1 : DbProviderFactory
        {
            public override DbConnection CreateConnection()
            {
                return new FakeSqlConnection();
            }
        }

        public class FakeProviderBase2 : DbProviderFactory
        {
            public override DbConnection CreateConnection()
            {
                return new DerivedFakeSqlConnection();
            }
        }

        public class FakeProviderFactory2 : DbProviderFactory
        {
            public static readonly FakeProviderFactory2 Instance = new FakeProviderFactory2();

            public override DbConnection CreateConnection()
            {
                return new DerivedFakeSqlConnection();
            }
        }

        public class DerivedFakeSqlConnection : FakeSqlConnection
        {
        }
    }
}
