// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class CascadeDeleteDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithMany(e => e.Entity1s);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_many_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithRequired(e => e.Entity1);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_self_referencing_many_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<SelfReferencingEntity>().HasMany(e => e.Children).WithRequired(e => e.Parent);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "SelfReferencingEntity");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Children");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_many_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithOptional(e => e.Entity1);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_required_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithMany(e => e.Entity1s);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_optional_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithMany(e => e.Entity1s);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_required_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithOptional(e => e.Entity1);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_optional_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithRequired(e => e.Entity1);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            Assert.Null(new CascadeDeleteDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_many_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithRequired(e => e.Entity1).WillCascadeOnDelete(
                false);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.None, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_self_referencing_many_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<SelfReferencingEntity>().HasMany(e => e.Children).WithRequired(e => e.Parent)
                .WillCascadeOnDelete();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "SelfReferencingEntity");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Children");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_many_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithOptional(e => e.Entity1).WillCascadeOnDelete();
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_required_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithMany(e => e.Entity1s).WillCascadeOnDelete(
                false);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.None, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_optional_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithMany(e => e.Entity1s).WillCascadeOnDelete();
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_convention_when_unconventional_required_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithOptional(e => e.Entity1)
                .WillCascadeOnDelete();
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteBehavior);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_optional_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithRequired(e => e.Entity1)
                .WillCascadeOnDelete();
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2s);
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(e => e.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");

            var configuration = new CascadeDeleteDiscoverer()
                .Discover(navigationProperty, model) as CascadeDeleteConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteBehavior);
        }

        private class Entity1
        {
            public int Id { get; set; }
            public Entity2 Entity2 { get; set; }
            public ICollection<Entity2> Entity2s { get; set; }
        }

        private class Entity2
        {
            public int Id { get; set; }
            public Entity1 Entity1 { get; set; }
            public ICollection<Entity1> Entity1s { get; set; }
        }

        private class SelfReferencingEntity
        {
            public int Id { get; set; }
            public SelfReferencingEntity Parent { get; set; }
            public ICollection<SelfReferencingEntity> Children { get; set; }
        }
    }
}
