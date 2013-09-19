// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>Represents the type comparison of a single argument against the specified type. This class cannot be inherited. </summary>
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

        /// <summary>Gets the type metadata that the type metadata of the argument should be compared to.</summary>
        /// <returns>The type metadata that the type metadata of the argument should be compared to.</returns>
        public TypeUsage OfType
        {
            get { return _ofType; }
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
