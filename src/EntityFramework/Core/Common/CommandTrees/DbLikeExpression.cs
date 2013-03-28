// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents a string comparison against the specified pattern with an optional escape string. This class cannot be inherited.  </summary>
    public sealed class DbLikeExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _pattern;
        private readonly DbExpression _escape;

        internal DbLikeExpression(TypeUsage booleanResultType, DbExpression input, DbExpression pattern, DbExpression escape)
            : base(DbExpressionKind.Like, booleanResultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(pattern);
            DebugCheck.NotNull(escape);
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(input.ResultType, PrimitiveTypeKind.String),
                "DbLikeExpression argument must have a string result type");
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(pattern.ResultType, PrimitiveTypeKind.String),
                "DbLikeExpression pattern must have a string result type");
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(escape.ResultType, PrimitiveTypeKind.String),
                "DbLikeExpression escape must have a string result type");
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbLikeExpression must have a Boolean result type");

            _argument = input;
            _pattern = pattern;
            _escape = escape;
        }

        /// <summary>Gets or sets an expression that specifies the string to compare against the given pattern.</summary>
        /// <returns>An expression that specifies the string to compare against the given pattern.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" />
        ///     , or its result type is not a string type.
        /// </exception>
        public DbExpression Argument
        {
            get { return _argument; }
        }

        /// <summary>Gets or sets an expression that specifies the pattern against which the given string should be compared.</summary>
        /// <returns>An expression that specifies the pattern against which the given string should be compared.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" />
        ///     , or its result type is not a string type.
        /// </exception>
        public DbExpression Pattern
        {
            get { return _pattern; }
        }

        /// <summary>Gets or sets an expression that provides an optional escape string to use for the comparison.</summary>
        /// <returns>An expression that provides an optional escape string to use for the comparison.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" />
        ///     , or its result type is not a string type.
        /// </exception>
        public DbExpression Escape
        {
            get { return _escape; }
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
