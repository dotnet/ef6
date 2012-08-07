// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     Represents the type comparison of a single argument against the specified type.
    /// </summary>
    public sealed class DbIsOfExpression : DbUnaryExpression
    {
        private readonly TypeUsage _ofType;

        internal DbIsOfExpression(DbExpressionKind isOfKind, TypeUsage booleanResultType, DbExpression argument, TypeUsage isOfType)
            : base(isOfKind, booleanResultType, argument)
        {
            Debug.Assert(
                DbExpressionKind.IsOf == ExpressionKind || DbExpressionKind.IsOfOnly == ExpressionKind,
                string.Format(
                    CultureInfo.InvariantCulture, "Invalid DbExpressionKind used in DbIsOfExpression: {0}",
                    Enum.GetName(typeof(DbExpressionKind), ExpressionKind)));
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbIsOfExpression requires a Boolean result type");

            _ofType = isOfType;
        }

        /// <summary>
        ///     Gets the type metadata that the type metadata of the argument should be compared to.
        /// </summary>
        public TypeUsage OfType
        {
            get { return _ofType; }
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
