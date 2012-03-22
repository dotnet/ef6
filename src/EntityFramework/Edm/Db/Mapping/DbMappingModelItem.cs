namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     DbMappingModelItem is the base for all types in the EDM-to-Database Mapping construction and modification API.
    /// </summary>
    internal abstract class DbMappingModelItem : DataModelItem
    {
#if IncludeUnusedEdmCode
    /// <summary>
    /// Gets a <see cref="DbMappingItemKind"/> value indicating which EDM-to-Database Mapping concept is represented by this item.
    /// </summary>
        internal DbMappingItemKind ItemKind { get { return this.GetItemKind(); } }
#endif

        internal abstract DbMappingItemKind GetItemKind();
    }
}
