// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Moq;
    using Xunit;

    public class EntityProxyFactoryTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(EntityWrapperFactory.CreateWrapperDelegateTypedLightweightMethod);
            Assert.NotNull(EntityWrapperFactory.CreateWrapperDelegateTypedWithRelationshipsMethod);
            Assert.NotNull(EntityWrapperFactory.CreateWrapperDelegateTypedWithoutRelationshipsMethod);
        }

#if NET452
        public class MarkAsNotSerializable : TestBase
        {
            [Fact]
            public void Field_is_marked_with_all_ignore_attributes_when_System_Web_loaded()
            {
                RunTestInAppDomain(typeof(MarkAsNotSerializablePublicWithSystemWeb));
            }

            public class MarkAsNotSerializablePublicWithSystemWeb : MarshalByRefObject
            {
                public MarkAsNotSerializablePublicWithSystemWeb()
                {
                    // Ensure System.Web is loaded for this test
                    Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                    var attributes = BuildTestField(FieldAttributes.Public)
                        .GetCustomAttributes(inherit: false)
                        .Select(a => a.GetType().Name)
                        .ToList();

                    Assert.Contains("NonSerializedAttribute", attributes);
                    Assert.Contains("IgnoreDataMemberAttribute", attributes);
                    Assert.Contains("XmlIgnoreAttribute", attributes);
                    Assert.Contains("ScriptIgnoreAttribute", attributes);
                }
            }

            [Fact]
            public void Field_is_marked_with_all_ignore_attributes_except_script_ignore_when_System_Web_not_loaded()
            {
                RunTestInAppDomain(typeof(MarkAsNotSerializablePublicWithNoSystemWeb));
            }

            public class MarkAsNotSerializablePublicWithNoSystemWeb : MarshalByRefObject
            {
                public MarkAsNotSerializablePublicWithNoSystemWeb()
                {
                    var attributes = BuildTestField(FieldAttributes.Public)
                        .GetCustomAttributes(inherit: false)
                        .Select(a => a.GetType().Name)
                        .ToList();

                    Assert.Contains("NonSerializedAttribute", attributes);
                    Assert.Contains("IgnoreDataMemberAttribute", attributes);
                    Assert.Contains("XmlIgnoreAttribute", attributes);
                    Assert.DoesNotContain("ScriptIgnoreAttribute", attributes);
                }
            }

            [Fact]
            public void Field_is_marked_with_only_non_serialized_when_not_public()
            {
                RunTestInAppDomain(typeof(MarkAsNotSerializableNonPublic));
            }

            public class MarkAsNotSerializableNonPublic : MarshalByRefObject
            {
                public MarkAsNotSerializableNonPublic()
                {
                    // Ensure System.Web is loaded for this test
                    Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                    var attributes = BuildTestField(FieldAttributes.Assembly)
                        .GetCustomAttributes(inherit: false)
                        .Select(a => a.GetType().Name)
                        .ToList();

                    Assert.Contains("NonSerializedAttribute", attributes);
                    Assert.DoesNotContain("IgnoreDataMemberAttribute", attributes);
                    Assert.DoesNotContain("XmlIgnoreAttribute", attributes);
                    Assert.DoesNotContain("ScriptIgnoreAttribute", attributes);
                }
            }

            private static FieldInfo BuildTestField(FieldAttributes fieldAttributes)
            {
                var name = Guid.NewGuid().ToString();
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(name), AssemblyBuilderAccess.Run).DefineDynamicModule(name);

                var typeBuilder = assemblyBuilder.DefineType("Elbow");
                var fieldBuilder = typeBuilder.DefineField("_knee", typeof(object), fieldAttributes);

                EntityProxyFactory.ProxyTypeBuilder.MarkAsNotSerializable(fieldBuilder);

                return typeBuilder.CreateType().GetRuntimeFields().Single(f => f.Name == "_knee");
            }
        }
