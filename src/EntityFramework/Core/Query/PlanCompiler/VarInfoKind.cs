namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// Kind of VarInfo
    /// </summary>
    internal enum VarInfoKind
    {
        /// <summary>
        /// The VarInfo is of <see cref="PrimitiveTypeVarInfo"/> type.
        /// </summary>
        PrimitiveTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="StructuredVarInfo"/> type.
        /// </summary>
        StructuredTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="CollectionVarInfo"/> type.
        /// </summary>
        CollectionVarInfo
    }
}