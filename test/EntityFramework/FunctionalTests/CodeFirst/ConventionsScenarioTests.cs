// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
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
    }
}
