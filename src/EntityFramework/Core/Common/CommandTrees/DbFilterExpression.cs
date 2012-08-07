// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a predicate applied to an input set to produce the set of elements that satisfy the predicate.
    /// </summary>
    public sealed class DbFilterExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _predicate;

        internal DbFilterExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression predicate)
            : base(DbExpressionKind.Filter, resultType)
        {
            Debug.Assert(input != null, "DbFilterExpression input cannot be null");
            Debug.Assert(predicate != null, "DbBFilterExpression predicate cannot be null");
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(predicate.ResultType, PrimitiveTypeKind.Boolean),
                "DbFilterExpression predicate must have a Boolean result type");

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
        ///     Gets the <see cref="DbExpression" /> that specifies the predicate used to filter the input set.
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
