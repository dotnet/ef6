namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// Available groups of rules, not necessarily mutually exclusive
    /// </summary>
    internal enum TransformationRulesGroup
    {
        All,
        Project,
        PostJoinElimination
    }
}
