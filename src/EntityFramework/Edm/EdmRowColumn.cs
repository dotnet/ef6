#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;

    /// <summary>
    /// Allows the construction and modification of a column of an Entity Data Model (EDM) row type.
    /// </summary>
    internal class EdmRowColumn
        : EdmStructuralMember
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.RowColumn;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Yield(this.ColumnType);
        }
        
        /// <summary>
        /// Gets or sets an <see cref="EdmTypeReference"/> that specifies the result type of the column.
        /// </summary>
        public virtual EdmTypeReference ColumnType { get; set; }
    }
}

#endif