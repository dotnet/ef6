namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an entity set in an Entity Data Model (EDM) <see cref = "EdmEntityContainer" /> .
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class EdmEntitySet
        : EdmEntityContainerItem
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EntitySet;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmEntityType" /> that specifies the entity type for the set.
        /// </summary>
        public virtual EdmEntityType ElementType { get; set; }
    }
}
