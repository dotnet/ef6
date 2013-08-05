// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EntityProxyFactoryTests : TestBase
    {
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
