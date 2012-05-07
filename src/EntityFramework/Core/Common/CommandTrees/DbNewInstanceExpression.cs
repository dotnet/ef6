namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents the construction of a new instance of a given type, including set and record types.
    /// </summary>
    public sealed class DbNewInstanceExpression : DbExpression
    {
        private readonly DbExpressionList _elements;
        private readonly ReadOnlyCollection<DbRelatedEntityRef> _relatedEntityRefs;

        internal DbNewInstanceExpression(TypeUsage type, DbExpressionList args)
            : base(DbExpressionKind.NewInstance, type)
        {
            Debug.Assert(args != null, "DbNewInstanceExpression arguments cannot be null");
            Debug.Assert(
                args.Count > 0 || TypeSemantics.IsCollectionType(type),
                "DbNewInstanceExpression requires at least one argument when not creating an empty collection");

            _elements = args;
        }

        internal DbNewInstanceExpression(
            TypeUsage resultType, DbExpressionList attributeValues, ReadOnlyCollection<DbRelatedEntityRef> relationships)
            : this(resultType, attributeValues)
        {
            Debug.Assert(
                TypeSemantics.IsEntityType(resultType), "An entity type is required to create a NewEntityWithRelationships expression");
            Debug.Assert(relationships != null, "Related entity ref collection cannot be null");

            _relatedEntityRefs = (relationships.Count > 0 ? relationships : null);
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> list that provides the property/column values or set elements for the new instance.
        /// </summary>
        public IList<DbExpression> Arguments
        {
            get { return _elements; }
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

        internal bool HasRelatedEntityReferences
        {
            get { return (_relatedEntityRefs != null); }
        }

        /// <summary>
        /// Gets the related entity references (if any) for an entity constructor. 
        /// May be null if no related entities were specified - use the <see cref="HasRelatedEntityReferences"/> property to determine this.
        /// </summary>
        internal ReadOnlyCollection<DbRelatedEntityRef> RelatedEntityReferences
        {
            get { return _relatedEntityRefs; }
        }
    }
}