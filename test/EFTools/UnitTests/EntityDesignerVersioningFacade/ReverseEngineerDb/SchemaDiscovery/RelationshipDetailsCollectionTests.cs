// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using Xunit;

    public class RelationshipDetailsCollectionTests
    {
        [Fact]
        public void Verify_RelationshipDetailsCollection_columns()
        {
            var relationshipDetailsCollection = new RelationshipDetailsCollection();
            Assert.Equal(12, relationshipDetailsCollection.Columns.Count);
            VerifyColumn(relationshipDetailsCollection.PKCatalogColumn, "PkCatalog", typeof(string));
            VerifyColumn(relationshipDetailsCollection.PKSchemaColumn, "PkSchema", typeof(string));
            VerifyColumn(relationshipDetailsCollection.PKTableColumn, "PkTable", typeof(string));
            VerifyColumn(relationshipDetailsCollection.PKColumnColumn, "PkColumn", typeof(string));
            VerifyColumn(relationshipDetailsCollection.FKCatalogColumn, "FkCatalog", typeof(string));
            VerifyColumn(relationshipDetailsCollection.FKSchemaColumn, "FkSchema", typeof(string));
            VerifyColumn(relationshipDetailsCollection.FKColumnColumn, "FkColumn", typeof(string));
            VerifyColumn(relationshipDetailsCollection.OrdinalColumn, "Ordinal", typeof(int));
            VerifyColumn(relationshipDetailsCollection.RelationshipNameColumn, "RelationshipName", typeof(string));
            VerifyColumn(relationshipDetailsCollection.RelationshipIdColumn, "RelationshipId", typeof(string));
            VerifyColumn(relationshipDetailsCollection.RelationshipIsCascadeDeleteColumn, "IsCascadeDelete", typeof(bool));
        }

        private void VerifyColumn(DataColumn dataColumn, string name, Type type)
        {
            Assert.Equal(name, dataColumn.ColumnName);
            Assert.Same(type, dataColumn.DataType);
        }
    }
}
