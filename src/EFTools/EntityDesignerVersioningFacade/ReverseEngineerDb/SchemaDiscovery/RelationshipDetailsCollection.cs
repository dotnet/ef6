// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Strongly type data table for holding the RelationshipDetails
    /// </summary>
    [Serializable]
    internal sealed class RelationshipDetailsCollection : DataTable, IEnumerable<RelationshipDetailsRow>
    {
        [NonSerialized]
        private DataColumn _columnPKCatalog;

        [NonSerialized]
        private DataColumn _columnPKSchema;

        [NonSerialized]
        private DataColumn _columnPKTable;

        [NonSerialized]
        private DataColumn _columnPKColumn;

        [NonSerialized]
        private DataColumn _columnFKCatalog;

        [NonSerialized]
        private DataColumn _columnFKSchema;

        [NonSerialized]
        private DataColumn _columnFKTable;

        [NonSerialized]
        private DataColumn _columnFKColumn;

        [NonSerialized]
        private DataColumn _columnOrdinal;

        [NonSerialized]
        private DataColumn _columnRelationshipName;

        [NonSerialized]
        private DataColumn _columnRelationshipId;

        [NonSerialized]
        private DataColumn _columnRelationshipIsCascadeDelete;

        /// <summary>
        ///     Constructs a RelationsipDetailsDataTable
        /// </summary>
        public RelationshipDetailsCollection()
        {
            TableName = "RelationshipDetails";
            InitClass();
        }

        /// <summary>
        ///     Constructs a new instance RelationshipDetailDataTable with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        internal RelationshipDetailsCollection(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            UpdateMemberFieldsAfterDeserialization();
        }

        /// <summary>
        ///     Gets the PkCatalog column
        /// </summary>
        public DataColumn PKCatalogColumn
        {
            get { return _columnPKCatalog; }
        }

        /// <summary>
        ///     Gets the PkSchema column
        /// </summary>
        public DataColumn PKSchemaColumn
        {
            get { return _columnPKSchema; }
        }

        /// <summary>
        ///     Gets the PkTable column
        /// </summary>
        public DataColumn PKTableColumn
        {
            get { return _columnPKTable; }
        }

        /// <summary>
        ///     Gets the PkColumn column
        /// </summary>
        public DataColumn PKColumnColumn
        {
            get { return _columnPKColumn; }
        }

        /// <summary>
        ///     Gets the FkCatalog column
        /// </summary>
        public DataColumn FKCatalogColumn
        {
            get { return _columnFKCatalog; }
        }

        /// <summary>
        ///     Gets the FkSchema column
        /// </summary>
        public DataColumn FKSchemaColumn
        {
            get { return _columnFKSchema; }
        }

        /// <summary>
        ///     Gets the FkTable column
        /// </summary>
        public DataColumn FKTableColumn
        {
            get { return _columnFKTable; }
        }

        /// <summary>
        ///     Gets the FkColumn column
        /// </summary>
        public DataColumn FKColumnColumn
        {
            get { return _columnFKColumn; }
        }

        /// <summary>
        ///     Gets the Ordinal column
        /// </summary>
        public DataColumn OrdinalColumn
        {
            get { return _columnOrdinal; }
        }

        /// <summary>
        ///     Gets the RelationshipName column
        /// </summary>
        public DataColumn RelationshipNameColumn
        {
            get { return _columnRelationshipName; }
        }

        public DataColumn RelationshipIdColumn
        {
            get { return _columnRelationshipId; }
        }

        /// <summary>
        ///     Gets the IsCascadeDelete value
        /// </summary>
        public DataColumn RelationshipIsCascadeDeleteColumn
        {
            get { return _columnRelationshipIsCascadeDelete; }
        }

        /// <summary>
        ///     Gets an enumerator over the rows.
        /// </summary>
        /// <returns>The row enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        /// <summary>
        ///     Creates an instance of this table
        /// </summary>
        /// <returns>The newly created instance.</returns>
        protected override DataTable CreateInstance()
        {
            return new RelationshipDetailsCollection();
        }

        private const string PkCatalogColumnName = "PkCatalog";
        private const string PkSchemaColumnName = "PkSchema";
        private const string PkTableColumnName = "PkTable";
        private const string PkColumnColumnName = "PkColumn";
        private const string FkCatalogColumnName = "FkCatalog";
        private const string FkSchemaColumnName = "FkSchema";
        private const string FkTableColumnName = "FkTable";
        private const string FkColumnColumnName = "FkColumn";
        private const string OrdinalColumnName = "Ordinal";
        private const string RelationshipNameColumnName = "RelationshipName";
        private const string RelationshipIdColumnName = "RelationshipId";
        private const string IsCascadeDeleteColumnName = "IsCascadeDelete";

        private void InitClass()
        {
            _columnPKCatalog = new DataColumn(PkCatalogColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnPKCatalog);
            _columnPKSchema = new DataColumn(PkSchemaColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnPKSchema);
            _columnPKTable = new DataColumn(PkTableColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnPKTable);
            _columnPKColumn = new DataColumn(PkColumnColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnPKColumn);
            _columnFKCatalog = new DataColumn(FkCatalogColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnFKCatalog);
            _columnFKSchema = new DataColumn(FkSchemaColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnFKSchema);
            _columnFKTable = new DataColumn(FkTableColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnFKTable);
            _columnFKColumn = new DataColumn(FkColumnColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnFKColumn);
            _columnOrdinal = new DataColumn(OrdinalColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnOrdinal);
            _columnRelationshipName = new DataColumn(RelationshipNameColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnRelationshipName);
            _columnRelationshipId = new DataColumn(RelationshipIdColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnRelationshipId);
            _columnRelationshipIsCascadeDelete = new DataColumn(IsCascadeDeleteColumnName, typeof(bool), null, MappingType.Element);
            Columns.Add(_columnRelationshipIsCascadeDelete);
        }

        private void UpdateMemberFieldsAfterDeserialization()
        {
            _columnPKCatalog = Columns[PkCatalogColumnName];
            _columnPKSchema = Columns[PkSchemaColumnName];
            _columnPKTable = Columns[PkTableColumnName];
            _columnPKColumn = Columns[PkColumnColumnName];
            _columnFKCatalog = Columns[FkCatalogColumnName];
            _columnFKSchema = Columns[FkSchemaColumnName];
            _columnFKTable = Columns[FkTableColumnName];
            _columnFKColumn = Columns[FkColumnColumnName];
            _columnOrdinal = Columns[OrdinalColumnName];
            _columnRelationshipName = Columns[RelationshipNameColumnName];
            _columnRelationshipId = Columns[RelationshipIdColumnName];
            _columnRelationshipIsCascadeDelete = Columns[IsCascadeDeleteColumnName];
        }

        /// <summary>
        ///     Create a new row from a DataRowBuilder object.
        /// </summary>
        /// <param name="builder">The builder to create the row from.</param>
        /// <returns>The row that was created.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new RelationshipDetailsRow(builder);
        }

        /// <summary>
        ///     Gets the Type that this row is.
        /// </summary>
        /// <returns>The type of this row.</returns>
        protected override Type GetRowType()
        {
            return typeof(RelationshipDetailsRow);
        }

        IEnumerator<RelationshipDetailsRow> IEnumerable<RelationshipDetailsRow>.GetEnumerator()
        {
            return Rows.Cast<RelationshipDetailsRow>().GetEnumerator();
        }
    }
}
