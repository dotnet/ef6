namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    // Detached - nothing

    // Added - _entity & _currentValues only for shadowState

    // Unchanged - _entity & _currentValues only for shadowState
    // Unchanged -> Deleted - _entity & _currentValues only for shadowState

    // Modified - _currentValues & _modifiedFields + _originalValues only on change
    // Modified -> Deleted - _currentValues & _modifiedFields + _originalValues only on change

    /// <summary>
    /// Represets either a entity, entity stub or relationship
    /// </summary>
    [ContractClass(typeof(ObjectStateEntryContracts))]
    public abstract class ObjectStateEntry : IEntityStateEntry, IEntityChangeTracker
    {
        #region common entry fields

        internal ObjectStateManager _cache;
        internal EntitySetBase _entitySet;
        internal EntityState _state;

        #endregion

        #region Constructor

        // ObjectStateEntry will not be detached and creation will be handled from ObjectStateManager
        internal ObjectStateEntry(ObjectStateManager cache, EntitySet entitySet, EntityState state)
        {
            Contract.Requires(cache != null);

            _cache = cache;
            _entitySet = entitySet;
            _state = state;
        }

        #endregion // Constructor

        #region Public members

        /// <summary>
        /// ObjectStateManager property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> ObjectStateManager </returns>
        public ObjectStateManager ObjectStateManager
        {
            get
            {
                ValidateState();
                return _cache;
            }
        }

        /// <summary> Extent property of ObjectStateEntry. </summary>
        /// <param></param>
        /// <returns> Extent </returns>
        public EntitySetBase EntitySet
        {
            get
            {
                ValidateState();
                return _entitySet;
            }
        }

        /// <summary>
        /// State property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> DataRowState </returns>
        public EntityState State
        {
            get { return _state; }
            internal set { _state = value; }
        }

        /// <summary>
        /// Entity property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> The entity encapsulated by this entry. </returns>
        public abstract object Entity { get; }

        /// <summary>
        /// The EntityKey associated with the ObjectStateEntry
        /// </summary>
        public abstract EntityKey EntityKey { get; internal set; }

        /// <summary>
        /// Determines if this ObjectStateEntry represents a relationship
        /// </summary>
        public abstract bool IsRelationship { get; }

        /// <summary>
        /// Gets bit array indicating which properties are modified.
        /// </summary>
        internal abstract BitArray ModifiedProperties { get; }

        BitArray IEntityStateEntry.ModifiedProperties
        {
            get { return ModifiedProperties; }
        }

        /// <summary>
        /// Original values
        /// </summary>
        /// <param></param>
        /// <returns> DbDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public abstract DbDataRecord OriginalValues { get; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract OriginalValueRecord GetUpdatableOriginalValues();

        /// <summary>
        /// Current values
        /// </summary>
        /// <param></param>
        /// <returns> DbUpdatableDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public abstract CurrentValueRecord CurrentValues { get; }

        /// <summary>
        /// API to accept the current values as original values and  mark the entity as Unchanged.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public abstract void AcceptChanges();

        /// <summary>
        /// API to mark the entity deleted. if entity is in added state, it will be detached
        /// </summary>
        /// <param></param>
        /// <returns> </returns>
        public abstract void Delete();

        /// <summary>
        /// API to return properties that are marked modified
        /// </summary>
        /// <param> </param>
        /// <returns> IEnumerable of modified properties names, names are in term of c-space </returns>
        public abstract IEnumerable<string> GetModifiedProperties();

        /// <summary>
        /// set the state to Modified.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        ///
        public abstract void SetModified();

        /// <summary>
        /// Marks specified property as modified.
        /// </summary>
        /// <param name="propertyName">This API recognizes the names in terms of OSpace</param>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        ///
        public abstract void SetModifiedProperty(string propertyName);

        /// <summary>
        /// Rejects any changes made to the property with the given name since the property was last loaded,
        /// attached, saved, or changes were accepted. The orginal value of the property is stored and the
        /// property will no longer be marked as modified. 
        /// </summary>
        /// <remarks>
        /// If the result is that no properties of the entity are marked as modified, then the entity will
        /// be marked as Unchanged.
        /// Changes to properties can only rejected for entities that are in the Modified or Unchanged state.
        /// Calling this method for entities in other states (Added, Deleted, or Detached) will result in
        /// an exception being thrown.
        /// Rejecting changes to properties of an Unchanged entity or unchanged properties of a Modifed
        /// is a no-op.
        /// </remarks>
        /// <param name="propertyName">The name of the property to change.</param>
        public abstract void RejectPropertyChanges(string propertyName);

        /// <summary>
        /// Uses DetectChanges to determine whether or not the current value of the property with the given
        /// name is different from its original value. Note that this may be different from the property being
        /// marked as modified since a property which has not changed can still be marked as modified.
        /// </summary>
        /// <remarks>
        /// For complex properties, a new instance of the complex object which has all the same property
        /// values as the original instance is not considered to be different by this method.
        /// </remarks>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property has changed; false otherwise.</returns>
        public abstract bool IsPropertyChanged(string propertyName);

        /// <summary>
        /// Returns the RelationshipManager for the entity represented by this ObjectStateEntry.
        /// Note that a RelationshipManager objects can only be returned if this entry represents a
        /// full entity.  Key-only entries (stubs) and entries representing relationships do not
        /// have associated RelationshipManagers.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry is a stub or represents a relationship</exception>
        public abstract RelationshipManager RelationshipManager { get; }

        /// <summary>
        /// Changes state of the entry to the specified <paramref name="state"/>
        /// </summary>
        /// <param name="state">The requested state</param>
        public abstract void ChangeState(EntityState state);

        /// <summary>
        /// Apply modified properties to the original object.
        /// </summary>
        /// <param name="current">object with modified properties</param>
        public abstract void ApplyCurrentValues(object currentEntity);

        /// <summary>
        /// Apply original values to the entity.
        /// </summary>
        /// <param name="original">The object with original values</param>
        public abstract void ApplyOriginalValues(object originalEntity);

        #endregion // Public members

        #region IEntityStateEntry

        IEntityStateManager IEntityStateEntry.StateManager
        {
            get { return ObjectStateManager; }
        }

        // must explicitly implement this because interface is internal & so is the property on the
        // class itself -- apparently the compiler won't let anything marked as internal be part of
        // an interface (even if the interface is also internal)
        bool IEntityStateEntry.IsKeyEntry
        {
            get { return IsKeyEntry; }
        }

        #endregion // IEntityStateEntry

        #region Public IEntityChangeTracker

        /// <summary>
        /// Used to report that a scalar entity property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that is changing</param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IEntityChangeTracker.EntityMemberChanging(string entityMemberName)
        {
            EntityMemberChanging(entityMemberName);
        }

        /// <summary>
        /// Used to report that a scalar entity property has been changed
        /// The property value that was cached during EntityMemberChanging is now
        /// added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that has changing</param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IEntityChangeTracker.EntityMemberChanged(string entityMemberName)
        {
            EntityMemberChanged(entityMemberName);
        }

        /// <summary>
        /// Used to report that a complex property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that is changing</param>
        /// <param name="complexObject">The complex object that contains the property that is changing</param>
        /// <param name="complexObjectMemberName">The name of the property that is changing on complexObject</param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            EntityComplexMemberChanging(entityMemberName, complexObject, complexObjectMemberName);
        }

        /// <summary>
        /// Used to report that a complex property has been changed
        /// The property value that was cached during EntityMemberChanging is now added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that has changed</param>
        /// <param name="complexObject">The complex object that contains the property that changed</param>
        /// <param name="complexObjectMemberName">The name of the property that changed on complexObject</param>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            EntityComplexMemberChanged(entityMemberName, complexObject, complexObjectMemberName);
        }

        /// <summary>
        /// Returns the EntityState from the ObjectStateEntry
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        EntityState IEntityChangeTracker.EntityState
        {
            get { return State; }
        }

        #endregion // IEntityChangeTracker

        #region Internal members

        internal abstract bool IsKeyEntry { get; }

        internal abstract int GetFieldCount(StateManagerTypeMetadata metadata);

        internal abstract Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata);

        internal abstract string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata);

        internal abstract int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata);

        internal abstract void RevertDelete();

        internal abstract void SetModifiedAll();

        internal abstract void EntityMemberChanging(string entityMemberName);
        internal abstract void EntityMemberChanged(string entityMemberName);
        internal abstract void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName);
        internal abstract void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName);

        /// <summary>
        /// Reuse or create a new (Entity)DataRecordInfo.
        /// </summary>
        internal abstract DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject);

        internal virtual void Reset()
        {
            _cache = null;
            _entitySet = null;
            _state = EntityState.Detached;
        }

        internal void ValidateState()
        {
            if (_state == EntityState.Detached)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_InvalidState);
            }
            Debug.Assert(null != _cache, "null ObjectStateManager");
            Debug.Assert(null != _entitySet, "null EntitySetBase");
        }

        #endregion // Internal members

        #region Base Member Contracts

        [ContractClassFor(typeof(ObjectStateEntry))]
        private abstract class ObjectStateEntryContracts : ObjectStateEntry
        {
            private ObjectStateEntryContracts()
                : base(new ObjectStateManager(), null, EntityState.Unchanged)
            {
                throw new NotImplementedException();
            }

            public override void ApplyCurrentValues(object currentEntity)
            {
                Contract.Requires<ArgumentNullException>(currentEntity != null);

                throw new NotImplementedException();
            }

            public override void ApplyOriginalValues(object originalEntity)
            {
                Contract.Requires(originalEntity != null);

                throw new NotImplementedException();
            }

            internal override void EntityComplexMemberChanging(
                string entityMemberName, object complexObject, string complexObjectMemberName)
            {
                Contract.Requires(complexObjectMemberName != null);
                Contract.Requires(complexObject != null);

                throw new NotImplementedException();
            }

            internal override void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
            {
                Contract.Requires(complexObjectMemberName != null);
                Contract.Requires(complexObject != null);

                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
