// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;
    
    public class ConventionTypeConfigurationOfTypeTests
    {
        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<object>(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.Ignore<object>(null));

            Assert.Equal("propertyExpression", ex.ParamName);
        }

        [Fact]
        public void Ignore_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.Ignore(t => t.Property1);

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_configures_complex_type_properties()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.Ignore(t => t.Property1);

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_type_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, innerConfig);

            config.Ignore();

            Assert.True(innerConfig.IsIgnoredType(typeof(LocalEntityType)));
        }

        [Fact]
        public void IsComplexType_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new ModelConfiguration();
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, innerConfig);

            config.IsComplexType();

            Assert.True(innerConfig.IsComplexType(typeof(LocalEntityType)));
        }

        [Fact]
        public void Property_evaluates_preconditions()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<object>(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.Property<object>(null));

            Assert.Equal("propertyExpression", ex.ParamName);
        }

        [Fact]
        public void Property_returns_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property(e => e.Property1);

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
            var type = typeof(LocalEntityType);
            var innerConfig = new ComplexTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            var result = config.Property(e => e.Property1);

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
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            Assert.Equal(
                "propertyExpression",
                Assert.Throws<ArgumentNullException>(
                    () => config.NavigationProperty<object>(null)).ParamName);

            Assert.Equal(
                Strings.LightweightEntityConfiguration_InvalidNavigationProperty("Property1"),
                Assert.Throws<InvalidOperationException>(
                    () => config.NavigationProperty(e => e.Property1)).Message);
        }

        [Fact]
        public void NavigationProperty_returns_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            var result = config.NavigationProperty(e => e.NavigationProperty);

            Assert.NotNull(result);
            Assert.NotNull(result.ClrPropertyInfo);
            Assert.Equal("NavigationProperty", result.ClrPropertyInfo.Name);
            Assert.Equal(typeof(LocalEntityType), result.ClrPropertyInfo.PropertyType);
            Assert.NotNull(result.Configuration);
            Assert.IsType<NavigationPropertyConfiguration>(result.Configuration);
        }

        [Fact]
        public void HasKey_evaluates_preconditions()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<object>(type, () => innerConfig, new ModelConfiguration());

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.HasKey<object>(null));

            Assert.Equal("keyExpression", ex.ParamName);
        }

        [Fact]
        public void HasKey_configures_when_unset()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(e => e.Property1);

            Assert.Equal(1, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_configures_when_unset()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            var result = config.HasKey(
                e => new
                    {
                        e.Property1,
                        e.Property2
                    });

            Assert.Equal(2, innerConfig.KeyProperties.Count());
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property1"));
            Assert.True(innerConfig.KeyProperties.Any(p => p.Name == "Property2"));
            Assert.Same(config, result);
        }

        [Fact]
        public void HasEntitySetName_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.HasEntitySetName("foo");

            Assert.Equal("foo", innerConfig.EntitySetName);
        }

        [Fact]
        public void MapToStoredProcedures_with_no_args_should_add_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.MapToStoredProcedures();

            Assert.True(innerConfig.ModificationStoredProceduresConfiguration != null);
        }

        [Fact]
        public void MapToStoredProcedures_with_action_should_invoke_and_add_configuration()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            ModificationStoredProceduresConfiguration<LocalEntityType> configuration = null;

            config.MapToStoredProcedures(c => configuration = c);

            Assert.Same(
                configuration.Configuration,
                innerConfig.ModificationStoredProceduresConfiguration);
        }

        [Fact]
        public void ToTable_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("foo");

            Assert.Equal("foo", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_with_schema_configures()
        {
            var type = typeof(LocalEntityType);
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new ConventionTypeConfiguration<LocalEntityType>(type, () => innerConfig, new ModelConfiguration());

            config.ToTable("foo", "bar");

            Assert.Equal("foo", innerConfig.TableName);
        }

        private class LocalEntityType
        {
            public decimal Property1 { get; set; }
            public int Property2 { get; set; }
            public LocalEntityType NavigationProperty { get; set; }
        }
    }
}
