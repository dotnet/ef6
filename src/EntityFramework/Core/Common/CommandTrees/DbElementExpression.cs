// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Represents the conversion of the specified set operand to a singleton.
    ///     If the set is empty the conversion will return null, otherwise the conversion will return one of the elements in the set.
    /// </summary>
    /// <remarks>
    ///     DbElementExpression requires that its argument has a collection result type
    /// </remarks>
    public sealed class DbElementExpression : DbUnaryExpression
    {
        private readonly bool _singlePropertyUnwrapped;

        internal DbElementExpression(TypeUsage resultType, DbExpression argument)
            : base(DbExpressionKind.Element, resultType, argument)
        {
            _singlePropertyUnwrapped = false;
        }

        internal DbElementExpression(TypeUsage resultType, DbExpression argument, bool unwrapSingleProperty)
            : base(DbExpressionKind.Element, resultType, argument)
        {
            _singlePropertyUnwrapped = unwrapSingleProperty;
        }

        /// <summary>
        ///     Is the result type of the element equal to the result type of the single property
        ///     of the element of its operand?
        /// </summary>
        internal bool IsSinglePropertyUnwrapped
        {
            get { return _singlePropertyUnwrapped; }
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
