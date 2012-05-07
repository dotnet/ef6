namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// Enumeration of Boolean expression node types.
    /// </summary>
    internal enum ExprType
    {
        And,
        Not,
        Or,
        Term,
        True,
        False,
    }
}