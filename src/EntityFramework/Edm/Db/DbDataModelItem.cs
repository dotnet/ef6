namespace System.Data.Entity.Edm.Db
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     DbDataModelItem is the base for all types in the Database Metadata construction and modification API.
    /// </summary>
    internal abstract class DbDataModelItem
        : DataModelItem
    {
#if IncludeUnusedEdmCode
    /// <summary>
    /// Gets a <see cref="DbItemKind"/> value indicating which Database Metadata concept is represented by this item.
    /// </summary>
        public DbItemKind ItemKind { get { return this.GetMetadataKind(); } }
#endif

        internal abstract DbItemKind GetMetadataKind();
    }
}