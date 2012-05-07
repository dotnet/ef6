namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Column map for a "typed" column
    /// - either an entity type or a complex type
    /// </summary>
    internal abstract class TypedColumnMap : StructuredColumnMap
    {
        /// <summary>
        /// Typed columnMap constructor
        /// </summary>
        /// <param name="type">Datatype of column</param>
        /// <param name="name">column name</param>
        /// <param name="properties">List of column maps - one for each property</param>
        internal TypedColumnMap(TypeUsage type, string name, ColumnMap[] properties)
            : base(type, name, properties)
        {
        }
    }
}