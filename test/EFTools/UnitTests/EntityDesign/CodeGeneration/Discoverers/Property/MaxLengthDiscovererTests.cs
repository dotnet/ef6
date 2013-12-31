// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class MaxLengthDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            Assert.Null(new MaxLengthDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_nonkey()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            Assert.Null(new MaxLengthDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional_key()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().HasKey(e => e.Name);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            Assert.Null(new MaxLengthDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_binary()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Data).HasMaxLength(256);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Data");

            var configuration = new MaxLengthDiscoverer().Discover(property, model) as MaxLengthConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(256, configuration.MaxLength);
        }

        [Fact]
        public void Discover_returns_configuration_when_string()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Name).HasMaxLength(30);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            var configuration = new MaxLengthDiscoverer().Discover(property, model) as MaxLengthStringConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(30, configuration.MaxLength);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }
    }
}
