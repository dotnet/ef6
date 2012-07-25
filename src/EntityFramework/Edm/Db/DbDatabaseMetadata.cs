// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of a database in a Database Metadata model.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class DbDatabaseMetadata
        : DbAliasedMetadataItem
    {
        private readonly BackingList<DbSchemaMetadata> schemasList = new BackingList<DbSchemaMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Database;
        }

        /// <summary>
        ///     Gets or sets an optional value that indicates the database model version.
        /// </summary>
        public virtual double Version { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbSchemaMetadata" /> instances that specifies the schemas within the database.
        /// </summary>
        public virtual IList<DbSchemaMetadata> Schemas
        {
            get { return schemasList.EnsureValue(); }
            set { schemasList.SetValue(value); }
        }

        internal bool HasSchemas
        {
            get { return schemasList.HasValue; }
        }
    }
}
