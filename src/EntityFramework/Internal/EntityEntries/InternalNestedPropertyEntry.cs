// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     A concrete implementation of <see cref="InternalPropertyEntry" /> used for properties of complex objects.
    /// </summary>
    internal class InternalNestedPropertyEntry : InternalPropertyEntry
    {
        #region Fields and constructors

        private readonly InternalPropertyEntry _parentPropertyEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalNestedPropertyEntry" /> class.
        /// </summary>
        /// <param name="parentPropertyEntry"> The parent property entry. </param>
        /// <param name="propertyMetadata"> The property metadata. </param>
        public InternalNestedPropertyEntry(
            InternalPropertyEntry parentPropertyEntry, PropertyEntryMetadata propertyMetadata)
            : base(parentPropertyEntry.InternalEntityEntry, propertyMetadata)
        {
            Contract.Requires(parentPropertyEntry != null);

            _parentPropertyEntry = parentPropertyEntry;
        }

        #endregion

        #region Parent property access

        /// <summary>
        ///     Returns parent property, or null if this is a property on the top-level entity.
        /// </summary>
        public override InternalPropertyEntry ParentPropertyEntry
        {
            get { return _parentPropertyEntry; }
        }

        #endregion

        #region Property access methods for properties of complex objects

        /// <summary>
        ///     Gets the current values of the parent complex property.
        ///     That is, the current values that contains the value for this property.
        /// </summary>
        /// <value> The parent current values. </value>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override InternalPropertyValues ParentCurrentValues
        {
            get
            {
                var parentCurrentValues = _parentPropertyEntry.ParentCurrentValues;
                var nestedValues = parentCurrentValues == null ? null : parentCurrentValues[_parentPropertyEntry.Name];

                Contract.Assert(
                    nestedValues == null || nestedValues is InternalPropertyValues,
                    "Nested values for nested property should be an InternalPropertyValues object.");

                return (InternalPropertyValues)nestedValues;
            }
        }

        /// <summary>
        ///     Gets the original values of the parent complex property.
        ///     That is, the original values that contains the value for this property.
        /// </summary>
        /// <value> The parent original values. </value>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override InternalPropertyValues ParentOriginalValues
        {
            get
            {
                var parentOriginalValues = _parentPropertyEntry.ParentOriginalValues;
                var nestedValues = parentOriginalValues == null ? null : parentOriginalValues[_parentPropertyEntry.Name];

                Contract.Assert(
                    nestedValues == null || nestedValues is InternalPropertyValues,
                    "Nested values for nested property should be an InternalPropertyValues object.");

                return (InternalPropertyValues)nestedValues;
            }
        }

        /// <summary>
        ///     Creates a delegate that will get the value of this property.
        /// </summary>
        /// <returns> The delegate. </returns>
        protected override Func<object, object> CreateGetter()
        {
            var parentGetter = _parentPropertyEntry.Getter;
            if (parentGetter == null)
            {
                return null;
            }

            Func<object, object> getter;
            if (!DbHelpers.GetPropertyGetters(EntryMetadata.DeclaringType).TryGetValue(Name, out getter))
            {
                return null;
            }

            return o =>
                       {
                           var parent = parentGetter(o);
                           return parent == null ? null : getter(parent);
                       };
        }

        /// <summary>
        ///     Creates a delegate that will set the value of this property.
        /// </summary>
        /// <returns> The delegate. </returns>
        protected override Action<object, object> CreateSetter()
        {
            var parentGetter = _parentPropertyEntry.Getter;
            if (parentGetter == null)
            {
                return null;
            }

            Action<object, object> setter;
            if (!DbHelpers.GetPropertySetters(EntryMetadata.DeclaringType).TryGetValue(Name, out setter))
            {
                return null;
            }

            return (o, v) =>
                       {
                           var parent = parentGetter(o);
                           if (parent == null)
                           {
                               throw Error.DbPropertyValues_CannotSetPropertyOnNullCurrentValue(
                                   Name, ParentPropertyEntry.Name);
                           }
                           setter(parentGetter(o), v);
                       };
        }

        /// <summary>
        ///     Returns true if the property of the entity that this property is ultimately part
        ///     of is set as modified.  Since this is a property of a complex object
        ///     this method returns true if the top-level complex property on the entity is modified.
        /// </summary>
        /// <returns> True if the entity property is modified. </returns>
        public override bool EntityPropertyIsModified()
        {
            return _parentPropertyEntry.EntityPropertyIsModified();
        }

        /// <summary>
        ///     Sets the property of the entity that this property is ultimately part of to modified.
        ///     Since this is a property of a complex object this method marks the top-level
        ///     complex property as modified.
        /// </summary>
        public override void SetEntityPropertyModified()
        {
            _parentPropertyEntry.SetEntityPropertyModified();
        }

        /// <summary>
        ///     Rejects changes to this property.
        ///     Since this is a property of a complex object this method rejects changes to the top-level
        ///     complex property.
        /// </summary>
        public override void RejectEntityPropertyChanges()
        {
            CurrentValue = OriginalValue;
            UpdateComplexPropertyState();
        }

        /// <summary>
        ///     Walks the tree from a property of a complex property back up to the top-level
        ///     complex property and then checks whether or not DetectChanges still considers
        ///     the complex property to be modified. If it does not, then the complex property
        ///     is marked as Unchanged.
        /// </summary>
        public override void UpdateComplexPropertyState()
        {
            _parentPropertyEntry.UpdateComplexPropertyState();
        }

        #endregion
    }
}
