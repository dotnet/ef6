// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public sealed class ConventionsScenarioTests : TestBase
    {
        [Fact]
        public void Add_custom_model_convention()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Conventions.Add<DbaTableNamingConvention>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(
                1,
                databaseMapping.Database.Schemas.Single().Tables.Count(
                    t => t.DatabaseIdentifier == "Customers_tbl"));
        }

        private sealed class DbaTableNamingConvention : IDbConvention<DbTableMetadata>
        {
            public void Apply(DbTableMetadata table, DbDatabaseMetadata database)
            {
                table.DatabaseIdentifier = table.DatabaseIdentifier + "_tbl";
            }
        }

        [Fact]
        public void Add_custom_model_convention_with_ordering()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<CountryRegion>();
            modelBuilder.Conventions.AddAfter<IdKeyDiscoveryConvention>(new CodeKeyDiscoveryConvention());

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        private sealed class CodeKeyDiscoveryConvention : KeyDiscoveryConvention
        {
            private const string Code = "Code";

            protected override EdmProperty MatchKeyProperty(
                EntityType entityType,
                IEnumerable<EdmProperty> primitiveProperties)
            {
                return primitiveProperties
                           .SingleOrDefault(p => Code.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                       ?? primitiveProperties
                              .SingleOrDefault(
                                  p => (entityType.Name + Code).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void Remove_an_existing_convention()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Conventions.Remove<IdKeyDiscoveryConvention>();

            Assert.Throws<ModelValidationException>(
                () => modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Add_lightweight_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Conventions.Add(
                entities => entities.Where(t => t == typeof(LightweightEntity)).Configure(e => e.ToTable("TheTable")));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.True(
                model.DatabaseMapping.Database.Schemas.Single().Tables.All(t => t.DatabaseIdentifier == "TheTable"));
        }

        [Fact]
        public void Lightweight_convention_does_not_override_explicit_configurations()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Configurations.Add(new LightweightEntityWithConfiguration.Configuration());
            modelBuilder.Conventions.Add(
                entities => entities.Properties().Where(p => p.PropertyType == typeof(string))
                    .Configure(p => p.MaxLength = 256));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var table
                = model.DatabaseMapping.Database.Schemas.Single()
                    .Tables.Single(t => t.Name == "LightweightEntityWithConfiguration");

            var attributeColumn = table.Columns.Single(c => c.Name == "PropertyConfiguredByAttribute");
            Assert.Equal(64, attributeColumn.Facets.MaxLength);

            var fluentColumn = table.Columns.Single(c => c.Name == "PropertyConfiguredByFluent");
            Assert.Equal(128, fluentColumn.Facets.MaxLength);

            var unconfiguredColumn = table.Columns.Single(c => c.Name == "PropertyNotConfigured");
            Assert.Equal(256, unconfiguredColumn.Facets.MaxLength);
        }

        [Fact]
        public void Can_configure_complex_type_properties()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Conventions.Add(
                entities => entities.Properties().Where(p => p.PropertyType == typeof(string))
                    .Configure(p => p.MaxLength = 256));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var column = model.DatabaseMapping.Database.Schemas.Single()
                .Tables.Single()
                .Columns.Single(c => c.Name == "ComplexProperty_StringProperty");
            Assert.Equal(256, column.Facets.MaxLength);
        }
    }

    public class LightweightEntityWithConfiguration
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string PropertyConfiguredByAttribute { get; set; }
        public string PropertyConfiguredByFluent { get; set; }
        public string PropertyNotConfigured { get; set; }

        internal class Configuration : EntityTypeConfiguration<LightweightEntityWithConfiguration>
        {
            public Configuration()
            {
                Property(e => e.PropertyConfiguredByFluent).HasMaxLength(128);
            }
        }
    }

    public class LightweightComplexType
    {
        public string StringProperty { get; set; }
    }

    public class LightweightEntity
    {
        public int Id { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
    }
}
