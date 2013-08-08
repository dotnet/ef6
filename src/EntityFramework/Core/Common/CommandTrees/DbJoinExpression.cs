// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents an inner, left outer, or full outer join operation between the given collection arguments on the specified join condition.</summary>
    public sealed class DbJoinExpression : DbExpression
    {
        private readonly DbExpressionBinding _left;
        private readonly DbExpressionBinding _right;
        private readonly DbExpression _condition;

        internal DbJoinExpression(
            DbExpressionKind joinKind, TypeUsage collectionOfRowResultType, DbExpressionBinding left, DbExpressionBinding right,
            DbExpression condition)
            : base(joinKind, collectionOfRowResultType)
        {
            DebugCheck.NotNull(left);
            DebugCheck.NotNull(right);
            DebugCheck.NotNull(condition);
            Debug.Assert(
                DbExpressionKind.InnerJoin == joinKind ||
                DbExpressionKind.LeftOuterJoin == joinKind ||
                DbExpressionKind.FullOuterJoin == joinKind,
                "Invalid DbExpressionKind specified for DbJoinExpression");

            _left = left;
            _right = right;
            _condition = condition;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that provides the left input.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that provides the left input.
        /// </returns>
        public DbExpressionBinding Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that provides the right input.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that provides the right input.
        /// </returns>
        public DbExpressionBinding Right
        {
            get { return _right; }
        }

        /// <summary>Gets the join condition to apply.</summary>
        /// <returns>The join condition to apply.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbJoinExpression" />
        /// , or its result type is not a Boolean type.
        /// </exception>
        public DbExpression JoinCondition
        {
            get { return _condition; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        /// An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        /// A result value of a specific type produced by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        /// .
        /// </returns>
        /// <param name="visitor">
        /// An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
        /// </param>
        /// <typeparam name="TResultType">The type of the result produced by  visitor .</typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
