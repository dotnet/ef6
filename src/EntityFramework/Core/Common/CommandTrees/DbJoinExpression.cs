// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents an inner, left outer or full outer join operation between the given collection arguments on the specified join condition.
    /// </summary>
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
        ///     Gets the <see cref="DbExpressionBinding" /> provides the left input.
        /// </summary>
        public DbExpressionBinding Left
        {
            get { return _left; }
        }

        /// <summary>
        ///     Gets the <see cref="DbExpressionBinding" /> provides the right input.
        /// </summary>
        public DbExpressionBinding Right
        {
            get { return _right; }
        }

        /// <summary>
        ///     Gets the <see cref="DbExpression" /> that defines the join condition to apply.
        /// </summary>
        public DbExpression JoinCondition
        {
            get { return _condition; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType">
        ///     The type of the result produced by <paramref name="visitor" />
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        /// <returns>
        ///     An instance of <typeparamref name="TResultType" /> .
        /// </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
