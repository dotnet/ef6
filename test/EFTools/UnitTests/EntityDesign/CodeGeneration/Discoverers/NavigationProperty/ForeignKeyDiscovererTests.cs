// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class ForeignKeyDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            Assert.Null(new ForeignKeyDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_fk_equals_property_plus_pk()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.TwoEntity2Id);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            Assert.Null(new ForeignKeyDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_fk_equals_property_plus_pk_and_more_than_one_association()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.TwoEntity2Id);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            Assert.Null(new ForeignKeyDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_fk_equals_entity_plus_pk()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.Entity2Entity2Id);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            Assert.Null(new ForeignKeyDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_fk_equals_pk()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.Entity2Id);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            Assert.Null(new ForeignKeyDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_fk_equals_entity_plus_pk_but_more_than_one_association()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.Entity2Entity2Id);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            var configuration = new ForeignKeyDiscoverer()
                .Discover(navigationProperty, model) as ForeignKeyConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(new[] { "Entity2Entity2Id" }, configuration.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Discover_returns_configuration_when_fk_equals_pk_but_more_than_one_association()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(e => e.Entity2Id);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            var configuration = new ForeignKeyDiscoverer()
                .Discover(navigationProperty, model) as ForeignKeyConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(new[] { "Entity2Id" }, configuration.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Discover_returns_configuration_when_composite_key()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity2>().HasKey(e => new { e.Entity2Id, e.Name });
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Two).WithMany().HasForeignKey(
                e => new { e.Entity2Id, e.Name });
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Two");

            var configuration = new ForeignKeyDiscoverer()
                .Discover(navigationProperty, model) as ForeignKeyConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(new[] { "Entity2Id", "Name" }, configuration.Properties.Select(p => p.Name));
        }

        private class Entity1
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Entity2Id { get; set; }
            public int Entity2Entity2Id { get; set; }
            public int TwoEntity2Id { get; set; }
            public Entity2 Two { get; set; }
            public ICollection<Entity2> Entity2s { get; set; }
        }

        private class Entity2
        {
            public int Entity2Id { get; set; }
            public string Name { get; set; }
        }
    }
}
