// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents the navigation of a (composition or association) relationship given the 'from' role, the 'to' role and an instance of the from role
    /// </summary>
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
            Debug.Assert(relType != null, "DbRelationshipNavigationExpression relationship type cannot be null");
            Debug.Assert(fromEnd != null, "DbRelationshipNavigationExpression 'from' end cannot be null");
            Debug.Assert(toEnd != null, "DbRelationshipNavigationExpression 'to' end cannot be null");
            Debug.Assert(navigateFrom != null, "DbRelationshipNavigationExpression navigation source cannot be null");

            _relation = relType;
            _fromRole = fromEnd;
            _toRole = toEnd;
            _from = navigateFrom;
        }

        /// <summary>
        /// Gets the metadata for the relationship over which navigation occurs
        /// </summary>
        public RelationshipType Relationship
        {
            get { return _relation; }
        }

        /// <summary>
        /// Gets the metadata for the relationship end to navigate from
        /// </summary>
        public RelationshipEndMember NavigateFrom
        {
            get { return _fromRole; }
        }

        /// <summary>
        /// Gets the metadata for the relationship end to navigate to
        /// </summary>
        public RelationshipEndMember NavigateTo
        {
            get { return _toRole; }
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that specifies the instance of the 'from' relationship end from which navigation should occur.
        /// </summary>
        public DbExpression NavigationSource
        {
            get { return _from; }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
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
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
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
