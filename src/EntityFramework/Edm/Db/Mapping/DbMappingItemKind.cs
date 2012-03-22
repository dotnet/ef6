namespace System.Data.Entity.Edm.Db.Mapping
{
    /// <summary>
    ///     Indicates which EDM-to-Database Mapping concept is represented by a given item.
    /// </summary>
    internal enum DbMappingItemKind
    {
        /// <summary>
        ///     Database Mapping Kind
        /// </summary>
        DatabaseMapping,

        /// <summary>
        ///     Entity Container Mapping Kind
        /// </summary>
        EntityContainerMapping,

        /// <summary>
        ///     Entity Set Mapping Kind
        /// </summary>
        EntitySetMapping,

        /// <summary>
        ///     Association Set Mapping Kind
        /// </summary>
        AssociationSetMapping,

        /// <summary>
        ///     Entity Type Mapping Kind
        /// </summary>
        EntityTypeMapping,

        /// <summary>
        ///     Query View Mapping Kind
        /// </summary>
        QueryViewMapping,

        /// <summary>
        ///     Entity Type Mapping Fragment Kind
        /// </summary>
        EntityTypeMappingFragment,

        /// <summary>
        ///     Edm Property Mapping Kind
        /// </summary>
        EdmPropertyMapping,

        /// <summary>
        ///     Association End Mapping Kind
        /// </summary>
        AssociationEndMapping,

        /// <summary>
        ///     Column Condition Kind
        /// </summary>
        ColumnCondition,

        /// <summary>
        ///     Property Condition Kind
        /// </summary>
        PropertyCondition
    }
}
