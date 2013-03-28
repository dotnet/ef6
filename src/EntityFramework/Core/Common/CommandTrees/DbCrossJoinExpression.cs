// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents an unconditional join operation between the given collection arguments. This class cannot be inherited. </summary>
    public sealed class DbCrossJoinExpression : DbExpression
    {
        private readonly ReadOnlyCollection<DbExpressionBinding> _inputs;

        internal DbCrossJoinExpression(TypeUsage collectionOfRowResultType, ReadOnlyCollection<DbExpressionBinding> inputs)
            : base(DbExpressionKind.CrossJoin, collectionOfRowResultType)
        {
            DebugCheck.NotNull(inputs);
            Debug.Assert(inputs.Count >= 2, "DbCrossJoin requires at least two inputs");

            _inputs = inputs;
        }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> list that provides the input sets to the join.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> list that provides the input sets to the join.
        /// </returns>
        public IList<DbExpressionBinding> Inputs
        {
            get { return _inputs; }
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
        /// <typeparam name="TResultType">The type of the result produced by  visitor. </typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
