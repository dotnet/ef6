
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db.Mapping
{
    /// <summary>
    /// Represents a a condition for a property in an Edm type.
    /// </summary>
    internal class DbPropertyCondition : DbMappingMetadataItem
    {
        /// <summary>
        /// Gets a <see cref="EdmProperty"/> value representing the edm property which must contain <see cref="Value"/> for this condition to hold.
        /// </summary>
        public virtual EdmProperty Property { get; set; }

        /// <summary>
        /// Gets the value that <see cref="Property"/> must contain for this condition to hold.
        /// </summary>
        public virtual object Value { get; set; }

        internal override DbMappingItemKind GetItemKind() { return DbMappingItemKind.PropertyCondition; }

        public virtual bool? IsNull { get; set; }
    }
}

#endif
