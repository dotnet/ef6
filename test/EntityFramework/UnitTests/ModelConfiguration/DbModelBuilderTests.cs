// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using BinaryPropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration;
    using DateTimePropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DateTimePropertyConfiguration;
    using DecimalPropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.DecimalPropertyConfiguration;
    using LengthPropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration;
    using PrimitivePropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;
    using StringPropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration;

    public sealed class DbModelBuilderTests
    {
        [Fact]
        public void Can_set_default_schema()
        {
            var modelConfiguration = new ModelConfiguration();
            var modelBuilder = new DbModelBuilder(modelConfiguration);

            modelBuilder.HasDefaultSchema("foo");

            Assert.Equal("foo", modelConfiguration.DefaultSchema);
        }

        [Fact]
        public void Ctor_should_throw_when_version_out_of_range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DbModelBuilder((DbModelBuilderVersion)(-1)));
        }

        [Fact]
        public void Conventions_should_return_non_null_configuration_object()
        {
            Assert.NotNull(new DbModelBuilder().Conventions);
        }

        [Fact]
        public void Ignore_should_add_to_list_of_ignored_types()
        {
            var modelConfiguration = new ModelConfiguration();

            new DbModelBuilder(modelConfiguration).Ignore<object>();

            Assert.True(modelConfiguration.IsIgnoredType(typeof(object)));
        }

        [Fact]
        public void Build_should_validate_and_throw_with_invalid_model()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(Random), true);

            Assert.Throws<ModelValidationException>(
                () => new DbModelBuilder(modelConfiguration).Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configurations_should_return_non_null_registrar()
        {
            Assert.NotNull(new DbModelBuilder().Configurations);
        }

        [Fact]
        public void Build_should_throw_when_entity_type_is_not_mappable()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<object>();

            Assert.Equal(
                Strings.InvalidEntityType(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).Message);
        }

        [Fact]
        public void Build_should_throw_when_complex_type_is_not_mappable()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<object>();

            Assert.Equal(
                Strings.CodeFirstInvalidComplexType(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).Message);
        }

        [Fact]
        public void Build_should_not_throw_when_complex_type_ignored_then_configured()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockType = typeof(AType1);
            modelConfiguration.Ignore(mockType);
            modelConfiguration.ComplexType(mockType);
            var modelBuilder = new DbModelBuilder();

            var databaseMapping = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            Assert.Equal(0, databaseMapping.Model.ComplexTypes.Count());
        }

        [Fact]
        public void Build_should_map_types()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(AType2)).Key(typeof(AType2).GetInstanceProperty("Id"));
            modelConfiguration.ComplexType(typeof(CType2));
            var modelBuilder = new DbModelBuilder(modelConfiguration);

            var databaseMapping = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            Assert.NotNull(databaseMapping);

            Assert.Equal(1, databaseMapping.Model.EntityTypes.Count());
            Assert.Equal(1, databaseMapping.Model.ComplexTypes.Count());
        }

        public class AType2
        {
            public int Id { get; set; }
        }

        public class CType2
        {
        }

        [Fact]
        public void Build_should_apply_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(AType1))
                .Property(new PropertyPath(typeof(AType1).GetInstanceProperty("Id")))
                .ConcurrencyMode = ConcurrencyMode.Fixed;

            var databaseMapping = new DbModelBuilder(modelConfiguration).Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            Assert.NotNull(databaseMapping);
            Assert.Equal(
                ConcurrencyMode.Fixed,
                databaseMapping.Model.EntityTypes.Single().DeclaredProperties.Single().ConcurrencyMode);
        }

        public class AType1
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Mapping_a_single_abstract_type_should_not_throw()
        {
            var modelConfiguration = new ModelConfiguration();
            modelConfiguration.Entity(typeof(AType1)).Key(typeof(AType1).GetDeclaredProperty("Id"));
            var modelBuilder = new DbModelBuilder();

            var databaseMapping = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.NotNull(databaseMapping);
        }

        public abstract class AType3
        {
            public int Id { get; set; }
        }

        #region Model builder cloning tests

        [Fact]
        public void Cloning_the_model_builder_clones_contained_types()
        {
            var builder = new DbModelBuilder(DbModelBuilderVersion.V4_1);
            builder.ModelConfiguration.Entity(typeof(AType1), true);

            Assert.Same(builder.ModelConfiguration, builder.ModelConfiguration);
            Assert.Same(builder.Conventions, builder.Conventions);

            var clone = builder.Clone();

            Assert.Equal(DbModelBuilderVersion.V4_1, clone.Version);

            Assert.NotSame(builder.ModelConfiguration, clone.ModelConfiguration);
            Assert.NotSame(builder.Conventions, clone.Conventions);

            Assert.Equal(1, clone.ModelConfiguration.Entities.Count());
            Assert.True(clone.Conventions.ConfigurationConventions.Count() > 0);
        }

        [Fact]
        public void DbModelBuilder_has_expected_number_of_fields()
        {
            VerifyFieldCount<DbModelBuilder>(4);
        }

        private void VerifyFieldCount<T>(int expectedCount)
        {
            var actualCount = typeof(T).GetRuntimeFields().Count(f => !f.IsStatic);

            if (expectedCount != actualCount)
            {
                Assert.True(
                    false,
                    String.Format(
                        "The number of fields on {0} was expected to be {1} but is {2}. If a field has been added then make sure it is being properly copied by Clone and then update this test.",
                        typeof(T).Name, expectedCount, actualCount));
            }
        }

        [Fact]
        public void ConventionsConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ConventionsConfiguration>(5);
        }

        [Fact]
        public void Cloning_the_model_configuration_clones_type_configurations_and_ignored_types()
        {
            var configuration = new ModelConfiguration();

            var entityType1 = new MockType("E1");
            var complexType1 = new MockType("C1");
            var ignoredType1 = new MockType("I1");

            configuration.Add(new EntityTypeConfiguration(entityType1));
            configuration.Add(new ComplexTypeConfiguration(complexType1));
            configuration.Ignore(ignoredType1);
            configuration.DefaultSchema = "Foo";
            configuration.ModelNamespace = "Bar";

            var clone = configuration.Clone();

            Assert.True(clone.Entities.Contains(entityType1));
            Assert.True(clone.ComplexTypes.Contains(complexType1));
            Assert.True(clone.IsIgnoredType(ignoredType1));
            Assert.Equal("Foo", clone.DefaultSchema);
            Assert.Equal("Bar", clone.ModelNamespace);

            var entityType2 = new MockType("E2");
            var complexType2 = new MockType("C2");
            var ignoredType2 = new MockType("I2");

            configuration.Add(new EntityTypeConfiguration(entityType2));
            configuration.Add(new ComplexTypeConfiguration(complexType2));
            configuration.Ignore(ignoredType2);

            Assert.False(clone.Entities.Contains(entityType2));
            Assert.False(clone.ComplexTypes.Contains(complexType2));
            Assert.False(clone.IsIgnoredType(ignoredType2));
        }

        [Fact]
        public void ModelConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ModelConfiguration>(5);
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_scalar_properties()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            configuration.IsReplaceable = true;
            configuration.ToTable("Table");
            configuration.IsExplicitEntity = true;
            configuration.EntitySetName = "ESN";

            var clone = configuration.Clone();

            Assert.True(clone.IsReplaceable);
            Assert.True(clone.IsTableNameConfigured);
            Assert.True(clone.IsExplicitEntity);
            Assert.Equal("ESN", clone.EntitySetName);
            Assert.Same(typeof(object), clone.ClrType);
        }

        [Fact]
        public void EntityTypeConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<EntityTypeConfiguration>(11);
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_key_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            var mockPropertyInfo1 = new MockPropertyInfo(typeof(int), "P1");
            configuration.Property(new PropertyPath(mockPropertyInfo1)).ColumnOrder = 0;

            var mockPropertyInfo2 = new MockPropertyInfo(typeof(int), "P2");
            configuration.Property(new PropertyPath(mockPropertyInfo2)).ColumnOrder = 1;

            // This will set _isKeyConfigured to true
            configuration.Key(
                new List<PropertyInfo>
                    {
                        mockPropertyInfo1
                    });

            var clone = configuration.Clone();

            VerifyKeyProperty(clone, "P1", mockPropertyInfo1, mockPropertyInfo2);

            // This should have no effect because _isKeyConfigured is set to true
            clone.Key(mockPropertyInfo2);

            VerifyKeyProperty(clone, "P1", mockPropertyInfo1, mockPropertyInfo2);

            // This should change the key on the original, but not on the clone.
            configuration.Key(
                new List<PropertyInfo>
                    {
                        mockPropertyInfo2
                    });

            VerifyKeyProperty(configuration, "P2", mockPropertyInfo1, mockPropertyInfo2);
            VerifyKeyProperty(clone, "P1", mockPropertyInfo1, mockPropertyInfo2);
        }

        private void VerifyKeyProperty(EntityTypeConfiguration configuration, string expectedKeyName, params PropertyInfo[] props)
        {
            var entityType = CreateEntityTypeWithProperties(props);
            configuration.Configure(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.Equal(expectedKeyName, entityType.KeyProperties.Single().Name);
        }

        private EntityType CreateEntityTypeWithProperties(params PropertyInfo[] props)
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            foreach (var prop in props)
            {
                var property = EdmProperty.CreatePrimitive(prop.Name, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

                entityType.AddMember(property);
                entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == prop.Name).SetClrPropertyInfo(prop);
            }

            return entityType;
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_navigation_property_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            var mockNavProp1 = new MockPropertyInfo(typeof(AType1), "Nav1");
            var navConfig1 = configuration.Navigation(mockNavProp1);

            var clone = configuration.Clone();

            Assert.True(clone.ConfiguredProperties.Contains(mockNavProp1));
            Assert.NotSame(navConfig1, clone.Navigation(mockNavProp1));

            var mockNavProp2 = new MockPropertyInfo(typeof(AType1), "Nav2");
            configuration.Navigation(mockNavProp2);

            Assert.False(clone.ConfiguredProperties.Contains(mockNavProp2));
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_primitive_property_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            var mockProperty1 = new MockPropertyInfo(typeof(int), "P1");
            var propConfig1 = configuration.Property(new PropertyPath(mockProperty1));

            var clone = configuration.Clone();

            Assert.True(clone.ConfiguredProperties.Contains(mockProperty1));
            Assert.NotSame(propConfig1, clone.Property(new PropertyPath(mockProperty1)));

            var mockProperty2 = new MockPropertyInfo(typeof(int), "P2");
            configuration.Property(new PropertyPath(mockProperty2));

            Assert.False(clone.ConfiguredProperties.Contains(mockProperty2));
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_ignored_properties()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            var mockProperty1 = new MockPropertyInfo(typeof(int), "P1");
            configuration.Ignore(mockProperty1);

            var clone = configuration.Clone();

            Assert.True(clone.IgnoredProperties.Contains(mockProperty1));

            var mockProperty2 = new MockPropertyInfo(typeof(int), "P2");
            configuration.Ignore(mockProperty2);

            Assert.False(clone.IgnoredProperties.Contains(mockProperty2));
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_mapping_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            configuration.ToTable("Table");

            var clone = configuration.Clone();
            Assert.Equal("Table", clone.GetTableName().Name);

            configuration.ToTable("AnotherTable");

            Assert.Equal("Table", clone.GetTableName().Name);
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_subtype_mapping_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            var mappingConfiguration = new EntityMappingConfiguration();
            configuration.AddSubTypeMappingConfiguration(typeof(object), mappingConfiguration);

            var clone = configuration.Clone();

            Assert.NotSame(configuration.SubTypeMappingConfigurations, clone.SubTypeMappingConfigurations);
            Assert.True(clone.SubTypeMappingConfigurations.ContainsKey(typeof(object)));
            Assert.NotSame(mappingConfiguration, clone.SubTypeMappingConfigurations[typeof(object)]);

            configuration.AddSubTypeMappingConfiguration(typeof(int), new EntityMappingConfiguration());
            Assert.False(clone.SubTypeMappingConfigurations.ContainsKey(typeof(int)));
        }

        [Fact]
        public void Cloning_an_entity_configuration_clones_its_modification_function_information()
        {
            var configuration = new EntityTypeConfiguration(typeof(object));

            configuration.MapToStoredProcedures();

            var clone = configuration.Clone();

            Assert.NotNull(clone.ModificationStoredProceduresConfiguration);
        }

        [Fact]
        public void Cloning_a_complex_type_configuration_clones_its_primitive_property_information()
        {
            var configuration = new ComplexTypeConfiguration(typeof(object));

            var mockProperty1 = new MockPropertyInfo(typeof(int), "P1");
            var propConfig1 = configuration.Property(new PropertyPath(mockProperty1));

            var clone = configuration.Clone();

            Assert.True(clone.ConfiguredProperties.Contains(mockProperty1));
            Assert.NotSame(propConfig1, clone.Property(new PropertyPath(mockProperty1)));

            var mockProperty2 = new MockPropertyInfo(typeof(int), "P2");
            configuration.Property(new PropertyPath(mockProperty2));

            Assert.False(clone.ConfiguredProperties.Contains(mockProperty2));
        }

        [Fact]
        public void ComplexTypeConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ComplexTypeConfiguration>(0);
        }

        [Fact]
        public void StructuralTypeConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<StructuralTypeConfiguration>(3);
        }

        [Fact]
        public void Cloning_a_complex_type_configuration_clones_its_ignored_properties()
        {
            var configuration = new ComplexTypeConfiguration(typeof(object));

            var mockProperty1 = new MockPropertyInfo(typeof(int), "P1");
            configuration.Ignore(mockProperty1);

            var clone = configuration.Clone();

            Assert.True(clone.IgnoredProperties.Contains(mockProperty1));

            var mockProperty2 = new MockPropertyInfo(typeof(int), "P2");
            configuration.Ignore(mockProperty2);

            Assert.False(clone.IgnoredProperties.Contains(mockProperty2));
        }

        [Fact]
        public void Cloning_a_complex_type_configuration_clones_its_scalar_properties()
        {
            var configuration = new ComplexTypeConfiguration(typeof(object));

            var clone = configuration.Clone();

            Assert.Same(typeof(object), clone.ClrType);
        }

        [Fact]
        public void Cloning_a_primitive_property_configuration_clones_its_property_information()
        {
            Cloning_a_primitive_property_configuration_clones_its_property_information_implementation(
                new PrimitivePropertyConfiguration());
        }

        [Fact]
        public void PrimitivePropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<PrimitivePropertyConfiguration>(10);
        }

        [Fact]
        public void Cloning_a_binary_property_configuration_clones_its_property_information()
        {
            var configuration = new BinaryPropertyConfiguration();
            configuration.IsRowVersion = true;

            var clone = (BinaryPropertyConfiguration)
                        Cloning_a_length_property_configuration_clones_its_property_information(configuration);

            Assert.True(clone.IsRowVersion.Value);
        }

        [Fact]
        public void BinaryPropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<BinaryPropertyConfiguration>(1);
        }

        [Fact]
        public void Cloning_a_decimal_property_configuration_clones_its_property_information()
        {
            var configuration = new DecimalPropertyConfiguration();
            configuration.Precision = 100;
            configuration.Scale = 101;

            var clone = (DecimalPropertyConfiguration)
                        Cloning_a_primitive_property_configuration_clones_its_property_information_implementation(configuration);

            Assert.Equal(100, clone.Precision.Value);
            Assert.Equal(101, clone.Scale.Value);
        }

        [Fact]
        public void DecimalPropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<DecimalPropertyConfiguration>(2);
        }

        [Fact]
        public void Cloning_a_date_time_property_configuration_clones_its_property_information()
        {
            var configuration = new DateTimePropertyConfiguration();
            configuration.Precision = 100;

            var clone = (DateTimePropertyConfiguration)
                        Cloning_a_primitive_property_configuration_clones_its_property_information_implementation(configuration);

            Assert.Equal(100, clone.Precision.Value);
        }

        [Fact]
        public void DateTimePropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<DateTimePropertyConfiguration>(1);
        }

        [Fact]
        public void Cloning_a_string_property_configuration_clones_its_property_information()
        {
            var configuration = new StringPropertyConfiguration();
            configuration.IsUnicode = true;

            var clone = (StringPropertyConfiguration)
                        Cloning_a_length_property_configuration_clones_its_property_information(configuration);

            Assert.True(clone.IsUnicode.Value);
        }

        [Fact]
        public void StringPropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<StringPropertyConfiguration>(1);
        }

        [Fact]
        public void LengthPropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<LengthPropertyConfiguration>(3);
        }

        private LengthPropertyConfiguration Cloning_a_length_property_configuration_clones_its_property_information(
            LengthPropertyConfiguration configuration)
        {
            configuration.IsFixedLength = true;
            configuration.IsMaxLength = true;
            configuration.MaxLength = 77;

            var clone = (LengthPropertyConfiguration)
                        Cloning_a_primitive_property_configuration_clones_its_property_information_implementation(configuration);

            Assert.True(clone.IsFixedLength.Value);
            Assert.True(clone.IsMaxLength.Value);
            Assert.Equal(77, clone.MaxLength);

            return clone;
        }

        private PrimitivePropertyConfiguration Cloning_a_primitive_property_configuration_clones_its_property_information_implementation(
            PrimitivePropertyConfiguration configuration)
        {
            configuration.IsNullable = true;
            configuration.ConcurrencyMode = ConcurrencyMode.Fixed;
            configuration.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;
            configuration.ColumnType = "ColumnType";
            configuration.ColumnName = "ColumnName";
            configuration.ColumnOrder = 1;
            configuration.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace;
            configuration.SetAnnotation("A1", "V1");
            configuration.SetAnnotation("A2", "V2");
            configuration.SetAnnotation("A3", "V3");
            configuration.SetAnnotation("A1", "V4");
            configuration.SetAnnotation("A2", null);

            var clone = configuration.Clone();

            Assert.True(clone.IsNullable.Value);
            Assert.Equal(ConcurrencyMode.Fixed, clone.ConcurrencyMode);
            Assert.Equal(DatabaseGeneratedOption.Identity, clone.DatabaseGeneratedOption);
            Assert.Equal("ColumnType", clone.ColumnType);
            Assert.Equal("ColumnName", clone.ColumnName);
            Assert.Equal(1, clone.ColumnOrder);
            Assert.Equal(OverridableConfigurationParts.OverridableInCSpace, clone.OverridableConfigurationParts);
            Assert.Equal("V4", clone.Annotations["A1"]);
            Assert.Null(clone.Annotations["A2"]);
            Assert.Equal("V3", clone.Annotations["A3"]);

            return clone;
        }

        [Fact]
        public void Cloning_a_navigation_property_configuration_clones_its_property_information()
        {
            var navProp = new MockPropertyInfo(typeof(AType1), "P1");
            var configuration = new NavigationPropertyConfiguration(navProp);

            configuration.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            var inverseNavProp = new MockPropertyInfo(typeof(int), "P2");
            configuration.InverseNavigationProperty = inverseNavProp;
            configuration.InverseEndKind = RelationshipMultiplicity.ZeroOrOne;
            configuration.DeleteAction = OperationAction.Cascade;
            configuration.IsNavigationPropertyDeclaringTypePrincipal = true;

            var clone = configuration.Clone();

            Assert.Equal(navProp, clone.NavigationProperty);
            Assert.Equal(RelationshipMultiplicity.Many, clone.RelationshipMultiplicity);
            Assert.Equal(inverseNavProp, clone.InverseNavigationProperty);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, clone.InverseEndKind);
            Assert.Equal(OperationAction.Cascade, clone.DeleteAction);
            Assert.True(clone.IsNavigationPropertyDeclaringTypePrincipal.Value);

            Assert.Null(clone.Constraint);
            Assert.Null(clone.AssociationMappingConfiguration);
        }

        [Fact]
        public void NavigationPropertyConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<NavigationPropertyConfiguration>(9);
        }

        [Fact]
        public void Cloning_a_navigation_property_configuration_clones_its_constraint_information()
        {
            var navProp = new MockPropertyInfo(typeof(AType1), "P1");
            var configuration = new NavigationPropertyConfiguration(navProp);

            configuration.Constraint =
                new ForeignKeyConstraintConfiguration(
                    new List<PropertyInfo>
                        {
                            new MockPropertyInfo(typeof(int), "P2")
                        });

            var clone = configuration.Clone();

            Assert.NotSame(configuration.Constraint, clone.Constraint);
            Assert.Equal(configuration.Constraint, clone.Constraint);
        }

        [Fact]
        public void Cloning_a_navigation_property_configuration_clones_its_association_mapping_configuration()
        {
            var navProp = new MockPropertyInfo(typeof(AType1), "P1");
            var configuration = new NavigationPropertyConfiguration(navProp);

            var mappingConfiguration = new ForeignKeyAssociationMappingConfiguration();
            mappingConfiguration.MapKey("C1");
            configuration.AssociationMappingConfiguration = mappingConfiguration;

            var clone = configuration.Clone();

            Assert.NotSame(configuration.AssociationMappingConfiguration, clone.AssociationMappingConfiguration);
            Assert.Equal(configuration.AssociationMappingConfiguration, clone.AssociationMappingConfiguration);
        }

        [Fact]
        public void Cloning_a_navigation_property_configuration_clones_its_function_mapping_configuration()
        {
            var navProp = new MockPropertyInfo(typeof(AType1), "P1");
            var configuration = new NavigationPropertyConfiguration(navProp);

            var functionsConfiguration = new ModificationStoredProceduresConfiguration();

            configuration.ModificationStoredProceduresConfiguration = functionsConfiguration;

            var clone = configuration.Clone();

            Assert.NotSame(configuration.ModificationStoredProceduresConfiguration, clone.ModificationStoredProceduresConfiguration);
            Assert.True(configuration.ModificationStoredProceduresConfiguration.IsCompatibleWith(clone.ModificationStoredProceduresConfiguration));
        }

        [Fact]
        public void Cloning_a_foreign_key_constraint_clones_its_property_information()
        {
            var configuration =
                new ForeignKeyConstraintConfiguration(
                    new List<PropertyInfo>
                        {
                            new MockPropertyInfo(typeof(int), "P1")
                        });

            var clone = (ForeignKeyConstraintConfiguration)configuration.Clone();

            Assert.True(clone.IsFullySpecified);
            Assert.True(clone.ToProperties.Any(p => p.Name == "P1"));
        }

        [Fact]
        public void ForeignKeyConstraintConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ForeignKeyConstraintConfiguration>(2);
        }

        [Fact]
        public void Cloning_an_indpendent_constraint_just_returns_the_singleton_instance()
        {
            var configuration = IndependentConstraintConfiguration.Instance;

            var clone = (IndependentConstraintConfiguration)configuration.Clone();

            Assert.Same(IndependentConstraintConfiguration.Instance, clone);
        }

        [Fact]
        public void IndependentConstraintConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<IndependentConstraintConfiguration>(0);
        }

        [Fact]
        public void ConstraintConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ConstraintConfiguration>(0);
        }

        [Fact]
        public void Cloning_an_foreign_key_mapping_configuration_clones_its_table_and_column_information()
        {
            var configuration = new ForeignKeyAssociationMappingConfiguration();
            configuration.MapKey("C1");
            configuration.ToTable("T", "S");

            var clone = (ForeignKeyAssociationMappingConfiguration)configuration.Clone();

            Assert.Equal(configuration, clone);

            configuration.MapKey("C2");

            Assert.NotEqual(configuration, clone);
        }

        [Fact]
        public void Cloning_an_foreign_key_mapping_configuration_clones_its_annotation_information()
        {
            var configuration = new ForeignKeyAssociationMappingConfiguration();
            configuration.MapKey("C1", "C2");
            configuration.HasKeyAnnotation("C1", "A1", "V1");
            configuration.HasKeyAnnotation("C2", "A2", "V2");

            var clone = (ForeignKeyAssociationMappingConfiguration)configuration.Clone();

            Assert.Equal(configuration, clone);

            configuration.HasKeyAnnotation("C2", "A2", "V3");

            Assert.NotEqual(configuration, clone);
        }

        [Fact]
        public void ForeignKeyAssociationMappingConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ForeignKeyAssociationMappingConfiguration>(3);
        }

        [Fact]
        public void Cloning_a_many_to_many_foreign_key_mapping_configuration_clones_its_table_and_column_information()
        {
            var configuration = new ManyToManyAssociationMappingConfiguration();
            configuration.MapLeftKey("C1");
            configuration.MapRightKey("C2");
            configuration.ToTable("T", "S");

            var clone = (ManyToManyAssociationMappingConfiguration)configuration.Clone();

            Assert.Equal(configuration, clone);

            configuration.MapLeftKey("C3");

            Assert.NotEqual(configuration, clone);
        }

        [Fact]
        public void ManyToManyAssociationMappingConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ManyToManyAssociationMappingConfiguration>(3);
        }

        [Fact]
        public void AssociationMappingConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<AssociationMappingConfiguration>(0);
        }

        [Fact]
        public void Cloning_an_entity_mapping_configuration_clones_its_table_property_and_condition_information()
        {
            var configuration = new EntityMappingConfiguration();

            var propertyInfo1 = new MockPropertyInfo(typeof(int), "P1");
            configuration.Properties = new List<PropertyPath>
                {
                    new PropertyPath(propertyInfo1)
                };

            configuration.TableName = new DatabaseName("T", "S");

            configuration.AddValueCondition(new ValueConditionConfiguration(configuration, "D"));
            configuration.AddNullabilityCondition(new NotNullConditionConfiguration(configuration, new PropertyPath(propertyInfo1)));

            configuration.MapInheritedProperties = true;

            var clone = configuration.Clone();

            Assert.True(clone.Properties.Any(p => p[0].Name == "P1"));

            Assert.Equal("T", clone.TableName.Name);
            Assert.Equal("S", clone.TableName.Schema);

            Assert.True(clone.ValueConditions.Any(c => c.Discriminator == "D"));
            Assert.True(clone.NullabilityConditions.Any(c => c.PropertyPath[0].Name == "P1"));

            configuration.AddValueCondition(new ValueConditionConfiguration(configuration, "D2"));
            configuration.AddNullabilityCondition(
                new NotNullConditionConfiguration(
                    configuration,
                    new PropertyPath(new MockPropertyInfo(typeof(int), "P2"))));

            Assert.False(clone.ValueConditions.Any(c => c.Discriminator == "D2"));
            Assert.False(clone.NullabilityConditions.Any(c => c.PropertyPath[0].Name == "P2"));

            Assert.True(clone.MapInheritedProperties);
        }

        [Fact]
        public void EntityMappingConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<EntityMappingConfiguration>(6);
        }

        [Fact]
        public void Cloning_an_entity_mapping_configuration_works_when_no_properties_have_been_set()
        {
            var configuration = new EntityMappingConfiguration();

            var clone = configuration.Clone();

            Assert.Null(clone.Properties);
        }

        [Fact]
        public void Cloning_a_value_condition_clones_its_discriminator_and_value()
        {
            var entityConfiguration = new EntityMappingConfiguration();
            var configuration = new ValueConditionConfiguration(entityConfiguration, "D");
            configuration.Value = "V";

            var clone = configuration.Clone(entityConfiguration);

            Assert.Equal("V", (string)clone.Value);
            Assert.Equal("D", clone.Discriminator);
        }

        [Fact]
        public void ValueConditionConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<ValueConditionConfiguration>(4);
        }

        [Fact]
        public void Cloning_a_null_condition_clones_its_property_info()
        {
            var entityConfiguration = new EntityMappingConfiguration();
            var configuration = new NotNullConditionConfiguration(
                entityConfiguration,
                new PropertyPath(new MockPropertyInfo(typeof(int), "P")));

            var clone = configuration.Clone(entityConfiguration);

            Assert.Equal("P", clone.PropertyPath[0].Name);
        }

        [Fact]
        public void NotNullConditionConfiguration_has_expected_number_of_fields()
        {
            VerifyFieldCount<NotNullConditionConfiguration>(2);
        }

        #endregion

        [Fact]
        public void Entities_returns_configuration_object()
        {
            Assert.NotNull(new DbModelBuilder().Types());
        }

        [Fact]
        public void Entities_with_type_returns_configuration_object()
        {
            Assert.NotNull(new DbModelBuilder().Types<object>());
        }

        [Fact]
        public void Properties_returns_configuration_object()
        {
            Assert.NotNull(new DbModelBuilder().Properties());
        }

        [Fact]
        public void Properties_with_type_returns_configuration_object()
        {
            var decimalProperty = new MockPropertyInfo(typeof(decimal), "Property1");
            var nullableDecimalProperty = new MockPropertyInfo(typeof(decimal?), "Property2");
            var nonDecimalProperty = new MockPropertyInfo(typeof(string), "Property3");

            var config = new DbModelBuilder().Properties<decimal>();
            Assert.NotNull(config);
            Assert.Equal(1, config.Predicates.Count());

            var predicate = config.Predicates.Single();
            Assert.True(predicate(decimalProperty));
            Assert.True(predicate(nullableDecimalProperty));
            Assert.False(predicate(nonDecimalProperty));
        }

        [Fact]
        public void Properties_with_type_throws_when_not_primitive()
        {
            var modelBuilder = new DbModelBuilder();

            // Ensure non-struct primitive types are allowed
            modelBuilder.Properties<byte[]>();
            modelBuilder.Properties<DbGeography>();
            modelBuilder.Properties<DbGeometry>();
            modelBuilder.Properties<string>();
            modelBuilder.Properties<DateTimeKind>();

            var ex = Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Properties<object>());

            Assert.Equal(
                Strings.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(object)),
                ex.Message);
        }

        [Fact]
        public void Build_adds_the_UseClrTypes_annotation_to_the_container()
        {
            var databaseMapping = new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            Assert.Equal(
                1,
                databaseMapping.Model.Container.Annotations.Count(
                    a => a.Name == XmlConstants.UseClrTypesAnnotation && a.Value.Equals("true")));
        }
    }
}
