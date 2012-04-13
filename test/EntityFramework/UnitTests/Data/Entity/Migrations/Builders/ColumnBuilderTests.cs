namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Spatial;
    using Xunit;

    public class ColumnBuilderTests
    {
        [Fact]
        public void Integer_should_add_integer_column_to_table_model()
        {
            var column = new ColumnBuilder().Int();

            Assert.Equal(PrimitiveTypeKind.Int32, column.Type);
            Assert.Equal(0, column.ClrDefaultValue);
        }

        [Fact]
        public void String_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().String();

            Assert.Equal(PrimitiveTypeKind.String, column.Type);
            Assert.Equal(string.Empty, column.ClrDefaultValue);
        }

        [Fact]
        public void Binary_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Binary();

            Assert.Equal(PrimitiveTypeKind.Binary, column.Type);
            Assert.Equal(new byte[] { }, (byte[])column.ClrDefaultValue);
        }

        [Fact]
        public void Boolean_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Boolean();

            Assert.Equal(PrimitiveTypeKind.Boolean, column.Type);
            Assert.Equal(false, column.ClrDefaultValue);
        }

        [Fact]
        public void Byte_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Byte();

            Assert.Equal(PrimitiveTypeKind.Byte, column.Type);
            Assert.Equal((byte)0, column.ClrDefaultValue);
        }

        [Fact]
        public void DateTime_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().DateTime();

            Assert.Equal(PrimitiveTypeKind.DateTime, column.Type);
            Assert.Equal(DateTime.MinValue, column.ClrDefaultValue);
        }

        [Fact]
        public void Decimal_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Decimal();

            Assert.Equal(PrimitiveTypeKind.Decimal, column.Type);
            Assert.Equal(0m, column.ClrDefaultValue);
        }

        [Fact]
        public void Double_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Double();

            Assert.Equal(PrimitiveTypeKind.Double, column.Type);
            Assert.Equal(0d, column.ClrDefaultValue);
        }

        [Fact]
        public void Guid_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Guid();

            Assert.Equal(PrimitiveTypeKind.Guid, column.Type);
            Assert.Equal(Guid.Empty, column.ClrDefaultValue);
        }

        [Fact]
        public void Single_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Single();

            Assert.Equal(PrimitiveTypeKind.Single, column.Type);
            Assert.Equal(0f, column.ClrDefaultValue);
        }

        [Fact]
        public void Short_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Short();

            Assert.Equal(PrimitiveTypeKind.Int16, column.Type);
            Assert.Equal((short)0, column.ClrDefaultValue);
        }

        [Fact]
        public void Long_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Long();

            Assert.Equal(PrimitiveTypeKind.Int64, column.Type);
            Assert.Equal(0L, column.ClrDefaultValue);
        }

        [Fact]
        public void Time_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Time();

            Assert.Equal(PrimitiveTypeKind.Time, column.Type);
            Assert.Equal(TimeSpan.Zero, column.ClrDefaultValue);
        }

        [Fact]
        public void DateTimeOffset_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().DateTimeOffset();

            Assert.Equal(PrimitiveTypeKind.DateTimeOffset, column.Type);
            Assert.Equal(DateTimeOffset.MinValue, column.ClrDefaultValue);
        }

        [Fact]
        public void Geography_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Geography();

            Assert.Equal(PrimitiveTypeKind.Geography, column.Type);
            Assert.True(DbGeography.FromText("POINT(0 0)").SpatialEquals((DbGeography)column.ClrDefaultValue));
        }

        [Fact]
        public void Geometry_should_add_column_to_table_model()
        {
            var column = new ColumnBuilder().Geometry();

            Assert.Equal(PrimitiveTypeKind.Geometry, column.Type);
            Assert.True(DbGeometry.FromText("POINT(0 0)").SpatialEquals((DbGeometry)column.ClrDefaultValue));
        }

        [Fact]
        public void Can_set_column_facets_from_fluent_method_optional_args()
        {
            var column
                = new ColumnBuilder().String(
                    nullable: false,
                    maxLength: 42,
                    fixedLength: false,
                    unicode: true,
                    name: "Foo",
                    storeType: "Bar",
                    defaultValue: "123",
                    defaultValueSql: "getdate()");

            Assert.NotNull(column);

            var columnModel = column;

            Assert.False(columnModel.IsNullable.Value);
            Assert.Equal(42, columnModel.MaxLength);
            Assert.False(columnModel.IsFixedLength.Value);
            Assert.True(columnModel.IsUnicode.Value);
            Assert.Equal("Foo", columnModel.Name);
            Assert.Equal("Bar", columnModel.StoreType);
            Assert.Equal("123", columnModel.DefaultValue);
            Assert.Equal("getdate()", columnModel.DefaultValueSql);
        }

        [Fact]
        public void Can_set_identity_facet_from_fluent_method_optional_args()
        {
            var column = new ColumnBuilder().Int(identity: true);

            Assert.NotNull(column);

            var columnModel = column;

            Assert.True(columnModel.IsIdentity);
        }
    }
}