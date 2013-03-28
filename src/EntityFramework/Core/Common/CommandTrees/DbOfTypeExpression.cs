// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents the retrieval of elements of the specified type from the given set argument. This class cannot be inherited.  </summary>
    public sealed class DbOfTypeExpression : DbUnaryExpression
    {
        private readonly TypeUsage _ofType;

        internal DbOfTypeExpression(DbExpressionKind ofTypeKind, TypeUsage collectionResultType, DbExpression argument, TypeUsage type)
            : base(ofTypeKind, collectionResultType, argument)
        {
            Debug.Assert(
                DbExpressionKind.OfType == ofTypeKind ||
                DbExpressionKind.OfTypeOnly == ofTypeKind,
                "ExpressionKind for DbOfTypeExpression must be OfType or OfTypeOnly");

            //
            // Assign the requested element type to the OfType property.
            //
            _ofType = type;
        }

        /// <summary>Gets the metadata of the type of elements that should be retrieved from the set argument.</summary>
        /// <returns>The metadata of the type of elements that should be retrieved from the set argument. </returns>
        public TypeUsage OfType
        {
            get { return _ofType; }
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
