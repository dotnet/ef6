// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a skip operation of the specified number of elements of the input set after the ordering described in the given sort keys is applied.
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
            Debug.Assert(input != null, "DbSkipExpression input cannot be null");
            Debug.Assert(sortOrder != null, "DbSkipExpression sort order cannot be null");
            Debug.Assert(count != null, "DbSkipExpression count cannot be null");
            Debug.Assert(TypeSemantics.IsCollectionType(resultType), "DbSkipExpression requires a collection result type");

            _input = input;
            _keys = sortOrder;
            _count = count;
        }

        /// <summary>
        ///     Gets the <see cref="DbExpressionBinding" /> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        ///     Gets a <see cref="DbSortClause" /> list that defines the sort order.
        /// </summary>
        public IList<DbSortClause> SortOrder
        {
            get { return _keys; }
        }

        /// <summary>
        ///     Gets the expression that specifies the number of elements from the input collection to skip.
        /// </summary>
        public DbExpression Count
        {
            get { return _count; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
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
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType"> The type of the result produced by <paramref name="visitor" /> </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        /// <returns> An instance of <typeparamref name="TResultType" /> . </returns>
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
