namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;

    /// <summary>
    /// Represents an eSQL expression classified as <see cref="ExpressionResolutionClass.Value"/>.
    /// </summary>
    internal sealed class ValueExpression : ExpressionResolution
    {
        internal ValueExpression(DbExpression value)
            : base(ExpressionResolutionClass.Value)
        {
            Value = value;
        }

        internal override string ExpressionClassName
        {
            get { return ValueClassName; }
        }

        internal static string ValueClassName
        {
            get { return Strings.LocalizedValueExpression; }
        }

        /// <summary>
        /// Null if <see cref="ValueExpression"/> represents the untyped null.
        /// </summary>
        internal readonly DbExpression Value;
    }
}
