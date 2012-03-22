namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Linq;

    internal class EdmEnumTypeMember : EdmStructuralMember
    {
        public virtual long Value { get; set; }

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EnumTypeMember;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }
    }
}
