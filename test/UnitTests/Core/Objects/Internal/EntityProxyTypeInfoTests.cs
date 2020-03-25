// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class EntityProxyTypeInfoTests : TestBase
    {
        [Fact]
        public void GetAllRelationshipsForType_returns_ref_type_relationships_with_navigation_properties()
        {
            using (var context = new RelationshipsContext())
            {
                var relationships = EntityProxyTypeInfo.GetAllRelationshipsForType(
                    ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace, typeof(WithNavProps))
                                                       .Select(r => r.Name);

                Assert.Equal(3, relationships.Count());
                Assert.Contains("WithNavProps_OneToOne", relationships);
                Assert.Contains("OneToMany_WithNavProps", relationships);
                Assert.Contains("WithNavProps_OneToManyIa", relationships);
            }
        }

        [Fact]
        public void GetAllRelationshipsForType_returns_collection_relationships_with_navigation_properties()
        {
            using (var context = new RelationshipsContext())
            {
                var relationships = EntityProxyTypeInfo.GetAllRelationshipsForType(
                    ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace, typeof(OneToMany))
                                                       .Select(r => r.Name);

                Assert.Equal("OneToMany_WithNavProps", relationships.Single());
            }
        }

        [Fact]
        public void GetAllRelationshipsForType_returns_IA_collection_relationships_with_navigation_properties()
        {
            using (var context = new RelationshipsContext())
            {
                var relationships = EntityProxyTypeInfo.GetAllRelationshipsForType(
                    ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace, typeof(OneToManyIa))
                                                       .Select(r => r.Name);

                Assert.Equal("WithNavProps_OneToManyIa", relationships.Single());
                Assert.Equal(1, relationships.Count());
            }
        }

        [Fact]
        public void GetAllRelationshipsForType_returns_relationships_without_navigation_properties()
        {
            using (var context = new RelationshipsContext())
            {
                var relationships = EntityProxyTypeInfo.GetAllRelationshipsForType(
                    ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace, typeof(NoNavProps))
                                                       .Select(r => r.Name);

                Assert.Equal(2, relationships.Count());
                Assert.Contains("NoNavPropsOneToOne_NoNavProps", relationships);
                Assert.Contains("NoNavPropsOneToMany_NoNavProps", relationships);
            }
        }

        public class RelationshipsContext : DbContext
        {
            static RelationshipsContext()
            {
                Database.SetInitializer<RelationshipsContext>(null);
            }

            public DbSet<WithNavProps> WithNavProps { get; set; }
            public DbSet<OneToOne> OneToOnes { get; set; }
            public DbSet<OneToMany> OneToManys { get; set; }
            public DbSet<OneToManyIa> OneToManyIas { get; set; }
            public DbSet<NoNavProps> NoNavProps { get; set; }
            public DbSet<NoNavPropsOneToOne> NoNavPropsOneToOnes { get; set; }
            public DbSet<NoNavPropsOneToMany> NoNavPropsOneToManys { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<WithNavProps>()
                            .HasRequired(e => e.OneToOne)
                            .WithRequiredPrincipal(e => e.WithNavProps);

                modelBuilder.Entity<NoNavPropsOneToOne>()
                            .HasRequired(e => e.NoNavProps)
                            .WithRequiredPrincipal();

                modelBuilder.Entity<NoNavPropsOneToMany>()
                            .HasRequired(e => e.NoNavProps)
                            .WithMany();
            }
        }
    }

    public class WithNavProps
    {
        public int Id { get; set; }
        public OneToOne OneToOne { get; set; }

        public int OneToManyId { get; set; }
        public OneToMany OneToMany { get; set; }

        public int OneToManyIaId { get; set; }
        public OneToManyIa OneToManyIa { get; set; }
    }

    public class OneToOne
    {
        public int Id { get; set; }
        public WithNavProps WithNavProps { get; set; }
    }

    public class OneToMany
    {
        public int Id { get; set; }
        public ICollection<WithNavProps> WithNavProps { get; set; }
    }

    public class OneToManyIa
    {
        public int Id { get; set; }
        public ICollection<WithNavProps> WithNavProps { get; set; }
    }

    public class NoNavProps
    {
        public int Id { get; set; }
    }

    public class NoNavPropsOneToOne
    {
        public int Id { get; set; }
        public NoNavProps NoNavProps { get; set; }
    }

    public class NoNavPropsOneToMany
    {
        public int Id { get; set; }

        public int NoNavPropsId { get; set; }
        public NoNavProps NoNavProps { get; set; }
    }
}
