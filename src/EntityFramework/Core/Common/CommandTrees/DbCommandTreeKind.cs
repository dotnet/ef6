namespace System.Data.Entity.Core.Common.CommandTrees
{
    /// <summary>
    /// Describes the different "kinds" (classes) of command trees.
    /// </summary>
    public enum DbCommandTreeKind
    {
        Query,
        Update,
        Insert,
        Delete,
        Function,
    }
}