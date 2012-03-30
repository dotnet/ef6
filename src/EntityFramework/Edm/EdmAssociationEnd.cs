namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of one end of an Entity Data Model (EDM) association.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class EdmAssociationEnd : EdmStructuralMember
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.AssociationEnd;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the entity type referenced by this association end.
        /// </summary>
        public virtual EdmEntityType EntityType { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEndKind" /> of this association end, which indicates the multiplicity of the end and whether or not it is required.
        /// </summary>
        public virtual EdmAssociationEndKind EndKind { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmOperationAction" /> to take when a delete operation is attempted.
        /// </summary>
        public virtual EdmOperationAction? DeleteAction { get; set; }
    }
}
