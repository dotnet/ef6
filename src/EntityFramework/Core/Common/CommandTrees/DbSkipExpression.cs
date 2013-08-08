// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Skips a specified number of elements in the input set.
    /// <see
    ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" />
    /// can only be used after the input collection has been sorted as specified by the sort keys.
    /// </summary>
    public sealed class DbSkipExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly ReadOnlyCollection<DbSortClause> _keys;
        private readonly DbExpression _count;

        internal DbSkipExpression(
            TypeUsage resultType, DbExpressionBinding input, ReadOnlyCollection<DbSortClause> sortOrder, DbExpression count)
            : base(DbExpressionKind.Skip, resultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(sortOrder);
            DebugCheck.NotNull(count);
            Debug.Assert(TypeSemantics.IsCollectionType(resultType), "DbSkipExpression requires a collection result type");

            _input = input;
            _keys = sortOrder;
            _count = count;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </returns>
        public DbExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortClause" /> list that defines the sort order.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortClause" /> list that defines the sort order.
        /// </returns>
        public IList<DbSortClause> SortOrder
        {
            get { return _keys; }
        }

        /// <summary>Gets an expression that specifies the number of elements to skip from the input collection.</summary>
        /// <returns>An expression that specifies the number of elements to skip from the input collection.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" />
        /// ; the expression is not either a
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" />
        /// or a
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" />
        /// ; or the result type of the expression is not equal or promotable to a 64-bit integer type.
        /// </exception>
        public DbExpression Count
        {
            get { return _count; }
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
