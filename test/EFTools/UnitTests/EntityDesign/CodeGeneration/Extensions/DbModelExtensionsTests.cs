// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class DbModelExtensionsTests
    {
        [Fact]
        public void GetProviderManifest_resolves_manifest()
        {
            var modelBuilder = new DbModelBuilder();
            var providerInfo = new DbProviderInfo("System.Data.SqlClient", "2012");
            var model = modelBuilder.Build(providerInfo);

            var providerServices = model.GetProviderManifest(DbConfiguration.DependencyResolver);

            Assert.NotNull(providerServices);
            Assert.Equal("SqlServer", providerServices.NamespaceName);
        }

        [Fact]
        public void GetColumn_resolves_simple_property_mappings()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>();
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var property = model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Name");

            var column = model.GetColumn(property);

            Assert.NotNull(column);
            Assert.Equal("Name", column.Name);
        }

        [Fact]
        public void GetColumn_resolves_rename_property_mappings()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.Name).HasColumnName("Rename");
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var property = model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Name");

            var column = model.GetColumn(property);

            Assert.NotNull(column);
            Assert.Equal("Rename", column.Name);
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
