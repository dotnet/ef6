// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class JoinTableDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithOptional();
            modelBuilder.Entity<Entity2>().Ignore(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            Assert.Null(new JoinTableDiscoverer().Discover(navigationProperty, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_conventional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithMany(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            // NOTE: This makes the model readonly. Without it, assertions fail
            model.Compile();

            var configuration = new JoinTableDiscoverer().Discover(navigationProperty, model) as JoinTableConfiguration;

            Assert.NotNull(configuration);
            Assert.Null(configuration.Schema);
            Assert.Equal("Entity1Entity2", configuration.Table);
            Assert.Empty(configuration.LeftKeys);
            Assert.Empty(configuration.RightKeys);
        }

        [Fact]
        public void Discover_returns_configuration_when_conventional_from_other_end()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithMany(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity1s");

            // NOTE: This makes the model readonly. Without it, assertions fail
            model.Compile();

            var configuration = new JoinTableDiscoverer().Discover(navigationProperty, model) as JoinTableConfiguration;

            Assert.NotNull(configuration);
            Assert.Null(configuration.Schema);
            Assert.Equal("Entity1Entity2", configuration.Table);
            Assert.Empty(configuration.LeftKeys);
            Assert.Empty(configuration.RightKeys);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithMany(e => e.Entity1s).Map(
                m => m.ToTable("Associations", "new").MapLeftKey("Entity1Id").MapRightKey("Entity2Id"));
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");

            // NOTE: This makes the model readonly. Without it, assertions fail
            model.Compile();

            var configuration = new JoinTableDiscoverer().Discover(navigationProperty, model) as JoinTableConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal("new", configuration.Schema);
            Assert.Equal("Associations", configuration.Table);
            Assert.Equal<string>(new[] { "Entity1Id" }, configuration.LeftKeys);
            Assert.Equal<string>(new[] { "Entity2Id" }, configuration.RightKeys);
        }

        private class Entity1
        {
            public int Id { get; set; }
            public ICollection<Entity2> Entity2s { get; set; }
        }

        private class Entity2
        {
            public int Id { get; set; }
            public ICollection<Entity1> Entity1s { get; set; }
        }
    }
}
