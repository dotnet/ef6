// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents the restriction of the number of elements in the argument collection to the specified limit value.</summary>
    public sealed class DbLimitExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _limit;
        private readonly bool _withTies;

        internal DbLimitExpression(TypeUsage resultType, DbExpression argument, DbExpression limit, bool withTies)
            : base(DbExpressionKind.Limit, resultType)
        {
            DebugCheck.NotNull(argument);
            DebugCheck.NotNull(limit);
            Debug.Assert(
                ReferenceEquals(resultType, argument.ResultType), "DbLimitExpression result type must be the result type of the argument");

            _argument = argument;
            _limit = limit;
            _withTies = withTies;
        }

        /// <summary>Gets an expression that specifies the input collection.</summary>
        /// <returns>An expression that specifies the input collection.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" />
        /// , or its result type is not a collection type.
        /// </exception>
        public DbExpression Argument
        {
            get { return _argument; }
        }

        /// <summary>Gets an expression that specifies the limit on the number of elements returned from the input collection.</summary>
        /// <returns>An expression that specifies the limit on the number of elements returned from the input collection.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" />
        /// , or is not one of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" />
        /// or
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" />
        /// , or its result type is not equal or promotable to a 64-bit integer type.
        /// </exception>
        public DbExpression Limit
        {
            get { return _limit; }
        }

        /// <summary>
        /// Gets whether the limit operation will include tied results. Including tied results might produce more results than specified by the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression.Limit" />
        /// value.
        /// </summary>
        /// <returns>true if the limit operation will include tied results; otherwise, false. The default is false.</returns>
        public bool WithTies
        {
            get { return _withTies; }
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
