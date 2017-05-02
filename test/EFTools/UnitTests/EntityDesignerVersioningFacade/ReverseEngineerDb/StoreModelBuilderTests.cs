// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery;
    using Moq;
    using Xunit;

    public class StoreModelBuilderTests
    {
        private static readonly IDbDependencyResolver DependencyResolver;

        static StoreModelBuilderTests()
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(SqlProviderServices.Instance);

            DependencyResolver = mockResolver.Object;
        }

        internal static TableDetailsRow CreateRow(
            string catalog = null, string schema = null, string table = null,
            string columnName = null, int? ordinal = null, bool isNullable = true,
            string dataType = null,
            int? maximumLength = null, int? precision = null,
            int? dateTimePrecision = null,
            int? scale = null, bool? isIdentiy = null,
            bool? isServerGenerated = null, bool isPrimaryKey = false)
        {
            var tableDetailsRow = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Action<TableDetailsRow, string, object> setColumnValue =
                (row, column, value) =>
                    {
                        if (value != null)
                        {
                            row[column] = value;
                        }
                    };

            setColumnValue(tableDetailsRow, "CatalogName", catalog);
            setColumnValue(tableDetailsRow, "SchemaName", schema);
            setColumnValue(tableDetailsRow, "TableName", table);
            setColumnValue(tableDetailsRow, "ColumnName", columnName);
            setColumnValue(tableDetailsRow, "Ordinal", ordinal);
            setColumnValue(tableDetailsRow, "IsNullable", isNullable);
            setColumnValue(tableDetailsRow, "DataType", dataType);
            setColumnValue(tableDetailsRow, "MaximumLength", maximumLength);
            setColumnValue(tableDetailsRow, "Precision", precision);
            setColumnValue(tableDetailsRow, "DateTimePrecision", dateTimePrecision);
            setColumnValue(tableDetailsRow, "Scale", scale);
            setColumnValue(tableDetailsRow, "IsIdentity", isIdentiy);
            setColumnValue(tableDetailsRow, "IsServerGenerated", isServerGenerated);
            setColumnValue(tableDetailsRow, "IsPrimaryKey", isPrimaryKey);

            return tableDetailsRow;
        }

        internal static RelationshipDetailsRow CreateRelationshipDetailsRow(
            string id, string name, int ordinal, bool isCascadeDelete,
            string pkCatalog, string pkSchema, string pkTable, string pkColumn,
            string fkCatalog, string fkSchema, string fkTable, string fkColumn)
        {
            var relationshipDetailsRow = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Action<RelationshipDetailsRow, string, object> setColumnValue =
                (row, column, value) =>
                    {
                        if (value != null)
                        {
                            row[column] = value;
                        }
                    };

            setColumnValue(relationshipDetailsRow, "RelationshipId", id);
            setColumnValue(relationshipDetailsRow, "RelationshipName", name);
            setColumnValue(relationshipDetailsRow, "Ordinal", ordinal);
            setColumnValue(relationshipDetailsRow, "IsCascadeDelete", isCascadeDelete);
            setColumnValue(relationshipDetailsRow, "PkCatalog", pkCatalog);
            setColumnValue(relationshipDetailsRow, "PkSchema", pkSchema);
            setColumnValue(relationshipDetailsRow, "PkTable", pkTable);
            setColumnValue(relationshipDetailsRow, "PkColumn", pkColumn);
            setColumnValue(relationshipDetailsRow, "FkCatalog", fkCatalog);
            setColumnValue(relationshipDetailsRow, "FkSchema", fkSchema);
            setColumnValue(relationshipDetailsRow, "FkTable", fkTable);
            setColumnValue(relationshipDetailsRow, "FkColumn", fkColumn);

            return relationshipDetailsRow;
        }

        private static FunctionDetailsRowView CreateFunctionDetailsRow(
            string catalog = null,
            string schema = null,
            string functionName = null, string returnTypeName = null, bool isAggregate = false,
            bool isComposable = false, bool isBuiltIn = false, bool isNiladic = false, bool isTvf = false,
            string paramName = null, string paramTypeName = null, string parameterDirection = null)
        {
            return new FunctionDetailsV3RowView(
                new[]
                    {
                        (object)catalog ?? DBNull.Value,
                        (object)schema ?? DBNull.Value,
                        (object)functionName ?? DBNull.Value,
                        (object)returnTypeName ?? DBNull.Value,
                        isAggregate,
                        isComposable,
                        isBuiltIn,
                        isNiladic,
                        isTvf,
                        (object)paramName ?? DBNull.Value,
                        (object)paramTypeName ?? DBNull.Value,
                        (object)parameterDirection ?? DBNull.Value
                    });
        }

        internal static StoreModelBuilder CreateStoreModelBuilder(
            string providerInvariantName = "System.Data.SqlClient",
            string providerManifestToken = "2008",
            Version targetEntityFrameworkVersion = null,
            string namespaceName = "myModel",
            bool generateForeignKeyProperties = false)
        {
            return new StoreModelBuilder(
                providerInvariantName,
                providerManifestToken,
                targetEntityFrameworkVersion ?? EntityFrameworkVersion.Version3,
                namespaceName,
                DependencyResolver,
                generateForeignKeyProperties);
        }

        public class CreatePropertyTests
        {
            [Fact]
            public void CreateProperty_creates_default_property_for_Int32_store_type()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(columnName: "IntColumn", dataType: "int"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal("IntColumn", property.Name);
                Assert.Equal(PrimitiveTypeKind.Int32, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(StoreGeneratedPattern.None, property.StoreGeneratedPattern);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_respects_IsNullable_column()
            {
                foreach (var isNullable in new[] { true, false })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(columnName: "IntColumn", dataType: "int", isNullable: isNullable),
                                errors);

                    Assert.NotNull(property);
                    Assert.Equal("IntColumn", property.Name);
                    Assert.Equal(PrimitiveTypeKind.Int32, property.PrimitiveType.PrimitiveTypeKind);
                    Assert.Equal(isNullable, property.Nullable);
                }
            }

            [Fact]
            public void CreateProperty_returns_error_for_null_property_type()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow("catalog", "schema", "table", "IntColumn", dataType: null),
                            errors);

                Assert.Null(property);
                Assert.Equal(1, errors.Count);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedDataTypeUnknownType,
                        "IntColumn",
                        "catalog.schema.table"),
                    errors.Single().Message);
                Assert.Equal(6005, errors.Single().ErrorCode);
            }

            [Fact]
            public void CreateProperty_returns_error_for_unknown_property_type()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow("catalog", "schema", "table", "IntColumn", dataType: "invalid-type"),
                            errors);

                Assert.Null(property);
                Assert.Equal(1, errors.Count);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedDataType,
                        "invalid-type",
                        "catalog.schema.table",
                        "IntColumn"),
                    errors.Single().Message);
                Assert.Equal(6005, errors.Single().ErrorCode);
            }

            [Fact]
            public void CreateProperty_returns_error_for_types_not_supported_in_the_target_EF_version()
            {
                var schemaGenerator = CreateStoreModelBuilder(targetEntityFrameworkVersion: EntityFrameworkVersion.Version2);

                foreach (var unsupportedTypeName in new[] { "geography", "geometry" })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        schemaGenerator.CreateProperty(
                            CreateRow("catalog", "schema", "table", "IntColumn", dataType: unsupportedTypeName),
                            errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.UnsupportedDataTypeForTarget,
                            unsupportedTypeName,
                            "catalog.schema.table",
                            "IntColumn"),
                        errors.Single().Message);
                    Assert.Equal(6005, errors.Single().ErrorCode);
                }
            }

            // datetime sql server type is const (as opposed to datetime2)
            [Fact]
            public void CreateProperty_uses_const_value_for_facets_that_are_const()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "datetimeColumn", dataType: "datetime",
                                dateTimePrecision: 2),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.DateTime, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(3, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_does_not_validate_value_for_facets_that_are_const()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "datetimeColumn", dataType: "datetime",
                                dateTimePrecision: byte.MaxValue),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.DateTime, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(3, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_decimal_property_with_specified_scale_and_precision()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "DecimalColumn", dataType: "decimal", scale: 4,
                                precision: 12),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.Decimal, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(4, (byte)property.Scale);
                Assert.Equal(12, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void
                CreateProperty_creates_decimal_property_with_default_scale_and_precision_if_they_are_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "DecimalColumn", dataType: "decimal"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.Decimal, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(0, (byte)property.Scale);
                Assert.Equal(18, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_error_if_precision_is_out_of_range_for_decimal_property()
            {
                foreach (var precision in new byte[] { 0, 255 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "DecimalColumn", dataType: "decimal",
                                    precision: precision),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "Precision",
                            precision,
                            1,
                            38,
                            "DecimalColumn",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_returns_error_if_scale_is_out_of_range_for_decimal_property()
            {
                foreach (var scale in new[] { -1, 255 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "DecimalColumn", dataType: "decimal",
                                    scale: scale),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "Scale",
                            scale,
                            0,
                            38,
                            "DecimalColumn",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_returns_datetime_property_with_specified_datetimeprecision()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "DateTime2Column", dataType: "datetime2",
                                dateTimePrecision: 4),
                            errors);

                Assert.NotNull(property);
                Assert.Empty(errors);
                Assert.Equal(PrimitiveTypeKind.DateTime, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(4, (byte)property.Precision);
            }

            [Fact]
            public void CreateProperty_creates_datetime_property_with_default_precision_if_it_is_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "DateTime2Column", dataType: "datetime2"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.DateTime, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(7, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_error_if_datetimeprecision_is_out_of_range_for_datetime_property()
            {
                foreach (var precision in new[] { -1, 255 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "DateTime2Column", dataType: "datetime2",
                                    dateTimePrecision: precision),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "Precision",
                            precision,
                            0,
                            7,
                            "DateTime2Column",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_returns_datetimeoffset_property_with_specified_datetimeprecision()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "DateTimeOffsetColumn", dataType: "datetimeoffset",
                                dateTimePrecision: 4),
                            errors);

                Assert.NotNull(property);
                Assert.Empty(errors);
                Assert.Equal(PrimitiveTypeKind.DateTimeOffset, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(4, (byte)property.Precision);
            }

            [Fact]
            public void CreateProperty_creates_datetimeoffset_property_with_default_precision_if_it_is_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "DateTimeOffsetColumn", dataType: "datetimeoffset"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.DateTimeOffset, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(7, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_error_if_datetimeprecision_is_out_of_range_for_datetimeoffset_property
                ()
            {
                foreach (var precision in new[] { -1, 255 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "DateTimeOffsetColumn",
                                    dataType: "datetimeoffset", dateTimePrecision: precision),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "Precision",
                            precision,
                            0,
                            7,
                            "DateTimeOffsetColumn",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_returns_time_property_with_specified_datetimeprecision()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "TimeColumn", dataType: "time", dateTimePrecision: 4),
                            errors);

                Assert.NotNull(property);
                Assert.Empty(errors);
                Assert.Equal(PrimitiveTypeKind.Time, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(4, (byte)property.Precision);
            }

            [Fact]
            public void CreateProperty_creates_time_property_with_default_precision_if_it_is_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "TimeColumn", dataType: "time"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.Time, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(7, (byte)property.Precision);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_error_if_datetimeprecision_is_out_of_range_for_time_property()
            {
                foreach (var precision in new[] { -1, 255 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "TimeColumn", dataType: "time",
                                    dateTimePrecision: precision),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "Precision",
                            precision,
                            0,
                            7,
                            "TimeColumn",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_creates_nvarcharmax_property_with_default_maxlength_if_it_is_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "NVarCharMaxColumn", dataType: "nvarchar(max)"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.String, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(1073741823, (long)property.MaxLength);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_ignores_requested_maxlength_for_nvarcharmax_and_uses_constant_value()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "NVarCharMaxColumn", dataType: "nvarchar(max)",
                                maximumLength: 4),
                            errors);

                Assert.NotNull(property);
                Assert.Empty(errors);
                Assert.Equal(PrimitiveTypeKind.String, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(1073741823, property.MaxLength);
            }

            [Fact]
            public void CreateProperty_returns_varbinary_property_with_specified_maxlength()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(
                                table: "table", columnName: "VarbinaryColumn", dataType: "varbinary",
                                maximumLength: 4),
                            errors);

                Assert.NotNull(property);
                Assert.Empty(errors);
                Assert.Equal(PrimitiveTypeKind.Binary, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(4, property.MaxLength);
            }

            [Fact]
            public void CreateProperty_creates_varbinary_property_with_default_maxlength_if_it_is_not_specified()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(table: "table", columnName: "VarbinaryColumn", dataType: "varbinary"),
                            errors);

                Assert.NotNull(property);
                Assert.Equal(PrimitiveTypeKind.Binary, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(8000, property.MaxLength);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_returns_error_if_maxlength_is_out_of_range_for_varbinary_property()
            {
                foreach (var maxLength in new[] { -1, 10000 })
                {
                    var errors = new List<EdmSchemaError>();
                    var property =
                        CreateStoreModelBuilder()
                            .CreateProperty(
                                CreateRow(
                                    "catalog", "schema", "table", "VarbinaryColumn", dataType: "varbinary",
                                    maximumLength: maxLength),
                                errors);

                    Assert.Null(property);
                    Assert.Equal(1, errors.Count);
                    Assert.Equal(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                            "MaxLength",
                            maxLength,
                            1,
                            8000,
                            "VarbinaryColumn",
                            "catalog.schema.table"),
                        errors.Single().Message);
                    Assert.Equal(
                        6006,
                        errors.Single().ErrorCode);
                }
            }

            [Fact]
            public void CreateProperty_sets_identity()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(columnName: "IntColumn", dataType: "int", isIdentiy: true),
                            errors);

                Assert.NotNull(property);
                Assert.Equal("IntColumn", property.Name);
                Assert.Equal(PrimitiveTypeKind.Int32, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateProperty_sets_computed()
            {
                var errors = new List<EdmSchemaError>();
                var property =
                    CreateStoreModelBuilder()
                        .CreateProperty(
                            CreateRow(columnName: "IntColumn", dataType: "int", isServerGenerated: true),
                            errors);

                Assert.NotNull(property);
                Assert.Equal("IntColumn", property.Name);
                Assert.Equal(PrimitiveTypeKind.Int32, property.PrimitiveType.PrimitiveTypeKind);
                Assert.Equal(StoreGeneratedPattern.Computed, property.StoreGeneratedPattern);
                Assert.Empty(errors);
            }
        }

        [Fact]
        public void CreateProperties_creates_properties_for_valid_rows_and_exclude_properties_for_invalid_rows()
        {
            var rows =
                new List<TableDetailsRow>
                    {
                        CreateRow(table: "TestTable", columnName: "IntColumn", dataType: "int", isPrimaryKey: true),
                        CreateRow(table: "TestTable", columnName: "GeographyKey", dataType: "geography", isPrimaryKey: true),
                        CreateRow(table: "TestTable", columnName: "DecimalColumn", dataType: "decimal", isPrimaryKey: false),
                        CreateRow(table: "TestTable", columnName: "InvalidColumn", isPrimaryKey: false)
                    };

            var errors = new List<EdmSchemaError>();
            List<string> excludedColumns;
            List<string> keyColumns;
            List<string> invalidKeyTypeColumns;
            var properties =
                CreateStoreModelBuilder()
                    .CreateProperties(rows, errors, out keyColumns, out excludedColumns, out invalidKeyTypeColumns);

            Assert.Equal(3, properties.Count);
            Assert.Equal("IntColumn", properties[0].Name);
            Assert.Equal("GeographyKey", properties[1].Name);
            Assert.Equal("DecimalColumn", properties[2].Name);

            Assert.Equal(3, errors.Count);
            Assert.Equal(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                    "IntColumn",
                    "TestTable"),
                errors[0].Message);
            Assert.Equal(EdmSchemaErrorSeverity.Warning, errors[0].Severity);
            Assert.Equal(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                    "GeographyKey",
                    "TestTable"),
                errors[1].Message);
            Assert.Equal(EdmSchemaErrorSeverity.Warning, errors[1].Severity);
            Assert.Equal(
                string.Format(
                    CultureInfo.InvariantCulture,
                    Resources_VersioningFacade.UnsupportedDataTypeUnknownType,
                    "InvalidColumn",
                    "TestTable"),
                errors[2].Message);
            Assert.Equal(EdmSchemaErrorSeverity.Warning, errors[2].Severity);

            Assert.Equal("InvalidColumn", excludedColumns.Single());
            Assert.Equal(2, keyColumns.Count);
            Assert.Equal("IntColumn", keyColumns[0]);
            Assert.Equal("GeographyKey", keyColumns[1]);
            Assert.Equal(1, invalidKeyTypeColumns.Count);
            Assert.Equal("GeographyKey", invalidKeyTypeColumns[0]);
        }

        [Fact]
        public void Build_creates_EdmModel_containing_converted_objects()
        {
            var tableDetails = new[] { CreateRow(null, "dbo", "table", "Id", 0, false, "int", isIdentiy: true) };
            var viewDetails = new[] { CreateRow(null, "dbo", "view", "Id", 0, false, "int", isIdentiy: true) };
            var relationshipDetails = new[]
                { CreateRelationshipDetailsRow("id", "name", 0, false, null, "dbo", "table", "Id", null, "dbo", "view", "Id") };
            var functionDetails = new[] { CreateFunctionDetailsRow(functionName: "function", isTvf: true) };
            var tvfReturnTypeDetails = new[] { CreateRow(null, null, "function", "age", 0, false, "int") };

            var storeModel =
                CreateStoreModelBuilder(
                    namespaceName: "my.Model",
                    targetEntityFrameworkVersion: EntityFrameworkVersion.Version3)
                    .Build(new StoreSchemaDetails(tableDetails, viewDetails, relationshipDetails, functionDetails, tvfReturnTypeDetails));

            Assert.NotNull(storeModel);
            Assert.Equal(3.0, storeModel.SchemaVersion);
            Assert.Equal("System.Data.SqlClient", storeModel.ProviderInfo.ProviderInvariantName);
            Assert.Equal("2008", storeModel.ProviderInfo.ProviderManifestToken);
            Assert.Equal("myModelContainer", storeModel.Containers.Single().Name);
            Assert.Equal(2, storeModel.Containers.Single().EntitySets.Count);
            Assert.Equal(1, storeModel.Containers.Single().AssociationSets.Count);
            Assert.Equal(2, storeModel.EntityTypes.Count());
            Assert.Equal(1, storeModel.AssociationTypes.Count());
            Assert.Equal(1, storeModel.Functions.Count());

            var returnParameter = storeModel.Functions.Single().ReturnParameter;
            Assert.IsType<CollectionType>(returnParameter.TypeUsage.EdmType);
            Assert.IsType<RowType>(
                ((CollectionType)returnParameter.TypeUsage.EdmType).TypeUsage.EdmType);
        }

        public class IsValidKeyTypeTests
        {
            [Fact]
            public void IsValidKeyType_returns_true_for_valid_key_type()
            {
                Assert.True(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version1,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));
            }

            [Fact]
            public void IsValidKeyType_returns_false_for_non_primitive_type()
            {
                var entityType = EntityType.Create("dummy", "namespace", DataSpace.SSpace, null, new EdmMember[0], null);
                Assert.False(StoreModelBuilder.IsValidKeyType(EntityFrameworkVersion.Version3, entityType));
            }

            [Fact]
            public void IsValidKeyType_returns_false_for_Binary_type_and_EFV1()
            {
                Assert.False(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version1,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)));
            }

            [Fact]
            public void IsValidKeyType_returns_true_for_Binary_type_and_non_EFV1()
            {
                Assert.True(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version2,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)));

                Assert.True(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version2,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)));
            }

            [Fact]
            public void IsValidKeyType_returns_false_for_Geometry_type()
            {
                Assert.False(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version3,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geometry)));
            }

            [Fact]
            public void IsValidKeyType_returns_false_for_Geography_type()
            {
                Assert.False(
                    StoreModelBuilder.IsValidKeyType(
                        EntityFrameworkVersion.Version3,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography)));
            }
        }

        public class InferColumnsTests
        {
            [Fact]
            public void InferKeyColumns_returns_names_for_key_valid_candidates()
            {
                var nonNullableIntProperty =
                    EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
                nonNullableIntProperty.Nullable = false;

                var nonNullableStringProperty =
                    EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                nonNullableStringProperty.Nullable = false;

                Assert.Equal(
                    new[] { "Id", "Name" },
                    CreateStoreModelBuilder()
                        .InferKeyProperties(new List<EdmProperty> { nonNullableIntProperty, nonNullableStringProperty }));
            }

            [Fact]
            public void InferKeyColumns_does_not_return_names_for_nullable_columns()
            {
                var nullableIntProperty =
                    EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
                nullableIntProperty.Nullable = true;

                Assert.Empty(
                    CreateStoreModelBuilder()
                        .InferKeyProperties(new List<EdmProperty> { nullableIntProperty }));
            }

            [Fact]
            public void InferKeyColumns_does_not_return_names_for_columns_of_types_that_are_not_valid_keys_types()
            {
                var geographyProperty =
                    EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography));
                geographyProperty.Nullable = false;

                Assert.Empty(
                    CreateStoreModelBuilder()
                        .InferKeyProperties(new List<EdmProperty> { geographyProperty }));
            }
        }

        public class CreateEntityTypeTests
        {
            [Fact]
            public void CreateEntityType_creates_entity_for_valid_properties()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                table: "table", columnName: "Id", dataType: "int", isPrimaryKey: true,
                                isNullable: false),
                            CreateRow(table: "table", columnName: "Name", dataType: "nvarchar(max)")
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Id" }.SequenceEqual(entity.KeyMembers.Select(m => m.Name)));
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.False(entity.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
                Assert.False(MetadataItemHelper.IsInvalid(entity));
            }

            [Fact]
            public void CreateEntityType_defining_query_not_needed_for_tables_where_all_columns_are_key_columns()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(table: "table", columnName: "Id", dataType: "int", isPrimaryKey: true),
                            CreateRow(table: "table", columnName: "Name", dataType: "nvarchar(max)", isPrimaryKey: true),
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.KeyMembers.Select(k => k.Name)));
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.False(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    (IList<EdmSchemaError>)entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value;
                Assert.Equal(2, edmSchemaErrors.Count());

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                        "Id",
                        "table"),
                    edmSchemaErrors[0].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                        "Name",
                        "table"),
                    edmSchemaErrors[1].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[1].Severity);
            }

            [Fact]
            public void CreateEntityType_creates_readonly_entity_if_some_column_keys_are_excluded()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id", dataType: "int",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id1", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id2", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "nvarchar(max)",
                                isPrimaryKey: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Id" }.SequenceEqual(entity.KeyMembers.Select(m => m.Name)));
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.True(needsDefiningQuery);
                Assert.False(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    ((IList<EdmSchemaError>)(entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value))
                        .Where(e => e.ErrorCode == 6031 && e.Severity == EdmSchemaErrorSeverity.Warning).ToArray();

                Assert.Equal(2, edmSchemaErrors.Length);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsReadOnly,
                        "Id1",
                        "dbo.table"),
                    edmSchemaErrors[0].Message);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsReadOnly,
                        "Id2",
                        "dbo.table"),
                    edmSchemaErrors[1].Message);
            }

            [Fact]
            public void CreateEntityType_creates_invalid_entity_if_all_column_keys_are_excluded()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id1", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id2", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "nvarchar(max)",
                                isPrimaryKey: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.Empty(entity.KeyMembers);
                Assert.True(new[] { "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.True(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    ((IList<EdmSchemaError>)(entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value))
                        .Where(e => e.ErrorCode == 6031 && e.Severity == EdmSchemaErrorSeverity.Warning).ToArray();

                Assert.Equal(2, edmSchemaErrors.Length);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsInvalid,
                        "Id1",
                        "dbo.table"),
                    edmSchemaErrors[0].Message);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsInvalid,
                        "Id2",
                        "dbo.table"),
                    edmSchemaErrors[1].Message);
            }

            [Fact]
            public void
                CreateEntityType_creates_readonly_entity_if_after_excluding_invalid_key_columns_only_valid_key_columns_of_invalid_types_exist
                ()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id1", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id2", dataType: "geography",
                                isPrimaryKey: true, isNullable: false),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id3", dataType: "int",
                                isPrimaryKey: true, isNullable: false),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "nvarchar(max)",
                                isPrimaryKey: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Id3" }.SequenceEqual(entity.KeyMembers.Select(m => m.Name)));
                Assert.True(new[] { "Id2", "Id3", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.True(needsDefiningQuery);
                Assert.False(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    ((IList<EdmSchemaError>)(entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value))
                        .Skip(1).ToArray();

                Assert.Equal(2, edmSchemaErrors.Length);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsReadOnly,
                        "Id1",
                        "dbo.table"),
                    edmSchemaErrors[0].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                        "dbo.table",
                        "Id2",
                        "geography"),
                    edmSchemaErrors[1].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[1].Severity);
            }

            [Fact]
            public void
                CreateEntityType_creates_invalid_entity_if_after_excluding_invalid_key_columns_only_key_columns_of_invalid_types_exist()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id1", dataType: "invalid-type",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id2", dataType: "geography",
                                isPrimaryKey: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "nvarchar(max)",
                                isPrimaryKey: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.Empty(entity.KeyMembers);
                Assert.True(new[] { "Id2", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.True(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    ((IList<EdmSchemaError>)(entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value))
                        .Skip(1).ToArray();

                Assert.Equal(3, edmSchemaErrors.Length);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                        "Id2",
                        "table"),
                    edmSchemaErrors[0].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsInvalid,
                        "Id1",
                        "dbo.table"),
                    edmSchemaErrors[1].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[1].Severity);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                        "dbo.table",
                        "Id2",
                        "geography"),
                    edmSchemaErrors[2].Message);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[2].Severity);
            }

            [Fact]
            public void CreateEntityType_creates_readonly_entity_if_no_keys_defined_but_keys_can_be_infered()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id", dataType: "int",
                                isNullable: false),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "nvarchar(max)",
                                isNullable: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.KeyMembers.Select(m => m.Name)));
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.True(needsDefiningQuery);
                Assert.False(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    (IList<EdmSchemaError>)entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value;

                Assert.Equal(1, edmSchemaErrors.Count);
                Assert.Equal(6002, edmSchemaErrors[0].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.NoPrimaryKeyDefined,
                        "dbo.table"),
                    edmSchemaErrors[0].Message);
            }

            [Fact]
            public void CreateEntityType_creates_invalid_entity_if_no_keys_defined_and_keys_cannot_be_infered()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(schema: "dbo", table: "table", columnName: "Id", dataType: "int", isNullable: true),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "geography",
                                isNullable: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.Empty(entity.KeyMembers);
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.True(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    (IList<EdmSchemaError>)entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value;

                Assert.Equal(1, edmSchemaErrors.Count);
                Assert.Equal(6013, edmSchemaErrors[0].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.CannotCreateEntityWithNoPrimaryKeyDefined,
                        "dbo.table"),
                    edmSchemaErrors[0].Message);
            }

            [Fact]
            public void
                CreateEntityType_creates_readonly_entity_if_defined_keys_have_invalid_key_type_but_keys_can_be_infered()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id", dataType: "varbinary",
                                isPrimaryKey: true, isNullable: false),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "int",
                                isNullable: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder(targetEntityFrameworkVersion: EntityFrameworkVersion.Version1)
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.True(new[] { "Name" }.SequenceEqual(entity.KeyMembers.Select(m => m.Name)));
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.True(needsDefiningQuery);
                Assert.False(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    (IList<EdmSchemaError>)entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value;

                Assert.Equal(2, edmSchemaErrors.Count);

                Assert.Equal(6032, edmSchemaErrors[0].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                        "dbo.table",
                        "Id",
                        "varbinary"),
                    edmSchemaErrors[0].Message);

                Assert.Equal(6002, edmSchemaErrors[1].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[1].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.NoPrimaryKeyDefined,
                        "dbo.table"),
                    edmSchemaErrors[1].Message);
            }

            [Fact]
            public void
                CreateEntityType_creates_invalid_entity_if_defined_keys_have_invalid_key_type_and_keys_cannot_be_infered()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Id", dataType: "geometry",
                                isPrimaryKey: true, isNullable: false),
                            CreateRow(
                                schema: "dbo", table: "table", columnName: "Name", dataType: "geography",
                                isNullable: false)
                        };

                bool needsDefiningQuery;
                var entity = CreateStoreModelBuilder()
                    .CreateEntityType(columns, out needsDefiningQuery);

                Assert.Equal("myModel.table", entity.FullName);
                Assert.Empty(entity.KeyMembers);
                Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
                Assert.False(needsDefiningQuery);
                Assert.True(MetadataItemHelper.IsInvalid(entity));

                var edmSchemaErrors =
                    (IList<EdmSchemaError>)entity.MetadataProperties.Single(p => p.Name == "EdmSchemaErrors").Value;

                Assert.Equal(2, edmSchemaErrors.Count);

                Assert.Equal(6032, edmSchemaErrors[0].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[0].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                        "dbo.table",
                        "Id",
                        "geometry"),
                    edmSchemaErrors[0].Message);

                Assert.Equal(6013, edmSchemaErrors[1].ErrorCode);
                Assert.Equal(EdmSchemaErrorSeverity.Warning, edmSchemaErrors[1].Severity);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.CannotCreateEntityWithNoPrimaryKeyDefined,
                        "dbo.table"),
                    edmSchemaErrors[1].Message);
            }
        }

        public class SplitRowsTests
        {
            [Fact]
            public void TableDetails_SplitRows_returns_empty_list_for_empty_input_rows()
            {
                Assert.Empty(StoreModelBuilder.SplitRows(new TableDetailsRow[0]));
            }

            [Fact]
            public void TableDetails_SplitRows_returns_grouped_row_details_for_multiple_input_tables_details()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "Order", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "Order", "Date", 1, false, "datetime", isPrimaryKey: false),
                            CreateRow("catalog", "dbo", "OrderLine", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "Customer", "Name", 1, false, "nvarchar", isPrimaryKey: false),
                        };

                var splitRows = StoreModelBuilder.SplitRows(inputTableDetailsRows);

                Assert.Equal(3, splitRows.Count);
                Assert.True(splitRows[0].All(r => r.GetMostQualifiedTableName() == "catalog.dbo.Customer"));
                Assert.True(splitRows[1].All(r => r.GetMostQualifiedTableName() == "catalog.dbo.Order"));
                Assert.True(splitRows[2].All(r => r.GetMostQualifiedTableName() == "catalog.dbo.OrderLine"));
            }

            [Fact]
            public void FunctionDetails_SplitRows_returns_empty_list_for_empty_input_rows()
            {
                Assert.Empty(StoreModelBuilder.SplitRows(new FunctionDetailsRowView[0]));
            }

            [Fact]
            public void FunctionDetails_SplitRows_returns_grouped_row_details_for_multiple_input_tables_details()
            {
                var functionDetailsRows =
                    new[]
                        {
                            CreateFunctionDetailsRow("catalog", "dbo", "function"),
                            CreateFunctionDetailsRow("catalog", "dbo", "function"),
                            CreateFunctionDetailsRow("catalog", "dbo", "function1"),
                            CreateFunctionDetailsRow("catalog", "dbo", "function"),
                            CreateFunctionDetailsRow(schema: "sch1", functionName: "function")
                        };

                var splitRows = StoreModelBuilder.SplitRows(functionDetailsRows);

                Assert.Equal(3, splitRows.Count);
                Assert.True(splitRows[0].All(r => r.GetMostQualifiedFunctionName() == "catalog.dbo.function"));
                Assert.True(splitRows[1].All(r => r.GetMostQualifiedFunctionName() == "catalog.dbo.function1"));
                Assert.True(splitRows[2].All(r => r.GetMostQualifiedFunctionName() == "sch1.function"));
            }
        }

        public class CreateEntitySetsTests
        {
            private const string StoreTypeMetadataPropertyName =
                "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Type";

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_for_valid_non_readonly_table_entities()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "OrderLine", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "Customer", "Name", 1, false, "nvarchar", isPrimaryKey: false),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.Table);

                Assert.Equal(2, entitySets.Count);
                Assert.Equal(2, entityTypes.Count);
                Assert.Empty(entitySetsForReadOnlyEntities);
                Assert.Equal(
                    entitySets.Select(s => s.ElementType.Name),
                    entityTypes.Select(t => t.Name));

                Assert.True(
                    entitySets
                        .All(
                            s =>
                            s.MetadataProperties.Any(
                                p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Tables")));
            }

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_for_valid_non_readonly_view_entities()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "OrderLine", "Id", 0, false, "int", isPrimaryKey: true),
                            CreateRow("catalog", "dbo", "Customer", "Name", 1, false, "nvarchar", isPrimaryKey: false),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.View);

                Assert.Equal(2, entitySets.Count);
                Assert.Equal(2, entityTypes.Count);
                Assert.Empty(entitySetsForReadOnlyEntities);
                Assert.Equal(
                    entitySets.Select(s => s.ElementType.Name),
                    entityTypes.Select(t => t.Name));

                Assert.True(
                    entitySets
                        .All(
                            s =>
                            s.MetadataProperties.Any(
                                p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Views")));
            }

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_with_schema_if_schema_defined()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.Table);

                Assert.Equal(1, entitySets.Count);
                Assert.Equal("dbo", entitySets[0].Schema);
                Assert.Empty(entitySetsForReadOnlyEntities);
            }

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_without_schema_if_schema_not_defined()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", null, "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.Table);

                Assert.Equal(1, entitySets.Count);
                Assert.Null(entitySets[0].Schema);
                Assert.Empty(entitySetsForReadOnlyEntities);
            }

            [Fact]
            public static void CreateEntitySets_uses_table_when_entity_type_name_different_than_table_name()
            {
                const string tableName = "Customer.Details";
                const string entityTypeName = "Customer_Details";

                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", "dbo", tableName, "Id", 0, false, "int", isPrimaryKey: true),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.Table);

                var entitySet = entitySets[0];
                Assert.Equal(entitySet.Table, tableName);
                Assert.Equal(entitySet.Name, entityTypeName);
            }

            [Fact]
            public static void CreateEntitySets_does_not_create_EntitySet_for_invalid_EntityType()
            {
                var inputTableDetailsRows =
                    new[]
                        {
                            CreateRow("catalog", null, "Customer", "Id", 0, /*isNullable*/ true, "geography", isPrimaryKey: true),
                            CreateRow("catalog", null, "Customer", "location", 0, false, "geometry", isPrimaryKey: true)
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;
                var entitySetsForReadOnlyEntities = new List<EntitySet>();

                CreateStoreModelBuilder()
                    .CreateEntitySets(inputTableDetailsRows, entityRegister, entitySetsForReadOnlyEntities, DbObjectType.Table);

                Assert.Empty(entitySets);
                Assert.Equal(1, entityTypes.Count);
                Assert.Empty(entitySetsForReadOnlyEntities);
            }

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_for_tables_and_views()
            {
                var tableDetailsRowsForTables =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, false, "int", isPrimaryKey: true),
                        };

                var tableDetailsRowsForViews =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "EvenBetterCustomer", "Id", 0, false, "int", isPrimaryKey: true),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;

                CreateStoreModelBuilder()
                    .CreateEntitySets(tableDetailsRowsForTables, tableDetailsRowsForViews, entityRegister);

                Assert.Equal(2, entitySets.Count);
                Assert.Equal(2, entityTypes.Count);
                Assert.True(entitySets.All(s => s.DefiningQuery == null));

                Assert.True(
                    entitySets[0].MetadataProperties.Any(
                        p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Tables"));

                Assert.True(
                    entitySets[1].MetadataProperties.Any(
                        p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Views"));
            }

            [Fact]
            public static void CreateEntitySets_creates_EntitySets_with_defining_queries_for_tables_and_views()
            {
                var tableDetailsRowsForTables =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "Customer", "Id", 0, true, "int", isPrimaryKey: false),
                            CreateRow("catalog", "dbo", "Customer", "SSN", 0, false, "nvarchar", isPrimaryKey: false),
                        };

                var tableDetailsRowsForViews =
                    new[]
                        {
                            CreateRow("catalog", "dbo", "EvenBetterCustomer", "Id", 0, true, "int", isPrimaryKey: false),
                            CreateRow("catalog", "dbo", "EvenBetterCustomer", "SSN", 0, false, "nvarchar", isPrimaryKey: false),
                        };

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entitySets = entityRegister.EntitySets;
                var entityTypes = entityRegister.EntityTypes;

                CreateStoreModelBuilder()
                    .CreateEntitySets(tableDetailsRowsForTables, tableDetailsRowsForViews, entityRegister);

                Assert.Equal(2, entitySets.Count);
                Assert.True(entitySets.All(s => s.DefiningQuery != null));
                Assert.Equal(2, entityTypes.Count);

                Assert.True(
                    entitySets[0].MetadataProperties.Any(
                        p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Tables"));

                Assert.True(
                    entitySets[1].MetadataProperties.Any(
                        p => p.Name == StoreTypeMetadataPropertyName && (string)p.Value == "Views"));
            }
        }

        public class CreateTvfReturnTypesTests
        {
            [Fact]
            public void CreateTvfReturnTypes_creates_row_types_for_valid_input_rows()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(table: "rowtype", columnName: "Id", dataType: "int"),
                            CreateRow(table: "rowtype", columnName: "Name", dataType: "nvarchar(max)")
                        };

                var rowTypes = CreateStoreModelBuilder().CreateTvfReturnTypes(columns);

                Assert.NotNull(rowTypes);
                Assert.Equal(1, rowTypes.Count);
                Assert.Equal(new[] { "Id", "Name" }, rowTypes.Single().Value.Properties.Select(p => p.Name));
                Assert.False(rowTypes.Single().Value.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void CreateTvfReturnTypes_creates_multiple_row_types_for_multiple_valid_definitons()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(table: "rowtype", columnName: "Id", dataType: "int"),
                            CreateRow(table: "rowtype", columnName: "Name", dataType: "nvarchar(max)"),
                            CreateRow(table: "rowtype1", columnName: "Name", dataType: "nvarchar(max)")
                        };

                var rowTypes = CreateStoreModelBuilder().CreateTvfReturnTypes(columns);

                Assert.NotNull(rowTypes);
                Assert.Equal(2, rowTypes.Count);
            }

            [Fact]
            public void CreateTvfReturnTypes_creates_row_types_with_errors_for_invalidtype()
            {
                var columns =
                    new List<TableDetailsRow>
                        {
                            CreateRow(table: "rowtype", columnName: "Id", dataType: "foo"),
                            CreateRow(table: "rowtype", columnName: "Name", dataType: "nvarchar(max)"),
                        };

                var rowTypes = CreateStoreModelBuilder().CreateTvfReturnTypes(columns);

                Assert.NotNull(rowTypes);
                Assert.Equal(1, rowTypes.Count);
                Assert.True(rowTypes.Single().Value.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }
        }

        public class FunctionReverseEngineeringTests
        {
            [Fact]
            public void GetFunctionParameterType_returns_PrimitiveType_for_valid_parameter_type()
            {
                var errors = new List<EdmSchemaError>();

                var type = CreateStoreModelBuilder()
                    .GetFunctionParameterType(
                        CreateFunctionDetailsRow(paramTypeName: "smallint"), 1, errors);

                Assert.NotNull(type);
                Assert.Equal(PrimitiveTypeKind.Int16, type.PrimitiveTypeKind);
            }

            [Fact]
            public void GetFunctionParameterType_returns_error_for_null_parameter_type_name()
            {
                var errors = new List<EdmSchemaError>();

                var type = CreateStoreModelBuilder()
                    .GetFunctionParameterType(
                        CreateFunctionDetailsRow(functionName: "function", paramName: "param", paramTypeName: null),
                        1, errors);

                Assert.Null(type);
                Assert.Equal(1, errors.Count);
                var error = errors.Single();

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionParameterDataType,
                        "function", "param", 1, "null"),
                    error.Message);
            }

            [Fact]
            public void GetFunctionParameterType_returns_error_for_invalid_parameter_type()
            {
                var errors = new List<EdmSchemaError>();

                var type = CreateStoreModelBuilder()
                    .GetFunctionParameterType(
                        CreateFunctionDetailsRow(functionName: "function", paramName: "param", paramTypeName: "foo-type"),
                        1, errors);

                Assert.Null(type);
                Assert.Equal(1, errors.Count);
                var error = errors.Single();

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionParameterDataType,
                        "function", "param", 1, "foo-type"),
                    error.Message);
            }

            [Fact]
            public void GetFunctionParameterType_returns_error_for_unsupported_type_for_schema_version()
            {
                var errors = new List<EdmSchemaError>();

                var type = CreateStoreModelBuilder(targetEntityFrameworkVersion: EntityFrameworkVersion.Version2)
                    .GetFunctionParameterType(
                        CreateFunctionDetailsRow(functionName: "function", paramName: "param", paramTypeName: "geography"),
                        1, errors);

                Assert.Null(type);
                Assert.Equal(1, errors.Count);
                var error = errors.Single();

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionParameterDataTypeForTarget,
                        "function", "param", 1, "geography"),
                    error.Message);
            }

            [Fact]
            public void CreateFunctionParameter_returns_null_for_invalid_type_name()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateFunctionParameter(
                        CreateFunctionDetailsRow(functionName: "function", paramName: "param", paramTypeName: null),
                        new UniqueIdentifierService(),
                        1, errors);

                Assert.Null(parameter);
                Assert.Equal(1, errors.Count);
                var error = errors.Single();

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionParameterDataType,
                        "function", "param", 1, "null"),
                    error.Message);
            }

            [Fact]
            public void CreateFunctionParameter_returns_null_for_invalid_parameter_direction()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateFunctionParameter(
                        CreateFunctionDetailsRow(
                            functionName: "function", paramName: "param", paramTypeName: "smallint", parameterDirection: "foo"),
                        new UniqueIdentifierService(),
                        1, errors);

                Assert.Null(parameter);
                Assert.Equal(1, errors.Count);
                var error = errors.Single();

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.ParameterDirectionNotValid, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.ParameterDirectionNotValid,
                        "function", "param", "foo"),
                    error.Message);
            }

            [Fact]
            public void CreateFunctionParameter_returns_parameter_for_valid_function_details_row()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateFunctionParameter(
                        CreateFunctionDetailsRow(
                            functionName: "function", paramName: "param", paramTypeName: "smallint", parameterDirection: "INOUT"),
                        new UniqueIdentifierService(),
                        1, errors);

                Assert.NotNull(parameter);
                Assert.Empty(errors);
                Assert.Equal("param", parameter.Name);
                Assert.Equal("smallint", parameter.TypeUsage.EdmType.Name);
                Assert.Equal(ParameterMode.InOut, parameter.Mode);
            }

            [Fact]
            public void CreateFunctionParameter_applies_ECMA_name_conversion_for_parameter_name()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateFunctionParameter(
                        CreateFunctionDetailsRow(
                            functionName: "function", paramName: "p@r@m", paramTypeName: "smallint", parameterDirection: "INOUT"),
                        new UniqueIdentifierService(),
                        1, errors);

                Assert.NotNull(parameter);
                Assert.Empty(errors);
                Assert.Equal("p_r_m", parameter.Name);
                Assert.Equal("smallint", parameter.TypeUsage.EdmType.Name);
                Assert.Equal(ParameterMode.InOut, parameter.Mode);
            }

            [Fact]
            public void CreateFunctionParameter_applies_ECMA_name_conversion_and_uniquifies_parameter_name()
            {
                var errors = new List<EdmSchemaError>();
                var uniquifiedIdentifierService = new UniqueIdentifierService();
                uniquifiedIdentifierService.AdjustIdentifier("p_r_m");

                var parameter = CreateStoreModelBuilder()
                    .CreateFunctionParameter(
                        CreateFunctionDetailsRow(
                            functionName: "function", paramName: "p@r@m", paramTypeName: "smallint", parameterDirection: "INOUT"),
                        uniquifiedIdentifierService,
                        1, errors);

                Assert.NotNull(parameter);
                Assert.Empty(errors);
                Assert.Equal("p_r_m1", parameter.Name);
                Assert.Equal("smallint", parameter.TypeUsage.EdmType.Name);
                Assert.Equal(ParameterMode.InOut, parameter.Mode);
            }

            [Fact]
            public void CreateFunctionParameters_creates_parameters_for_all_valid_rows()
            {
                var functionDetailsRows =
                    new[]
                        {
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param2",
                                paramTypeName: "int", parameterDirection: "INOUT"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param3",
                                paramTypeName: "geometry", parameterDirection: "OUT")
                        };

                var errors = new List<EdmSchemaError>();
                var parameters = CreateStoreModelBuilder()
                    .CreateFunctionParameters(functionDetailsRows, errors).ToArray();

                Assert.Equal(3, parameters.Length);
                Assert.Equal(new[] { "param", "param2", "param3" }, parameters.Select(p => p.Name));
                Assert.Equal(new[] { "smallint", "int", "geometry" }, parameters.Select(p => p.TypeName));
                Assert.Equal(
                    new[] { ParameterMode.In, ParameterMode.InOut, ParameterMode.Out, },
                    parameters.Select(p => p.Mode));

                Assert.Empty(errors);
            }

            [Fact]
            public void CreateFunctionParameters_uniquifies_parameter_names()
            {
                var functionDetailsRows =
                    new[]
                        {
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param",
                                paramTypeName: "geometry", parameterDirection: "OUT")
                        };

                var errors = new List<EdmSchemaError>();
                var parameters = CreateStoreModelBuilder()
                    .CreateFunctionParameters(functionDetailsRows, errors).ToArray();

                Assert.Equal(2, parameters.Length);
                Assert.Equal(new[] { "param", "param1" }, parameters.Select(p => p.Name));
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateFunctionParameters_does_not_return_parameters_for_invalid_rows()
            {
                var functionDetailsRows =
                    new[]
                        {
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param2",
                                paramTypeName: "foo", parameterDirection: "INOUT"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param3",
                                paramTypeName: "geometry", parameterDirection: "OUT")
                        };

                var errors = new List<EdmSchemaError>();
                var parameters = CreateStoreModelBuilder()
                    .CreateFunctionParameters(functionDetailsRows, errors).ToArray();

                Assert.Equal(2, parameters.Length);
                Assert.False(parameters.Any(p => p == null));

                Assert.Equal(1, errors.Count);
                var error = errors.Single();
                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionParameterDataType,
                        "function", "param2", 1, "foo"),
                    error.Message);
            }

            [Fact]
            public void CreateReturnParameter_creates_return_parameter_for_scalar_function()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateReturnParameter(
                        CreateFunctionDetailsRow(functionName: "function", returnTypeName: "int"),
                        new Dictionary<string, RowType>(),
                        errors);

                Assert.NotNull(parameter);
                Assert.Equal("ReturnType", parameter.Name);
                Assert.Equal("int", parameter.TypeName);
                Assert.Empty(errors);
            }

            [Fact]
            public void CreateReturnParameter_returns_error_if_return_type_not_valid()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateReturnParameter(
                        CreateFunctionDetailsRow(functionName: "function", returnTypeName: "foo"),
                        new Dictionary<string, RowType>(),
                        errors);

                Assert.Null(parameter);
                Assert.Equal(1, errors.Count);

                var error = errors.Single();
                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionReturnDataType,
                        "function", "foo"),
                    error.Message);
            }

            [Fact]
            public void CreateReturnParameter_returns_error_if_return_type_not_valid_for_schema_version()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder(targetEntityFrameworkVersion: EntityFrameworkVersion.Version1)
                    .CreateReturnParameter(
                        CreateFunctionDetailsRow(functionName: "function", returnTypeName: "geometry"),
                        new Dictionary<string, RowType>(),
                        errors);

                Assert.Null(parameter);
                Assert.Equal(1, errors.Count);

                var error = errors.Single();
                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedType, error.ErrorCode);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedFunctionReturnDataTypeForTarget,
                        "function", "geometry"),
                    error.Message);
            }

            [Fact]
            public void CreateReturnParameter_returns_null_for_stored_proc()
            {
                var errors = new List<EdmSchemaError>();

                var parameter = CreateStoreModelBuilder()
                    .CreateReturnParameter(
                        CreateFunctionDetailsRow(functionName: "function", returnTypeName: null, isTvf: false),
                        new Dictionary<string, RowType>(),
                        errors);

                Assert.Null(parameter);
                Assert.Equal(0, errors.Count);
            }

            [Fact]
            public void CreateFunction_returns_TVF_with_errors_if_return_rowtype_for_TVF_is_invalid_and_copies_errors_from_invalid_RowType()
            {
                var tvfReturnTypeDetailsRow =
                    new List<TableDetailsRow>
                        {
                            CreateRow(
                                catalog: "myDb", schema: "dbo", table: "function", columnName: "Id",
                                dataType: "foo")
                        };

                var functionDetailsRow =
                    CreateFunctionDetailsRow(catalog: "myDb", schema: "dbo", functionName: "function", isTvf: true);

                var errors = new List<EdmSchemaError>();
                var storeModelBuilder = CreateStoreModelBuilder();
                var tvfReturnTypes = storeModelBuilder.CreateTvfReturnTypes(tvfReturnTypeDetailsRow);
                var parameter = storeModelBuilder.CreateReturnParameter(functionDetailsRow, tvfReturnTypes, errors);

                Assert.Null(parameter);

                Assert.Equal(2, errors.Count);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.TableReferencedByTvfWasNotFound,
                        "myDb.dbo.function"),
                    errors[0].Message);

                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedDataType,
                        "foo", "myDb.dbo.function", "Id"),
                    errors[1].Message);
            }

            [Fact]
            public void CreateReturnParameter_returns_null_if_return_type_for_TVF_not_found()
            {
                var functionDetailsRows =
                    CreateFunctionDetailsRow(
                        catalog: "myDb", schema: "dbo",
                        functionName: "function", isTvf: true, paramName: "param",
                        paramTypeName: "smallint", parameterDirection: "IN");

                var errors = new List<EdmSchemaError>();
                var parameter =
                    CreateStoreModelBuilder()
                        .CreateReturnParameter(functionDetailsRows, new Dictionary<string, RowType>(), errors);

                Assert.Null(parameter);

                Assert.Equal(1, errors.Count);
                Assert.Equal(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.TableReferencedByTvfWasNotFound,
                        "myDb.dbo.function"),
                    errors.Single().Message);

                Assert.Equal((int)ModelBuilderErrorCode.MissingTvfReturnTable, errors.Single().ErrorCode);
            }

            [Fact]
            public void CreateFunction_does_not_create_function_for_TVF_if_TVF_not_supported_by_schema_version()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(isTvf: true)
                        };

                Assert.Null(
                    CreateStoreModelBuilder(targetEntityFrameworkVersion: EntityFrameworkVersion.Version2)
                        .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>()));
            }

            [Fact]
            public void CreateFunction_creates_scalar_function_for_valid_function_details_rows()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(
                                catalog: "myDb", schema: "dbo",
                                functionName: "function", returnTypeName: "int",
                                isAggregate: true, isComposable: true, isBuiltIn: true,
                                isNiladic: true, isTvf: false, paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param2",
                                paramTypeName: "int", parameterDirection: "INOUT"),
                            CreateFunctionDetailsRow(
                                functionName: "function", paramName: "param3",
                                paramTypeName: "geometry", parameterDirection: "OUT")
                        };

                var function = CreateStoreModelBuilder()
                    .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.function", function.FullName);
                Assert.Null(function.StoreFunctionNameAttribute);
                Assert.Equal(3, function.Parameters.Count);
                Assert.Equal("int", function.ReturnParameter.TypeName);
                Assert.True(function.AggregateAttribute);
                Assert.True(function.IsComposableAttribute);
                Assert.True(function.BuiltInAttribute);
                Assert.True(function.NiladicFunctionAttribute);
                Assert.False(function.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void CreateFunction_creates_TVF_function_for_valid_function_details_rows()
            {
                var tvfReturnTypeDetailsRow =
                    new List<TableDetailsRow>
                        {
                            CreateRow(catalog: "myDb", schema: "dbo", table: "function", columnName: "Id", dataType: "int"),
                            CreateRow(catalog: "myDb", schema: "dbo", table: "function", columnName: "Name", dataType: "nvarchar(max)")
                        };

                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(
                                catalog: "myDb", schema: "dbo",
                                functionName: "function", isTvf: true, paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                        };

                var storeModelBuilder = CreateStoreModelBuilder();
                var tvfReturnTypes = storeModelBuilder.CreateTvfReturnTypes(tvfReturnTypeDetailsRow);
                var function = storeModelBuilder.CreateFunction(functionDetailsRows, tvfReturnTypes);

                Assert.NotNull(function);
                Assert.Equal("myModel.function", function.FullName);
                Assert.Same(tvfReturnTypes.Values.Single().GetCollectionType(), function.ReturnParameter.TypeUsage.EdmType);
                Assert.False(function.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void CreateFunction_creates_EdmSchemaErrors_metadata_property_for_invalid_functions()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(functionName: "function", isTvf: true),
                        };

                var function =
                    CreateStoreModelBuilder()
                        .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.True(function.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void CreateFunction_creates_stored_procedure_for_valid_function_details_rows()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(
                                catalog: "myDb", schema: "dbo",
                                functionName: "function", isTvf: false, paramName: "param",
                                paramTypeName: "smallint", parameterDirection: "IN"),
                        };

                var function = CreateStoreModelBuilder()
                    .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.function", function.FullName);
                Assert.Null(function.StoreFunctionNameAttribute);
                Assert.Equal(1, function.Parameters.Count);
                Assert.Null(function.ReturnParameter);
                Assert.False(function.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void Can_CreateFunction_without_parameters()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(
                                catalog: "myDb", schema: "dbo",
                                functionName: "function", isTvf: false, paramName: null),
                        };

                var function = CreateStoreModelBuilder()
                    .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.function", function.FullName);
                Assert.Null(function.StoreFunctionNameAttribute);
                Assert.Equal(0, function.Parameters.Count);
                Assert.Null(function.ReturnParameter);
                Assert.False(function.MetadataProperties.Any(p => p.Name == "EdmSchemaErrors"));
            }

            [Fact]
            public void CreateFunction_applies_ECMA_name_conversion_and_uniquifies_parameter_name()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(functionName: "#@$&!", isTvf: false, paramName: null),
                        };

                var storeModelBuilder = CreateStoreModelBuilder();
                var function = storeModelBuilder.CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.f_____", function.FullName);

                function = storeModelBuilder.CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.f_____1", function.FullName);
            }

            [Fact]
            public void CreateFunction_sets_StoreFunctionName_if_the_original_name_was_changed()
            {
                var functionDetailsRows =
                    new List<FunctionDetailsRowView>
                        {
                            CreateFunctionDetailsRow(functionName: "#@$&!", isTvf: false, paramName: null),
                        };

                var function = CreateStoreModelBuilder()
                    .CreateFunction(functionDetailsRows, new Dictionary<string, RowType>());

                Assert.NotNull(function);
                Assert.Equal("myModel.f_____", function.FullName);
                Assert.Equal("#@$&!", function.StoreFunctionNameAttribute);
            }
        }

        public class CreateAssociationSetsTests
        {
            [Fact]
            public static void CreateAssociationSets_creates_expected_association_types_and_sets()
            {
                var tableDetails = new[]
                    {
                        CreateRow(
                            "catalog", "schema", "source1", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true, isPrimaryKey: true)
                        ,
                        CreateRow(
                            "catalog", "schema", "source1", "Other", 1, isNullable: false, dataType: "int", isIdentiy: false,
                            isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target1", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true),
                        CreateRow("catalog", "schema", "target1", "Other", 1, isNullable: false, dataType: "int", isIdentiy: false),
                        CreateRow(
                            "catalog", "schema", "source2", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true, isPrimaryKey: true)
                        ,
                        CreateRow("catalog", "schema", "target2", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId1", "name1", 0, false, "catalog", "schema", "source1", "Id", "catalog", "schema", "target1", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId1", "name1", 1, false, "catalog", "schema", "source1", "Other", "catalog", "schema", "target1",
                            "Other"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId2", "name2", 0, false, "catalog", "schema", "source2", "Id", "catalog", "schema", "target2", "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entityTypes = entityRegister.EntityTypes;
                var entitySets = entityRegister.EntitySets;
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSets = storeModelBuilder.CreateAssociationSets(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(2, associationTypes.Count);
                Assert.Equal(2, associationSets.Count);

                var associationType1 = associationTypes[0];
                var associationType2 = associationTypes[1];

                Assert.Equal("myModel.name1", associationType1.FullName);
                Assert.Equal("myModel.name2", associationType2.FullName);
                Assert.Null(associationType1.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));
                Assert.Null(associationType2.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));
                Assert.False(MetadataItemHelper.IsInvalid(associationType1));
                Assert.False(MetadataItemHelper.IsInvalid(associationType2));
            }

            [Fact]
            public static void CreateAssociationSets_does_not_create_set_for_shared_foreign_key()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source1", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "source2", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId1", "name1", 0, false, "catalog", "schema", "source1", "Id", "catalog", "schema", "target", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId2", "name2", 0, false, "catalog", "schema", "source2", "Id", "catalog", "schema", "target", "Id"),
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSets = storeModelBuilder.CreateAssociationSets(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(2, associationTypes.Count);
                Assert.Equal(1, associationSets.Count);

                var associationType1 = associationTypes[0];
                var associationType2 = associationTypes[1];

                Assert.False(MetadataItemHelper.IsInvalid(associationType1));
                Assert.True(MetadataItemHelper.IsInvalid(associationType2));

                Assert.Null(associationType1.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));

                var metaProperty = associationType2.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;

                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count);

                var error = errors[0];

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.SharedForeignKey, error.ErrorCode);
            }

            [Fact]
            public static void TryCreateAssociationSet_creates_valid_association_type_and_set()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                var entityTypes = entityRegister.EntityTypes;
                var entitySets = entityRegister.EntitySets;
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.NotNull(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(2, associationType.AssociationEndMembers.Count);
                Assert.NotNull(associationType.Constraint);
                Assert.False(MetadataItemHelper.IsInvalid(associationType));
                Assert.Null(associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));

                var sourceEnd = associationType.AssociationEndMembers.FirstOrDefault();
                var targetEnd = associationType.AssociationEndMembers.ElementAtOrDefault(1);

                Assert.Equal(entityTypes[0], sourceEnd.GetEntityType());
                Assert.Equal(entityTypes[1], targetEnd.GetEntityType());

                var sourceEndSet = associationSet.AssociationSetEnds.FirstOrDefault();
                var targetEndSet = associationSet.AssociationSetEnds.ElementAtOrDefault(1);

                Assert.Equal(entitySets[0], sourceEndSet.EntitySet);
                Assert.Equal(entitySets[1], targetEndSet.EntitySet);
            }

            [Fact]
            public static void TryCreateAssociationSet_does_not_create_set_if_end_entity_is_missing()
            {
                Check_does_not_create_set_if_end_entity_is_missing(sourceMissing: true, targetMissing: false);
                Check_does_not_create_set_if_end_entity_is_missing(sourceMissing: false, targetMissing: true);
                Check_does_not_create_set_if_end_entity_is_missing(sourceMissing: true, targetMissing: true);
            }

            private static void Check_does_not_create_set_if_end_entity_is_missing(bool sourceMissing, bool targetMissing)
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var sourceColumn = sourceMissing ? "missing" : "source";
                var targetColumn = targetMissing ? "missing" : "target";

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", sourceColumn, "Id", "catalog", "schema", targetColumn,
                            "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.Null(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(0, associationType.AssociationEndMembers.Count);
                Assert.Null(associationType.Constraint);
                Assert.True(MetadataItemHelper.IsInvalid(associationType));

                var metaProperty = associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;
                var expectedCount = (sourceMissing ? 1 : 0) + (targetMissing ? 1 : 0);

                Assert.NotNull(errors);
                Assert.Equal(expectedCount, errors.Count);

                foreach (var error in errors)
                {
                    Assert.Equal(EdmSchemaErrorSeverity.Error, error.Severity);
                    Assert.Equal((int)ModelBuilderErrorCode.MissingEntity, error.ErrorCode);
                }
            }

            [Fact]
            public static void TryCreateAssociationSet_does_not_create_set_if_relationship_column_count_does_not_match()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "source", "Other", 1, false, "int", isIdentiy: false, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.Null(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(0, associationType.AssociationEndMembers.Count);
                Assert.Null(associationType.Constraint);
                Assert.True(MetadataItemHelper.IsInvalid(associationType));

                var metaProperty = associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;

                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count);

                var error = errors[0];

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedDbRelationship, error.ErrorCode);
            }

            [Fact]
            public static void TryCreateAssociationSet_does_not_create_set_if_relationship_column_name_does_not_match()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Mismatch", "catalog", "schema", "target",
                            "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.Null(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(0, associationType.AssociationEndMembers.Count);
                Assert.Null(associationType.Constraint);
                Assert.True(MetadataItemHelper.IsInvalid(associationType));

                var metaProperty = associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;

                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count);

                var error = errors[0];

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedDbRelationship, error.ErrorCode);
            }

            [Fact]
            public static void TryCreateAssociationSet_does_not_create_set_if_fk_is_partially_contained_in_pk()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "source", "Other", 1, false, "int", isIdentiy: false, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Other", 1, false, "int", isIdentiy: false, isPrimaryKey: false)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Other", "catalog", "schema", "target",
                            "Other")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.Null(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(2, associationType.AssociationEndMembers.Count);
                Assert.NotNull(associationType.Constraint);
                Assert.True(MetadataItemHelper.IsInvalid(associationType));

                var metaProperty = associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;

                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count);

                var error = errors[0];

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.UnsupportedForeinKeyPattern, error.ErrorCode);
            }

            [Fact]
            public static void TryCreateAssociationSet_does_not_create_set_if_association_is_missing_key_column()
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "source", "Other", 1, false, "int", isIdentiy: false, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 1, false, "catalog", "schema", "source", "Other", "catalog", "schema", "target",
                            "Other")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.Null(associationSet);

                var associationType = associationTypes[0];

                Assert.NotNull(associationType);
                Assert.Equal(2, associationType.AssociationEndMembers.Count);
                Assert.Null(associationType.Constraint);
                Assert.True(MetadataItemHelper.IsInvalid(associationType));

                var metaProperty = associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors");

                Assert.NotNull(metaProperty);

                var errors = metaProperty.Value as List<EdmSchemaError>;

                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count);

                var error = errors[0];

                Assert.Equal(EdmSchemaErrorSeverity.Warning, error.Severity);
                Assert.Equal((int)ModelBuilderErrorCode.AssociationMissingKeyColumn, error.ErrorCode);
            }

            [Fact]
            public static void TryCreateAssociationSet_expected_association_end_multiplicity_pk_to_pk()
            {
                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_pk(
                    EntityFrameworkVersion.Version1);

                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_pk(
                    EntityFrameworkVersion.Version3);
            }

            private static void Check_two_column_relationship_expected_association_end_multiplicity_pk_to_pk(
                Version targetEntityFrameworkVersion)
            {
                var tableDetails = new[]
                    {
                        CreateRow(
                            "catalog", "schema", "source", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "source", "Other", 1, isNullable: false, dataType: "int", isIdentiy: false,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "target", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "target", "Other", 1, isNullable: false, dataType: "int", isIdentiy: false,
                            isPrimaryKey: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 1, false, "catalog", "schema", "source", "Other", "catalog", "schema", "target",
                            "Other")
                    };

                var storeModelBuilder = CreateStoreModelBuilder("System.Data.SqlClient", "2008", targetEntityFrameworkVersion);

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.NotNull(associationSet);

                var associationType = associationTypes[0];

                Assert.False(MetadataItemHelper.IsInvalid(associationType));
                Assert.Null(associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));

                var sourceEnd = associationType.AssociationEndMembers.FirstOrDefault();
                var targetEnd = associationType.AssociationEndMembers.ElementAtOrDefault(1);

                Assert.Equal(RelationshipMultiplicity.One, sourceEnd.RelationshipMultiplicity);
                Assert.Equal(RelationshipMultiplicity.ZeroOrOne, targetEnd.RelationshipMultiplicity);
            }

            [Fact]
            public static void TryCreateAssociationSet_expected_association_end_multiplicity_pk_to_fk()
            {
                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_fk(
                    EntityFrameworkVersion.Version1,
                    column1Nullable: true,
                    column2Nullable: true,
                    expectedSourceEndMultiplicity: RelationshipMultiplicity.ZeroOrOne,
                    expectedTargetEndMultiplicity: RelationshipMultiplicity.Many);

                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_fk(
                    EntityFrameworkVersion.Version1,
                    column1Nullable: false,
                    column2Nullable: true,
                    expectedSourceEndMultiplicity: RelationshipMultiplicity.One,
                    expectedTargetEndMultiplicity: RelationshipMultiplicity.Many);

                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_fk(
                    EntityFrameworkVersion.Version3,
                    column1Nullable: false,
                    column2Nullable: true,
                    expectedSourceEndMultiplicity: RelationshipMultiplicity.ZeroOrOne,
                    expectedTargetEndMultiplicity: RelationshipMultiplicity.Many);

                Check_two_column_relationship_expected_association_end_multiplicity_pk_to_fk(
                    EntityFrameworkVersion.Version3,
                    column1Nullable: false,
                    column2Nullable: false,
                    expectedSourceEndMultiplicity: RelationshipMultiplicity.One,
                    expectedTargetEndMultiplicity: RelationshipMultiplicity.Many);
            }

            private static void Check_two_column_relationship_expected_association_end_multiplicity_pk_to_fk(
                Version targetEntityFrameworkVersion,
                bool column1Nullable,
                bool column2Nullable,
                RelationshipMultiplicity expectedSourceEndMultiplicity,
                RelationshipMultiplicity expectedTargetEndMultiplicity)
            {
                var tableDetails = new[]
                    {
                        CreateRow(
                            "catalog", "schema", "source", "Id", 0, isNullable: false, dataType: "int", isIdentiy: true,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "source", "Other", 1, isNullable: false, dataType: "int", isIdentiy: false,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "target", "TargetId", 0, isNullable: false, dataType: "int", isIdentiy: true,
                            isPrimaryKey: true),
                        CreateRow(
                            "catalog", "schema", "target", "Id", 0, isNullable: column1Nullable, dataType: "int", isIdentiy: true,
                            isPrimaryKey: false),
                        CreateRow(
                            "catalog", "schema", "target", "Other", 1, isNullable: column2Nullable, dataType: "int", isIdentiy: false,
                            isPrimaryKey: false)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, false, "catalog", "schema", "source", "Id", "catalog", "schema", "target", "Id"),
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 1, false, "catalog", "schema", "source", "Other", "catalog", "schema", "target",
                            "Other")
                    };

                var storeModelBuilder = CreateStoreModelBuilder("System.Data.SqlClient", "2008", targetEntityFrameworkVersion);

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.NotNull(associationSet);

                var associationType = associationTypes[0];

                Assert.False(MetadataItemHelper.IsInvalid(associationType));
                Assert.Null(associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));

                var sourceEnd = associationType.AssociationEndMembers.FirstOrDefault();
                var targetEnd = associationType.AssociationEndMembers.ElementAtOrDefault(1);

                Assert.Equal(expectedSourceEndMultiplicity, sourceEnd.RelationshipMultiplicity);
                Assert.Equal(expectedTargetEndMultiplicity, targetEnd.RelationshipMultiplicity);
            }

            [Fact]
            public static void TryCreateAssociationSet_cascade_delete_flag_is_reflected_by_delete_behavior()
            {
                Check_cascade_delete_flag_is_reflected_by_delete_behavior(
                    isCascadeDelete: false, expectedDeleteBehavior: OperationAction.None);
                Check_cascade_delete_flag_is_reflected_by_delete_behavior(
                    isCascadeDelete: true, expectedDeleteBehavior: OperationAction.Cascade);
            }

            private static void Check_cascade_delete_flag_is_reflected_by_delete_behavior(
                bool isCascadeDelete, OperationAction expectedDeleteBehavior)
            {
                var tableDetails = new[]
                    {
                        CreateRow("catalog", "schema", "source", "Id", 0, false, "int", isIdentiy: true, isPrimaryKey: true),
                        CreateRow("catalog", "schema", "target", "Id", 0, false, "int", isIdentiy: true)
                    };

                var relationshipDetails = new List<RelationshipDetailsRow>
                    {
                        CreateRelationshipDetailsRow(
                            "RelationshipId", "name", 0, isCascadeDelete, "catalog", "schema", "source", "Id", "catalog", "schema", "target",
                            "Id")
                    };

                var storeModelBuilder = CreateStoreModelBuilder();

                var entityRegister = new StoreModelBuilder.EntityRegister();
                storeModelBuilder.CreateEntitySets(tableDetails, new TableDetailsRow[0], entityRegister);

                var associationTypes = new List<AssociationType>();
                var associationSet = storeModelBuilder.TryCreateAssociationSet(relationshipDetails, entityRegister, associationTypes);

                Assert.Equal(1, associationTypes.Count);
                Assert.NotNull(associationSet);

                var associationType = associationTypes[0];

                Assert.False(MetadataItemHelper.IsInvalid(associationType));
                Assert.Null(associationType.MetadataProperties.SingleOrDefault(p => p.Name == "EdmSchemaErrors"));

                var sourceEnd = associationType.AssociationEndMembers.FirstOrDefault();
                var targetEnd = associationType.AssociationEndMembers.ElementAtOrDefault(1);

                Assert.Equal(expectedDeleteBehavior, sourceEnd.DeleteBehavior);
                Assert.Equal(OperationAction.None, targetEnd.DeleteBehavior);
            }
        }
    }
}
