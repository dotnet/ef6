namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    ///     Allows the construction and modification of the mapping of an EDM entity set ( <see cref = "EdmEntitySet" /> ) to a database ( <see cref = "DbDatabaseMetadata" /> ).
    /// </summary>
    internal class DbEntitySetMapping
        : DbMappingMetadataItem
    {
        private readonly BackingList<DbEntityTypeMapping> entityTypeMappingsList =
            new BackingList<DbEntityTypeMapping>();

#if IncludeUnusedEdmCode
        private readonly BackingList<DbQueryViewMapping> queryViewMappings = new BackingList<DbQueryViewMapping>();
#endif

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntitySetMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref = "EdmEntitySet" /> value representing the entity set that is being mapped.
        /// </summary>
        public virtual EdmEntitySet EntitySet { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "DbEntityTypeMapping" /> s that specifies how the set's entity types are mapped to the database.
        /// </summary>
        public virtual IList<DbEntityTypeMapping> EntityTypeMappings
        {
            get { return entityTypeMappingsList.EnsureValue(); }
            set { entityTypeMappingsList.SetValue(value); }
        }

#if IncludeUnusedEdmCode
    /// <summary>
    /// Gets the collection of <see cref="DbQueryViewMapping"/>s that specifies the .
    /// </summary>
        public virtual IList<DbQueryViewMapping> QueryViewMappings { get { return this.queryViewMappings.EnsureValue(); } set { this.queryViewMappings.SetValue(value); } }
        
        internal bool HasEntityTypeMappings { get { return this.entityTypeMappingsList.HasValue; } }
#endif
    }
}
