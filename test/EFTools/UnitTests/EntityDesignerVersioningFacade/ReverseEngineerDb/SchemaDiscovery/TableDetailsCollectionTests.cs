// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Linq;
    using Xunit;

    public class TableDetailsCollectionTests
    {
        [Fact]
        public void Verify_TableDetailsCollection_columns()
        {
            var tableDetailsCollection = new TableDetailsCollection();
            Assert.Equal(14, tableDetailsCollection.Columns.Count);
            VerifyColumn(tableDetailsCollection.CatalogColumn, "CatalogName", typeof(string));
            VerifyColumn(tableDetailsCollection.SchemaColumn, "SchemaName", typeof(string));
            VerifyColumn(tableDetailsCollection.TableNameColumn, "TableName", typeof(string));
            VerifyColumn(tableDetailsCollection.ColumnNameColumn, "ColumnName", typeof(string));
            VerifyColumn(tableDetailsCollection.IsNullableColumn, "IsNullable", typeof(bool));
            VerifyColumn(tableDetailsCollection.DataTypeColumn, "DataType", typeof(string));
            VerifyColumn(tableDetailsCollection.MaximumLengthColumn, "MaximumLength", typeof(int));
            VerifyColumn(tableDetailsCollection.PrecisionColumn, "Precision", typeof(int));
            VerifyColumn(tableDetailsCollection.DateTimePrecisionColumn, "DateTimePrecision", typeof(int));
            VerifyColumn(tableDetailsCollection.ScaleColumn, "Scale", typeof(int));
            VerifyColumn(tableDetailsCollection.IsIdentityColumn, "IsIdentity", typeof(bool));
            VerifyColumn(tableDetailsCollection.IsServerGeneratedColumn, "IsServerGenerated", typeof(bool));
            VerifyColumn(tableDetailsCollection.IsPrimaryKeyColumn, "IsPrimaryKey", typeof(bool));
            VerifyColumn(
                tableDetailsCollection.Columns.OfType<DataColumn>().Single(c => c.ColumnName == "Ordinal"),
                "Ordinal",
                typeof(int));
        }

        private void VerifyColumn(DataColumn dataColumn, string name, Type type)
        {
            Assert.Equal(name, dataColumn.ColumnName);
            Assert.Same(type, dataColumn.DataType);
        }
    }
}
