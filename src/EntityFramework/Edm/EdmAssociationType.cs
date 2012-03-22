namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;

    // <summary>
    /// Allows the construction and modification of an association type in an Entity Data Model (EDM)
    /// <see cref = "EdmNamespace" />
    /// .
    /// </summary>
    internal class EdmAssociationType
        : EdmStructuralType
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.AssociationType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Yield(SourceEnd, TargetEnd, Constraint);
        }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEnd" /> that defines the source end of the association.
        /// </summary>
        public virtual EdmAssociationEnd SourceEnd { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEnd" /> that defines the target end of the association.
        /// </summary>
        public virtual EdmAssociationEnd TargetEnd { get; set; }

        /// <summary>
        ///     Gets or sets the optional constraint that indicates whether the relationship is an independent association (no constraint present) or a foreign key relationship ( <see cref = "EdmAssociationConstraint" /> specified).
        /// </summary>
        public virtual EdmAssociationConstraint Constraint { get; set; }

        public override EdmStructuralTypeMemberCollection Members
        {
            get { return new EdmStructuralTypeMemberCollection(() => new EdmStructuralMember[] { SourceEnd, TargetEnd }); }
        }
    }
}
