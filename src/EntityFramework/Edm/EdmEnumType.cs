namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    internal class EdmEnumType : EdmDataModelType
    {
        private readonly BackingList<EdmEnumTypeMember> membersList = new BackingList<EdmEnumTypeMember>();

        public virtual bool IsFlags { get; set; }
        public virtual EdmPrimitiveType UnderlyingType { get; set; }

        public virtual IList<EdmEnumTypeMember> Members
        {
            get { return membersList.EnsureValue(); }
            set { membersList.SetValue(value); }
        }

        internal bool HasMembers
        {
            get { return membersList.HasValue; }
        }

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EnumType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return membersList;
        }
    }
}