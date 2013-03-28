// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents a group by operation. A group by operation is a grouping of the elements in the input set based on the specified key expressions followed by the application of the specified aggregates. This class cannot be inherited. </summary>
    public sealed class DbGroupByExpression : DbExpression
    {
        private readonly DbGroupExpressionBinding _input;
        private readonly DbExpressionList _keys;
        private readonly ReadOnlyCollection<DbAggregate> _aggregates;

        internal DbGroupByExpression(
            TypeUsage collectionOfRowResultType,
            DbGroupExpressionBinding input,
            DbExpressionList groupKeys,
            ReadOnlyCollection<DbAggregate> aggregates)
            : base(DbExpressionKind.GroupBy, collectionOfRowResultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(groupKeys);
            DebugCheck.NotNull(aggregates);
            Debug.Assert(groupKeys.Count > 0 || aggregates.Count > 0, "At least one key or aggregate is required");

            _input = input;
            _keys = groupKeys;
            _aggregates = aggregates;
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupExpressionBinding" /> that specifies the input set and provides access to the set element and group element variables.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupExpressionBinding" /> that specifies the input set and provides access to the set element and group element variables.
        /// </returns>
        public DbGroupExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides grouping keys.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides grouping keys.
        /// </returns>
        public IList<DbExpression> Keys
        {
            get { return _keys; }
        }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" /> list that provides the aggregates to apply.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" /> list that provides the aggregates to apply.
        /// </returns>
        public IList<DbAggregate> Aggregates
        {
            get { return _aggregates; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        ///     An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        ///     A result value of a specific type produced by
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        ///     .
        /// </returns>
        /// <param name="visitor">
        ///     An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
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
