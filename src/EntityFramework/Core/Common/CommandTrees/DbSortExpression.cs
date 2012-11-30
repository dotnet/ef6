// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a sort operation applied to the elements of the specified input set based on the given sort keys.
    /// </summary>
    public sealed class DbSortExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly ReadOnlyCollection<DbSortClause> _keys;

        internal DbSortExpression(TypeUsage resultType, DbExpressionBinding input, ReadOnlyCollection<DbSortClause> sortOrder)
            : base(DbExpressionKind.Sort, resultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(sortOrder);
            Debug.Assert(TypeSemantics.IsCollectionType(resultType), "DbSkipExpression requires a collection result type");

            _input = input;
            _keys = sortOrder;
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
