// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Represents the navigation of a relationship. This class cannot be inherited.</summary>
    public sealed class DbRelationshipNavigationExpression : DbExpression
    {
        private readonly RelationshipType _relation;
        private readonly RelationshipEndMember _fromRole;
        private readonly RelationshipEndMember _toRole;
        private readonly DbExpression _from;

        internal DbRelationshipNavigationExpression(
            TypeUsage resultType,
            RelationshipType relType,
            RelationshipEndMember fromEnd,
            RelationshipEndMember toEnd,
            DbExpression navigateFrom)
            : base(DbExpressionKind.RelationshipNavigation, resultType)
        {
            DebugCheck.NotNull(relType);
            DebugCheck.NotNull(fromEnd);
            DebugCheck.NotNull(toEnd);
            DebugCheck.NotNull(navigateFrom);

            _relation = relType;
            _fromRole = fromEnd;
            _toRole = toEnd;
            _from = navigateFrom;
        }

        /// <summary>Gets the metadata for the relationship over which navigation occurs.</summary>
        /// <returns>The metadata for the relationship over which navigation occurs.</returns>
        public RelationshipType Relationship
        {
            get { return _relation; }
        }

        /// <summary>Gets the metadata for the relationship end to navigate from.</summary>
        /// <returns>The metadata for the relationship end to navigate from.</returns>
        public RelationshipEndMember NavigateFrom
        {
            get { return _fromRole; }
        }

        /// <summary>Gets the metadata for the relationship end to navigate to.</summary>
        /// <returns>The metadata for the relationship end to navigate to.</returns>
        public RelationshipEndMember NavigateTo
        {
            get { return _toRole; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies the starting point of the navigation and must be a reference to an entity instance.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies the instance of the source relationship end from which navigation should occur.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression" />
        /// , or its result type is not equal or promotable to the reference type of the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression.NavigateFrom" />
        /// property.
        /// </exception>
        public DbExpression NavigationSource
        {
            get { return _from; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value. </summary>
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
