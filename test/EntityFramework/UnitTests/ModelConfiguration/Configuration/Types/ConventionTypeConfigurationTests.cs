// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    
    public class ConventionTypeConfigurationTests
    {
        [Fact]
        public void Ctor_does_not_invoke_delegate()
        {
            var initialized = false;

            new ConventionTypeConfiguration(
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
                () => new ConventionTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()));
            Methods_dont_throw_when_configuration_is_null_implementation(
                () => new ConventionTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()));
            Methods_dont_throw_when_configuration_is_null_implementation(
                () => new ConventionTypeConfiguration(type, new ModelConfiguration()));
        }

        private void Methods_dont_throw_when_configuration_is_null_implementation(Func<ConventionTypeConfiguration> config)
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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.HasEntitySetName(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("entitySetName"), ex.Message);
        }

        [Fact]
        public void HasEntitySetName_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasEntitySetName("EntitySet2");

            Assert.Equal("EntitySet1", innerConfig.EntitySetName);
            Assert.Same(config, result);
        }

        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.Ignore((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), ex.Message);

            Assert.Equal(
                "propertyInfo",
                Assert.Throws<ArgumentNullException>(() => config.Ignore((PropertyInfo)null)).ParamName);
        }

        [Fact]
        public void Ignore_configures()
        {
            var type = new MockType()
                .Property<int>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.IsComplexType();
            config.Ignore("Property1");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_throws_when_property_not_found()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("foo", type.Name),
                Assert.Throws<InvalidOperationException>(() => config.Ignore("foo")).Message);
        }

        [Fact]
        public void Ignore_type_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new ConventionTypeConfiguration(type, innerConfig);

            config.Ignore();

            Assert.True(innerConfig.IsIgnoredType(typeof(LocalEntityType)));
        }

        [Fact]
        public void Ignore_type_throws_with_any_other_configuration()
        {
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.IsComplexType(), "IsComplexType");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Ignore("Property1"), "Ignore");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasEntitySetName("EntitySet1"), "HasEntitySetName");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey("Property1"), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(new[] { "Property1" }), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(config.ClrType.GetProperties().First()), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetProperties().First() }), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property("Property1"), "Property");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property(config.ClrType.GetProperties().First()), "Property");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.NavigationProperty("Property2"), "NavigationProperty");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.NavigationProperty(config.ClrType.GetProperties().Last()), "NavigationProperty");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1"), "ToTable");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1", "Schema1"), "ToTable");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures(), "MapToStoredProcedures");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures(c => { }), "MapToStoredProcedures");
        }

        private void Ignore_type_throws_with_any_other_configuration_implementation(Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            var type = new MockType();
            type.Property<int>("Property1");
            type.Property(type, "Property2");

            Ignore_type_throws_with_any_other_configuration_assert(
                new ConventionTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()),
                configAction, methodName);
            Ignore_type_throws_with_any_other_configuration_assert(
                new ConventionTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()),
                configAction, methodName);
            Ignore_type_throws_with_any_other_configuration_assert(
                new ConventionTypeConfiguration(type, new ModelConfiguration()),
                configAction, methodName);
        }

        private void Ignore_type_throws_with_any_other_configuration_assert(
            ConventionTypeConfiguration config,
            Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            config.Ignore();

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType(methodName, config.ClrType.Name),
                Assert.Throws<InvalidOperationException>(
                    () => configAction(config)).Message);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_IgnoreType(methodName, config.ClrType.Name),
                Assert.Throws<InvalidOperationException>(
                    () => config.Ignore()).Message);
        }

        [Fact]
        public void IsComplexType_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new ConventionTypeConfiguration(type, innerConfig);

            config.IsComplexType();

            Assert.True(innerConfig.IsComplexType(typeof(LocalEntityType)));
        }

        [Fact]
        public void IsComplexType_throws_with_conflicting_configuration()
        {
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasEntitySetName("EntitySet1"), "HasEntitySetName");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey("Property1"), "HasKey");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(new[] { "Property1" }), "HasKey");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(config.ClrType.GetProperties().First()), "HasKey");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetProperties().First() }), "HasKey");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.ToTable("Table1"), "ToTable");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.ToTable("Table1", "Schema1"), "ToTable");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.MapToStoredProcedures(), "MapToStoredProcedures");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.MapToStoredProcedures(c => { }), "MapToStoredProcedures");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.NavigationProperty("Property1"), "NavigationProperty");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.NavigationProperty(config.ClrType.GetProperties().First()), "NavigationProperty");
        }

        private void IsComplexType_throws_with_conflicting_configuration_implementation(Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            var type = new MockType();
            type.Property<int>("Property1");

            IsComplexType_throws_with_conflicting_configuration_assert(
                new ConventionTypeConfiguration(type, () => new EntityTypeConfiguration(type), new ModelConfiguration()),
                configAction, methodName);
            IsComplexType_throws_with_conflicting_configuration_assert(
                new ConventionTypeConfiguration(type, () => new ComplexTypeConfiguration(type), new ModelConfiguration()),
                configAction, methodName);
            IsComplexType_throws_with_conflicting_configuration_assert(
                new ConventionTypeConfiguration(type, new ModelConfiguration()),
                configAction, methodName);
        }

        private void IsComplexType_throws_with_conflicting_configuration_assert(
            ConventionTypeConfiguration config,
            Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            config.IsComplexType();

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType(methodName, config.ClrType.Name),
                Assert.Throws<InvalidOperationException>(
                    () => configAction(config)).Message);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_ConfigurationConflict_ComplexType(methodName, config.ClrType.Name),
                Assert.Throws<InvalidOperationException>(
                    () => config.IsComplexType()).Message);
        }

        [Fact]
        public void Property_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.Property((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), ex.Message);
        }

        [Fact]
        public void Property_throws_when_nonscalar()
        {
            var type = new MockType()
                .Property<object>("NonScalar");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<InvalidOperationException>(
                () => config.Property("Item"));

            Assert.Equal(
                Strings.LightweightEntityConfiguration_NonScalarProperty("Item"),
                ex.Message);
        }

        [Fact]
        public void Property_throws_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("Property1", type.Object.FullName),
                Assert.Throws<InvalidOperationException>(() => config.Property("Property1")).Message);

            Assert.Equal(
                "propertyInfo",
                Assert.Throws<ArgumentNullException>(() => config.Property((PropertyInfo)null)).ParamName);
        }

        [Fact]
        public void Property_returns_configuration()
        {
            var type = new MockType()
                .Property<decimal>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property("Property1");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property1", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<Properties.Primitive.DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void Property_returns_configuration_for_complex_type_properties()
        {
            var type = new MockType()
                .Property<decimal>("Property1");
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.IsComplexType();
            var result = config.Property("Property1");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property1", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<Properties.Primitive.DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void NavigationProperty_evaluates_preconditions()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"),
                Assert.Throws<ArgumentException>(
                    () => config.NavigationProperty((string)null)).Message);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_InvalidNavigationProperty("Property1"),
                Assert.Throws<InvalidOperationException>(
                    () => config.NavigationProperty(type.GetProperty("Property1"))).Message);
        }

        [Fact]
        public void NavigationProperty_returns_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.NavigationProperty(type.GetProperty("NavigationProperty"));

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("NavigationProperty", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(LocalEntityType), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<NavigationPropertyConfiguration>(result.Configuration);
        }

        [Fact]
        public void NavigationProperty_returns_configuration_by_name()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.NavigationProperty("NavigationProperty");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("NavigationProperty", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(LocalEntityType), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<NavigationPropertyConfiguration>(result.Configuration);
        }

        [Fact]
        public void NavigationProperty_throws_when_property_not_found()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("foo", type.Name),
                Assert.Throws<InvalidOperationException>(() => config.NavigationProperty("foo")).Message);

            Assert.Equal(
                "propertyInfo",
                Assert.Throws<ArgumentNullException>(() => config.NavigationProperty((PropertyInfo)null)).ParamName);
        }

        [Fact]
        public void ToTable_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table1");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_is_noop_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1");
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table2");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_handles_dot()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Schema1.Table1");

            Assert.Equal("Schema1", innerConfig.SchemaName);
            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_with_schema_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null, null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_with_schema_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("Table2", "Schema2");

            Assert.Equal("Table1", innerConfig.TableName);
            Assert.Equal("Schema1", innerConfig.SchemaName);
        }

        [Fact]
        public void HasKey_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentException>(
                () => config.HasKey((string)null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), ex.Message);
        }

        [Fact]
        public void HasKey_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property1").HasKey("Property2");

            Assert.Equal(new[] { "Property1", "Property2" }, innerConfig.KeyProperties.Select(p => p.Name));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_PropertyInfo_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(config.ClrType.GetProperties().First())
                .HasKey(config.ClrType.GetProperties().Last());

            Assert.Equal(new[] { "Property1", "Property2" }, innerConfig.KeyProperties.Select(p => p.Name));
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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property2");

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_throws_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("Property2", "T"),
                Assert.Throws<InvalidOperationException>(() => config.HasKey("Property2")).Message);
        }

        [Fact]
        public void HasKey_composite_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.HasKey((string[])null));

            Assert.Equal("propertyNames", ex.ParamName);

            Assert.Equal(
                Strings.CollectionEmpty("keyProperties", "HasKey"),
                Assert.Throws<ArgumentException>(
                    () => config.HasKey(Enumerable.Empty<string>())).Message);
        }

        [Fact]
        public void HasKey_composite_configures_when_unset()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

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
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property2", "Property3" });

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property3"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_is_noop_when_called_twice()
        {
            var type = new MockType()
                .Property<int>("Property1")
                .Property<int>("Property2")
                .Property<int>("Property3");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1" })
                .HasKey(new[] { "Property2", "Property3" });

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property3"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_throws_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("Property1", "T"),
                Assert.Throws<InvalidOperationException>(() => config.HasKey(new[] { "Property1", "Property2" })).Message);
        }

        [Fact]
        public void MapToStoredProcedures_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationStoredProceduresConfiguration);
        }

        [Fact]
        public void ClrType_returns_type()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Same(type.Object, config.ClrType);
        }

        [Fact]
        public void MapToStoredProcedures_with_no_args_should_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationStoredProceduresConfiguration);
        }

        [Fact]
        public void MapToStoredProcedures_with_action_should_invoke_and_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            ConventionModificationStoredProceduresConfiguration configuration = null;

            config.MapToStoredProcedures(c => configuration = c);

            Assert.Same(
                configuration.Configuration,
                innerConfig.ModificationStoredProceduresConfiguration);
        }

        private class LocalEntityType
        {
            public int this[int index]
            {
                get { return 0; }
            }

            public decimal Property1 { get; set; }
            public LocalEntityType NavigationProperty { get; set; }
        }
    }
}
