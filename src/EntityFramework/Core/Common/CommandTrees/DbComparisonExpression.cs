namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a comparison operation (equality, greater than, greather than or equal, less than, less than or equal, inequality) applied to two arguments.
    /// </summary>
    /// <remarks>
    ///     DbComparisonExpression requires that its arguments have a common result type
    ///     that is equality comparable (for <see cref="DbExpressionKind"/>.Equals and <see cref="DbExpressionKind"/>.NotEquals),
    ///     order comparable (for <see cref="DbExpressionKind"/>.GreaterThan and <see cref="DbExpressionKind"/>.LessThan),
    ///     or both (for <see cref="DbExpressionKind"/>.GreaterThanOrEquals and <see cref="DbExpressionKind"/>.LessThanOrEquals).
    /// </remarks>
    public sealed class DbComparisonExpression : DbBinaryExpression
    {
        internal DbComparisonExpression(DbExpressionKind kind, TypeUsage booleanResultType, DbExpression left, DbExpression right)
            : base(kind, booleanResultType, left, right)
        {
            Debug.Assert(left != null, "DbComparisonExpression left cannot be null");
            Debug.Assert(right != null, "DbComparisonExpression right cannot be null");
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbComparisonExpression result type must be a Boolean type");
            Debug.Assert(
                DbExpressionKind.Equals == kind ||
                DbExpressionKind.LessThan == kind ||
                DbExpressionKind.LessThanOrEquals == kind ||
                DbExpressionKind.GreaterThan == kind ||
                DbExpressionKind.GreaterThanOrEquals == kind ||
                DbExpressionKind.NotEquals == kind,
                "Invalid DbExpressionKind used in DbComparisonExpression: " + Enum.GetName(typeof(DbExpressionKind), kind)
                );
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            if (visitor != null)
            {
                return visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }
    }
}