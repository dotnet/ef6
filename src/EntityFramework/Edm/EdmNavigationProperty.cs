namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an Entity Data Model (EDM) navigation property.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    internal class EdmNavigationProperty
        : EdmStructuralMember
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.NavigationProperty;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationType" /> that specifies the association over which navigation takes place.
        /// </summary>
        public virtual EdmAssociationType Association { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEnd" /> that specifies which association end is the 'destination' end of the navigation and produces the navigation property result.
        /// </summary>
        public virtual EdmAssociationEnd ResultEnd { get; set; }
    }
}
