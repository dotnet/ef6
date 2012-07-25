// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of a database schema in a <see cref = "DbDatabaseMetadata" /> database model.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class DbSchemaMetadata
        : DbAliasedMetadataItem
    {
        private readonly BackingList<DbTableMetadata> tablesList = new BackingList<DbTableMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Schema;
        }

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
    }
}
