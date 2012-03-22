
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Allows the construction and modification of a an Entity Data Model (EDM) entity reference (Ref) type.
    /// </summary>
    internal class EdmRefType
        : EdmDataModelType
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.RefType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        /// Gets or sets the <see cref="EdmEntityType"/> that this Ref type represents a reference to.
        /// </summary>
        public virtual EdmEntityType EntityType { get; set; }
    }
}

#endif
