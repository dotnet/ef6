// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class LightweightEntityConfigurationTests
    {
        [Fact]
        public void Ctor_evaluates_preconditions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new LightweightEntityConfiguration(null));

            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void Ctor_does_not_invoke_delegate()
        {
            var initialized = false;

            new LightweightEntityConfiguration(
                () =>
                {
                    initialized = true;

                    return null;
                });

            Assert.False(initialized);
        }

        [Fact]
        public void Ignore_evaluates_preconditions()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);

            var ex = Assert.Throws<ArgumentNullException>(
                () => config.Ignore(null));

            Assert.Equal("propertyInfo", ex.ParamName);
        }

        [Fact]
        public void Ignore_calls_inner_method()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);
            var propertyInfo = new MockPropertyInfo();

            config.Ignore(propertyInfo);

            Assert.Contains(propertyInfo, innerConfig.IgnoredProperties);
        }

        [Fact]
        public void ToTable_evaluates_preconditions()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_calls_inner_method_when_not_set()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);

            config.ToTable("Table1");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
        }

        [Fact]
        public void ToTable_does_not_call_inner_method_when_set()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            innerConfig.ToTable("Table1");

            var config = new LightweightEntityConfiguration(() => innerConfig);

            config.ToTable("Table2");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
        }

        [Fact]
        public void ToTable_with_schema_evaluates_preconditions()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);

            var ex = Assert.Throws<ArgumentException>(
                () => config.ToTable(null, null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("tableName"), ex.Message);
        }

        [Fact]
        public void ToTable_with_schema_calls_inner_method_when_not_set()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            var config = new LightweightEntityConfiguration(() => innerConfig);

            config.ToTable("Table1", "db");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
            Assert.Equal("db", innerConfig.GetTableName().Schema);
        }

        [Fact]
        public void ToTable_with_schema_does_not_call_inner_method_when_set()
        {
            var innerConfig = new EntityTypeConfiguration(new MockType());
            innerConfig.ToTable("Table1", "db");

            var config = new LightweightEntityConfiguration(() => innerConfig);

            config.ToTable("Table2", "my");

            Assert.Equal("Table1", innerConfig.GetTableName().Name);
            Assert.Equal("db", innerConfig.GetTableName().Schema);
        }
    }
}
