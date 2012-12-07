// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
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
            modelBuilder.Conventions.Add<EntitySetNamingConvention>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(
                1,
                databaseMapping.Model.GetEntitySets().Count(
                    t => t.Name == "CustomersFoo"));
        }

        private sealed class EntitySetNamingConvention : IEdmConvention<EntitySet>
        {
            public void Apply(EntitySet entitySet, EdmModel model)
            {
                entitySet.Name = entitySet.Name + "Foo";
            }
        }

        [Fact]
        public void Add_custom_model_convention_with_ordering()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<CountryRegion>();
            modelBuilder.Conventions.AddAfter<IdKeyDiscoveryConvention>(new CodeKeyDiscoveryConvention());

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
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
            modelBuilder.Entities()
                .Where(t => t == typeof(LightweightEntity))
                .Configure(e => e.ToTable("TheTable"));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.True(
                model.DatabaseMapping.Database.GetEntitySets().All(t => t.Table == "TheTable"));
        }

        [Fact]
        public void Lightweight_convention_does_not_override_explicit_configurations()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Configurations.Add(new LightweightEntityWithConfiguration.Configuration());
            modelBuilder.Properties<string>()
                .Configure(p => p.HasMaxLength(256));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var table
                = model.DatabaseMapping.Database.GetEntityTypes()
                    .Single(t => t.Name == "LightweightEntityWithConfiguration");

            var attributeColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByAttribute");
            Assert.Equal(64, attributeColumn.MaxLength);

            var fluentColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByFluent");
            Assert.Equal(128, fluentColumn.MaxLength);

            var unconfiguredColumn = table.Properties.Single(c => c.Name == "PropertyNotConfigured");
            Assert.Equal(256, unconfiguredColumn.MaxLength);
        }

        [Fact]
        public void Can_configure_complex_type_properties()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Properties<string>()
                .Configure(p => p.HasMaxLength(256));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var column = model.DatabaseMapping.Database.GetEntityTypes()
                .Single()
                .Properties.Single(c => c.Name == "ComplexProperty_StringProperty");
            Assert.Equal(256, column.MaxLength);
        }

        [Fact]
        public void Lightweight_conventions_can_filter_by_interface()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Entities<ILightweightEntity>()
                .Configure(c => c.HasKey(e => e.IntProperty));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            Assert.Equal(1, entity.DeclaredKeyProperties.Count());
            Assert.Equal("IntProperty", entity.DeclaredKeyProperties.Single().Name);
        }

        [Fact]
        public void Lightweight_conventions_can_filter_by_object()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Entities<LightweightEntity>()
                .Configure(c => c.HasKey(e => e.IntProperty));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            Assert.Equal(1, entity.DeclaredKeyProperties.Count());
            Assert.Equal("IntProperty", entity.DeclaredKeyProperties.Single().Name);
        }

        [Fact]
        public void Single_key_configurable_with_isKey()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Properties()
                .Where(p => p.Name == "IntProperty")
                .Configure(c => c.IsKey());

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            Assert.Equal(1, entity.DeclaredKeyProperties.Count());
            Assert.Equal("IntProperty", entity.DeclaredKeyProperties.Single().Name);
        }

        [Fact]
        public void Composite_key_configurable_with_isKey()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Properties()
                .Where(p => p.Name == "IntProperty")
                .Configure(c => c.HasColumnOrder(0).IsKey());

            modelBuilder.Properties()
                .Where(p => p.Name == "IntProperty1")
                .Configure(c => c.HasColumnOrder(1).IsKey());

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            var keys = entity.DeclaredKeyProperties;
            Assert.Equal(2, keys.Count());
            Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            Assert.Equal("IntProperty1", keys.ElementAt(1).Name);
        }

        [Fact]
        public void Is_key_is_ignored_if_key_is_already_set()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntityWithKeyConfiguration>();
            modelBuilder.Properties()
                .Where(p => p.Name == "IntProperty1")
                .Configure(c => c.HasColumnOrder(1).IsKey());

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            var keys = entity.DeclaredKeyProperties;
            Assert.Equal(1, keys.Count());
            Assert.Equal("IntProperty", keys.ElementAt(0).Name);
        }

        [Fact]
        public void HasKey_can_build_composite_keys_filtered_by_interface()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Entities<ILightweightEntity>()
                .Configure(c => c.HasKey(e => new { e.IntProperty, e.IntProperty1 }));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            var keys = entity.DeclaredKeyProperties;
            Assert.Equal(2, keys.Count());
            Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            Assert.Equal("IntProperty1", keys.ElementAt(1).Name);
        }

        [Fact]
        public void HasKey_can_build_composite_keys_with_annotations()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntityWithKeyConfiguration>();
            modelBuilder.Entities<ILightweightEntity>()
                .Configure(c => c.HasKey(e => e.IntProperty));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            var keys = entity.DeclaredKeyProperties;
            Assert.Equal(2, keys.Count());
            Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            Assert.Equal("IntProperty1", keys.ElementAt(1).Name);
        }

        [Fact]
        public void Ignores_invalid_property_name()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<LightweightEntity>();
            modelBuilder.Properties()
                .Where(p => p.Name == "TypoProperty")
                .Configure(c => c.HasColumnOrder(1).IsKey());

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var entity = model.DatabaseMapping.Model.GetEntityTypes().Single();
            var keys = entity.DeclaredKeyProperties;
            Assert.Equal(1, keys.Count());
            Assert.Equal("Id", keys.First().Name);
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

    public interface ILightweightEntity
    {
        int IntProperty { get; set; }
        int IntProperty1 { get; set; }
    }

    public class LightweightEntity : ILightweightEntity
    {
        public int Id { get; set; }
        public int IntProperty { get; set; }
        public int IntProperty1 { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
    }

    public class LightweightEntityWithKeyConfiguration : ILightweightEntity
    {
        public int Id { get; set; }
        [Key, Column(Order = 0)]
        public int IntProperty { get; set; }
        public int IntProperty1 { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
    }
}
