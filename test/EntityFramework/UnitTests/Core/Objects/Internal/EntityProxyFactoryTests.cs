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
