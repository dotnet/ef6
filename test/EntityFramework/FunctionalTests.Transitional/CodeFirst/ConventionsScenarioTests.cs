// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;
    using System.Data.Entity.Infrastructure;

    public class ConventionsScenarioTests
    {
        public class ConventionTests : TestBase
        {
            internal abstract class BaseEntity
            {
                public int ID { get; set; }
                public DateTime Created { get; set; }
                public DateTime LastModified { get; set; }
            }

            internal class DerivedEntity : BaseEntity
            {
                [StringLength(128)]
                public string Name { get; set; }
            }

            [Fact]
            public void Can_override_precision_via_api_base()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Properties<DateTime>().Configure(
                    dateTimeConfig =>
                        {
                            dateTimeConfig.HasColumnType("datetime2");
                            dateTimeConfig.HasPrecision(1);
                        });

                modelBuilder.Entity<DerivedEntity>().Map(m => m.MapInheritedProperties());
                modelBuilder.Entity<BaseEntity>().Property(e => e.LastModified).HasPrecision(3);

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
                databaseMapping.Assert<DerivedEntity>(e => e.LastModified).FacetEqual((byte)3, p => p.Precision);
            }

            [Fact]
            public void Can_override_precision_via_api_derived()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Properties<DateTime>().Configure(
                    dateTimeConfig =>
                    {
                        dateTimeConfig.HasColumnType("datetime2");
                        dateTimeConfig.HasPrecision(1);
                    });

                modelBuilder.Entity<DerivedEntity>().Map(m => m.MapInheritedProperties());
                modelBuilder.Entity<DerivedEntity>().Property(e => e.LastModified).HasPrecision(3);
                
                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();
                databaseMapping.Assert<DerivedEntity>(e => e.LastModified).FacetEqual((byte)3, p => p.Precision);
            }

            [Fact]
            public void Add_custom_model_convention()
            {
                var modelBuilder = new AdventureWorksModelBuilder();

                modelBuilder.Entity<Customer>();
                modelBuilder.Conventions.Add<EntitySetNamingConvention>();

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(
                    1,
                    databaseMapping.Model.GetEntitySets().Count(
                        t => t.Name == "CustomersFoo"));
            }

            private sealed class EntitySetNamingConvention : IConceptualModelConvention<EntitySet>
            {
                public void Apply(EntitySet entitySet, DbModel model)
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
                modelBuilder.Conventions.Remove<IdKeyDiscoveryConvention>();

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

            [Fact]
            public void Encapsulated_lightweight_convention_does_not_override_annotations()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Configurations.Add(new LightweightEntityWithConfiguration.Configuration());
                var convention = new Convention();
                convention.Properties<string>()
                    .Configure(p => p.HasMaxLength(256));
                modelBuilder.Conventions.Add(convention);

                var databaseMapping = BuildMapping(modelBuilder);

                var table = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithConfiguration");

                var attributeColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByAttribute");
                Assert.Equal(64, attributeColumn.MaxLength);

                var fluentColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByFluent");
                Assert.Equal(128, fluentColumn.MaxLength);

                var unconfiguredColumn = table.Properties.Single(c => c.Name == "PropertyNotConfigured");
                Assert.Equal(256, unconfiguredColumn.MaxLength);
            }

            [Fact]
            public void Encapsulated_lightweight_convention_overrides_annotations_if_added_after()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Configurations.Add(new LightweightEntityWithConfiguration.Configuration());
                var convention = new Convention();
                convention.Properties<string>()
                    .Configure(p => p.HasMaxLength(256));
                modelBuilder.Conventions.AddAfter<StringLengthAttributeConvention>(convention);

                var databaseMapping = BuildMapping(modelBuilder);

                var table = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithConfiguration");

                var attributeColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByAttribute");
                Assert.Equal(256, attributeColumn.MaxLength);

                var fluentColumn = table.Properties.Single(c => c.Name == "PropertyConfiguredByFluent");
                Assert.Equal(128, fluentColumn.MaxLength);

                var unconfiguredColumn = table.Properties.Single(c => c.Name == "PropertyNotConfigured");
                Assert.Equal(256, unconfiguredColumn.MaxLength);
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

                var entity = databaseMapping.Model.EntityTypes.Single(e => e.Name == "LightweightEntity");
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

                var entity = databaseMapping.Model.EntityTypes.Single(e => e.Name == "LightweightEntity");
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

                var entity = databaseMapping.Model.EntityTypes.Single(e => e.Name == "LightweightEntity");
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.First().Name);
                Assert.Equal("IntProperty1", keys.Last().Name);
            }

            [Fact]
            public void HasKey_overrides_key_configured_with_annotations()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();
                modelBuilder.Types<ILightweightEntity>()
                    .Configure(c => c.HasKey(e => e.IntProperty1));

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(1, keys.Count());
                Assert.Equal("IntProperty1", keys.First().Name);
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
                    Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder)).Message,
                    Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType(
                        "HasKey", typeof(LightweightComplexTypeWithId).Name));
            }

            [Fact]
            public void IsComplexType_can_configure_as_complex_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(c => c.IsComplexType());

                var databaseMapping = BuildMapping(modelBuilder);

                Assert.Equal(3, databaseMapping.Model.EntityTypes.Count());
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

                Assert.Equal(3, databaseMapping.Model.EntityTypes.Count());
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

                Assert.Equal(2, databaseMapping.Model.EntityTypes.Count());
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
                        .Message, Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType(
                            "HasKey", typeof(LightweightComplexTypeWithId).Name));
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
                    .ValidateMessage("NoSuchProperty", "Foo", typeof(LightweightEntity).Name);
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

                databaseMapping.AssertValid();

                Assert.NotNull(databaseMapping.Model.EntityTypes.First().Properties.Single(p => p.Name == "InternalNavigationPropertyId"));
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_column_name_inheritance()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                modelBuilder.Types<LightweightDerivedEntity>().Configure(t => t.Property(e => e.StringProperty).HasColumnName("Bar"));
                modelBuilder.Types<LightweightEntity>().Configure(t => t.Property(e => e.StringProperty).HasColumnName("Foo"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<LightweightEntity>(e => e.StringProperty).DbEqual("Foo", c => c.Name);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_column_name_complex_type()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Types<LightweightEntity>().Configure(t => t.Property(e => e.ComplexProperty.StringProperty).HasColumnName("Foo"));
                modelBuilder.Entity<LightweightEntity>().Property(e => e.ComplexProperty.StringProperty).HasColumnName("Bar");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<LightweightComplexType>(c => c.StringProperty).DbEqual("Bar", c => c.Name);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_column_name_tpt()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightDerivedEntity>().ToTable("Derived");
                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(t => t.Property("StringProperty").HasColumnName("Foo"));
                modelBuilder.Entity<LightweightEntity>().Property(e => e.StringProperty).HasColumnName("Bar");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<LightweightEntity>(e => e.StringProperty).DbEqual("Bar", c => c.Name);
                databaseMapping.Assert<LightweightDerivedEntity>(e => e.StringProperty).DbEqual("Foo", c => c.Name);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_column_name()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(t => t.Property("StringProperty").HasColumnName("Foo"));
                modelBuilder.Entity<LightweightEntity>().Property(e => e.StringProperty).HasColumnName("Bar");

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                databaseMapping.Assert<LightweightEntity>(e => e.StringProperty).DbEqual("Bar", c => c.Name);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_IsUnicode()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(t => t.Property("StringProperty").IsUnicode(false));
                modelBuilder.Entity<LightweightEntity>().Property(e => e.StringProperty).IsUnicode();

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(
                    true,
                    storeModel.EntityTypes
                        .Single(e => e.Name == "LightweightEntity").Properties
                        .Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_precision()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>().Property(e => e.DecimalProperty).HasPrecision(5, 2);
                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(t => t.Property("DecimalProperty").HasPrecision(7, 4));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.True(storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties.Single(p => p.Name == "DecimalProperty").Precision == 5);
                Assert.True(storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties.Single(p => p.Name == "DecimalProperty").Scale == 2);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_base_types_when_generic()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>().Property(e => e.StringProperty).IsUnicode();
                modelBuilder.Types<LightweightEntity>().Configure(t => t.Property(e => e.StringProperty).IsUnicode(false));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(true,storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties.Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_derived_types()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightDerivedEntity>().Property(e => e.StringProperty).IsUnicode();
                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(t => t.Property("StringProperty").IsUnicode(false));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(true,storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties.Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_derived_types_when_generic()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightDerivedEntity>().Property(e => e.StringProperty).IsUnicode();
                modelBuilder.Types<LightweightEntity>().Configure(t => t.Property(e => e.StringProperty).IsUnicode(false));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(true,storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties.Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_derived_types_with_unmapped_base_type()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightDerivedEntity>().Property(e => e.StringProperty).IsUnicode();
                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(
                    t => t.Property("StringProperty").IsUnicode(false));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(
                    true,
                    storeModel.EntityTypes.Single(e => e.Name == "LightweightDerivedEntity").Properties
                    .Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_does_not_override_explicit_configurations_on_derived_types_with_unmapped_base_type_when_generic()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightDerivedEntity>().Property(e => e.StringProperty).IsUnicode();
                modelBuilder.Types<LightweightEntity>().Configure(
                    t => t.Property(e => e.StringProperty).IsUnicode(false));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(
                    true,
                    storeModel.EntityTypes.Single(e => e.Name == "LightweightDerivedEntity").Properties
                    .Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_IsUnicode_overrides_previous_convention()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(
                    t => t.Property("StringProperty").IsUnicode(false));

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(
                    t => t.Property("StringProperty").IsUnicode(true));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.Equal(
                    true,
                    storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties
                    .Single(p => p.Name == "StringProperty").IsUnicode);
            }

            [Fact]
            public void Property_HasColumnName_overrides_previous_convention()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(
                    t => t.Property("StringProperty").HasColumnName("foo"));

                modelBuilder.Types().Where(typeof(LightweightEntity).IsAssignableFrom).Configure(
                    t => t.Property("StringProperty").HasColumnName("bar"));

                var storeModel = BuildMapping(modelBuilder).Database;

                Assert.True(
                    storeModel.EntityTypes.Single(e => e.Name == "LightweightEntity").Properties
                    .Any(p => p.Name == "bar"));
            }

            [Fact]
            public void Can_configure_annotations_on_entity_types()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightEntityWithAnnotations))
                    .Configure(e => e.HasTableAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>("LightweightEntities")
                    .HasAnnotation("A1", "V1")
                    .HasNoAnnotation("A2");

                databaseMapping.Assert<LightweightEntityWithAnnotations>("LightweightEntityWithAnnotations")
                    .HasAnnotation("A2", "V2")
                    .HasNoAnnotation("A1");
            }

            [Fact]
            public void Can_configure_annotations_on_entity_types_using_generic_API()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Types<LightweightEntity>()
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                modelBuilder.Types<LightweightEntityWithAnnotations>()
                    .Configure(e => e.HasTableAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>("LightweightEntities")
                    .HasAnnotation("A1", "V1")
                    .HasNoAnnotation("A2");

                databaseMapping.Assert<LightweightEntityWithAnnotations>("LightweightEntityWithAnnotations")
                    .HasAnnotation("A2", "V2")
                    .HasNoAnnotation("A1");
            }

            [Fact]
            public void Annotation_on_entity_types_does_not_get_set_if_annotation_already_set()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>().HasTableAnnotation("A1", "V1A");
                modelBuilder.Entity<LightweightDerivedEntity>().HasTableAnnotation("A1", "V1A");
                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Types<LightweightEntity>()
                    .Configure(e => e.HasTableAnnotation("A1", "V1B").HasTableAnnotation("A2", "V2"));

                modelBuilder.Types<LightweightEntityWithAnnotations>()
                    .Configure(e => e.HasTableAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>("LightweightEntities")
                    .HasAnnotation("A1", "V1A")
                    .HasAnnotation("A2", "V2");

                databaseMapping.Assert<LightweightEntityWithAnnotations>("LightweightEntityWithAnnotations")
                    .HasAnnotation("A2", "V2")
                    .HasNoAnnotation("A1");
            }

            [Fact]
            public void Configure_annotations_on_complex_types_when_complex_type_is_not_configured_is_no_op()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.ComplexType<LightweightComplexTypeWithId>();

                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<RelatedLightweightEntity>("RelatedLightweightEntities")
                    .HasNoAnnotation("A1");
            }

            [Fact]
            public void Configure_annotations_on_configured_complex_types_throws()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.ComplexType<LightweightComplexTypeWithId>();

                modelBuilder.Types<LightweightComplexTypeWithId>()
                    .Configure(e => e.HasTableAnnotation("A1", "V1").IsComplexType());

                Assert.Equal(
                    Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                        .Message, Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType(
                            "HasTableAnnotation", typeof(LightweightComplexTypeWithId).Name));
            }

            [Fact]
            public void Ignore_throws_if_annotations_also_configured()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<RelatedLightweightEntity>();
                modelBuilder.Types<LightweightComplexTypeWithId>().Configure(c => c.Ignore().HasTableAnnotation("A1", "V1"));

                Assert.Equal(
                    Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder)).Message,
                    Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType(
                        "HasTableAnnotation", typeof(LightweightComplexTypeWithId).Name));
            }

            [Fact]
            public void Annotations_configured_on_multiple_types_are_unified()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightDerivedEntity>();

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightDerivedEntity))
                    .Configure(e => e.HasTableAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>("LightweightEntities")
                    .HasAnnotation("A1", "V1")
                    .HasAnnotation("A2", "V2");
            }

            [Fact]
            public void Conflicting_annotations_configured_on_multiple_types_throws()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Entity<LightweightDerivedEntity>();

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightEntity))
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                modelBuilder.Types()
                    .Where(t => t == typeof(LightweightDerivedEntity))
                    .Configure(e => e.HasTableAnnotation("A1", "V2"));

                Assert.Throws<InvalidOperationException>(
                     () => BuildMapping(modelBuilder))
                     .ValidateMessage("ConflictingTypeAnnotation", "A1", "V2", "V1", "LightweightEntity");
            }

            [Fact]
            public void Annotations_can_fan_out_to_multiple_tables()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<SplitMeGood>()
                    .Map(m => m.ToTable("Left").Properties(p => new { p.Id, p.Prop1 }))
                    .Map(m => m.ToTable("Right").Properties(p => new { p.Id, p.Prop2 }));

                modelBuilder.Types()
                    .Where(t => t == typeof(SplitMeGood))
                    .Configure(e => e.HasTableAnnotation("A1", "V1"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<SplitMeGood>("Left")
                    .HasAnnotation("A1", "V1");

                databaseMapping.Assert<SplitMeGood>("Right")
                    .HasAnnotation("A1", "V1");
            }
        }

        public class LightweightPropertyConventions : TestBase
        {
            [Fact]
            public void HasMaxLength_does_not_override_explicit_configurations()
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
            public void HasColumnName_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Properties().Where(p => p.Name == "IntProperty")
                    .Configure(p => p.HasColumnName("foo"));
                
                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithAnnotations");

                Assert.True(table.Properties.Any(p => p.Name == "PrimaryKey"));
                Assert.False(table.Properties.Any(p => p.Name == "IntProperty"));
                Assert.False(table.Properties.Any(p => p.Name == "foo"));
            }

            [Fact]
            public void HasColumnOrder_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();
                
                modelBuilder.Properties().Where(p => p.Name == "IntProperty1")
                    .Configure(p => p.HasColumnOrder(0));
                
                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithAnnotations");

                Assert.Equal("IntProperty1", table.Properties[1].Name);
            }

            [Fact]
            public void HasColumnType_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();
                
                modelBuilder.Properties().Where(p => p.Name == "IntProperty1")
                    .Configure(p => p.HasColumnType("bigint"));
                
                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithAnnotations");

                Assert.Equal("int", table.Properties.Single(c => c.Name == "IntProperty1").TypeName);
            }
            
            [Fact]
            public void HasDatabaseGeneratedOption_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Properties().Where(p => p.Name == "Id")
                    .Configure(p => p.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed));

                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithAnnotations");

                Assert.Equal(StoreGeneratedPattern.Identity, table.Properties.Single(c => c.Name == "Id").StoreGeneratedPattern);
            }

            [Fact]
            public void IsConcurrencyToken_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Properties().Where(p => p.Name == "Id")
                    .Configure(p => p.IsConcurrencyToken(false));

                var databaseMapping = BuildMapping(modelBuilder);

                var entityType
                    = databaseMapping.Model.EntityTypes.Single(e => e.Name == "LightweightEntityWithAnnotations");

                Assert.Equal(ConcurrencyMode.Fixed, entityType.Properties.Single(c => c.Name == "Id").ConcurrencyMode);
            }

            [Fact]
            public void IsFixedLength_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Properties().Where(p => p.Name == "StringProperty")
                    .Configure(p => p.IsFixedLength());

                var databaseMapping = BuildMapping(modelBuilder);

                var table
                    = databaseMapping.Database.EntityTypes
                        .Single(t => t.Name == "LightweightEntityWithAnnotations");

                Assert.Equal(false, table.Properties.Single(c => c.Name == "StringProperty").IsFixedLength);
            }
            
            [Fact]
            public void IsOptional_does_not_override_annotation()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();
                
                modelBuilder.Properties().Where(p => p.Name == "StringProperty")
                    .Configure(p => p.IsOptional());

                var databaseMapping = BuildMapping(modelBuilder);

                var entityType
                    = databaseMapping.Model.EntityTypes.Single(e => e.Name == "LightweightEntityWithAnnotations");

                Assert.Equal(false, entityType.Properties.Single(c => c.Name == "StringProperty").Nullable);
            }

            [Fact]
            public void IsUnicode_does_not_override_explicit_configuration()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>().Property(t => t.StringProperty).IsUnicode(true);

                modelBuilder.Properties().Where(p => p.Name == "StringProperty")
                    .Configure(p => p.IsUnicode(false));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntityWithAnnotations>(t => t.StringProperty).FacetEqual(true, p => p.IsUnicode);
            }

            [Fact]
            public void IsUnicode_works_with_explicit_MaxLength()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>().Property(t => t.StringProperty).HasMaxLength(42);

                modelBuilder.Properties().Where(p => p.Name == "StringProperty")
                    .Configure(p => p.IsUnicode(false));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>(t => t.StringProperty).FacetEqual(false, p => p.IsUnicode);
                databaseMapping.Assert<LightweightEntity>(t => t.StringProperty).FacetEqual(42, p => p.MaxLength);
                databaseMapping.Assert<LightweightEntity>(t => t.StringProperty).DbEqual(false, p => p.IsUnicode);
                databaseMapping.Assert<LightweightEntity>(t => t.StringProperty).DbEqual(42, p => p.MaxLength);
            }

            [Fact]
            public void IsUnicode_works_with_StringLengthAttribute()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>();

                modelBuilder.Properties().Where(p => p.Name == "StringProperty")
                    .Configure(p => p.IsUnicode(false));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntityWithAnnotations>(t => t.StringProperty).FacetEqual(false, p => p.IsUnicode);
                databaseMapping.Assert<LightweightEntityWithAnnotations>(t => t.StringProperty).FacetEqual(15, p => p.MaxLength);
                databaseMapping.Assert<LightweightEntityWithAnnotations>(t => t.StringProperty).DbEqual(false, p => p.IsUnicode);
                databaseMapping.Assert<LightweightEntityWithAnnotations>(t => t.StringProperty).DbEqual(15, p => p.MaxLength);
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

                var entity = databaseMapping.Model.EntityTypes.Single(c => c.Name == "LightweightEntity");
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

                var entity = databaseMapping.Model.EntityTypes.Single(c => c.Name == "LightweightEntity");
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.ElementAt(0).Name);
                Assert.Equal("IntProperty1", keys.ElementAt(1).Name);
            }

            [Fact]
            public void Is_key_is_ignored_if_key_is_already_set()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntityWithAnnotations>().HasKey(e => e.IntProperty);
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

                modelBuilder.Entity<LightweightEntityWithAnnotations>();
                modelBuilder.Properties()
                    .Where(p => p.Name == "IntProperty1")
                    .Configure(c => c.HasColumnOrder(1).IsKey());

                var databaseMapping = BuildMapping(modelBuilder);

                var entity = databaseMapping.Model.EntityTypes.Single();
                var keys = entity.KeyProperties;
                Assert.Equal(2, keys.Count());
                Assert.Equal("IntProperty", keys.ElementAt(0).Name);
            }

            [Fact]
            public void HasAnnotation_adds_column_annotations()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>();

                modelBuilder.Properties()
                    .Where(p => p.Name == "StringProperty")
                    .Configure(p => p.HasColumnAnnotation("A1", "V1"));

                modelBuilder.Properties()
                    .Where(p => p.Name.StartsWith("IntProperty"))
                    .Configure(p => p.HasColumnAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>()
                    .Column("StringProperty")
                    .HasAnnotation("A1", "V1")
                    .HasNoAnnotation("A2");

                databaseMapping.Assert<LightweightEntity>()
                    .Column("IntProperty")
                    .HasNoAnnotation("A1")
                    .HasAnnotation("A2", "V2");

                databaseMapping.Assert<LightweightEntity>()
                    .Column("IntProperty1")
                    .HasNoAnnotation("A1")
                    .HasAnnotation("A2", "V2");
            }

            [Fact]
            public void HasAnnotation_does_not_override_custom_model_annotations_set_outside_of_convention()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<LightweightEntity>()
                    .Property(e => e.IntProperty)
                    .HasColumnAnnotation("A1", "V1");

                modelBuilder.Properties()
                    .Where(p => p.Name.StartsWith("IntProperty"))
                    .Configure(p => p.HasColumnAnnotation("A1", "V1B").HasColumnAnnotation("A2", "V2"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>()
                    .Column("IntProperty")
                    .HasAnnotation("A1", "V1")
                    .HasAnnotation("A2", "V2");

                databaseMapping.Assert<LightweightEntity>()
                    .Column("IntProperty1")
                    .HasAnnotation("A1", "V1B")
                    .HasAnnotation("A2", "V2");
            }

            [Fact]
            public void Can_configure_annotations_on_complex_type_properties()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<LightweightEntity>();
                modelBuilder.Properties<string>()
                    .Configure(p => p.HasColumnAnnotation("A1", "V1"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<LightweightEntity>()
                    .Column("StringProperty")
                    .HasAnnotation("A1", "V1");

                databaseMapping.Assert<LightweightEntity>()
                    .Column("ComplexProperty_StringProperty")
                    .HasAnnotation("A1", "V1");
            }

            [Fact]
            public void Annotations_configured_on_multiple_properties_are_unified_to_same_column()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<JoinMeGood>();
                modelBuilder.Entity<JoinMeMore>();

                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop1")
                    .Configure(p => p.HasColumnName("MyCatHasPaws").HasColumnAnnotation("A1", "V1").HasColumnAnnotation("A2", "V2"));

                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop2")
                    .Configure(p => p.HasColumnName("MyCatHasPaws").HasColumnAnnotation("A1", "V1").HasColumnAnnotation("A3", "V3"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<JoinMeGood>("JoinMeGoods")
                    .Column("MyCatHasPaws")
                    .HasAnnotation("A1", "V1")
                    .HasAnnotation("A2", "V2")
                    .HasAnnotation("A3", "V3");
            }

            [Fact]
            public void Conflicting_annotations_configured_on_multiple_properties_throws()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<JoinMeGood>();
                modelBuilder.Entity<JoinMeMore>();

                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop1")
                    .Configure(p => p.HasColumnName("MyCatHasPaws").HasColumnAnnotation("A1", "V1A"));

                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop2")
                    .Configure(p => p.HasColumnName("MyCatHasPaws").HasColumnAnnotation("A1", "V1B"));

                var details = Environment.NewLine + "\t" +
                              string.Format(
                                  LookupString(
                                      EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "ConflictingAnnotationValue"),
                                  "A1", "V1A", "V1B");

                Assert.Throws<MappingException>(() => BuildMapping(modelBuilder))
                    .ValidateMessage(
                        "BadTphMappingToSharedColumn", "Prop1", "JoinMeGood", "Prop2", "JoinMeMore", "MyCatHasPaws", "JoinMeGood", details);
            }

            [Fact]
            public void Annotations_can_fan_out_to_multiple_columns()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<SplitMeGood>()
                    .Map(m => m.ToTable("Left").Properties(p => new { p.Id, p.Prop1 }))
                    .Map(m => m.ToTable("Right").Properties(p => new { p.Id, p.Prop2 }));

                modelBuilder.Properties()
                    .Where(p => p.Name == "Id")
                    .Configure(p => p.HasColumnAnnotation("A1", "V1"));

                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop1")
                    .Configure(p => p.HasColumnAnnotation("A2", "V2"));
                
                modelBuilder.Properties()
                    .Where(p => p.Name == "Prop2")
                    .Configure(p => p.HasColumnAnnotation("A3", "V3"));

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<SplitMeGood>("Left")
                    .Column("Id")
                    .HasAnnotation("A1", "V1")
                    .HasNoAnnotation("A2")
                    .HasNoAnnotation("A3");

                databaseMapping.Assert<SplitMeGood>("Left")
                    .Column("Prop1")
                    .HasAnnotation("A2", "V2")
                    .HasNoAnnotation("A1")
                    .HasNoAnnotation("A3");

                databaseMapping.Assert<SplitMeGood>("Right")
                    .Column("Id")
                    .HasAnnotation("A1", "V1")
                    .HasNoAnnotation("A2")
                    .HasNoAnnotation("A3");

                databaseMapping.Assert<SplitMeGood>("Right")
                    .Column("Prop2")
                    .HasAnnotation("A3", "V3")
                    .HasNoAnnotation("A1")
                    .HasNoAnnotation("A2");
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
        internal LightweightEntityWithAnnotations InternalNavigationProperty { get; set; }
        internal ICollection<LightweightEntity> InternalCollectionNavigationProperty { get; set; }
        public string StringProperty { get; set; }
        public LightweightComplexType ComplexProperty { get; set; }
        public Decimal DecimalProperty { get; set; }
    }

    public class LightweightEntityWithAnnotations : ILightweightEntity
    {
        [ConcurrencyCheck]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        [Column("PrimaryKey", Order = 0)]
        public int IntProperty { get; set; }

        [Column(Order = 1, TypeName = "int")]
        public int IntProperty1 { get; set; }

        internal ICollection<LightweightEntity> InternalCollectionNavigationProperty { get; set; }

        [Required]
        [StringLength(15)]
        public string StringProperty { get; set; }
    }

    public class SplitMeGood
    {
        public int Id { get; set; }
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }
    }

    public class JoinMeGood
    {
        public int Id { get; set; }
        public int Prop1 { get; set; }
    }

    public class JoinMeMore : JoinMeGood
    {
        public int Prop2 { get; set; }
    }

    public class LightweightDerivedEntity : LightweightEntity
    {        
    }

    #endregion
}
