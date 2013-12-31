// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class PrecisionDecimalDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            Assert.Null(new PrecisionDecimalDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Decimal");

            Assert.Null(new PrecisionDecimalDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_configuration()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Decimal).HasPrecision(9, 0);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Decimal");

            var configuration = new PrecisionDecimalDiscoverer()
                .Discover(property, model) as PrecisionDecimalConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(9, configuration.Precision);
            Assert.Equal(0, configuration.Scale);
        }

        private class Entity
        {
            public int Id { get; set; }
            public decimal Decimal { get; set; }
        }
    }
}
