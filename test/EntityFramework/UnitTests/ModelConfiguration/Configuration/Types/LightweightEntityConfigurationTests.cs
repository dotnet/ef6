// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.Resources;
    using Moq;
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
        public void EntitySetName_gets_inner_value()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type)
            {
                EntitySetName = "EntitySet1"
            };
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            Assert.Equal("EntitySet1", config.EntitySetName);
        }

        [Fact]
        public void EntitySetName_sets_unset_inner_value()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.EntitySetName = "EntitySet1";

            Assert.Equal("EntitySet1", config.EntitySetName);
        }

        [Fact]
        public void EntitySetName_do_not_set_already_set_inner_value()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type)
            {
                EntitySetName = "EntitySet1"
            };
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.EntitySetName = "EntitySet2";

            Assert.Equal("EntitySet1", config.EntitySetName);
        }

        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.Ignore(null));

            Assert.Equal("propertyInfo", ex.ParamName);
        }

        [Fact]
        public void Ignore_calls_inner_method()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);
            var propertyInfo = new MockPropertyInfo();

            config.Ignore(propertyInfo);

            Assert.Contains(propertyInfo, innerConfig.IgnoredProperties);
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
        public void ToTable_calls_inner_method_when_not_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table1");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
        }

        [Fact]
        public void ToTable_does_not_call_inner_method_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1");

            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table2");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
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
        public void ToTable_with_schema_calls_inner_method_when_not_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table1", "db");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
            Assert.Equal("db", innerConfig.GetTableName().Schema);
        }

        [Fact]
        public void ToTable_with_schema_does_not_call_inner_method_when_set()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            innerConfig.ToTable("Table1", "db");

            var config = new LightweightEntityConfiguration(type, () => innerConfig);

            config.ToTable("Table2", "my");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
            Assert.Equal("db", innerConfig.GetTableName().Schema);
        }

        [Fact]
        public void HasKey_calls_inner_method()
        {
            Type type = new MockType();
            var innerConfig = new Mock<EntityTypeConfiguration>(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig.Object);
            var propertyInfo = new MockPropertyInfo();

            config.HasKey(propertyInfo);

            innerConfig.Verify(c => c.Key(propertyInfo, null), Times.Once());
        }

        [Fact]
        public void ClrType_returns_type()
        {
            var type = new MockType();
            var innerConfig = new EntityTypeConfiguration(type);
            var config = new LightweightEntityConfiguration(type, () => innerConfig);

             Assert.Same(type.Object, config.ClrType);
        }
    }
}
