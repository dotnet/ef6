// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    internal class DbDatabaseMapping : DbMappingMetadataItem
    {
        private readonly BackingList<DbEntityContainerMapping> entityContainerMappingsList =
            new BackingList<DbEntityContainerMapping>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.DatabaseMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref = "EdmModel" /> value representing the model that is being mapped.
        /// </summary>
        internal virtual EdmModel Model { get; set; }

        /// <summary>
        ///     Gets or sets a <see cref = "DbDatabaseMetadata" /> value representing the database that is the target of the mapping.
        /// </summary>
        internal virtual DbDatabaseMetadata Database { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbEntityContainerMapping" /> s that specifies how the model's entity containers are mapped to the database.
        /// </summary>
        internal virtual IList<DbEntityContainerMapping> EntityContainerMappings
        {
            get { return entityContainerMappingsList.EnsureValue(); }
            set { entityContainerMappingsList.SetValue(value); }
        }
    }
}
