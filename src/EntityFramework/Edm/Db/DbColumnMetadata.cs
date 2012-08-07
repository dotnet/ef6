// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     When implemented in derived types, allows the construction and modification of a column in a Database Metadata table or row.
    /// </summary>
    internal abstract class DbColumnMetadata : DbNamedMetadataItem
    {
        /// <summary>
        ///     Gets or sets a string indicating the database-specific type of the column.
        /// </summary>
        public virtual string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the column is nullable.
        /// </summary>
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets an optional <see cref="DbPrimitiveTypeFacets" /> instance that applies additional constraints to the referenced database-specific type of the column.
        /// </summary>
        public virtual DbPrimitiveTypeFacets Facets { get; set; }
    }
}
