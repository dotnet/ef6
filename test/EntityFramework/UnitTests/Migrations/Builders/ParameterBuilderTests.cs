// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Builders
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ParameterBuilderTests
    {
        [Fact]
        public void Integer_should_add_integer_column_to_table_model()
        {
            var column = new ParameterBuilder().Int();

            Assert.Equal(PrimitiveTypeKind.Int32, column.Type);
        }

        [Fact]
        public void String_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().String();

            Assert.Equal(PrimitiveTypeKind.String, column.Type);
        }

        [Fact]
        public void Binary_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Binary();

            Assert.Equal(PrimitiveTypeKind.Binary, column.Type);
        }

        [Fact]
        public void Boolean_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Boolean();

            Assert.Equal(PrimitiveTypeKind.Boolean, column.Type);
        }

        [Fact]
        public void Byte_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Byte();

            Assert.Equal(PrimitiveTypeKind.Byte, column.Type);
        }

        [Fact]
        public void DateTime_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().DateTime();

            Assert.Equal(PrimitiveTypeKind.DateTime, column.Type);
        }

        [Fact]
        public void Decimal_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Decimal();

            Assert.Equal(PrimitiveTypeKind.Decimal, column.Type);
        }

        [Fact]
        public void Double_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Double();

            Assert.Equal(PrimitiveTypeKind.Double, column.Type);
        }

        [Fact]
        public void Guid_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Guid();

            Assert.Equal(PrimitiveTypeKind.Guid, column.Type);
        }

        [Fact]
        public void Single_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Single();

            Assert.Equal(PrimitiveTypeKind.Single, column.Type);
        }

        [Fact]
        public void Short_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Short();

            Assert.Equal(PrimitiveTypeKind.Int16, column.Type);
        }

        [Fact]
        public void Long_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Long();

            Assert.Equal(PrimitiveTypeKind.Int64, column.Type);
        }

        [Fact]
        public void Time_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Time();

            Assert.Equal(PrimitiveTypeKind.Time, column.Type);
        }

        [Fact]
        public void DateTimeOffset_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().DateTimeOffset();

            Assert.Equal(PrimitiveTypeKind.DateTimeOffset, column.Type);
        }

        [Fact]
        public void Geography_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Geography();

            Assert.Equal(PrimitiveTypeKind.Geography, column.Type);
        }

        [Fact]
        public void Geometry_should_add_column_to_table_model()
        {
            var column = new ParameterBuilder().Geometry();

            Assert.Equal(PrimitiveTypeKind.Geometry, column.Type);
        }

        [Fact]
        public void Can_set_column_facets_from_fluent_method_optional_args()
        {
            var column
                = new ParameterBuilder().String(
                    maxLength: 42,
                    fixedLength: false,
                    unicode: true,
                    name: "Foo",
                    storeType: "Bar",
                    defaultValue: "123",
                    defaultValueSql: "getdate()");

            Assert.NotNull(column);

            var columnModel = column;

            Assert.Equal(42, columnModel.MaxLength);
            Assert.False(columnModel.IsFixedLength.Value);
            Assert.True(columnModel.IsUnicode.Value);
            Assert.Equal("Foo", columnModel.Name);
            Assert.Equal("Bar", columnModel.StoreType);
            Assert.Equal("123", columnModel.DefaultValue);
            Assert.Equal("getdate()", columnModel.DefaultValueSql);
        }
    }
}
