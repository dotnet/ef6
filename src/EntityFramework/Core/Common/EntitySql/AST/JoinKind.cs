namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents join kind (cross,inner,leftouter,rightouter).
    /// </summary>
    internal enum JoinKind
    {
        Cross,
        Inner,
        LeftOuter,
        FullOuter,
        RightOuter
    }
}