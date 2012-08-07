// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a Case When...Then...Else logical operation.
    /// </summary>
    public sealed class DbCaseExpression : DbExpression
    {
        private readonly DbExpressionList _when;
        private readonly DbExpressionList _then;
        private readonly DbExpression _else;

        internal DbCaseExpression(TypeUsage commonResultType, DbExpressionList whens, DbExpressionList thens, DbExpression elseExpr)
            : base(DbExpressionKind.Case, commonResultType)
        {
            Debug.Assert(whens != null, "DbCaseExpression whens cannot be null");
            Debug.Assert(thens != null, "DbCaseExpression thens cannot be null");
            Debug.Assert(elseExpr != null, "DbCaseExpression else cannot be null");
            Debug.Assert(whens.Count == thens.Count, "DbCaseExpression whens count must match thens count");

            _when = whens;
            _then = thens;
            _else = elseExpr;
        }

        /// <summary>
        ///     Gets the When clauses of this DbCaseExpression.
        /// </summary>
        public IList<DbExpression> When
        {
            get { return _when; }
        }

        /// <summary>
        ///     Gets the Then clauses of this DbCaseExpression.
        /// </summary>
        public IList<DbExpression> Then
        {
            get { return _then; }
        }

        /// <summary>
        ///     Gets the Else clause of this DbCaseExpression.
        /// </summary>
        public DbExpression Else
        {
            get { return _else; }
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
