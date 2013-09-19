// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.ComponentModel;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// This is the class is the basis for all perscribed EntityObject classes.
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public abstract class EntityObject : StructuralObject, IEntityWithKey, IEntityWithChangeTracker, IEntityWithRelationships
    {
        #region Privates

        // The following 2 fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private RelationshipManager _relationships;
        private EntityKey _entityKey;

        [NonSerialized]
        private IEntityChangeTracker _entityChangeTracker = _detachedEntityChangeTracker;

        [NonSerialized]
        private static readonly DetachedEntityChangeTracker _detachedEntityChangeTracker = new DetachedEntityChangeTracker();

        /// <summary>
        /// Helper class used when we are not currently attached to a change tracker.
        /// Simplifies the code so we don't always have to check for null before using the change tracker
        /// </summary>
        private class DetachedEntityChangeTracker : IEntityChangeTracker
        {
            void IEntityChangeTracker.EntityMemberChanging(string entityMemberName)
            {
            }

            void IEntityChangeTracker.EntityMemberChanged(string entityMemberName)
            {
            }

            void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexMemberName)
            {
            }

            void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexMemberName)
            {
            }

            EntityState IEntityChangeTracker.EntityState
            {
                get { return EntityState.Detached; }
            }
        }

        private IEntityChangeTracker EntityChangeTracker
        {
            get
            {
                if (_entityChangeTracker == null)
                {
                    _entityChangeTracker = _detachedEntityChangeTracker;
                }
                return _entityChangeTracker;
            }
            set { _entityChangeTracker = value; }
        }

        #endregion

        #region Publics

        /// <summary>Gets the entity state of the object.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.EntityState" /> of this object.
        /// </returns>
        [Browsable(false)]
        [XmlIgnore]
        public EntityState EntityState
        {
            get
            {
                Debug.Assert(
                    EntityChangeTracker != null,
                    "EntityChangeTracker should never return null -- if detached should be set to _detachedEntityChangeTracker");
                Debug.Assert(
                    EntityChangeTracker != _detachedEntityChangeTracker ? EntityChangeTracker.EntityState != EntityState.Detached : true,
                    "Should never get a detached state from an attached change tracker.");

                return EntityChangeTracker.EntityState;
            }
        }

        #region IEntityWithKey

        /// <summary>Gets or sets the key for this object.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.EntityKey" /> for this object.
        /// </returns>
        [Browsable(false)]
        [DataMember]
        public EntityKey EntityKey
        {
            get { return _entityKey; }
            set
            {
                // Report the change to the change tracker
                // If we are not attached to a change tracker, we can do anything we want to the key
                // If we are attached, the change tracker should make sure the new value is valid for the current state
                Debug.Assert(
                    EntityChangeTracker != null,
                    "_entityChangeTracker should never be null -- if detached it should return _detachedEntityChangeTracker");
                EntityChangeTracker.EntityMemberChanging(EntityKeyPropertyName);
                _entityKey = value;
                EntityChangeTracker.EntityMemberChanged(EntityKeyPropertyName);
            }
        }

        #endregion

        #region IEntityWithChangeTracker

        /// <summary>
        /// Used by the ObjectStateManager to attach or detach this EntityObject to the cache.
        /// </summary>
        /// <param name="changeTracker"> Reference to the ObjectStateEntry that contains this entity </param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IEntityWithChangeTracker.SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            // Fail if the change tracker is already set for this EntityObject and it's being set to something different
            // If the original change tracker is associated with a disposed ObjectStateManager, then allow
            // the entity to be attached
            if (changeTracker != null
                && EntityChangeTracker != _detachedEntityChangeTracker
                && !ReferenceEquals(changeTracker, EntityChangeTracker))
            {
                var entry = EntityChangeTracker as EntityEntry;
                if (entry == null
                    || !entry.ObjectStateManager.IsDisposed)
                {
                    throw new InvalidOperationException(Strings.Entity_EntityCantHaveMultipleChangeTrackers);
                }
            }

            EntityChangeTracker = changeTracker;
        }

        #endregion IEntityWithChangeTracker

        #region IEntityWithRelationships

        /// <summary>
        /// Returns the container for the lazily created relationship
        /// navigation property objects, collections and refs.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        RelationshipManager IEntityWithRelationships.RelationshipManager
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = RelationshipManager.Create(this);
                }

                return _relationships;
            }
        }

        #endregion

        #endregion

        #region Protected Change Tracking Methods

        /// <summary>Notifies the change tracker that a property change is pending.</summary>
        /// <param name="property">The name of the changing property.</param>
        /// <exception cref="T:System.ArgumentNullException"> property  is null.</exception>
        protected override sealed void ReportPropertyChanging(
            string property)
        {
            Check.NotEmpty(property, "property");

            Debug.Assert(
                EntityChangeTracker != null,
                "_entityChangeTracker should never be null -- if detached it should return _detachedEntityChangeTracker");

            base.ReportPropertyChanging(property);

            EntityChangeTracker.EntityMemberChanging(property);
        }

        /// <summary>Notifies the change tracker that a property has changed.</summary>
        /// <param name="property">The name of the changed property.</param>
        /// <exception cref="T:System.ArgumentNullException"> property  is null.</exception>
        protected override sealed void ReportPropertyChanged(
            string property)
        {
            Check.NotEmpty(property, "property");

            Debug.Assert(
                EntityChangeTracker != null,
                "EntityChangeTracker should never return null -- if detached it should be return _detachedEntityChangeTracker");
            EntityChangeTracker.EntityMemberChanged(property);

            base.ReportPropertyChanged(property);
        }

        #endregion

        #region Internal ComplexObject Change Tracking Methods and Properties

        internal override sealed bool IsChangeTracked
        {
            get { return EntityState != EntityState.Detached; }
        }

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity
        /// whenever a change is about to be made to a property of the
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that contains the ComplexObject that is calling this method. </param>
        /// <param name="complexObject"> The instance of the ComplexObject on which the property is changing. </param>
        /// <param name="complexMemberName"> The name of the changing property on complexObject. </param>
        internal override sealed void ReportComplexPropertyChanging(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexMemberName);

            EntityChangeTracker.EntityComplexMemberChanging(entityMemberName, complexObject, complexMemberName);
        }

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity
        /// whenever a change has been made to a property of the
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that contains the ComplexObject that is calling this method. </param>
        /// <param name="complexObject"> The instance of the ComplexObject on which the property is changing. </param>
        /// <param name="complexMemberName"> The name of the changing property on complexObject. </param>
        internal override sealed void ReportComplexPropertyChanged(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexMemberName);

            EntityChangeTracker.EntityComplexMemberChanged(entityMemberName, complexObject, complexMemberName);
        }

        #endregion
    }
}
