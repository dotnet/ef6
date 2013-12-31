// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class ColumnDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_conventional()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            Assert.Null(new ColumnDiscoverer(code).Discover(property, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_name()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Id).HasColumnName("EntityId");
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            var configuration = new ColumnDiscoverer(code).Discover(property, model) as ColumnConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal("EntityId", configuration.Name);
            Assert.Null(configuration.TypeName);
            Assert.Null(configuration.Order);
        }

        [Fact]
        public void Discover_returns_configuration_when_type()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Name).HasColumnType("xml");
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Name");

            var configuration = new ColumnDiscoverer(code).Discover(property, model) as ColumnConfiguration;

            Assert.NotNull(configuration);
            Assert.Null(configuration.Name);
            Assert.Equal("xml", configuration.TypeName);
            Assert.Null(configuration.Order);
        }

        [Fact]
        public void Discover_returns_configuration_when_order()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().HasKey(e => new { e.Id, e.Name });
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            var configuration = new ColumnDiscoverer(code).Discover(property, model) as ColumnConfiguration;

            Assert.NotNull(configuration);
            Assert.Null(configuration.Name);
            Assert.Null(configuration.TypeName);
            Assert.Equal(0, configuration.Order);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
