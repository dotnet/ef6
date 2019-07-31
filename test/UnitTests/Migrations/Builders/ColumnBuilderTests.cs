// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Builders
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Model;
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

        [Fact]
        public void Annotations_are_added_to_model_when_passed_to_any_builder_method()
        {
            var builder = new ColumnBuilder();

            var annotations = new Dictionary<string, AnnotationValues>
            {
                { "A1", new AnnotationValues("O1", "N1") },
                { "A2", new AnnotationValues("O2", "N2") }
            };
            
            VerifyAnnotations(builder.Binary(annotations: annotations));
            VerifyAnnotations(builder.Boolean(annotations: annotations));
            VerifyAnnotations(builder.Byte(annotations: annotations));
            VerifyAnnotations(builder.DateTime(annotations: annotations));
            VerifyAnnotations(builder.DateTimeOffset(annotations: annotations));
            VerifyAnnotations(builder.Decimal(annotations: annotations));
            VerifyAnnotations(builder.Double(annotations: annotations));
            VerifyAnnotations(builder.HierarchyId(annotations: annotations));
            VerifyAnnotations(builder.Geography(annotations: annotations));
            VerifyAnnotations(builder.Geometry(annotations: annotations));
            VerifyAnnotations(builder.Guid(annotations: annotations));
            VerifyAnnotations(builder.Int(annotations: annotations));
            VerifyAnnotations(builder.Long(annotations: annotations));
            VerifyAnnotations(builder.Short(annotations: annotations));
            VerifyAnnotations(builder.Single(annotations: annotations));
            VerifyAnnotations(builder.String(annotations: annotations));
            VerifyAnnotations(builder.Time(annotations: annotations));
        }

        private static void VerifyAnnotations(ColumnModel model)
        {
            Assert.Equal(2, model.Annotations.Count);
            Assert.Equal("O1", model.Annotations["A1"].OldValue);
            Assert.Equal("N1", model.Annotations["A1"].NewValue);
            Assert.Equal("O2", model.Annotations["A2"].OldValue);
            Assert.Equal("N2", model.Annotations["A2"].NewValue);
        }

        [Fact]
        public void Annotations_are_empty_but_not_null_in_model_if_not_passed_to_builder()
        {
            var builder = new ColumnBuilder();

            Assert.Equal(0, builder.Binary().Annotations.Count);
            Assert.Equal(0, builder.Boolean().Annotations.Count);
            Assert.Equal(0, builder.Byte().Annotations.Count);
            Assert.Equal(0, builder.DateTime().Annotations.Count);
            Assert.Equal(0, builder.DateTimeOffset().Annotations.Count);
            Assert.Equal(0, builder.Decimal().Annotations.Count);
            Assert.Equal(0, builder.Double().Annotations.Count);
            Assert.Equal(0, builder.Geography().Annotations.Count);
            Assert.Equal(0, builder.Geometry().Annotations.Count);
            Assert.Equal(0, builder.Guid().Annotations.Count);
            Assert.Equal(0, builder.Int().Annotations.Count);
            Assert.Equal(0, builder.Long().Annotations.Count);
            Assert.Equal(0, builder.Short().Annotations.Count);
            Assert.Equal(0, builder.Single().Annotations.Count);
            Assert.Equal(0, builder.String().Annotations.Count);
            Assert.Equal(0, builder.Time().Annotations.Count);
        }
    }
}
