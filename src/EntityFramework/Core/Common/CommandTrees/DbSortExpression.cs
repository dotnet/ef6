// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents a sort operation applied to the elements of the specified input set based on the given sort keys. This class cannot be inherited.</summary>
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
        /// <typeparam name="TResultType">The type of the result produced by 
        /// visitor
        /// </typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// visitor  
        /// is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
