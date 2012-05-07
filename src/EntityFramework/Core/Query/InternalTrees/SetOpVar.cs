namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// A SetOp Var - used as the output var for set operations (Union, Intersect, Except)
    /// </summary>
    internal sealed class SetOpVar : Var
    {
        internal SetOpVar(int id, TypeUsage type)
            : base(id, VarType.SetOp, type)
        {
        }
    }
}