// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class DatabaseGeneratedDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_identity_int_key()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            Assert.Null(new DatabaseGeneratedDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_timestamp()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Timestamp");

            Assert.Null(new DatabaseGeneratedDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_none_nonkey()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            Assert.Null(new DatabaseGeneratedDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_nonidentity_int_key()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            var configuration = new DatabaseGeneratedDiscoverer()
                .Discover(property, model) as DatabaseGeneratedConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(StoreGeneratedPattern.None, configuration.StoreGeneratedPattern);
        }

        [Fact]
        public void Discover_returns_configuration_when_computed()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Name).HasDatabaseGeneratedOption(
                DatabaseGeneratedOption.Computed);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            var configuration = new DatabaseGeneratedDiscoverer()
                .Discover(property, model) as DatabaseGeneratedConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(StoreGeneratedPattern.Computed, configuration.StoreGeneratedPattern);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [Timestamp]
            public byte[] Timestamp { get; set; }
        }
    }
}
