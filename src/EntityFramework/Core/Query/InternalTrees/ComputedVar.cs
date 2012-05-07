namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// A computed expression. Defined by a VarDefOp
    /// </summary>
    internal sealed class ComputedVar : Var
    {
        internal ComputedVar(int id, TypeUsage type)
            : base(id, VarType.Computed, type)
        {
        }
    }
}