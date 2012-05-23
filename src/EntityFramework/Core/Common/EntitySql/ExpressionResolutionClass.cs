namespace System.Data.Entity.Core.Common.EntitySql
{
    /// <summary>
    /// Represents eSQL expression class.
    /// </summary>
    internal enum ExpressionResolutionClass
    {
        /// <summary>
        /// A value expression such as a literal, variable or a value-returning expression.
        /// </summary>
        Value,

        /// <summary>
        /// An expression returning an entity container.
        /// </summary>
        EntityContainer,

        /// <summary>
        /// An expression returning a metadata member such as a type, function group or namespace.
        /// </summary>
        MetadataMember
    }
}
