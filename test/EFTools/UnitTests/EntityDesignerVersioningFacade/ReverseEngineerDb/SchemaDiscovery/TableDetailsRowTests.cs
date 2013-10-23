// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Globalization;
    using Xunit;

    public class TableDetailsRowTests
    {
        [Fact]
        public void Table_returns_owning_table()
        {
            var tableDetailsCollection = new TableDetailsCollection();
            Assert.Same(tableDetailsCollection, tableDetailsCollection.NewRow().Table);
        }

        [Fact]
        public void CatalogName_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["CatalogName"] = "catalog";
            Assert.Equal("catalog", ((TableDetailsRow)row).Catalog);
        }

        [Fact]
        public void CatalogName_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).Catalog = "catalog";
            Assert.Equal("catalog", row["CatalogName"]);
        }

        [Fact]
        public void CatalogName_IsDbNull_returns_true_for_null_CatalogName_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsCatalogNull());
            row["CatalogName"] = DBNull.Value;
            Assert.True(row.IsCatalogNull());
        }

        [Fact]
        public void CatalogName_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "CatalogName",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.Catalog).Message);
        }

        [Fact]
        public void SchemaName_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["SchemaName"] = "schema";
            Assert.Equal("schema", ((TableDetailsRow)row).Schema);
        }

        [Fact]
        public void SchemaName_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).Schema = "schema";
            Assert.Equal("schema", row["SchemaName"]);
        }

        [Fact]
        public void SchemaName_IsDbNull_returns_true_for_null_SchemaName_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsSchemaNull());
            row["SchemaName"] = DBNull.Value;
            Assert.True(row.IsSchemaNull());
        }

        [Fact]
        public void SchemaName_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "SchemaName",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.Schema).Message);
        }

        [Fact]
        public void TableName_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["TableName"] = "table";
            Assert.Equal("table", ((TableDetailsRow)row).TableName);
        }

        [Fact]
        public void TableName_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).TableName = "table";
            Assert.Equal("table", row["TableName"]);
        }

        [Fact]
        public void TableName_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "TableName",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.TableName).Message);
        }

        [Fact]
        public void ColumnName_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["ColumnName"] = "column";
            Assert.Equal("column", ((TableDetailsRow)row).ColumnName);
        }

        [Fact]
        public void ColumnName_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).ColumnName = "column";
            Assert.Equal("column", row["ColumnName"]);
        }

        [Fact]
        public void ColumnName_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "ColumnName",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.ColumnName).Message);
        }

        [Fact]
        public void IsNullable_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["IsNullable"] = true;
            Assert.True(((TableDetailsRow)row).IsNullable);
        }

        [Fact]
        public void IsNullable_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).IsNullable = true;
            Assert.True((bool)row["IsNullable"]);
        }

        [Fact]
        public void IsNullable_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "IsNullable",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.IsNullable).Message);
        }

        [Fact]
        public void DataType_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["DataType"] = "myType";
            Assert.Equal("myType", ((TableDetailsRow)row).DataType);
        }

        [Fact]
        public void DataType_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).DataType = "myType";
            Assert.Equal("myType", row["DataType"]);
        }

        [Fact]
        public void DataType_IsDbNull_returns_true_for_null_DataType_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsDataTypeNull());
            row["DataType"] = DBNull.Value;
            Assert.True(row.IsDataTypeNull());
        }

        [Fact]
        public void DataType_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "DataType",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.DataType).Message);
        }

        [Fact]
        public void MaximumLength_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["MaximumLength"] = 42;
            Assert.Equal(42, ((TableDetailsRow)row).MaximumLength);
        }

        [Fact]
        public void MaximumLength_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).MaximumLength = 42;
            Assert.Equal(42, row["MaximumLength"]);
        }

        [Fact]
        public void MaximumLength_IsDbNull_returns_true_for_null_MaximumLength_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsMaximumLengthNull());
            row["MaximumLength"] = DBNull.Value;
            Assert.True(row.IsMaximumLengthNull());
        }

        [Fact]
        public void MaximumLength_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "MaximumLength",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.MaximumLength).Message);
        }

        [Fact]
        public void DateTimePrecision_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["DateTimePrecision"] = 18;
            Assert.Equal(18, ((TableDetailsRow)row).DateTimePrecision);
        }

        [Fact]
        public void DateTimePrecision_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).DateTimePrecision = 18;
            Assert.Equal(18, row["DateTimePrecision"]);
        }

        [Fact]
        public void DateTimePrecision_IsDbNull_returns_true_for_null_DateTimePrecision_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsDateTimePrecisionNull());
            row["DateTimePrecision"] = DBNull.Value;
            Assert.True(row.IsDateTimePrecisionNull());
        }

        [Fact]
        public void DateTimePrecision_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "DateTimePrecision",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.DateTimePrecision).Message);
        }

        [Fact]
        public void Precision_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["Precision"] = 18;
            Assert.Equal(18, ((TableDetailsRow)row).Precision);
        }

        [Fact]
        public void Precision_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).Precision = 18;
            Assert.Equal(18, row["Precision"]);
        }

        [Fact]
        public void Precision_IsDbNull_returns_true_for_null_Precision_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsPrecisionNull());
            row["Precision"] = DBNull.Value;
            Assert.True(row.IsPrecisionNull());
        }

        [Fact]
        public void Precision_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "Precision",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.Precision).Message);
        }

        [Fact]
        public void Scale_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["Scale"] = 3;
            Assert.Equal(3, ((TableDetailsRow)row).Scale);
        }

        [Fact]
        public void Scale_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).Scale = 3;
            Assert.Equal(3, row["Scale"]);
        }

        [Fact]
        public void Scale_IsDbNull_returns_true_for_null_Scale_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsScaleNull());
            row["Scale"] = DBNull.Value;
            Assert.True(row.IsScaleNull());
        }

        [Fact]
        public void Scale_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "Scale",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.Scale).Message);
        }

        [Fact]
        public void IsIdentity_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["IsIdentity"] = true;
            Assert.Equal(true, ((TableDetailsRow)row).IsIdentity);
        }

        [Fact]
        public void IsIdentity_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).IsIdentity = true;
            Assert.Equal(true, row["IsIdentity"]);
        }

        [Fact]
        public void IsIdentity_IsDbNull_returns_true_for_null_IsIdentity_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsIsIdentityNull());
            row["IsIdentity"] = DBNull.Value;
            Assert.True(row.IsIsIdentityNull());
        }

        [Fact]
        public void IsIdentity_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "IsIdentity",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.IsIdentity).Message);
        }

        [Fact]
        public void IsServerGenerated_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["IsServerGenerated"] = true;
            Assert.Equal(true, ((TableDetailsRow)row).IsServerGenerated);
        }

        [Fact]
        public void IsServerGenerated_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).IsServerGenerated = true;
            Assert.Equal(true, row["IsServerGenerated"]);
        }

        [Fact]
        public void IsServerGenerated_IsDbNull_returns_true_for_null_IsServerGenerated_value()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            Assert.True(row.IsIsServerGeneratedNull());
            row["IsServerGenerated"] = DBNull.Value;
            Assert.True(row.IsIsServerGeneratedNull());
        }

        [Fact]
        public void IsServerGenerated_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "IsServerGenerated",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.IsServerGenerated).Message);
        }

        [Fact]
        public void IsPrimaryKey_getter_returns_value_set_with_indexer()
        {
            var row = new TableDetailsCollection().NewRow();
            row["IsPrimaryKey"] = true;
            Assert.Equal(true, ((TableDetailsRow)row).IsPrimaryKey);
        }

        [Fact]
        public void IsPrimaryKey_setter_sets_value_in_uderlying_row()
        {
            var row = new TableDetailsCollection().NewRow();
            ((TableDetailsRow)row).IsPrimaryKey = true;
            Assert.Equal(true, row["IsPrimaryKey"]);
        }

        [Fact]
        public void IsPrimaryKey_throws_StrongTypingException_for_null_vale()
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "IsPrimaryKey",
                    "TableDetails"),
                Assert.Throws<StrongTypingException>(() => row.IsPrimaryKey).Message);
        }

        [Fact]
        public void GetMostQualifiedTableName_uses_available_catalog_schema_table()
        {
            Assert.Equal(
                "catalog.schema.table",
                CreateTableDetailsRow("catalog", "schema", "table").GetMostQualifiedTableName());
            Assert.Equal("schema.table", CreateTableDetailsRow(null, "schema", "table").GetMostQualifiedTableName());
            Assert.Equal("catalog.table", CreateTableDetailsRow("catalog", null, "table").GetMostQualifiedTableName());
            Assert.Equal("table", CreateTableDetailsRow(null, null, "table").GetMostQualifiedTableName());
        }

        private TableDetailsRow CreateTableDetailsRow(string catalog, string schema, string table)
        {
            var row = (TableDetailsRow)new TableDetailsCollection().NewRow();
            row.Catalog = catalog;
            row.Schema = schema;
            row.TableName = table;

            return row;
        }
    }
}