#endif

        public class TryGetAssociationTypeFromProxyInfo : TestBase
        {
            [Fact]
            public void TryGetAssociationTypeFromProxyInfo_can_get_association_using_simple_name()
            {
                using (var context = new RelationshipsContext())
                {
                    var proxy = context.WithRelationships.Create();

                    var mockWrapper = new Mock<IEntityWrapper>();
                    mockWrapper.Setup(m => m.Entity).Returns(proxy);

                    AssociationType associationType;
                    Assert.True(EntityProxyFactory.TryGetAssociationTypeFromProxyInfo(
                        mockWrapper.Object, "ProxyWithRelationships_OneToOne", out associationType));
                    Assert.Equal("System.Data.Entity.Core.Objects.Internal.ProxyWithRelationships_OneToOne", associationType.FullName);
                }
            }

            [Fact]
            public void TryGetAssociationTypeFromProxyInfo_can_get_association_using_full_name()
            {
                using (var context = new RelationshipsContext())
                {
                    var proxy = context.WithRelationships.Create();

                    var mockWrapper = new Mock<IEntityWrapper>();
                    mockWrapper.Setup(m => m.Entity).Returns(proxy);

                    AssociationType associationType;
                    Assert.True(EntityProxyFactory.TryGetAssociationTypeFromProxyInfo(
                        mockWrapper.Object, "System.Data.Entity.Core.Objects.Internal.ProxyWithRelationships_ManyToOnes", out associationType));
                    Assert.Equal("System.Data.Entity.Core.Objects.Internal.ProxyWithRelationships_ManyToOnes", associationType.FullName);
                }
            }
        }

        public class TryGetAllAssociationTypesFromProxyInfo : TestBase
        {
            [Fact]
            public void TryGetAllAssociationTypesFromProxyInfo_returns_all_associations_with_no_duplicates()
            {
                using (var context = new RelationshipsContext())
                {
                    var proxy = context.WithRelationships.Create();

                    var mockWrapper = new Mock<IEntityWrapper>();
                    mockWrapper.Setup(m => m.Entity).Returns(proxy);

                    var associations = EntityProxyFactory.TryGetAllAssociationTypesFromProxyInfo(mockWrapper.Object).Select(a => a.Name);

                    Assert.Equal(3, associations.Count());
                    Assert.Contains("ProxyWithRelationships_OneToOne", associations);
                    Assert.Contains("ProxyOneToMany_WithRelationships", associations);
                    Assert.Contains("ProxyWithRelationships_ManyToOnes", associations);
                }
            }
        }

        public class RelationshipsContext : DbContext
        {
            static RelationshipsContext()
            {
                Database.SetInitializer<RelationshipsContext>(null);
            }

            public DbSet<ProxyWithRelationships> WithRelationships { get; set; }
            public DbSet<ProxyOneToOne> OneToOnes { get; set; }
            public DbSet<ProxyOneToMany> OneToManys { get; set; }
            public DbSet<ProxyManyToOne> ManyToOnes { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ProxyWithRelationships>()
                            .HasRequired(e => e.OneToOne)
                            .WithRequiredPrincipal(e => e.WithRelationships);
            }
        }

        [Fact] // CodePlex 997
        public void It_is_not_erroneously_assumed_that_internal_nested_classes_can_be_proxied()
        {
            using (var context = new Context997())
            {
                // IsType checks exact type; will fail if types are proxies
                Assert.IsType<NestingSiteA.Product997A>(context.ProductAs.Create());
                Assert.IsType<NestingSiteB.NestingSiteB2.Product997B>(context.ProductBs.Create());
            }
        }

        internal class Context997 : DbContext
        {
            static Context997()
            {
                Database.SetInitializer<Context997>(null);
            }

            public virtual DbSet<NestingSiteA.Product997A> ProductAs { get; set; }
            public virtual DbSet<NestingSiteB.NestingSiteB2.Product997B> ProductBs { get; set; }
        }

        internal class NestingSiteA
        {
            // Public inside internal inside public; change-tracking proxy
            public class Product997A
            {
                public virtual int Id { get; set; }
                public virtual ICollection<Product997A> Products { get; set; }
            }
        }
    }

    internal class NestingSiteB
    {
        public class NestingSiteB2
        {
            // Public inside public inside internal; lazy-loading proxy
            public class Product997B
            {
                public int Id { get; set; }
                public virtual ICollection<Product997B> Products { get; set; }
            }
        }
    }

    public class ProxyWithRelationships
    {
        public int Id { get; set; }
        public virtual ProxyOneToOne OneToOne { get; set; }
        public virtual ProxyOneToMany OneToMany { get; set; }
        public virtual ICollection<ProxyManyToOne> ManyToOnes { get; set; }
    }

    public class ProxyOneToOne
    {
        public int Id { get; set; }
        public virtual ProxyWithRelationships WithRelationships { get; set; }
    }

    public class ProxyOneToMany
    {
        public int Id { get; set; }
        public virtual ICollection<ProxyWithRelationships> WithRelationships { get; set; }
    }

    public class ProxyManyToOne
    {
        public int Id { get; set; }
        public virtual ProxyWithRelationships WithRelationships { get; set; }
    }
}
