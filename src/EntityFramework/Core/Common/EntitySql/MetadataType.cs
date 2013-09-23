// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    // <summary>
    // Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.Type" />.
    // </summary>
    internal sealed class MetadataType : MetadataMember
    {
        internal MetadataType(string name, TypeUsage typeUsage)
            : base(MetadataMemberClass.Type, name)
        {
            DebugCheck.NotNull(typeUsage);
            TypeUsage = typeUsage;
        }

        internal override string MetadataMemberClassName
        {
            get { return TypeClassName; }
        }

        internal static string TypeClassName
        {
            get { return Strings.LocalizedType; }
        }

        internal readonly TypeUsage TypeUsage;
    }
}
