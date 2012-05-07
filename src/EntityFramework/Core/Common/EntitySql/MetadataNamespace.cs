namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Resources;

    /// <summary>
    /// Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.Namespace"/>.
    /// </summary>
    internal sealed class MetadataNamespace : MetadataMember
    {
        internal MetadataNamespace(string name)
            : base(MetadataMemberClass.Namespace, name)
        {
        }

        internal override string MetadataMemberClassName
        {
            get { return NamespaceClassName; }
        }

        internal static string NamespaceClassName
        {
            get { return Strings.LocalizedNamespace; }
        }
    }
}