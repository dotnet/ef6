// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using Xunit;

    /// <summary>
    /// Tests for complex types.
    /// </summary>
    public class ComplexTypeTests : FunctionalTestBase
    {
        #region Complex type discovery

        public class ContextWithSimpleComplexType : DbContext
        {
            public DbSet<EntityWithComplexType> Entities1 { get; set; }
        }

        public class EntityWithComplexType
        {
            public int Id { get; set; }
            public SimpleComplexType ComplexProp { get; set; }
        }

        public class SimpleComplexType
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
        }

        [Fact]
        public void Single_complex_type_is_discovered_by_convention()
        {
            Database.SetInitializer<ContextWithSimpleComplexType>(null);

            using (var context = new ContextWithSimpleComplexType())
            {
                context.Assert<SimpleComplexType>().IsComplexType();
            }
        }

        public class ContextWithSimpleRelatedEntities : DbContext
        {
            public DbSet<EntityWithRelatedEntity> Entities1 { get; set; }
        }

        public class EntityWithRelatedEntity
        {
            public int Id { get; set; }
            public SimpleRelatedEntity RelatedEntity { get; set; }
        }

        public class SimpleRelatedEntity
        {
            public int Id { get; set; }
            public ICollection<EntityWithRelatedEntity> RelatedEntities { get; set; }
        }

        [Fact]
        public void Single_related_entity_type_is_discovered_by_convention()
        {
            Database.SetInitializer<ContextWithSimpleRelatedEntities>(null);

            using (var context = new ContextWithSimpleRelatedEntities())
            {
                context.Assert<SimpleRelatedEntity>().IsInModel();
            }
        }

        public class ContextWithComplexTypeTwiceOnSameEntity : DbContext
        {
            public DbSet<EntityWithComplexTypeTwice> Entities1 { get; set; }
        }

        public class EntityWithComplexTypeTwice
        {
            public int Id { get; set; }
            public SimpleComplexType ComplexProp1 { get; set; }
            public SimpleComplexType ComplexProp2 { get; set; }
        }

        [Fact]
        public void Complex_type_reused_on_same_entity_is_discovered_by_convention()
        {
            Database.SetInitializer<ContextWithComplexTypeTwiceOnSameEntity>(null);

            using (var context = new ContextWithComplexTypeTwiceOnSameEntity())
            {
                context.Assert<SimpleComplexType>().IsComplexType();
            }
        }

        public class ContextWithSameComplexTypeOnDifferentEntities : DbContext
        {
            public DbSet<EntityWithComplexType> Entities1 { get; set; }
            public DbSet<AnotherEntityWithComplexType> Entities2 { get; set; }
        }

        public class DerivedFromEntityWithComplexType : EntityWithComplexType
        {
        }

        public class AnotherEntityWithComplexType
        {
            public int Id { get; set; }
            public SimpleComplexType ComplexProp { get; set; }
        }

        [Fact]
        public void Complex_type_reused_on_different_entities_is_discovered_by_convention()
        {
            Database.SetInitializer<ContextWithSameComplexTypeOnDifferentEntities>(null);

            using (var context = new ContextWithSameComplexTypeOnDifferentEntities())
            {
                context.Assert<SimpleComplexType>().IsComplexType();
            }
        }

        public class ContextWithComplexTypeAndTpt : DbContext
        {
            public DbSet<EntityWithComplexType> Entities { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<EntityWithComplexType>().ToTable("Table1");
                modelBuilder.Entity<DerivedFromEntityWithComplexType>().ToTable("Table2");
            }
        }

        [Fact]
        public void Complex_type_on_base_class_with_derived_class_using_TPT_works()
        {
            Database.SetInitializer<ContextWithComplexTypeAndTpt>(null);

            using (var context = new ContextWithComplexTypeAndTpt())
            {
                context.Assert<SimpleComplexType>().IsComplexType();
            }
        }

        #endregion
    }
}
