// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents the construction of a new instance of a given type, including set and record types. This class cannot be inherited. </summary>
    public sealed class DbNewInstanceExpression : DbExpression
    {
        private readonly DbExpressionList _elements;
        private readonly ReadOnlyCollection<DbRelatedEntityRef> _relatedEntityRefs;

        internal DbNewInstanceExpression(TypeUsage type, DbExpressionList args)
            : base(DbExpressionKind.NewInstance, type)
        {
            DebugCheck.NotNull(args);
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
            DebugCheck.NotNull(relationships);

            _relatedEntityRefs = (relationships.Count > 0 ? relationships : null);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides the property/column values or set elements for the new instance.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides the property/column values or set elements for the new instance.
        /// </returns>
        public IList<DbExpression> Arguments
        {
            get { return _elements; }
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

        internal bool HasRelatedEntityReferences
        {
            get { return (_relatedEntityRefs != null); }
        }

        /// <summary>
        /// Gets the related entity references (if any) for an entity constructor.
        /// May be null if no related entities were specified - use the <see cref="HasRelatedEntityReferences" /> property to determine this.
        /// </summary>
        internal ReadOnlyCollection<DbRelatedEntityRef> RelatedEntityReferences
        {
            get { return _relatedEntityRefs; }
        }
    }
}
