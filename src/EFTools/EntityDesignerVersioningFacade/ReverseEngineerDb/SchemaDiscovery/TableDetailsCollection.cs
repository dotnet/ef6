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
    ///     Strongly typed DataTable for TableDetails
    /// </summary>
    [Serializable]
    internal sealed class TableDetailsCollection : DataTable, IEnumerable<TableDetailsRow>
    {
        [NonSerialized]
        private DataColumn _columnCatalog;

        [NonSerialized]
        private DataColumn _columnSchema;

        [NonSerialized]
        private DataColumn _columnTable;

        [NonSerialized]
        private DataColumn _columnFieldOrdinal;

        [NonSerialized]
        private DataColumn _columnFieldColumn;

        [NonSerialized]
        private DataColumn _columnIsNullable;

        [NonSerialized]
        private DataColumn _columnDataType;

        [NonSerialized]
        private DataColumn _columnMaximumLength;

        [NonSerialized]
        private DataColumn _columnDateTimePrecision;

        [NonSerialized]
        private DataColumn _columnPrecision;

        [NonSerialized]
        private DataColumn _columnScale;

        [NonSerialized]
        private DataColumn _columnIsIdentity;

        [NonSerialized]
        private DataColumn _columnIsServerGenerated;

        [NonSerialized]
        private DataColumn _columnIsPrimaryKey;

        /// <summary>
        ///     Constructs a TableDetailsDataTable
        /// </summary>
        public TableDetailsCollection()
        {
            TableName = "TableDetails";
            InitClass();
        }

        /// <summary>
        ///     Constructs a new instance TableDetailsDataTable with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        internal TableDetailsCollection(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            UpdateMemberFieldsAfterDeserialization();
        }

        /// <summary>
        ///     Gets the Catalog column
        /// </summary>
        public DataColumn CatalogColumn
        {
            get { return _columnCatalog; }
        }

        /// <summary>
        ///     Gets the Schema column
        /// </summary>
        public DataColumn SchemaColumn
        {
            get { return _columnSchema; }
        }

        /// <summary>
        ///     Gets the TableName column
        /// </summary>
        public DataColumn TableNameColumn
        {
            get { return _columnTable; }
        }

        /// <summary>
        ///     Gets the ColumnName column
        /// </summary>
        public DataColumn ColumnNameColumn
        {
            get { return _columnFieldColumn; }
        }

        /// <summary>
        ///     Gets the IsNullable column
        /// </summary>
        public DataColumn IsNullableColumn
        {
            get { return _columnIsNullable; }
        }

        /// <summary>
        ///     Gets the DataType column
        /// </summary>
        public DataColumn DataTypeColumn
        {
            get { return _columnDataType; }
        }

        /// <summary>
        ///     Gets the MaximumLength column
        /// </summary>
        public DataColumn MaximumLengthColumn
        {
            get { return _columnMaximumLength; }
        }

        /// <summary>
        ///     Gets the Precision column
        /// </summary>
        public DataColumn PrecisionColumn
        {
            get { return _columnPrecision; }
        }

        /// <summary>
        ///     Gets the Precision column
        /// </summary>
        public DataColumn DateTimePrecisionColumn
        {
            get { return _columnDateTimePrecision; }
        }

        /// <summary>
        ///     Gets the Scale column
        /// </summary>
        public DataColumn ScaleColumn
        {
            get { return _columnScale; }
        }

        /// <summary>
        ///     Gets the IsIdentityColumn column
        /// </summary>
        public DataColumn IsIdentityColumn
        {
            get { return _columnIsIdentity; }
        }

        /// <summary>
        ///     Gets the IsIdentityColumn column
        /// </summary>
        public DataColumn IsServerGeneratedColumn
        {
            get { return _columnIsServerGenerated; }
        }

        /// <summary>
        ///     Gets the IsPrimaryKey column
        /// </summary>
        public DataColumn IsPrimaryKeyColumn
        {
            get { return _columnIsPrimaryKey; }
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
            return new TableDetailsCollection();
        }

        private const string CatalogNameColumnName = "CatalogName";
        private const string SchamaNameColumnName = "SchemaName";
        private const string TableNameColumnName = "TableName";
        private const string ColumnNameColumnName = "ColumnName";
        private const string OrdinalColumnName = "Ordinal";
        private const string IsNullableColumnName = "IsNullable";
        private const string DataTypeColumnName = "DataType";
        private const string MaxLengthColumnName = "MaximumLength";
        private const string PrecisionColumnName = "Precision";
        private const string DateTimePrecisionColumnName = "DateTimePrecision";
        private const string ScaleColumnName = "Scale";
        private const string IsIdentityColumnName = "IsIdentity";
        private const string IsServerGeneratedColumnName = "IsServerGenerated";
        private const string IsPrimaryKeyColumnName = "IsPrimaryKey";

        private void InitClass()
        {
            _columnCatalog = new DataColumn(CatalogNameColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnCatalog);
            _columnSchema = new DataColumn(SchamaNameColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnSchema);
            _columnTable = new DataColumn(TableNameColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnTable);
            _columnFieldColumn = new DataColumn(ColumnNameColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnFieldColumn);
            _columnFieldOrdinal = new DataColumn(OrdinalColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnFieldOrdinal);
            _columnIsNullable = new DataColumn(IsNullableColumnName, typeof(bool), null, MappingType.Element);
            Columns.Add(_columnIsNullable);
            _columnDataType = new DataColumn(DataTypeColumnName, typeof(string), null, MappingType.Element);
            Columns.Add(_columnDataType);
            _columnMaximumLength = new DataColumn(MaxLengthColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnMaximumLength);
            _columnPrecision = new DataColumn(PrecisionColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnPrecision);
            _columnDateTimePrecision = new DataColumn(DateTimePrecisionColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnDateTimePrecision);
            _columnScale = new DataColumn(ScaleColumnName, typeof(int), null, MappingType.Element);
            Columns.Add(_columnScale);
            _columnIsIdentity = new DataColumn(IsIdentityColumnName, typeof(bool), null, MappingType.Element);
            Columns.Add(_columnIsIdentity);
            _columnIsServerGenerated = new DataColumn(IsServerGeneratedColumnName, typeof(bool), null, MappingType.Element);
            Columns.Add(_columnIsServerGenerated);
            _columnIsPrimaryKey = new DataColumn(IsPrimaryKeyColumnName, typeof(bool), null, MappingType.Element);
            Columns.Add(_columnIsPrimaryKey);
        }

        private void UpdateMemberFieldsAfterDeserialization()
        {
            _columnCatalog = Columns[CatalogNameColumnName];
            _columnSchema = Columns[SchamaNameColumnName];
            _columnTable = Columns[TableNameColumnName];
            _columnFieldColumn = Columns[ColumnNameColumnName];
            _columnFieldOrdinal = Columns[OrdinalColumnName];
            _columnIsNullable = Columns[IsNullableColumnName];
            _columnDataType = Columns[DataTypeColumnName];
            _columnMaximumLength = Columns[MaxLengthColumnName];
            _columnPrecision = Columns[PrecisionColumnName];
            _columnDateTimePrecision = Columns[DateTimePrecisionColumnName];
            _columnScale = Columns[ScaleColumnName];
            _columnIsIdentity = Columns[IsIdentityColumnName];
            _columnIsServerGenerated = Columns[IsServerGeneratedColumnName];
            _columnIsPrimaryKey = Columns[IsPrimaryKeyColumnName];
        }

        /// <summary>
        ///     Create a new row from a DataRowBuilder object.
        /// </summary>
        /// <param name="builder">The builder to create the row from.</param>
        /// <returns>The row that was created.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new TableDetailsRow(builder);
        }

        /// <summary>
        ///     Gets the Type that this row is.
        /// </summary>
        /// <returns>The type of this row.</returns>
        protected override Type GetRowType()
        {
            return typeof(TableDetailsRow);
        }

        IEnumerator<TableDetailsRow> IEnumerable<TableDetailsRow>.GetEnumerator()
        {
            return Rows.Cast<TableDetailsRow>().GetEnumerator();
        }
    }
}
