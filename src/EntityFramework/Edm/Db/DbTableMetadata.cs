// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification a table in a <see cref="DbSchemaMetadata" /> database schema.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    [DebuggerDisplay("{Name}")]
    public class DbTableMetadata : DbSchemaMetadataItem
    {
        private IList<DbTableColumnMetadata> columnsList = new List<DbTableColumnMetadata>();

        private readonly List<DbForeignKeyConstraintMetadata> fkConstraintsList =
            new List<DbForeignKeyConstraintMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Table;
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbTableColumnMetadata" /> instances that specifies the columns present within the table.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbTableColumnMetadata> Columns
        {
            get { return columnsList; }
            set { columnsList = value; }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbTableColumnMetadata" /> instances from the <see cref="Columns" /> collection of the table that are part of the primary key.
        /// </summary>
        public IEnumerable<DbTableColumnMetadata> KeyColumns
        {
            get { return Columns.Where(c => c != null && c.IsPrimaryKeyColumn); }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbForeignKeyConstraintMetadata" /> instances that defines the foreign key constraints sourced from the table.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbForeignKeyConstraintMetadata> ForeignKeyConstraints
        {
            get { return fkConstraintsList; }
        }
    }
}
