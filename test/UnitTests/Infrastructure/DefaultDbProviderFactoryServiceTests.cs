// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.SqlClient;
    using System.Reflection;
    using Xunit;

    public class DefaultDbProviderFactoryServiceTests : TestBase
    {
#if !NET40

        [Fact]
        public void GetProviderFactory_throws_for_null_connection()
        {
            Assert.Equal(
                "connection",
                Assert.Throws<ArgumentNullException>(() => new DefaultDbProviderFactoryResolver().ResolveProviderFactory(null)).ParamName);
        }

        [Fact]
        public void GetProviderFactory_for_SqlConnection_should_return_SqlClientFactory()
        {
            Assert.Equal(SqlClientFactory.Instance, new DefaultDbProviderFactoryResolver().ResolveProviderFactory(new SqlConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_EntityConnection_should_return_EntityProviderFactory()
        {
            Assert.Equal(EntityProviderFactory.Instance, new DefaultDbProviderFactoryResolver().ResolveProviderFactory(new EntityConnection()));
        }

        [Fact]
        public void GetProviderFactory_for_generic_connection_should_return_correct_generic_factory()
        {
            Assert.NotNull(GenericProviderFactory<DbProviderFactory>.Instance);
            Assert.Equal(
                GenericProviderFactory<DbProviderFactory>.Instance,
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new GenericConnection<DbProviderFactory>()));
        }

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_same_assembly()
        {
            Assert.Same(
                FakeSqlProviderFactory.Instance,
                new DefaultDbProviderFactoryResolver().ResolveProviderFactory(new FakeSqlConnection()));
        }

        [Fact]
        public void GetProviderFactory_returns_exact_connection_type_match_in_different_assembly()
        {
            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1))));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_same_assembly()
        {
            Assert.Same(
                FakeProviderFactory2.Instance,
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new FakeSqlConnection("2008", FakeProviderFactory2.Instance)));
        }

        [Fact]
        public void GetProviderFactory_returns_derived_connection_type_match_in_different_assembly()
        {
            Assert.Same(
                GetFactoryInstance(WeakProviderType2),
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType2))));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_without_version_or_key_in_name()
        {
            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1))));
        }

        [Fact]
        public void GetProviderFactory_returns_provider_in_weakly_named_assembly_with_non_standard_spacing_in_name()
        {
            Assert.Same(
                GetFactoryInstance(WeakProviderType1),
                new DefaultDbProviderFactoryResolver()
                    .ResolveProviderFactory(new FakeSqlConnection("2008", GetFactoryInstance(WeakProviderType1))));
        }
#endif

        protected static readonly Type WeakProviderType1 = CreateWeakProviderType(typeof(FakeProviderBase1), "ProviderAssembly1");
        protected static readonly Type WeakProviderType2 = CreateWeakProviderType(typeof(FakeProviderBase2), "ProviderAssembly2");

        protected static Type CreateWeakProviderType(Type baseProviderType, string assemblyName)
        {
            var assembly = new DynamicAssembly();
            var dynamicType =
                assembly.DynamicStructuralType("WeakProviderFactory").HasBaseClass(baseProviderType);
            dynamicType.CtorAccess = MemberAccess.Public;
            dynamicType.Field("Instance").HasType(dynamicType).IsStatic().IsInstance();
            var compiledAssembly = assembly.Compile(new AssemblyName(assemblyName));

            // We need this so that Type.GetType() used in DbProviderFactories.GetFactory will work for
            // the dynamic assembly. In other words, this is only needed for the test code to work.
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) => args.Name.StartsWith(assemblyName) ? compiledAssembly : null;

            return assembly.GetType("WeakProviderFactory");
        }

        protected static DbProviderFactory GetFactoryInstance(Type factoryType)
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
