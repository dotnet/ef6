
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    /// Allows the construction and modification of a specific use of a type as a Database Metadata function return or parameter type. 
    /// </summary>
    internal class DbFunctionTypeMetadata : DbTypeMetadata
    {
        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.FunctionType;
        }
    }
}

#endif
