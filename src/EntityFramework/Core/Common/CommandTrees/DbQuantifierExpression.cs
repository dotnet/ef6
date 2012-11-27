// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a quantifier operation of the specified kind (Any, All) over the elements of the specified input set.
    /// </summary>
    public sealed class DbQuantifierExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _predicate;

        internal DbQuantifierExpression(
            DbExpressionKind kind, TypeUsage booleanResultType, DbExpressionBinding input, DbExpression predicate)
            : base(kind, booleanResultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(predicate);
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean),
                "DbQuantifierExpression must have a Boolean result type");
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(predicate.ResultType, PrimitiveTypeKind.Boolean),
                "DbQuantifierExpression predicate must have a Boolean result type");

            _input = input;
            _predicate = predicate;
        }

        /// <summary>
        ///     Gets the <see cref="DbExpressionBinding" /> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        ///     Gets the Boolean predicate that should be evaluated for each element in the input set.
        /// </summary>
        public DbExpression Predicate
        {
            get { return _predicate; }
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
