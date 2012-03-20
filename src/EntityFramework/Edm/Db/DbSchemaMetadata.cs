namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    ///     Allows the construction and modification of a database schema in a <see cref = "DbDatabaseMetadata" /> database model.
    /// </summary>
    internal class DbSchemaMetadata
        : DbAliasedMetadataItem
    {
        private readonly BackingList<DbTableMetadata> tablesList = new BackingList<DbTableMetadata>();

#if IncludeUnusedEdmCode
        private readonly BackingList<DbFunctionMetadata> functionsList = new BackingList<DbFunctionMetadata>();
#endif

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Schema;
        }

#if IncludeUnusedEdmCode
    /// <summary>
    /// Gets all <see cref="DbSchemaMetadataItem"/>s declared within the schema. Includes <see cref="DbTableMetadata"/> instances and <see cref="DbFunctionMetadata"/> instances.
    /// </summary>
        public IEnumerable<DbSchemaMetadataItem> SchemaItems
        {
            get
            {
                return this.tablesList.Concat<DbSchemaMetadataItem>(this.functionsList);
            }
        }
#endif

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbTableMetadata" /> instances that specifies the tables declared within the schema.
        /// </summary>
        public virtual IList<DbTableMetadata> Tables
        {
            get { return tablesList.EnsureValue(); }
            set { tablesList.SetValue(value); }
        }

        internal bool HasTables
        {
            get { return tablesList.HasValue; }
        }

#if IncludeUnusedEdmCode
    /// <summary>
    /// Gets or sets the collection of <see cref="DbFunctionMetadata"/> instances that specifies the functions declared withing the schema.
    /// </summary>
        public virtual IList<DbFunctionMetadata> Functions { get { return this.functionsList.EnsureValue(); } set { this.functionsList.SetValue(value); } }

        internal bool HasFunctions { get { return this.functionsList.HasValue; } }
#endif
    }
}