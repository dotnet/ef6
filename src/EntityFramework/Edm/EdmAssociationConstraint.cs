namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of a constraint applied to an Entity Data Model (EDM) association.
    /// </summary>
    internal class EdmAssociationConstraint : EdmMetadataItem
    {
        private readonly BackingList<EdmProperty> dependentPropertiesList = new BackingList<EdmProperty>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.AssociationConstraint;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEnd" /> that represents the 'dependent' end of the constraint; properties from this association end's entity type contribute to the <see cref = "DependentProperties" /> collection.
        /// </summary>
        public virtual EdmAssociationEnd DependentEnd { get; set; }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "EdmProperty" /> instances from the <see cref = "DependentEnd" /> of the constraint. The values of these properties are constrained against the primary key values of the remaining, 'principal' association end's entity type.
        /// </summary>
        public virtual IList<EdmProperty> DependentProperties
        {
            get { return dependentPropertiesList.EnsureValue(); }
            set { dependentPropertiesList.SetValue(value); }
        }
    }
}