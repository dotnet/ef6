// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    public class DbDatabaseMapping : DbMappingMetadataItem
    {
        private readonly BackingList<DbEntityContainerMapping> entityContainerMappingsList =
            new BackingList<DbEntityContainerMapping>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.DatabaseMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref="EdmModel" /> value representing the model that is being mapped.
        /// </summary>
        public virtual EdmModel Model { get; set; }

        /// <summary>
        ///     Gets or sets a <see cref="DbDatabaseMetadata" /> value representing the database that is the target of the mapping.
        /// </summary>
        public virtual DbDatabaseMetadata Database { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbEntityContainerMapping" /> s that specifies how the model's entity containers are mapped to the database.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbEntityContainerMapping> EntityContainerMappings
        {
            get { return entityContainerMappingsList.EnsureValue(); }
            set { entityContainerMappingsList.SetValue(value); }
        }
    }
}
