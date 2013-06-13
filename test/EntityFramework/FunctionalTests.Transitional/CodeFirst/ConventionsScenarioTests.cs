// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public class ConventionsScenarioTests
    {
        public class ConventionTests : TestBase
        {
            [Fact]
            public void Add_custom_model_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<Customer>();
                modelBuilder.Conventions.Add<EntitySetNamingConvention>(DataSpace.CSpace);

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(
                    1,
                    databaseMapping.Model.GetEntitySets().Count(
                        t => t.Name == "CustomersFoo"));
            }

            private sealed class EntitySetNamingConvention : IModelConvention<EntitySet>
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
                modelBuilder.Conventions.AddAfter<IdKeyDiscoveryConvention>(DataSpace.CSpace, new CodeKeyDiscoveryConvention());

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            }

            private sealed class CodeKeyDiscoveryConvention : KeyDiscoveryConvention
            {
                private const string Code = "Code";

                protected override IEnumerable<EdmProperty> MatchKeyProperty(
                    EntityType entityType,
                    IEnumerable<EdmProperty> primitiveProperties)
                {
                    var codeProperties = primitiveProperties
                        .Where(p => Code.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                    return codeProperties.Any()
                               ? codeProperties
                               : primitiveProperties
                                     .Where(
                                         p => (entityType.Name + Code).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                }
            }

            [Fact]
            public void Remove_an_existing_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<Customer>();
                modelBuilder.Conventions.Remove<IdKeyDiscoveryConvention>(DataSpace.CSpace);

                Assert.Throws<ModelValidationException>(
                    () => BuildMapping(modelBuilder));
            }

            [Fact]
            public void Add_encapsulated_lightweight_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                var lightweightTableConvention = new Convention();
                lightweightTableConvention.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.ToTable("TheTable"));
                modelBuilder.Conventions.Add(lightweightTableConvention);

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.True(databaseMapping.Database.GetEntitySets().All(t => t.Table == "TheTable"));
            }

            [Fact]
            public void Remove_encapsulated_lightweight_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                var lightweightTableConvention = new Convention();
                lightweightTableConvention.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.ToTable("TheTable"));
                modelBuilder.Conventions.Add(lightweightTableConvention);
                modelBuilder.Conventions.Remove(lightweightTableConvention);

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.False(databaseMapping.Database.GetEntitySets().Any(t => t.Table == "TheTable"));
            }

            [Fact]
            public void Add_derived_encapsulated_lightweight_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Conventions.Add<LightweightTableConvention>();

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.True(databaseMapping.Database.GetEntitySets().All(t => t.Table == "TheTable"));
            }

            [Fact]
            public void Remove_derived_encapsulated_lightweight_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Conventions.Add<LightweightTableConvention>();
                modelBuilder.Conventions.Remove<LightweightTableConvention>();

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.False(databaseMapping.Database.GetEntitySets().Any(t => t.Table == "TheTable"));
            }

            private sealed class LightweightTableConvention : Convention
            {
                public LightweightTableConvention()
                {
                    Types()
                        .Where(t => t == typeof(LightweightEntity))
                        .Configure(e => e.ToTable("TheTable"));
                }
            }
        }

        public class LightweightTypeConventions : TestBase
        {
            [Fact]
            public void Can_configure_entity_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.ToTable("TheTable"));

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.True(databaseMapping.Database.GetEntitySets().All(t => t.Table == "TheTable"));
            }

            [Fact]
            public void Can_filter_by_interface()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types<ILightweightEntity>()
                    .Configure(c => c.HasKey(e => e.IntProperty));

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                Assert.Equal(1, entity.KeyProperties.Count());
                Assert.Equal("IntProperty", entity.KeyProperties.Single().Name);
            }

            [Fact]
            public void Can_filter_by_object()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types<LightweightEntity>()
                    .Configure(c => c.HasKey(e => e.IntProperty));

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                Assert.Equal(1, entity.KeyProperties.Count());
                Assert.Equal("IntProperty", entity.KeyProperties.Single().Name);
            }

            [Fact]
            public void HasKey_can_build_composite_keys_filtered_by_interface()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types<ILightweightEntity>()
                    .Configure(c => c.HasKey(e => new { e.IntProperty, e.IntProperty1 }));

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.First().Name);
                Assert.Equal("IntProperty1", keys.Last().Name);
            }

            [Fact]
            public void HasKey_ignores_rule_if_key_configured_with_annotations()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithKeyConfiguration>();
                modelBuilder.Types<ILightweightEntity>()
                    .Configure(c => c.HasKey(e => e.IntProperty));

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(1, keys.Count());
                Assert.Equal("IntProperty", keys.First().Name);
            }

            [Fact]
            public void Ignore_can_ignore_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightEntity>()
                    .Configure(c => c.Ignore());

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightComplexTypeWithId))
                    .Configure(c => c.Ignore());

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(1, databaseMapping.Model.EntityTypes.Count());
            }

            [Fact]
            public void Ignore_throws_on_conflicting_configuration()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(
                        c =>
                            {
                                c.HasKey(e => e.Id);
                                c.Ignore();
                            });

                Assert.Equal(
                    Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                        .Message, Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType);
            }

            [Fact]
            public void IsComplexType_can_configure_as_complex_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(c => c.IsComplexType());

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(2, databaseMapping.Model.EntityTypes.Count());
                Assert.Equal(2, databaseMapping.Model.ComplexTypes.Count());
            }

            [Fact]
            public void IsComplexType_can_configure_complex_type_and_its_properties()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(
                        c =>
                            {
                                c.IsComplexType();
                                c.Ignore(l => l.Id);
                                c.Property(l => l.StringProperty).HasColumnName("foo");
                            });

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(
                    new[] { "Id", "foo", typeof(LightweightEntity).Name + "_Id" },
                    databaseMapping.Database.EntityTypes.Single(t => t.Name == typeof(RelatedLightweightEntity).Name)
                        .Properties.Select(p => p.Name));
            }

            [Fact]
            public void Can_configure_complex_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.ComplexType<LightweightComplexTypeWithId>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(
                        c =>
                            {
                                c.Ignore(l => l.Id);
                                c.Property(l => l.StringProperty).HasColumnName("foo");
                                // This will be ignored
                                c.HasKey(l => l.StringProperty);
                            });

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(
                    new[] { "Id", "foo", typeof(LightweightEntity).Name + "_Id" },
                    databaseMapping.Database.EntityTypes.Single(t => t.Name == typeof(RelatedLightweightEntity).Name)
                        .Properties.Select(p => p.Name));
                Assert.Equal(
                    new[] { "StringProperty" },
                    databaseMapping.Model.ComplexTypes.Single(t => t.Name == typeof(LightweightComplexTypeWithId).Name)
                        .Properties.Select(p => p.Name));
            }

            [Fact]
            public void Implicitly_convert_complex_types_to_entity_types_if_entity_specific_configuration_is_performed()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types()
                    .Configure(e => e.HasKey("StringProperty"));

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(2, databaseMapping.Model.EntityTypes.Count());
                Assert.True(databaseMapping.Model.EntityTypes.All(e => e.KeyProperties.Any(p => p.Name == "StringProperty")));
                Assert.Equal(0, databaseMapping.Model.ComplexTypes.Count());
            }

            [Fact]
            public void Does_not_implicitly_convert_complex_types_to_entity_types_if_only_structural_type_configuration_is_performed()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types()
                    .Configure(e => e.Property("StringProperty"));

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(1, databaseMapping.Model.EntityTypes.Count());
                Assert.Equal(1, databaseMapping.Model.ComplexTypes.Count());
            }

            [Fact]
            public void Throws_on_conflicting_complex_type_configuration()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(
                        c =>
                            {
                                c.HasKey(e => e.Id);
                                c.IsComplexType();
                            });

                Assert.Equal(
                    Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                        .Message, Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType);
            }

            [Fact]
            public void NavigationProperty_throws_for_nonexisting_properties()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types()
                    .Where(t => t.Name == "LightweightEntity")
                    .Configure(c => c.NavigationProperty("Foo"));

                Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                    .ValidateMessage("NoSuchProperty", "Foo", typeof(LightweightEntity).FullName);
            }

            [Fact]
            public void NavigationProperty_throws_for_scalar_properties()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightEntity>()
                    .Configure(c => c.NavigationProperty(e => e.IntProperty));

                Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                    .ValidateMessage("LightweightEntityConfiguration_InvalidNavigationProperty", "IntProperty");
            }

            [Fact]
            public void Property_can_configure_internal_properties()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Types<LightweightEntity>()
                    .Configure(e => e.Property(t => t.InternalNavigationPropertyId));

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.NotNull(databaseMapping.Model.EntityTypes.Single().Properties.Single(p => p.Name == "InternalNavigationPropertyId"));
            }
        }

        public class LightweightPropertyConventions : TestBase
        {
            [Fact]
            public void Does_not_override_explicit_configurations()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Configurations.Add(new LightweightEntityWithConfiguration.Configuration());
                modelBuilder.Properties<string>()
                    .Configure(p => p.HasMaxLength(256));

                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
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

                var databaseMapping = BuildMapping(modelBuilder);

                var column = databaseMapping.Database.EntityTypes
                    .Single()
                    .Properties.Single(c => c.Name == "ComplexProperty_StringProperty");
                Assert.Equal(256, column.MaxLength);
            }

            [Fact]
            public void Single_key_configurable_with_isKey()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Properties()
                    .Where(p => p.Name == "IntProperty")
                    .Configure(c => c.IsKey());

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                Assert.Equal(1, entity.KeyProperties.Count());
                Assert.Equal("IntProperty", entity.KeyProperties.Single().Name);
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

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.ElementAt(0).Name);
                Assert.Equal("IntProperty1", keys.ElementAt(1).Name);
            }

            [Fact]
            public void Is_key_is_ignored_if_key_is_already_set()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithKeyConfiguration>().HasKey(e => e.IntProperty);
                modelBuilder.Properties()
                    .Where(p => p.Name == "IntProperty1")
                    .Configure(c => c.HasColumnOrder(1).IsKey());

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(1, keys.Count());
                Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            }

            [Fact]
            public void Is_key_adds_key_if_key_attribute_present()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithKeyConfiguration>();
                modelBuilder.Properties()
                    .Where(p => p.Name == "IntProperty1")
                    .Configure(c => c.HasColumnOrder(1).IsKey());

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            }
        }
    }

    #region Test model

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

    public class LightweightComplexTypeWithId
    {
        public int Id { get; set; }
        public string StringProperty { get; set; }
    }

    public interface ILightweightEntity
    {
        int IntProperty { get; set; }
        int IntProperty1 { get; set; }
    }

    public class RelatedLightweightEntity
    {
        public int Id { get; set; }
        public LightweightEntity LightweightEntity { get; set; }
        public LightweightComplexTypeWithId LightweightComplexTypeWithId { get; set; }
    }

    public class LightweightEntity : ILightweightEntity
    {
        public int Id { get; set; }
        public int IntProperty { get; set; }
        public int IntProperty1 { get; set; }
        internal int? InternalNavigationPropertyId { get; set; }
        internal LightweightEntityWithKeyConfiguration InternalNavigationProperty { get; set; }
        internal ICollection<LightweightEntity> InternalCollectionNavigationProperty { get; set; }
        public string StringProperty { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
    }

    public class LightweightEntityWithKeyConfiguration : ILightweightEntity
    {
        public int Id { get; set; }

        [Key]
        [Column(Order = 0)]
        public int IntProperty { get; set; }

        public int IntProperty1 { get; set; }
        internal ICollection<LightweightEntity> InternalCollectionNavigationProperty { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
    }

    #endregion
}
