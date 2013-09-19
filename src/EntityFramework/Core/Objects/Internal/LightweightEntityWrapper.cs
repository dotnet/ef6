// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Diagnostics;

    /// <summary>
    /// Implementation of IEntityWrapper for any entity that implements IEntityWithChangeTracker, IEntityWithRelationships,
    /// and IEntityWithKey and is not a proxy.  This is a lightweight wrapper that delegates functionality to those interfaces.
    /// This improves the speed and memory utilization for the standard code-gen cases in materialization.
    /// </summary>
    /// <typeparam name="TEntity"> The type of entity wrapped </typeparam>
    internal sealed class LightweightEntityWrapper<TEntity> : BaseEntityWrapper<TEntity>
        where TEntity : class, IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
    {
        private readonly TEntity _entity;

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity"> The entity to wrap </param>
        internal LightweightEntityWrapper(TEntity entity, bool overridesEquals)
            : base(entity, entity.RelationshipManager, overridesEquals)
        {
            Debug.Assert(
                entity is IEntityWithChangeTracker,
                "LightweightEntityWrapper only works with entities that implement IEntityWithChangeTracker");
            Debug.Assert(
                entity is IEntityWithRelationships,
                "LightweightEntityWrapper only works with entities that implement IEntityWithRelationships");
            Debug.Assert(entity is IEntityWithKey, "LightweightEntityWrapper only works with entities that implement IEntityWithKey");
            Debug.Assert(
                !EntityProxyFactory.IsProxyType(entity.GetType()), "LightweightEntityWrapper only works with entities that are not proxies");
            
            _entity = entity;
        }

        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity"> The entity to wrap </param>
        /// <param name="key"> The key for the entity </param>
        /// <param name="entitySet"> The entity set, or null if none is known </param>
        /// <param name="context"> The context to which the entity should be attached </param>
        /// <param name="mergeOption"> NoTracking for non-tracked entities, AppendOnly otherwise </param>
        /// <param name="identityType"> The type of the entity ignoring any possible proxy type </param>
        internal LightweightEntityWrapper(
            TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType, bool overridesEquals)
            : base(entity, entity.RelationshipManager, entitySet, context, mergeOption, identityType, overridesEquals)
        {
            Debug.Assert(
                entity is IEntityWithChangeTracker,
                "LightweightEntityWrapper only works with entities that implement IEntityWithChangeTracker");
            Debug.Assert(
                entity is IEntityWithRelationships,
                "LightweightEntityWrapper only works with entities that implement IEntityWithRelationships");
            Debug.Assert(entity is IEntityWithKey, "LightweightEntityWrapper only works with entities that implement IEntityWithKey");
            Debug.Assert(
                !EntityProxyFactory.IsProxyType(entity.GetType()), "LightweightEntityWrapper only works with entities that are not proxies");
            _entity = entity;
            _entity.EntityKey = key;
        }

        // See IEntityWrapper documentation
        public override void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            _entity.SetChangeTracker(changeTracker);
        }

        // See IEntityWrapper documentation
        public override void TakeSnapshot(EntityEntry entry)
        {
        }

        // See IEntityWrapper documentation
        public override void TakeSnapshotOfRelationships(EntityEntry entry)
        {
        }

        // See IEntityWrapper documentation
        public override EntityKey EntityKey
        {
            get { return _entity.EntityKey; }
            set { _entity.EntityKey = value; }
        }

        public override bool OwnsRelationshipManager
        {
            get { return true; }
        }

        // See IEntityWrapper documentation
        public override EntityKey GetEntityKeyFromEntity()
        {
            return _entity.EntityKey;
        }

        // See IEntityWrapper documentation
        public override void CollectionAdd(RelatedEnd relatedEnd, object value)
        {
        }

        // See IEntityWrapper documentation
        public override bool CollectionRemove(RelatedEnd relatedEnd, object value)
        {
            return false;
        }

        // See IEntityWrapper documentation
        public override void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
        }

        // See IEntityWrapper documentation
        public override void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
        }

        // See IEntityWrapper documentation
        public override void EnsureCollectionNotNull(RelatedEnd relatedEnd)
        {
        }

        // See IEntityWrapper documentation
        public override object GetNavigationPropertyValue(RelatedEnd relatedEnd)
        {
            return null;
        }

        // See IEntityWrapper documentation
        public override object Entity
        {
            get { return _entity; }
        }

        // See IEntityWrapper<TEntity> documentation
        public override TEntity TypedEntity
        {
            get { return _entity; }
        }

        // See IEntityWrapper documentation
        public override void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
        {
            member.SetValue(target, value);
        }

        // See IEntityWrapper documentation
        public override void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            // No extra work to do because we know that the entity is not a proxy and has a change tracker
            entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
        }

        // See IEntityWrapper documentation
        public override bool RequiresRelationshipChangeTracking
        {
            get { return false; }
        }
    }
}
