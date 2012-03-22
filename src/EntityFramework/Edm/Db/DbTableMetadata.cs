namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification a table in a <see cref = "DbSchemaMetadata" /> database schema.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    internal class DbTableMetadata : DbSchemaMetadataItem
    {
        private readonly BackingList<DbTableColumnMetadata> columnsList = new BackingList<DbTableColumnMetadata>();

        private readonly BackingList<DbForeignKeyConstraintMetadata> fkConstraintsList =
            new BackingList<DbForeignKeyConstraintMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Table;
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbTableColumnMetadata" /> instances that specifies the columns present within the table.
        /// </summary>
        public virtual IList<DbTableColumnMetadata> Columns
        {
            get { return columnsList.EnsureValue(); }
            set { columnsList.SetValue(value); }
        }

        internal bool HasColumns
        {
            get { return columnsList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbTableColumnMetadata" /> instances from the <see cref = "Columns" /> collection of the table that are part of the primary key.
        /// </summary>
        public IEnumerable<DbTableColumnMetadata> KeyColumns
        {
            get { return Columns.Where(c => c != null && c.IsPrimaryKeyColumn); }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbForeignKeyConstraintMetadata" /> instances that defines the foreign key constraints sourced from the table.
        /// </summary>
        public virtual IList<DbForeignKeyConstraintMetadata> ForeignKeyConstraints
        {
            get { return fkConstraintsList.EnsureValue(); }
            set { fkConstraintsList.SetValue(value); }
        }

        internal bool HasForeignKeyConstraints
        {
            get { return fkConstraintsList.HasValue; }
        }

        /*
        /// <summary>
        /// Gets or sets the collection of <see cref="DbUniqueConstraintMetadata"/> instances that specifies the unique constraints defined using columns from the table.
        /// </summary>
        public virtual IList<DbUniqueConstraintMetadata> UniqueConstraints { get { return this.uniqueConstraintsList.EnsureValue(); } set { this.uniqueConstraintsList.SetValue(value); } }

        internal bool HasUniqueConstraints { get { return this.uniqueConstraintsList.HasValue; } }*/
    }
}
