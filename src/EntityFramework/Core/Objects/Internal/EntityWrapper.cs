// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Implementation of the IEntityWrapper interface that is used for non-null entities that do not implement
    /// all of our standard interfaces: IEntityWithKey, IEntityWithRelationships, and IEntityWithChangeTracker, and
    /// are not proxies.
    /// Different strategies for dealing with these entities are defined by strategy objects that are set into the
    /// wrapper at constructionn time.
    /// </summary>
    internal abstract class EntityWrapper<TEntity> : BaseEntityWrapper<TEntity>
    {
        private readonly TEntity _entity;
        private readonly IPropertyAccessorStrategy _propertyStrategy;
        private readonly IChangeTrackingStrategy _changeTrackingStrategy;
        private readonly IEntityKeyStrategy _keyStrategy;

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="relationshipManager">The RelationshipManager associated with the entity</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        protected EntityWrapper(
            TEntity entity, RelationshipManager relationshipManager,
            Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy,
            Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, relationshipManager)
        {
            if (relationshipManager == null)
            {
                throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
            }
            _entity = entity;
            _propertyStrategy = propertyStrategy(entity);
            _changeTrackingStrategy = changeTrackingStrategy(entity);
            _keyStrategy = keyStrategy(entity);
            Debug.Assert(_changeTrackingStrategy != null, "Change tracking strategy cannot be null.");
            Debug.Assert(_keyStrategy != null, "Key strategy cannot be null.");
        }

        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="relationshipManager">The RelationshipManager associated with the entity</param>
        /// <param name="key">The entity's key</param>
        /// <param name="entitySet">The entity set, or null if none is known</param>
        /// <param name="context">The context to which the entity should be attached</param>
        /// <param name="mergeOption">NoTracking for non-tracked entities, AppendOnly otherwise</param>
        /// <param name="identityType">The type of the entity ignoring any possible proxy type</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        protected EntityWrapper(
            TEntity entity, RelationshipManager relationshipManager, EntityKey key, EntitySet set, ObjectContext context,
            MergeOption mergeOption, Type identityType,
            Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy,
            Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, relationshipManager, set, context, mergeOption, identityType)
        {
            if (relationshipManager == null)
            {
                throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
            }
            _entity = entity;
            _propertyStrategy = propertyStrategy(entity);
            _changeTrackingStrategy = changeTrackingStrategy(entity);
            _keyStrategy = keyStrategy(entity);
            Debug.Assert(_changeTrackingStrategy != null, "Change tracking strategy cannot be null.");
            Debug.Assert(_keyStrategy != null, "Key strategy cannot be null.");
            _keyStrategy.SetEntityKey(key);
        }

        // See IEntityWrapper documentation
        public override void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            _changeTrackingStrategy.SetChangeTracker(changeTracker);
        }

        // See IEntityWrapper documentation
        public override void TakeSnapshot(EntityEntry entry)
        {
            _changeTrackingStrategy.TakeSnapshot(entry);
        }

        // See IEntityWrapper documentation
        public override EntityKey EntityKey
        {
            // If no strategy is set, then the key maintained by the wrapper is used,
            // otherwise the request is passed to the strategy.
            get { return _keyStrategy.GetEntityKey(); }
            set { _keyStrategy.SetEntityKey(value); }
        }

        public override EntityKey GetEntityKeyFromEntity()
        {
            return _keyStrategy.GetEntityKeyFromEntity();
        }

        public override void CollectionAdd(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                _propertyStrategy.CollectionAdd(relatedEnd, value);
            }
        }

        public override bool CollectionRemove(RelatedEnd relatedEnd, object value)
        {
            return _propertyStrategy != null ? _propertyStrategy.CollectionRemove(relatedEnd, value) : false;
        }

        // See IEntityWrapper documentation
        public override void EnsureCollectionNotNull(RelatedEnd relatedEnd)
        {
            if (_propertyStrategy != null)
            {
                var collection = _propertyStrategy.GetNavigationPropertyValue(relatedEnd);
                if (collection == null)
                {
                    collection = _propertyStrategy.CollectionCreate(relatedEnd);
                    _propertyStrategy.SetNavigationPropertyValue(relatedEnd, collection);
                }
            }
        }

        // See IEntityWrapper documentation
        public override object GetNavigationPropertyValue(RelatedEnd relatedEnd)
        {
            return _propertyStrategy != null ? _propertyStrategy.GetNavigationPropertyValue(relatedEnd) : null;
        }

        // See IEntityWrapper documentation
        public override void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                _propertyStrategy.SetNavigationPropertyValue(relatedEnd, value);
            }
        }

        // See IEntityWrapper documentation
        public override void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                var currentValue = _propertyStrategy.GetNavigationPropertyValue(relatedEnd);

                if (ReferenceEquals(currentValue, value))
                {
                    _propertyStrategy.SetNavigationPropertyValue(relatedEnd, null);
                }
            }
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
            _changeTrackingStrategy.SetCurrentValue(entry, member, ordinal, target, value);
        }

        // See IEntityWrapper documentation
        public override void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            _changeTrackingStrategy.UpdateCurrentValueRecord(value, entry);
        }
    }
}
