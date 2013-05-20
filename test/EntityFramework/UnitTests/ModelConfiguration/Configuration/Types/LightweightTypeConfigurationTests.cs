// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class LightweightTypeConfigurationTests
    {
        [Fact]
        public void Ctor_does_not_invoke_delegate()
        {
            var initialized = false;

            new LightweightTypeConfiguration(
                new MockType(),
                () =>
                    {
                        initialized = true;

                        return (EntityTypeConfiguration)null;
                    }, new ModelConfiguration());

            Assert.False(initialized);
        }

        [Fact]
        public void Methods_dont_throw_when_configuration_is_null()
        {
            var type = new MockType();
            type.Property<int>("Property1");

            Methods_dont_throw_when_configuration_is_null_implementation(
                () => new LightweightTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()));
            Methods_dont_throw_when_configuration_is_null_implementation(
                () => new LightweightTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()));
            Methods_dont_throw_when_configuration_is_null_implementation(
                () => new LightweightTypeConfiguration(type, new ModelConfiguration()));
        }

        private void Methods_dont_throw_when_configuration_is_null_implementation(Func<LightweightTypeConfiguration> config)
        {
            Assert.NotNull(config().ClrType);
            config().IsComplexType();
            config().Ignore();
            config().Ignore("Property1");
            config().HasEntitySetName("EntitySet1");
            config().HasKey("Property1");
            config().HasKey(new[] { "Property1" });
            config().HasKey(config().ClrType.GetProperties().First());
            config().HasKey(new[] { config().ClrType.GetProperties().First() });
            config().Property("Property1");
            config().Property(config().ClrType.GetProperties().First());
            config().Property(new PropertyPath(config().ClrType.GetProperties().First()));
            config().ToTable("Table1");
            config().ToTable("Table1", "Schema1");
            config().MapToStoredProcedures();
            config().MapToStoredProcedures(c => { });
        }

        [Fact]
        public void HasEntitySetName_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.HasEntitySetName(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("entitySetName"), ex.Message);
        }

        [Fact]
        public void HasEntitySetName_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasEntitySetName("EntitySet1");

            Assert.Equal("EntitySet1", innerConfig.EntitySetName);
            Assert.Same(config, result);
        }

        [Fact]
        public void HasEntitySetName_is_noop_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type)
                {
                    EntitySetName = "EntitySet1"
                };
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasEntitySetName("EntitySet2");

            Assert.Equal("EntitySet1", innerConfig.EntitySetName);
            Assert.Same(config, result);
        }

        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.Ignore((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), ex.Message);
        }

        [Fact]
        public void Ignore_configures()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.Ignore("Property1");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_configures_complex_type_property()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.IsComplexType();
            config.Ignore("Property1");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_is_noop_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.Ignore("Property1");

            Assert.Empty(innerConfig.IgnoredProperties);
        }

        [Fact]
        public void Ignore_type_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new LightweightTypeConfiguration(type, innerConfig);

            config.Ignore();

            Assert.True(innerConfig.IsIgnoredType(typeof(LocalEntityType)));
        }

        [Fact]
        public void Ignore_type_throws_with_any_other_configuration()
        {
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.IsComplexType());
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Ignore("Property1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasEntitySetName("EntitySet1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey("Property1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(new[] { "Property1" }));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(config.ClrType.GetProperties().First()));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetProperties().First() }));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property("Property1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property(config.ClrType.GetProperties().First()));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property(new PropertyPath(config.ClrType.GetProperties().First())));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1", "Schema1"));
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures());
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures(c => { }));
        }

        private void Ignore_type_throws_with_any_other_configuration_implementation(Action<LightweightTypeConfiguration> configAction)
        {
            var type = new MockType();
            type.Property<int>("Property1");

            Ignore_type_throws_with_any_other_configuration_assert(
                new LightweightTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()),
                configAction);
            Ignore_type_throws_with_any_other_configuration_assert(
                new LightweightTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()),
                configAction);
            Ignore_type_throws_with_any_other_configuration_assert(
                new LightweightTypeConfiguration(type, new ModelConfiguration()),
                configAction);
        }

        private void Ignore_type_throws_with_any_other_configuration_assert(
            LightweightTypeConfiguration config,
            Action<LightweightTypeConfiguration> configAction)
        {
            config.Ignore();

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType,
                Assert.Throws<InvalidOperationException>(
                    () => configAction(config)).Message);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType,
                Assert.Throws<InvalidOperationException>(
                    () => config.Ignore()).Message);
        }

        [Fact]
        public void IsComplexType_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new LightweightTypeConfiguration(type, innerConfig);

            config.IsComplexType();

            Assert.True(innerConfig.IsComplexType(typeof(LocalEntityType)));
        }

        [Fact]
        public void IsComplexType_throws_with_conflicting_configuration()
        {
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasEntitySetName("EntitySet1"));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey("Property1"));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(new[] { "Property1" }));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(config.ClrType.GetProperties().First()));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetProperties().First() }));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.ToTable("Table1"));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.ToTable("Table1", "Schema1"));
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.MapToStoredProcedures());
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.MapToStoredProcedures(c => { }));
        }

        private void IsComplexType_throws_with_conflicting_configuration_implementation(Action<LightweightTypeConfiguration> configAction)
        {
            var type = new MockType();
            type.Property<int>("Property1");

            IsComplexType_throws_with_conflicting_configuration_assert(
                new LightweightTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()),
                configAction);
            IsComplexType_throws_with_conflicting_configuration_assert(
                new LightweightTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()),
                configAction);
            IsComplexType_throws_with_conflicting_configuration_assert(
                new LightweightTypeConfiguration(type, new ModelConfiguration()),
                configAction);
        }

        private void IsComplexType_throws_with_conflicting_configuration_assert(
            LightweightTypeConfiguration config,
            Action<LightweightTypeConfiguration> configAction)
        {
            config.IsComplexType();

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType,
                Assert.Throws<InvalidOperationException>(
                    () => configAction(config)).Message);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType,
                Assert.Throws<InvalidOperationException>(
                    () => config.IsComplexType()).Message);
        }

        [Fact]
        public void Property_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.Property((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("name"), ex.Message);
        }

        [Fact]
        public void Property_throws_when_nonscalar()
        {
            var type = new MockType()
                .Property<object>("NonScalar");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<InvalidOperationException>(
                () => config.Property("NonScalar"));

            Assert.Equal(
                Strings.LightweightEntityConfiguration_NonScalarProperty("NonScalar"),
                ex.Message);
        }

        [Fact]
        public void Property_throws_when_indexer()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<InvalidOperationException>(
                () => config.Property("Item"));

            Assert.Equal(
                Strings.LightweightEntityConfiguration_NonScalarProperty("Item"),
                ex.Message);
        }

        [Fact]
        public void Property_returns_MissingPropertyConfiguration_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property("Property1");

            Assert.IsType<MissingPropertyConfiguration>(result);
        }

        [Fact]
        public void Property_returns_configuration()
        {
            var type = new MockType()
                .Property<decimal>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property("Property1");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property1", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void Property_returns_configuration_for_complex_type_properties()
        {
            var type = new MockType()
                .Property<decimal>("Property1");
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.IsComplexType();
            var result = config.Property("Property1");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property1", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void ToTable_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table1");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_is_noop_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1");
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table2");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_handles_dot()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Schema1.Table1");

            Assert.Equal("Schema1", innerConfig.SchemaName);
            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_with_schema_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null, null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_with_schema_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table1", "Schema1");

            Assert.Equal("Table1", innerConfig.TableName);
            Assert.Equal("Schema1", innerConfig.SchemaName);
        }

        [Fact]
        public void ToTable_with_schema_is_noop_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1", "Schema1");
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table2", "Schema2");

            Assert.Equal("Table1", innerConfig.TableName);
            Assert.Equal("Schema1", innerConfig.SchemaName);
        }

        [Fact]
        public void HasKey_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.HasKey((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), ex.Message);
        }

        [Fact]
        public void HasKey_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property1");

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_PropertyInfo_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(config.ClrType.GetProperties().First());

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_is_noop_when_set()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2");
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.Key(new[] { type.GetProperty("Property1") });
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property2");

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_is_noop_when_not_exists()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property2");

            Assert.Empty(innerConfig.KeyProperties);
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.HasKey((string[])null));

            Assert.Equal("propertyNames", ex.ParamName);
        }

        [Fact]
        public void HasKey_composite_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1", "Property2" });

            Assert.Equal(2, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_is_noop_when_set()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2")
                .Property<int>("Property3");
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.Key(new[] { type.GetProperty("Property1") });
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property2", "Property3" });

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property3"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_is_noop_when_not_exists()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1", "Property2" });

            Assert.Empty(innerConfig.KeyProperties);
            Assert.Same(config, result);
        }

        [Fact]
        public void MapToStoredProcedures_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationFunctionsConfiguration);
        }

        [Fact]
        public void ClrType_returns_type()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Same(type.Object, config.ClrType);
        }

        [Fact]
        public void MapToStoredProcedures_with_no_args_should_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationFunctionsConfiguration);
        }

        [Fact]
        public void MapToStoredProcedures_with_action_should_invoke_and_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            LightweightModificationFunctionsConfiguration configuration = null;

            config.MapToStoredProcedures(c => configuration = c);

            Assert.Same(
                configuration.Configuration,
                innerConfig.ModificationFunctionsConfiguration);
        }

        private class LocalEntityType
        {
            public int this[int index]
            {
                get { return 0; }
            }
        }
    }
}
