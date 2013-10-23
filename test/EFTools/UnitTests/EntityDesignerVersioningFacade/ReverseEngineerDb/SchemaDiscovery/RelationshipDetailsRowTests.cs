// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Globalization;
    using Xunit;

    public class RelationshipDetailsRowTests
    {
        [Fact]
        public void Table_returns_owning_table()
        {
            var relationshipDetailsCollection = new RelationshipDetailsCollection();
            Assert.Same(relationshipDetailsCollection, relationshipDetailsCollection.NewRow().Table);
        }

        [Fact]
        public void PKCatalog_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["PkCatalog"] = "catalog";
            Assert.Equal("catalog", ((RelationshipDetailsRow)row).PKCatalog);
        }

        [Fact]
        public void PKCatalog_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).PKCatalog = "catalog";
            Assert.Equal("catalog", row["PkCatalog"]);
        }

        [Fact]
        public void PKCatalog_IsDbNull_returns_true_for_null_PKCatalog_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsPKCatalogNull());
            row["PkCatalog"] = DBNull.Value;
            Assert.True(row.IsPKCatalogNull());
        }

        [Fact]
        public void PKCatalog_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "PkCatalog",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.PKCatalog).Message);
        }

        [Fact]
        public void PKSchema_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["PkSchema"] = "schema";
            Assert.Equal("schema", ((RelationshipDetailsRow)row).PKSchema);
        }

        [Fact]
        public void PKSchema_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).PKSchema = "schema";
            Assert.Equal("schema", row["PkSchema"]);
        }

        [Fact]
        public void PKSchema_IsDbNull_returns_true_for_null_PkSchema_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsPKSchemaNull());
            row["PkSchema"] = DBNull.Value;
            Assert.True(row.IsPKSchemaNull());
        }

        [Fact]
        public void PKSchema_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "PkSchema",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.PKSchema).Message);
        }

        [Fact]
        public void PKTable_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["PkTable"] = "table";
            Assert.Equal("table", ((RelationshipDetailsRow)row).PKTable);
        }

        [Fact]
        public void PKTable_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).PKTable = "table";
            Assert.Equal("table", row["PkTable"]);
        }

        [Fact]
        public void PKTable_IsDbNull_returns_true_for_null_PkTable_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsPKTableNull());
            row["PkTable"] = DBNull.Value;
            Assert.True(row.IsPKTableNull());
        }

        [Fact]
        public void PKTable_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "PkTable",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.PKTable).Message);
        }

        [Fact]
        public void PKColumn_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["PkColumn"] = "column";
            Assert.Equal("column", ((RelationshipDetailsRow)row).PKColumn);
        }

        [Fact]
        public void PKColumn_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).PKColumn = "column";
            Assert.Equal("column", row["PkColumn"]);
        }

        [Fact]
        public void PKColumn_IsDbNull_returns_true_for_null_PkColumn_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsPKColumnNull());
            row["PkColumn"] = DBNull.Value;
            Assert.True(row.IsPKColumnNull());
        }

        [Fact]
        public void PKColumn_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "PkColumn",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.PKColumn).Message);
        }

        [Fact]
        public void FKCatalog_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["FkCatalog"] = "fk-catalog";
            Assert.Equal("fk-catalog", ((RelationshipDetailsRow)row).FKCatalog);
        }

        [Fact]
        public void FKCatalog_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).FKCatalog = "fk-catalog";
            Assert.Equal("fk-catalog", row["FkCatalog"]);
        }

        [Fact]
        public void FKCatalog_IsDbNull_returns_true_for_null_FkCatalog_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsFKCatalogNull());
            row["FkCatalog"] = DBNull.Value;
            Assert.True(row.IsFKCatalogNull());
        }

        [Fact]
        public void FKCatalog_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "FkCatalog",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.FKCatalog).Message);
        }

        [Fact]
        public void FKSchema_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["FkSchema"] = "fk-schema";
            Assert.Equal("fk-schema", ((RelationshipDetailsRow)row).FKSchema);
        }

        [Fact]
        public void FKSchema_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).FKSchema = "fk-schema";
            Assert.Equal("fk-schema", row["FkSchema"]);
        }

        [Fact]
        public void FKSchema_IsDbNull_returns_true_for_null_FkSchema_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsFKSchemaNull());
            row["FkSchema"] = DBNull.Value;
            Assert.True(row.IsFKSchemaNull());
        }

        [Fact]
        public void FKSchema_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "FkSchema",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.FKSchema).Message);
        }

        [Fact]
        public void FKTable_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["FkTable"] = "fk-table";
            Assert.Equal("fk-table", ((RelationshipDetailsRow)row).FKTable);
        }

        [Fact]
        public void FKTable_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).FKTable = "fk-table";
            Assert.Equal("fk-table", row["FkTable"]);
        }

        [Fact]
        public void FKTable_IsDbNull_returns_true_for_null_FkTable_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsFKTableNull());
            row["FkTable"] = DBNull.Value;
            Assert.True(row.IsFKTableNull());
        }

        [Fact]
        public void FKTable_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "FkTable",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.FKTable).Message);
        }

        [Fact]
        public void FKColumn_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["FkColumn"] = "fk-column";
            Assert.Equal("fk-column", ((RelationshipDetailsRow)row).FKColumn);
        }

        [Fact]
        public void FKColumn_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).FKColumn = "fk-column";
            Assert.Equal("fk-column", row["FkColumn"]);
        }

        [Fact]
        public void FKColumn_IsDbNull_returns_true_for_null_FkColumn_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsFKColumnNull());
            row["FkColumn"] = DBNull.Value;
            Assert.True(row.IsFKColumnNull());
        }

        [Fact]
        public void FKColumn_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "FkColumn",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.FKColumn).Message);
        }

        [Fact]
        public void Ordinal_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["Ordinal"] = 42;
            Assert.Equal(42, ((RelationshipDetailsRow)row).Ordinal);
        }

        [Fact]
        public void Ordinal_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).Ordinal = 42;
            Assert.Equal(42, row["Ordinal"]);
        }

        [Fact]
        public void Ordinal_IsDbNull_returns_true_for_null_Ordinal_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsOrdinalNull());
            row["Ordinal"] = DBNull.Value;
            Assert.True(row.IsOrdinalNull());
        }

        [Fact]
        public void Ordinal_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "Ordinal",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.Ordinal).Message);
        }

        [Fact]
        public void RelationshipName_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["RelationshipName"] = "relationship";
            Assert.Equal("relationship", ((RelationshipDetailsRow)row).RelationshipName);
        }

        [Fact]
        public void RelationshipName_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).RelationshipName = "relationship";
            Assert.Equal("relationship", row["RelationshipName"]);
        }

        [Fact]
        public void RelationshipName_IsDbNull_returns_true_for_null_RelationshipName_value()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();
            Assert.True(row.IsRelationshipNameNull());
            row["RelationshipName"] = DBNull.Value;
            Assert.True(row.IsRelationshipNameNull());
        }

        [Fact]
        public void RelationshipName_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "RelationshipName",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.RelationshipName).Message);
        }

        [Fact]
        public void RelationshipId_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["RelationshipId"] = "relationship";
            Assert.Equal("relationship", ((RelationshipDetailsRow)row).RelationshipId);
        }

        [Fact]
        public void RelationshipId_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).RelationshipId = "relationship";
            Assert.Equal("relationship", row["RelationshipId"]);
        }

        [Fact]
        public void RelationshipId_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "RelationshipId",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.RelationshipId).Message);
        }

        [Fact]
        public void RelationshipIsCascadeDelete_getter_returns_value_set_with_indexer()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            row["IsCascadeDelete"] = true;
            Assert.Equal(true, ((RelationshipDetailsRow)row).RelationshipIsCascadeDelete);
        }

        [Fact]
        public void RelationshipIsCascadeDelete_setter_sets_value_in_uderlying_row()
        {
            var row = new RelationshipDetailsCollection().NewRow();
            ((RelationshipDetailsRow)row).RelationshipIsCascadeDelete = true;
            Assert.Equal(true, row["IsCascadeDelete"]);
        }

        [Fact]
        public void RelationshipIsCascadeDelete_throws_StrongTypingException_for_null_vale()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                    "IsCascadeDelete",
                    "RelationshipDetails"),
                Assert.Throws<StrongTypingException>(() => row.RelationshipIsCascadeDelete).Message);
        }

        [Fact]
        public void GetMostQualifiedPrimaryKey_returns_expected_result()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            row["PkTable"] = "table";
            Assert.Equal("table", row.GetMostQualifiedPrimaryKey());

            row["PkSchema"] = "schema";
            Assert.Equal("schema.table", row.GetMostQualifiedPrimaryKey());

            row["PkCatalog"] = "catalog";
            Assert.Equal("catalog.schema.table", row.GetMostQualifiedPrimaryKey());
        }

        [Fact]
        public void GetMostQualifiedForeignKey_returns_expected_result()
        {
            var row = (RelationshipDetailsRow)new RelationshipDetailsCollection().NewRow();

            row["FkTable"] = "table";
            Assert.Equal("table", row.GetMostQualifiedForeignKey());

            row["FkSchema"] = "schema";
            Assert.Equal("schema.table", row.GetMostQualifiedForeignKey());

            row["FkCatalog"] = "catalog";
            Assert.Equal("catalog.schema.table", row.GetMostQualifiedForeignKey());
        }
    }
}
