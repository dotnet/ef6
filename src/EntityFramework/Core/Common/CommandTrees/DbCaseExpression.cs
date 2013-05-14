// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents the When, Then, and Else clauses of the
    ///     <see
    ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />
    ///     . This class cannot be inherited.
    /// </summary>
    public sealed class DbCaseExpression : DbExpression
    {
        private readonly DbExpressionList _when;
        private readonly DbExpressionList _then;
        private readonly DbExpression _else;

        internal DbCaseExpression(TypeUsage commonResultType, DbExpressionList whens, DbExpressionList thens, DbExpression elseExpr)
            : base(DbExpressionKind.Case, commonResultType)
        {
            DebugCheck.NotNull(whens);
            DebugCheck.NotNull(thens);
            DebugCheck.NotNull(elseExpr);
            Debug.Assert(whens.Count == thens.Count, "DbCaseExpression whens count must match thens count");

            _when = whens;
            _then = thens;
            _else = elseExpr;
        }

        /// <summary>
        ///     Gets the When clauses of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </summary>
        /// <returns>
        ///     The When clauses of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </returns>
        public IList<DbExpression> When
        {
            get { return _when; }
        }

        /// <summary>
        ///     Gets the Then clauses of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </summary>
        /// <returns>
        ///     The Then clauses of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </returns>
        public IList<DbExpression> Then
        {
            get { return _then; }
        }

        /// <summary>
        ///     Gets the Else clause of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </summary>
        /// <returns>
        ///     The Else clause of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />
        ///     ,or its result type is not equal or promotable to the result type of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />
        ///     .
        /// </exception>
        public DbExpression Else
        {
            get { return _else; }
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
