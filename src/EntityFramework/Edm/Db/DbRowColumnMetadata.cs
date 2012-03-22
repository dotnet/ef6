
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    /// Allows the construction and modification of a column in the structure of a row <see cref="DbFunctionTypeMetadata"/>.
    /// </summary>
    internal class DbRowColumnMetadata : DbColumnMetadata
    {
        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.RowColumn;
        }
    }
}

#endif
