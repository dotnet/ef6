namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// The abstract base type for expressions that accept two expression operands.
    /// </summary>
    public abstract class DbBinaryExpression : DbExpression
    {
        private readonly DbExpression _left;
        private readonly DbExpression _right;

        internal DbBinaryExpression()
        {
        }

        internal DbBinaryExpression(DbExpressionKind kind, TypeUsage type, DbExpression left, DbExpression right)
            : base(kind, type)
        {
            Debug.Assert(left != null, "DbBinaryExpression.Left cannot be null");
            Debug.Assert(right != null, "DbBinaryExpression.Right cannot be null");

            _left = left;
            _right = right;
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the left argument.
        /// </summary>
        public virtual DbExpression Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the right argument.
        /// </summary>
        public virtual DbExpression Right
        {
            get { return _right; }
        }
    }
}