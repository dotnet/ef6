// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.EnumMember" />.
    /// </summary>
    internal sealed class MetadataEnumMember : MetadataMember
    {
        internal MetadataEnumMember(string name, TypeUsage enumType, EnumMember enumMember)
            : base(MetadataMemberClass.EnumMember, name)
        {
            DebugCheck.NotNull(enumType);
            DebugCheck.NotNull(enumMember);
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
