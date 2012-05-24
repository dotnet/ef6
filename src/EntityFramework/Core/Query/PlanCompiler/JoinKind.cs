namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// The only join kinds we care about
    /// </summary>
    internal enum JoinKind
    {
        Inner,
        LeftOuter
    }
}
