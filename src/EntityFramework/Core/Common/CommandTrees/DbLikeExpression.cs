// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a string comparison against the specified pattern with an optional escape string
    /// </summary>
    public sealed class DbLikeExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _pattern;
        private readonly DbExpression _escape;

        internal DbLikeExpression(TypeUsage booleanResultType, DbExpression input, DbExpression pattern, DbExpression escape)
            : base(DbExpressionKind.Like, booleanResultType)
        {
            Debug.Assert(input != null, "DbLikeExpression argument cannot be null");
            Debug.Assert(pattern != null, "DbLikeExpression pattern cannot be null");
            Debug.Assert(escape != null, "DbLikeExpression escape cannot be null");
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

        /// <summary>
        ///     Gets the expression that specifies the string to compare against the given pattern
        /// </summary>
        public DbExpression Argument
        {
            get { return _argument; }
        }

        /// <summary>
        ///     Gets the expression that specifies the pattern against which the given string should be compared
        /// </summary>
        public DbExpression Pattern
        {
            get { return _pattern; }
        }

        /// <summary>
        ///     Gets the expression that provides an optional escape string to use for the comparison
        /// </summary>
        public DbExpression Escape
        {
            get { return _escape; }
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
