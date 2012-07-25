// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a (strongly typed) reference to a specific instance within a given entity set.
    /// </summary>
    public sealed class DbRefExpression : DbUnaryExpression
    {
        private readonly EntitySet _entitySet;

        internal DbRefExpression(TypeUsage refResultType, EntitySet entitySet, DbExpression refKeys)
            : base(DbExpressionKind.Ref, refResultType, refKeys)
        {
            Debug.Assert(TypeSemantics.IsReferenceType(refResultType), "DbRefExpression requires a reference result type");

            _entitySet = entitySet;
        }

        /// <summary>
        /// Gets the metadata for the entity set that contains the instance.
        /// </summary>
        public EntitySet EntitySet
        {
            get { return _entitySet; }
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
