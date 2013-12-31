// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class TableDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_conventional()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entitySet = model.ConceptualModel.Container.EntitySets.First();

            Assert.Null(new TableDiscoverer(code).Discover(entitySet, model));
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_name()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().ToTable("Entity");
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entitySet = model.ConceptualModel.Container.EntitySets.First();

            var configuration = new TableDiscoverer(code).Discover(entitySet, model) as TableConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal("Entity", configuration.Table);
            Assert.Null(configuration.Schema);
        }

        [Fact]
        public void Discover_returns_configuration_when_unconventional_schema()
        {
            var code = new CSharpCodeHelper();
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().ToTable("Entities", "old");
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entitySet = model.ConceptualModel.Container.EntitySets.First();

            var configuration = new TableDiscoverer(code).Discover(entitySet, model) as TableConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal("Entities", configuration.Table);
            Assert.Equal("old", configuration.Schema);
        }

        private class Entity
        {
            public int Id { get; set; }
        }
    }
}
