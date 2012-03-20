namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    ///     Allows the construction and modification of the mapping of an EDM entity container ( <see cref = "EdmEntityContainer" /> ) to a database ( <see cref = "DbDatabaseMetadata" /> ).
    /// </summary>
    internal class DbEntityContainerMapping
        : DbMappingMetadataItem
    {
        private readonly BackingList<DbEntitySetMapping> entitySetMappingsList = new BackingList<DbEntitySetMapping>();
        private readonly BackingList<DbAssociationSetMapping> associationSetMappings = new BackingList<DbAssociationSetMapping>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntityContainerMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref = "EdmEntityContainer" /> value representing the entity container that is being mapped.
        /// </summary>
        public virtual EdmEntityContainer EntityContainer { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbEntitySetMapping" /> s that specifies how the container's entity sets are mapped to the database.
        /// </summary>
        public virtual IList<DbEntitySetMapping> EntitySetMappings
        {
            get { return entitySetMappingsList.EnsureValue(); }
            set { entitySetMappingsList.SetValue(value); }
        }

        /// <summary>
        ///     Gets the collection of <see cref = "DbAssociationSetMapping" /> s that specifies how the container's association sets are mapped to the database.
        /// </summary>
        public virtual IList<DbAssociationSetMapping> AssociationSetMappings
        {
            get { return associationSetMappings.EnsureValue(); }
            set { associationSetMappings.SetValue(value); }
        }

#if IncludeUnusedEdmCode
        internal bool HasEntitySetMappings { get { return this.entitySetMappingsList.HasValue; } }
#endif
    }
}