// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class KeyDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_conventional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entitySet = model.ConceptualModel.Container.EntitySets.First();

            Assert.Null(new KeyDiscoverer().Discover(entitySet, model));
        }

        [Fact]
        public void Discover_returns_configuration()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().HasKey(e => e.Name);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entitySet = model.ConceptualModel.Container.EntitySets.First();

            var configuration = new KeyDiscoverer().Discover(entitySet, model) as KeyConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(1, configuration.KeyProperties.Count);
            Assert.Equal("Name", configuration.KeyProperties.First().Name);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
