// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a group by operation, which is a grouping of the elements in the input set based on the specified key expressions followed by the application of the specified aggregates.
    /// </summary>
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
        ///     Gets the <see cref="DbGroupExpressionBinding" /> that specifies the input set and provides access to the set element and group element variables.
        /// </summary>
        public DbGroupExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        ///     Gets an <see cref="DbExpression" /> list that provides grouping keys.
        /// </summary>
        public IList<DbExpression> Keys
        {
            get { return _keys; }
        }

        /// <summary>
        ///     Gets an <see cref="DbAggregate" /> list that provides the aggregates to apply.
        /// </summary>
        public IList<DbAggregate> Aggregates
        {
            get { return _aggregates; }
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
