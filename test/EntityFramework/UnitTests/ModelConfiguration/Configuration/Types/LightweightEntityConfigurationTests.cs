// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class LightweightEntityConfigurationTests
    {
        [Fact]
        public void Ctor_evaluates_preconditions()
        {
            var type = new MockType();

            var ex = Assert.Throws<ArgumentNullException>(
                () => new LightweightEntityConfiguration(null, () => new EntityTypeConfiguration(type)));

            Assert.Equal("type", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(
                () => new LightweightEntityConfiguration(type, null));

            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void Ctor_does_not_invoke_delegate()
        {
            var initialized = false;

            new LightweightEntityConfiguration(
                new MockType(),
                () =>
                {
                    initialized = true;

                    return null;
                });

            Assert.False(initialized);
        }

        [Fact]
        public void HasEntitySetName_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var ex = Assert.Throws<ArgumentException>(
                () => config.HasEntitySetName(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("entitySetName"), ex.Message);
        }

        [Fact]
        public void HasEntitySetName_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var result = config.HasEntitySetName("EntitySet2");

            Assert.Equal("EntitySet1", innerConfig.EntitySetName);
            Assert.Same(config, result);
        }

        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.Ignore("Property1");

            Assert.Equal(1, innerConfig.IgnoredProperties.Count());
            Assert.True(innerConfig.IgnoredProperties.Any(p => p.Name == "Property1"));
        }

        [Fact]
        public void Ignore_is_noop_when_not_exists()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.Ignore("Property1");

            Assert.Empty(innerConfig.IgnoredProperties);
        }

        [Fact]
        public void Property_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var result = config.Property("Property1");

            Assert.IsType<MissingPropertyConfiguration>(result);
        }

        [Fact]
        public void Property_returns_configuration()
        {
            var type = new MockType()
                .Property<decimal>("Property1");
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table1");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_is_noop_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1");
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table2");

            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_handles_dot()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Schema1.Table1");

            Assert.Equal("Schema1", innerConfig.SchemaName);
            Assert.Equal("Table1", innerConfig.TableName);
        }

        [Fact]
        public void ToTable_with_schema_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null, null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_with_schema_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table2", "Schema2");

            Assert.Equal("Table1", innerConfig.TableName);
            Assert.Equal("Schema1", innerConfig.SchemaName);
        }

        [Fact]
        public void HasKey_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var result = config.HasKey("Property1");

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var result = config.HasKey("Property2");

            Assert.Empty(innerConfig.KeyProperties);
            Assert.Same(config, result);
        }

        [Fact]
        public void HasKey_composite_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var result = config.HasKey(new[] { "Property1", "Property2" });

            Assert.Empty(innerConfig.KeyProperties);
            Assert.Same(config, result);
        }

        [Fact]
        public void MapToStoredProcedures_configures_when_unset()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationFunctionsConfiguration);
        }

        [Fact]
        public void ClrType_returns_type()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            Assert.Same(type.Object, config.ClrType);
        }

        [Fact]
        public void MapToStoredProcedures_with_no_args_should_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.MapToStoredProcedures();

            Assert.NotNull(innerConfig.ModificationFunctionsConfiguration);
        }

        [Fact]
        public void MapToStoredProcedures_with_action_should_invoke_and_add_configuration()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

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
