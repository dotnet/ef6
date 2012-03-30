namespace System.Data.Entity.Edm.Db
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     DbSchemaMetadataItem is the base for all types that can be contained in a <see cref = "DbSchemaMetadata" /> schema.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal abstract class DbSchemaMetadataItem
        : DbAliasedMetadataItem
    {
    }
}
