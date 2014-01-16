// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
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

        public class AType
        {
            public int Property1 { get; set; }
            public AType Property2 { get; set; }
            private decimal Property3 { get; set; }
            public int Property4 { get; set; }
            public decimal Property5 { get; set; }
            public object NonScalar { get; set; }
        }

        [Fact]
        public void Methods_dont_throw_when_configuration_is_null()
        {
            var type = typeof(AType);

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
            config().HasKey(config().ClrType.GetRuntimeProperties().First(p => p.IsPublic()));
            config().HasKey(new[] { config().ClrType.GetRuntimeProperties().First(p => p.IsPublic()) });
            config().Property("Property1");
            config().Property(config().ClrType.GetRuntimeProperties().First(p => p.IsPublic()));
            config().Property(new PropertyPath(config().ClrType.GetRuntimeProperties().First(p => p.IsPublic())));
            config().ToTable("Table1");
            config().ToTable("Table1", "Schema1");
            config().MapToStoredProcedures();
            config().MapToStoredProcedures(c => { });
            config().HasTableAnnotation("A", "V");
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
            var type = typeof(AType);

            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.Ignore("Property1");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_configures_private_property()
        {
            var type = typeof(AType);

            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.Ignore("Property3");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property3"));
        }

        [Fact]
        public void Ignore_configures_complex_type_property()
        {
            var type = typeof(AType);

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
                config => config.HasKey(config.ClrType.GetRuntimeProperties().First(p => p.IsPublic())), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetRuntimeProperties().First(p => p.IsPublic()) }), "HasKey");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property("Property1"), "Property");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.Property(config.ClrType.GetRuntimeProperties().First(p => p.IsPublic())), "Property");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.NavigationProperty("Property2"), "NavigationProperty");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.NavigationProperty(config.ClrType.GetRuntimeProperties().Last(p => p.IsPublic())), "NavigationProperty");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1"), "ToTable");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.ToTable("Table1", "Schema1"), "ToTable");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures(), "MapToStoredProcedures");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.MapToStoredProcedures(c => { }), "MapToStoredProcedures");
            Ignore_type_throws_with_any_other_configuration_implementation(
                config => config.HasTableAnnotation("A1", "V1"), "HasTableAnnotation");
        }

        private void Ignore_type_throws_with_any_other_configuration_implementation(Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            var type = typeof(AType);

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
                config => config.HasKey(config.ClrType.GetRuntimeProperties().First(p => p.IsPublic())), "HasKey");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasKey(new[] { config.ClrType.GetRuntimeProperties().First(p => p.IsPublic()) }), "HasKey");
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
                config => config.NavigationProperty(config.ClrType.GetRuntimeProperties().First(p => p.IsPublic())), "NavigationProperty");
            IsComplexType_throws_with_conflicting_configuration_implementation(
                config => config.HasTableAnnotation("A1", "V1"), "HasTableAnnotation");
        }

        private void IsComplexType_throws_with_conflicting_configuration_implementation(Action<ConventionTypeConfiguration> configAction,
            string methodName)
        {
            var type = typeof(AType);

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
            var type = typeof(AType);
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
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("DoesNotExist", type.Name),
                Assert.Throws<InvalidOperationException>(() => config.Property("DoesNotExist")).Message);

            Assert.Equal(
                "propertyInfo",
                Assert.Throws<ArgumentNullException>(() => config.Property((PropertyInfo)null)).ParamName);
        }

        [Fact]
        public void Property_returns_configuration()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property("Property5");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property5", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<Properties.Primitive.DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void Property_returns_configuration_for_private_property()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property("Property3");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property3", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(decimal), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<Properties.Primitive.DecimalPropertyConfiguration>(result.Configuration());
        }

        [Fact]
        public void Property_returns_configuration_for_complex_type_properties()
        {
            var type = typeof(AType);
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            config.IsComplexType();
            var result = config.Property("Property5");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("Property5", result.ClrPropertyInfo.Name);
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
                    () => config.NavigationProperty(type.GetDeclaredProperty("Property1"))).Message);
        }

        [Fact]
        public void NavigationProperty_returns_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.NavigationProperty(type.GetDeclaredProperty("NavigationProperty"));

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
        public void NavigationProperty_returns_configuration_by_name_for_private_property()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.NavigationProperty("PrivateNavigationProperty");

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("PrivateNavigationProperty", result.ClrPropertyInfo.Name);
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
        public void HasAnnotation_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => config.HasTableAnnotation(null, null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => config.HasTableAnnotation(" ", null)).Message);

            Assert.Equal(
                Strings.BadAnnotationName("Cheese:Pickle"),
                Assert.Throws<ArgumentException>(() => config.HasTableAnnotation("Cheese:Pickle", null)).Message);
        }

        [Fact]
        public void HasAnnotation_configures_only_annotations_that_have_not_already_been_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.SetAnnotation("A1", "V1");
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasTableAnnotation("A1", "V1B").HasTableAnnotation("A2", "V2");

            Assert.Equal("V1", innerConfig.Annotations["A1"]);
            Assert.Equal("V2", innerConfig.Annotations["A2"]);
            Assert.Same(config, result);
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
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property1").HasKey("Property4");

            Assert.Equal(new[] { "Property1", "Property4" }, innerConfig.KeyProperties.Select(p => p.Name));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_configures_with_private_property()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property1").HasKey("Property3");

            Assert.Equal(new[] { "Property1", "Property3" }, innerConfig.KeyProperties.Select(p => p.Name));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_PropertyInfo_configures_when_unset()
        {
            var innerConfig = new EntityTypeConfiguration(typeof(AType2));
            var config = new ConventionTypeConfiguration(typeof(AType2), () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(config.ClrType.GetRuntimeProperties().First(p => p.IsPublic()))
                .HasKey(config.ClrType.GetRuntimeProperties().Last(p => p.IsPublic()));

            Assert.Equal(new[] { "Property1", "Property2" }, innerConfig.KeyProperties.Select(p => p.Name));
            Assert.Same(config, result);
        }

        public class AType2
        {
            public int Property1 { get; set; }
            public int Property2 { get; set; }
        }

        [Fact]
        public void HasKey_is_noop_when_set()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.Key(new[] { type.GetDeclaredProperty("Property1") });
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey("Property2");

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_throws_when_not_exists()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("DoesNotExist", "AType"),
                Assert.Throws<InvalidOperationException>(() => config.HasKey("DoesNotExist")).Message);
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
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1", "Property4" });

            Assert.Equal(2, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property4"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_configures_with_private_property_when_unset()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1", "Property3" });

            Assert.Equal(2, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property3"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_is_noop_when_set()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.Key(new[] { type.GetDeclaredProperty("Property1") });
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property2", "Property4" });

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property4"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_is_noop_when_called_twice()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(new[] { "Property1" })
                .HasKey(new[] { "Property2", "Property4" });

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.False(innerConfig.KeyProperties.Any(p => p.Name == "Property4"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_throws_when_not_exists()
        {
            var type = typeof(AType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                Strings.NoSuchProperty("DoesNotExist1", "AType"),
                Assert.Throws<InvalidOperationException>(() => config.HasKey(new[] { "DoesNotExist1", "DoesNotExist2" })).Message);
        }

        [Fact]
        public void MapToStoredProcedures_configures_when_unset()
        {
            var type = typeof(AType);
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
            private LocalEntityType PrivateNavigationProperty { get; set; }
        }
    }
}
