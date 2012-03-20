#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db.Mapping
{
    /// <summary>
    /// Represents a query view based mapping for a single type or a hierarchy.
    /// </summary>
    internal class DbQueryViewMapping : DbMappingMetadataItem
    {
        /// <summary>
        /// Gets an <see cref="EdmEntityType"/> value representing the entity type or hierarchy that is being mapped.
        /// </summary>
        public virtual EdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this type mapping applies to <see cref="EntityType"/> 
        /// and all its direct or indirect subtypes (<code>true</code>), or only to <see cref="EntityType"/> (<code>false</code>).
        /// </summary>
        public virtual bool IsHierarchyMapping { get; set; }

        /// <summary>
        /// Gets a QueryView mapping for this type.
        /// </summary>
        public virtual string QueryView { get; set; }

        internal override DbMappingItemKind GetItemKind() { return DbMappingItemKind.QueryViewMapping; }
    }
}

#endif