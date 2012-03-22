
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    /// Allows the construction and modification of a an Entity Data Model (EDM) row type.
    /// </summary>
    internal class EdmRowType
        : EdmStructuralType
    {
        private readonly BackingList<EdmRowColumn> columnsList = new BackingList<EdmRowColumn>();

        public override string Name
        {
            get { return base.Name; }
            set { throw EdmUtil.NotSupported(System.Data.Entity.Edm.Strings.EdmRowType_SetPropertyNotSupported(EdmConstants.Property_Name)); }
        }

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.RowType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return this.columnsList;
        }

        public override EdmStructuralTypeMemberCollection Members
        {
            get { return new EdmStructuralTypeMemberCollection(() => this.columnsList); }
        }

        /// <summary>
        /// Gets or sets the columns that define the structure of the row type.
        /// </summary>
        public virtual IList<EdmRowColumn> Columns { get { return this.columnsList.EnsureValue(); } set { this.columnsList.SetValue(value); } }
    }
}

#endif
