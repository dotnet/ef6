namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Defines literal value kind, including the eSQL untyped NULL.
    /// </summary>
    internal enum LiteralKind
    {
        Number,
        String,
        UnicodeString,
        Boolean,
        Binary,
        DateTime,
        Time,
        DateTimeOffset,
        Guid,
        Null
    }
}