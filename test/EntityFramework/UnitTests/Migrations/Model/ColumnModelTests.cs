namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.SqlClient;
    using Xunit;

    public class ColumnModelTests
    {
        private readonly DbProviderManifest _providerManifest
            = DbProviderServices.GetProviderServices(new SqlConnection()).GetProviderManifest("2008");

        [Fact]
        public void Can_get_and_set_column_properties()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                {
                    Name = "C",
                    IsNullable = true,
                    IsIdentity = true,
                    IsFixedLength = true,
                    IsUnicode = true,
                    MaxLength = 42,
                    Precision = 23,
                    Scale = 1,
                    StoreType = "foobar"
                };

            Assert.Equal("C", column.Name);
            Assert.Equal(PrimitiveTypeKind.Guid, column.Type);
            Assert.True(column.IsNullable.Value);
            Assert.True(column.IsIdentity);
            Assert.True(column.IsFixedLength.Value);
            Assert.True(column.IsUnicode.Value);
            Assert.Equal(42, column.MaxLength);
            Assert.Equal((byte)23, column.Precision);
            Assert.Equal((byte)1, column.Scale);
            Assert.Equal("foobar", column.StoreType);
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_max_length_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { MaxLength = 1 };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { MaxLength = 2 };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { MaxLength = 1 };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { MaxLength = 1 };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_nullable_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsNullable = false };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsNullable = true };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsNullable = false };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsNullable = false };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_unicode_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsUnicode = false };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsUnicode = true };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsUnicode = false };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsUnicode = false };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_fixed_length_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsFixedLength = true };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsFixedLength = false };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String) { IsFixedLength = true };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String) { IsFixedLength = false };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_precision_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal) { Precision = 1 };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal) { Precision = 2 };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal) { Precision = 1 };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal) { Precision = 1 };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_scale_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal) { Scale = 1 };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal) { Scale = 2 };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal) { Scale = 1 };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal);

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal) { Scale = 1 };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_type_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.Int16);
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.Int32);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Int16);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Int64);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Int32);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Int16);

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }
    }
}