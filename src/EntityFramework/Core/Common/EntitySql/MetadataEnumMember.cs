namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.EnumMember"/>.
    /// </summary>
    internal sealed class MetadataEnumMember : MetadataMember
    {
        internal MetadataEnumMember(string name, TypeUsage enumType, EnumMember enumMember)
            : base(MetadataMemberClass.EnumMember, name)
        {
            Debug.Assert(enumType != null, "enumType must not be null");
            Debug.Assert(enumMember != null, "enumMember must not be null");
            EnumType = enumType;
            EnumMember = enumMember;
        }

        internal override string MetadataMemberClassName
        {
            get { return EnumMemberClassName; }
        }

        internal static string EnumMemberClassName
        {
            get { return Strings.LocalizedEnumMember; }
        }

        internal readonly TypeUsage EnumType;
        internal readonly EnumMember EnumMember;
    }
}
