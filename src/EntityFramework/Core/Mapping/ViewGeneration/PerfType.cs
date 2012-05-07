namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    internal enum PerfType
    {
        InitialSetup = 0,
        CellCreation,
        KeyConstraint,
        ViewgenContext,
        UpdateViews,
        DisjointConstraint,
        PartitionConstraint,
        DomainConstraint,
        ForeignConstraint,
        QueryViews,
        BoolResolution,
        Unsatisfiability,
        ViewParsing,
    }
}