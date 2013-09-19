// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    /// An extension of the EntityWrapper class for entities that implement IEntityWithRelationships.
    /// Using this class causes creation of the RelationshipManager to be defered to the entity object.
    /// </summary>
    /// <typeparam name="TEntity"> The type of entity wrapped </typeparam>
    internal sealed class EntityWrapperWithRelationships<TEntity> : EntityWrapper<TEntity>
        where TEntity : class, IEntityWithRelationships
    {
        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity"> The entity to wrap </param>
        /// <param name="key"> The entity's key </param>
        /// <param name="entitySet"> The entity set, or null if none is known </param>
        /// <param name="context"> The context to which the entity should be attached </param>
        /// <param name="mergeOption"> NoTracking for non-tracked entities, AppendOnly otherwise </param>
        /// <param name="identityType"> The type of the entity ignoring any possible proxy type </param>
        /// <param name="propertyStrategy"> A delegate to create the property accesor strategy object </param>
        /// <param name="changeTrackingStrategy"> A delegate to create the change tracking strategy object </param>
        /// <param name="keyStrategy"> A delegate to create the entity key strategy object </param>
        internal EntityWrapperWithRelationships(
            TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType,
            Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy,
            Func<object, IEntityKeyStrategy> keyStrategy, bool overridesEquals)
            : base(entity, entity.RelationshipManager, key, entitySet, context, mergeOption, identityType,
                propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
        {
        }

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity"> The entity to wrap </param>
        /// <param name="propertyStrategy"> A delegate to create the property accesor strategy object </param>
        /// <param name="changeTrackingStrategy"> A delegate to create the change tracking strategy object </param>
        /// <param name="keyStrategy"> A delegate to create the entity key strategy object </param>
        internal EntityWrapperWithRelationships(
            TEntity entity, Func<object, IPropertyAccessorStrategy> propertyStrategy,
            Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy,
            bool overridesEquals)
            : base(entity, entity.RelationshipManager, propertyStrategy, changeTrackingStrategy, keyStrategy, overridesEquals)
        {
        }

        public override bool OwnsRelationshipManager
        {
            get { return true; }
        }

        public override void TakeSnapshotOfRelationships(EntityEntry entry)
        {
        }

        // See IEntityWrapper documentation
        public override bool RequiresRelationshipChangeTracking
        {
            get { return false; }
        }
    }
}
