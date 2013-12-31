// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class PrecisionDateTimeDiscovererTests
    {
        [Fact]
        public void Discover_returns_null_when_inapplicable()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "Id");

            Assert.Null(new PrecisionDateTimeDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_null_when_conventional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "DateCreated");

            Assert.Null(new PrecisionDateTimeDiscoverer().Discover(property, model));
        }

        [Fact]
        public void Discover_returns_configuration()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.DateCreated).HasPrecision(2);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First();
            var property = entityType.Properties.First(p => p.Name == "DateCreated");

            var configuration = new PrecisionDateTimeDiscoverer()
                .Discover(property, model) as PrecisionDateTimeConfiguration;

            Assert.NotNull(configuration);
            Assert.Equal(2, configuration.Precision);
        }

        private class Entity
        {
            public int Id { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateCreated { get; set; }
        }
    }
}
