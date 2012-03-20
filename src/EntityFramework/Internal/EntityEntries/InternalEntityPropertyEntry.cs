namespace System.Data.Entity.Internal
{
    using System.Linq;

    /// <summary>
    ///     A concrete implementation of <see cref = "InternalPropertyEntry" /> used for properties of entities.
    /// </summary>
    internal class InternalEntityPropertyEntry : InternalPropertyEntry
    {
        #region Fields and constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref = "InternalEntityPropertyEntry" /> class.
        /// </summary>
        /// <param name = "internalEntityEntry">The internal entry.</param>
        /// <param name = "propertyMetadata">The property info.</param>
        public InternalEntityPropertyEntry(InternalEntityEntry internalEntityEntry, PropertyEntryMetadata propertyMetadata)
            : base(internalEntityEntry, propertyMetadata)
        {
        }

        #endregion

        #region Parent property access

        /// <summary>
        ///     Returns parent property, or null if this is a property on the top-level entity.
        /// </summary>
        public override InternalPropertyEntry ParentPropertyEntry
        {
            get { return null; }
        }

        #endregion

        #region Property access methods for properties of entities

        /// <summary>
        ///     Gets the current values of the parent entity.
        ///     That is, the current values that contains the value for this property.
        /// </summary>
        /// <value>The parent current values.</value>
        public override InternalPropertyValues ParentCurrentValues
        {
            get { return InternalEntityEntry.CurrentValues; }
        }

        /// <summary>
        ///     Gets the original values of the parent entity.
        ///     That is, the original values that contains the value for this property.
        /// </summary>
        /// <value>The parent original values.</value>
        public override InternalPropertyValues ParentOriginalValues
        {
            get { return InternalEntityEntry.OriginalValues; }
        }

        /// <summary>
        ///     Creates a delegate that will get the value of this property.
        /// </summary>
        /// <returns>The delegate.</returns>
        protected override Func<object, object> CreateGetter()
        {
            Func<object, object> getter;
            DbHelpers.GetPropertyGetters(InternalEntityEntry.EntityType).TryGetValue(Name, out getter);
            return getter; // May be null
        }

        /// <summary>
        ///     Creates a delegate that will set the value of this property.
        /// </summary>
        /// <returns>The delegate.</returns>
        protected override Action<object, object> CreateSetter()
        {
            Action<object, object> setter;
            DbHelpers.GetPropertySetters(InternalEntityEntry.EntityType).TryGetValue(Name, out setter);
            return setter; // May be null
        }

        /// <summary>
        ///     Returns true if the property of the entity that this property is ultimately part
        ///     of is set as modified.  Since this is a property of an entity this method returns
        ///     true if the property is modified.
        /// </summary>
        /// <returns>True if the entity property is modified.</returns>
        public override bool EntityPropertyIsModified()
        {
            // TODO: Change this to be more efficient when we are integrated with core EF
            return InternalEntityEntry.ObjectStateEntry.GetModifiedProperties().Contains(Name);
        }

        /// <summary>
        ///     Sets the property of the entity that this property is ultimately part of to modified.
        ///     Since this is a property of an entity this method marks it as modified.
        /// </summary>
        public override void SetEntityPropertyModified()
        {
            InternalEntityEntry.ObjectStateEntry.SetModifiedProperty(Name);
        }


    /// <summary>
    /// Rejects changes to this property.
    /// </summary>
        public override void RejectEntityPropertyChanges()
        {
            InternalEntityEntry.ObjectStateEntry.RejectPropertyChanges(Name);
        }

        /// <summary>
        /// Walks the tree from a property of a complex property back up to the top-level
        /// complex property and then checks whether or not DetectChanges still considers
        /// the complex property to be modified. If it does not, then the complex property
        /// is marked as Unchanged.
        /// </summary>
        public override void UpdateComplexPropertyState()
        {
            if (!InternalEntityEntry.ObjectStateEntry.IsPropertyChanged(Name))
            {
                RejectEntityPropertyChanges();
            }
        }

        #endregion
    }
}