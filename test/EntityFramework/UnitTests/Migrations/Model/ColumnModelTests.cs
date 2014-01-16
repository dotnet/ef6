// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.SqlClient;
    using System.Linq;
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
                    StoreType = "goobar"
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
            Assert.Equal("goobar", column.StoreType);
        }

        [Fact]
        public void Can_get_and_set_annotations()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Guid);

            Assert.Empty(column.Annotations);

            column.Annotations = new Dictionary<string, AnnotationValues> { { "A1", new AnnotationValues("V1", "V2") } };

            Assert.Equal("V1", column.Annotations["A1"].OldValue);
            Assert.Equal("V2", column.Annotations["A1"].NewValue);

            column.Annotations = null;

            Assert.Empty(column.Annotations);
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_max_length_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    MaxLength = 1
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    MaxLength = 2
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    MaxLength = 1
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    MaxLength = 1
                };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_nullable_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsNullable = false
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsNullable = true
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsNullable = false
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsNullable = false
                };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_unicode_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsUnicode = false
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsUnicode = true
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsUnicode = false
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsUnicode = false
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_fixed_length_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsFixedLength = true
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsFixedLength = false
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsFixedLength = true
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.String);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.String)
                {
                    IsFixedLength = false
                };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_precision_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Precision = 1
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Precision = 2
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Precision = 1
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal);

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Precision = 1
                };

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));
        }

        [Fact]
        public void IsNarrowerThan_should_return_true_when_scale_narrower()
        {
            var columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Scale = 1
                };
            var columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Scale = 2
                };

            Assert.True(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Scale = 1
                };
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal);

            Assert.False(columnModel1.IsNarrowerThan(columnModel2, _providerManifest));

            columnModel1 = new ColumnModel(PrimitiveTypeKind.Decimal);
            columnModel2 = new ColumnModel(PrimitiveTypeKind.Decimal)
                {
                    Scale = 1
                };

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

        [Fact] // CodePlex 478
        public void IsNarrowerThan_should_handle_every_supported_primitive_type()
        {
            var booleanColumnModel = new ColumnModel(PrimitiveTypeKind.Boolean);
            foreach (var typeKind in Enum.GetValues(typeof(PrimitiveTypeKind))
                                         .OfType<PrimitiveTypeKind>()
                                         .Where(t => t != PrimitiveTypeKind.SByte))
            {
                booleanColumnModel.IsNarrowerThan(new ColumnModel(typeKind), _providerManifest);
            }
        }

        [Fact]
        public void ToFacetValues_returns_empty_by_default()
        {
            var columnModel = new ColumnModel(PrimitiveTypeKind.Boolean);
            Assert.True(FacetValuesHelpers.Equal<int>(new FacetValues(), columnModel.ToFacetValues()));
        }

        [Fact]
        public void ToFacetValues_returns_specified_facet_values()
        {
            var columnModel = new ColumnModel(PrimitiveTypeKind.Boolean)
            {
                DefaultValue = 0.1,
                IsFixedLength = true,
                IsIdentity = true,
                IsNullable = true,
                IsUnicode = true,
                MaxLength = 64,
                Precision = 5,
                Scale = 3
            };

            var expectedFacetValues = new FacetValues()
            {
                DefaultValue = 0.1,
                FixedLength = true,
                StoreGeneratedPattern = StoreGeneratedPattern.Identity,
                Nullable = true,
                Unicode = true,
                MaxLength = (int?)64,
                Precision = (byte?)5,
                Scale = (byte?)3
            };

            Assert.True(FacetValuesHelpers.Equal<double>(expectedFacetValues, columnModel.ToFacetValues()));
        }
    }
}
