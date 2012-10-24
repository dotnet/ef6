// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Moq;
    using Xunit;

    public class LightweightPropertyConfigurationTests
    {
        [Fact]
        public void Ctor_evaluates_preconditions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new LightweightPropertyConfiguration(null, () => new PrimitivePropertyConfiguration()));

            Assert.Equal("propertyInfo", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(
                () => new LightweightPropertyConfiguration(new MockPropertyInfo(), null));

            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void Ctor_does_not_invoke_delegate()
        {
            var initialized = false;

            new LightweightPropertyConfiguration(
                new MockPropertyInfo(),
                () =>
                {
                    initialized = true;

                    return null;
                });

            Assert.False(initialized);
        }

        [Fact]
        public void Properties_get_inner_values()
        {
            var innerConfig = new PrimitivePropertyConfiguration
                {
                    ColumnName = "Column1",
                    ColumnOrder = 1,
                    ColumnType = "int",
                    ConcurrencyMode = ConcurrencyMode.None,
                    DatabaseGeneratedOption = DatabaseGeneratedOption.None,
                    IsNullable = false
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            Assert.Equal("Column1", config.ColumnName);
            Assert.Equal(1, config.ColumnOrder);
            Assert.Equal("int", config.ColumnType);
            Assert.Equal(ConcurrencyMode.None, config.ConcurrencyMode);
            Assert.Equal(DatabaseGeneratedOption.None, config.DatabaseGeneratedOption);
            Assert.Equal(false, config.IsNullable);
            Assert.Equal(null, config.IsUnicode);
            Assert.Equal(null, config.IsFixedLength);
            Assert.Equal(null, config.MaxLength);
            Assert.Equal(null, config.IsMaxLength);
            Assert.Equal(null, config.Scale);
            Assert.Equal(null, config.Precision);
            Assert.Equal(null, config.IsRowVersion);
        }

        [Fact]
        public void Properties_get_inner_values_when_binary()
        {
            var innerConfig = new BinaryPropertyConfiguration
                {
                    IsFixedLength = false,
                    MaxLength = 256,
                    IsMaxLength = false,
                    IsRowVersion = false
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsRowVersion);
        }

        [Fact]
        public void Properties_get_inner_values_when_dateTime()
        {
            var innerConfig = new DateTimePropertyConfiguration
                {
                    Precision = 8
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_get_inner_values_when_decimal()
        {
            var innerConfig = new DecimalPropertyConfiguration
                {
                    Scale = 2,
                    Precision = 8
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            Assert.Equal<byte?>(2, config.Scale);
            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_get_inner_values_when_string()
        {
            var innerConfig = new StringPropertyConfiguration
                {
                    IsFixedLength = false,
                    MaxLength = 256,
                    IsMaxLength = false,
                    IsUnicode = false
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsUnicode);
        }

        [Fact]
        public void Properties_set_applicable_unset_inner_values()
        {
            var innerConfig = new PrimitivePropertyConfiguration();
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.ColumnName = "Column1";
            config.ColumnOrder = 1;
            config.ColumnType = "int";
            config.ConcurrencyMode = ConcurrencyMode.None;
            config.DatabaseGeneratedOption = DatabaseGeneratedOption.None;
            config.IsNullable = false;
            config.IsUnicode = false;
            config.IsFixedLength = false;
            config.MaxLength = 255;
            config.IsMaxLength = false;
            config.Scale = 2;
            config.Precision = 8;
            config.IsRowVersion = false;

            Assert.Equal("Column1", config.ColumnName);
            Assert.Equal(1, config.ColumnOrder);
            Assert.Equal("int", config.ColumnType);
            Assert.Equal(ConcurrencyMode.None, config.ConcurrencyMode);
            Assert.Equal(DatabaseGeneratedOption.None, config.DatabaseGeneratedOption);
            Assert.Equal(false, config.IsNullable);
            Assert.Equal(null, config.IsUnicode);
            Assert.Equal(null, config.IsFixedLength);
            Assert.Equal(null, config.MaxLength);
            Assert.Equal(null, config.IsMaxLength);
            Assert.Equal(null, config.Scale);
            Assert.Equal(null, config.Precision);
            Assert.Equal(null, config.IsRowVersion);
        }

        [Fact]
        public void Properties_set_unset_inner_values_when_binary()
        {
            var innerConfig = new BinaryPropertyConfiguration();
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.IsFixedLength = false;
            config.MaxLength = 256;
            config.IsMaxLength = false;
            config.IsRowVersion = false;

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsRowVersion);
        }

        [Fact]
        public void Properties_set_unset_inner_values_when_dateTime()
        {
            var innerConfig = new DateTimePropertyConfiguration();
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.Precision = 8;

            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_set_unset_inner_values_when_decimal()
        {
            var innerConfig = new DecimalPropertyConfiguration();
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.Scale = 2;
            config.Precision = 8;

            Assert.Equal<byte?>(2, config.Scale);
            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_set_unset_inner_values_when_string()
        {
            var innerConfig = new StringPropertyConfiguration();
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.IsFixedLength = false;
            config.MaxLength = 256;
            config.IsMaxLength = false;
            config.IsUnicode = false;

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsUnicode);
        }

        [Fact]
        public void Properties_do_not_set_already_set_or_nonapplicable_inner_values()
        {
            var innerConfig = new PrimitivePropertyConfiguration
                {
                    ColumnName = "Column1",
                    ColumnOrder = 1,
                    ColumnType = "int",
                    ConcurrencyMode = ConcurrencyMode.None,
                    DatabaseGeneratedOption = DatabaseGeneratedOption.None,
                    IsNullable = false
                };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.ColumnName = "Column2";
            config.ColumnOrder = 2;
            config.ColumnType = "long";
            config.ConcurrencyMode = ConcurrencyMode.Fixed;
            config.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;
            config.IsNullable = true;
            config.IsUnicode = true;
            config.IsFixedLength = true;
            config.MaxLength = -1;
            config.IsMaxLength = true;
            config.Scale = 4;
            config.Precision = 16;
            config.IsRowVersion = true;

            Assert.Equal("Column1", config.ColumnName);
            Assert.Equal(1, config.ColumnOrder);
            Assert.Equal("int", config.ColumnType);
            Assert.Equal(ConcurrencyMode.None, config.ConcurrencyMode);
            Assert.Equal(DatabaseGeneratedOption.None, config.DatabaseGeneratedOption);
            Assert.Equal(false, config.IsNullable);
            Assert.Equal(null, config.IsUnicode);
            Assert.Equal(null, config.IsFixedLength);
            Assert.Equal(null, config.MaxLength);
            Assert.Equal(null, config.IsMaxLength);
            Assert.Equal(null, config.Scale);
            Assert.Equal(null, config.Precision);
            Assert.Equal(null, config.IsRowVersion);
        }

        [Fact]
        public void Properties_do_not_set_already_set_inner_values_when_binary()
        {
            var innerConfig = new BinaryPropertyConfiguration
            {
                IsFixedLength = false,
                MaxLength = 256,
                IsMaxLength = false,
                IsRowVersion = false
            };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.IsFixedLength = true;
            config.MaxLength = -1;
            config.IsMaxLength = true;
            config.IsRowVersion = true;

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsRowVersion);
        }

        [Fact]
        public void Properties_do_not_set_already_set_inner_values_when_dateTime()
        {
            var innerConfig = new DateTimePropertyConfiguration
            {
                Precision = 8
            };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.Precision = 16;

            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_do_not_set_already_set_inner_values_when_decimal()
        {
            var innerConfig = new DecimalPropertyConfiguration
            {
                Scale = 2,
                Precision = 8
            };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.Scale = 4;
            config.Precision = 16;

            Assert.Equal<byte?>(2, config.Scale);
            Assert.Equal<byte?>(8, config.Precision);
        }

        [Fact]
        public void Properties_do_not_set_already_set_inner_values_when_string()
        {
            var innerConfig = new StringPropertyConfiguration
            {
                IsFixedLength = false,
                MaxLength = 256,
                IsMaxLength = false,
                IsUnicode = false
            };
            var config = new LightweightPropertyConfiguration(new MockPropertyInfo(), () => innerConfig);

            config.IsFixedLength = true;
            config.MaxLength = -1;
            config.IsMaxLength = true;
            config.IsUnicode = true;

            Assert.Equal(false, config.IsFixedLength);
            Assert.Equal(256, config.MaxLength);
            Assert.Equal(false, config.IsMaxLength);
            Assert.Equal(false, config.IsUnicode);
        }

        [Fact]
        public void IsKey_calls_entity_configuration_key_for_property()
        {
            var typeConfig = new Mock<EntityTypeConfiguration>((Type)new MockType());
            var innerConfig = new PrimitivePropertyConfiguration
                {
                    TypeConfiguration = typeConfig.Object
                };
            var propertyInfo = new MockPropertyInfo();
            var config = new LightweightPropertyConfiguration(propertyInfo, () => innerConfig);

            config.IsKey();

            typeConfig.Verify(e => e.Key(propertyInfo, null), Times.Once());
        }

        [Fact]
        public void ClrPropertyInfo_returns_propertyInfo()
        {
            var propertyInfo = new MockPropertyInfo();
            var innerConfig = new PrimitivePropertyConfiguration();
            var config = new LightweightPropertyConfiguration(propertyInfo, () => innerConfig);

            Assert.Same(propertyInfo.Object, config.ClrPropertyInfo);
        }
    }
}
