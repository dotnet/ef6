// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using Xunit;

    public class ProviderServicesFactoryTests
    {
        public class TryGetInstance
        {
            [Fact]
            public void TryGetInstance_can_return_SqlProviderServices()
            {
                Assert.Same(
                    SqlProviderServices.Instance,
                    new ProviderServicesFactory().TryGetInstance(
                        "System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer"));
            }

            [Fact]
            public void TryGetInstance_returns_null_if_type_cannot_be_loaded()
            {
                Assert.Null(new ProviderServicesFactory().TryGetInstance("Wonderbolt.Acadamy"));
            }
        }

        public class GetDbProviderServices
        {
            [Fact]
            public void GetDbProviderServices_uses_public_Instance_property_if_available()
            {
                Assert.Same(
                    FakeProviderWithPublicProperty.Instance,
                    new ProviderServicesFactory().GetInstance(
                        typeof(FakeProviderWithPublicProperty).AssemblyQualifiedName, "Learning.To.Fly"));
            }

            [Fact]
            public void GetDbProviderServices_uses_public_Instance_field_if_available()
            {
                Assert.Same(
                    FakeProviderWithPublicField.Instance,
                    new ProviderServicesFactory().GetInstance(
                        typeof(FakeProviderWithPublicField).AssemblyQualifiedName, "I.Wanna.Hold.Your.Hand"));
            }

            [Fact]
            public void GetDbProviderServices_uses_non_public_Instance_property_if_available()
            {
                Assert.IsType<FakeProviderWithNonPublicProperty>(
                    new ProviderServicesFactory().GetInstance(
                        typeof(FakeProviderWithNonPublicProperty).AssemblyQualifiedName, "Stairway.To.Heaven"));
            }

            [Fact]
            public void GetDbProviderServices_uses_non_public_Instance_field_if_available()
            {
                Assert.IsType<FakeProviderWithNonPublicField>(
                    new ProviderServicesFactory().GetInstance(
                        typeof(FakeProviderWithNonPublicField).AssemblyQualifiedName, "Does.Anybody.Remember.Laughter?"));
            }

            [Fact]
            public void GetDbProviderServices_throws_if_provider_type_cannot_be_loaded()
            {
                Assert.Equal(
                    Strings.EF6Providers_ProviderTypeMissing("Killer.Queen.ProviderServices, Sheer.Heart.Attack", "Killer.Queen"),
                    Assert.Throws<InvalidOperationException>(
                        () => new ProviderServicesFactory().GetInstance(
                            "Killer.Queen.ProviderServices, Sheer.Heart.Attack", "Killer.Queen")).Message);
            }

            [Fact]
            public void GetDbProviderServices_throws_if_there_is_no_Instance_field_or_property()
            {
                Assert.Equal(
                    Strings.EF6Providers_InstanceMissing(typeof(FakeProviderWithNoInstance).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(
                        () => new ProviderServicesFactory().GetInstance(
                            typeof(FakeProviderWithNoInstance).AssemblyQualifiedName, "One.Headlight")).Message);
            }

            [Fact]
            public void GetDbProviderServices_throws_if_Instance_member_returns_null()
            {
                Assert.Equal(
                    Strings.EF6Providers_NotDbProviderServices(typeof(FakeProviderWithNullInstance).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(
                        () => new ProviderServicesFactory().GetInstance(
                            typeof(FakeProviderWithNullInstance).AssemblyQualifiedName, "Another.One.Bites.The.Dust")).Message);
            }

            [Fact]
            public void GetDbProviderServices_throws_if_Instance_member_is_not_a_DbProviderServices_instance()
            {
                Assert.Equal(
                    Strings.EF6Providers_NotDbProviderServices(typeof(FakeProviderWithBadInstance).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(
                        () => new ProviderServicesFactory().GetInstance(
                            typeof(FakeProviderWithBadInstance).AssemblyQualifiedName, "Everlong")).Message);
            }
        }

        public class FakeProviderWithBadInstance : FakeProviderBase
        {
            private static readonly Random Instance = new Random();
        }

        public class FakeProviderWithNullInstance : FakeProviderBase
        {
#pragma warning disable 169 // Used through Reflection
            private static readonly FakeProviderWithNullInstance Instance;
#pragma warning restore 169
        }

        public class FakeProviderWithNoInstance : FakeProviderBase
        {
        }

        public class FakeProviderWithPublicField : FakeProviderBase
        {
            public static readonly FakeProviderWithPublicField Instance = new FakeProviderWithPublicField();
        }

        public class FakeProviderWithPublicProperty : FakeProviderBase
        {
            private static readonly FakeProviderWithPublicProperty Singleton = new FakeProviderWithPublicProperty();

            public static FakeProviderWithPublicProperty Instance
            {
                get { return Singleton; }
            }
        }

        public class FakeProviderWithNonPublicField : FakeProviderBase
        {
            private static readonly FakeProviderWithNonPublicField Instance = new FakeProviderWithNonPublicField();
        }

        public class FakeProviderWithNonPublicProperty : FakeProviderBase
        {
            private static readonly FakeProviderWithNonPublicProperty Singleton = new FakeProviderWithNonPublicProperty();

            private static FakeProviderWithNonPublicProperty Instance
            {
                get { return Singleton; }
            }
        }

        public class FakeProviderBase : DbProviderServices
        {
            protected override DbCommandDefinition CreateDbCommandDefinition(
                DbProviderManifest providerManifest, DbCommandTree commandTree)
            {
                throw new NotImplementedException();
            }

            protected override string GetDbProviderManifestToken(DbConnection connection)
            {
                throw new NotImplementedException();
            }

            protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
