// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of the mapping of an EDM entity container ( <see cref="EntityContainer" /> ) to a database ( <see
    ///      cref="EdmModel" /> ).
    /// </summary>
    public class DbEntityContainerMapping
        : DbMappingMetadataItem
    {
        private readonly List<DbEntitySetMapping> entitySetMappingsList = new List<DbEntitySetMapping>();

        private readonly List<DbAssociationSetMapping> associationSetMappings =
            new List<DbAssociationSetMapping>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntityContainerMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref="EntityContainer" /> value representing the entity container that is being mapped.
        /// </summary>
        public virtual EntityContainer EntityContainer { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref="DbEntitySetMapping" /> s that specifies how the container's entity sets are mapped to the database.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbEntitySetMapping> EntitySetMappings
        {
            get { return entitySetMappingsList; }
        }

        /// <summary>
        ///     Gets the collection of <see cref="DbAssociationSetMapping" /> s that specifies how the container's association sets are mapped to the database.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbAssociationSetMapping> AssociationSetMappings
        {
            get { return associationSetMappings; }
        }
    }
}
