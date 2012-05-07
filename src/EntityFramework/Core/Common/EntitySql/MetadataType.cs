namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.Type"/>.
    /// </summary>
    internal sealed class MetadataType : MetadataMember
    {
        internal MetadataType(string name, TypeUsage typeUsage)
            : base(MetadataMemberClass.Type, name)
        {
            Debug.Assert(typeUsage != null, "typeUsage must not be null");
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