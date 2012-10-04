// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of the mapping of an EDM entity set ( <see cref="EntitySet" /> ) to a database ( <see
    ///      cref="DbDatabaseMetadata" /> ).
    /// </summary>
    public class DbEntitySetMapping
        : DbMappingMetadataItem
    {
        private readonly List<DbEntityTypeMapping> entityTypeMappingsList =
            new List<DbEntityTypeMapping>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntitySetMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref="EntitySet" /> value representing the entity set that is being mapped.
        /// </summary>
        public virtual EntitySet EntitySet { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbEntityTypeMapping" /> s that specifies how the set's entity types are mapped to the database.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbEntityTypeMapping> EntityTypeMappings
        {
            get { return entityTypeMappingsList; }
        }
    }
}
