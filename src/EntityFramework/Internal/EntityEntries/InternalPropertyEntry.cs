// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     The internal class used to implement <see cref="System.Data.Entity.Infrastructure.DbPropertyEntry" /> and
    ///     <see cref="System.Data.Entity.Infrastructure.DbPropertyEntry{TEntity, TProperty}" />.
    ///     This internal class contains all the common implementation between the generic and non-generic
    ///     entry classes and also allows for a clean internal factoring without compromising the public API.
    /// </summary>
    internal abstract class InternalPropertyEntry : InternalMemberEntry
    {
        #region Fields and constructors

        private bool _getterIsCached;
        private Func<object, object> _getter;
        private bool _setterIsCached;
        private Action<object, object> _setter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalPropertyEntry" /> class.
        /// </summary>
        /// <param name="internalEntityEntry"> The internal entry. </param>
        /// <param name="propertyMetadata"> The property info. </param>
        protected InternalPropertyEntry(InternalEntityEntry internalEntityEntry, PropertyEntryMetadata propertyMetadata)
            : base(internalEntityEntry, propertyMetadata)
        {
            DebugCheck.NotNull(propertyMetadata);
        }

        #endregion

        #region Parent property access

        /// <summary>
        ///     Returns parent property, or null if this is a property on the top-level entity.
        /// </summary>
        public abstract InternalPropertyEntry ParentPropertyEntry { get; }

        #endregion

        #region Abstract property access methods

        /// <summary>
        ///     Gets the current values of the parent entity or complex property.
        ///     That is, the current values that contains the value for this property.
        /// </summary>
        /// <value> The parent current values. </value>
        public abstract InternalPropertyValues ParentCurrentValues { get; }

        /// <summary>
        ///     Gets the original values of the parent entity or complex property.
        ///     That is, the original values that contains the value for this property.
        /// </summary>
        /// <value> The parent original values. </value>
        public abstract InternalPropertyValues ParentOriginalValues { get; }

        /// <summary>
        ///     Creates a delegate that will get the value of this property.
        /// </summary>
        /// <returns> The delegate. </returns>
        protected abstract Func<object, object> CreateGetter();

        /// <summary>
        ///     Creates a delegate that will set the value of this property.
        /// </summary>
        /// <returns> The delegate. </returns>
        protected abstract Action<object, object> CreateSetter();

        /// <summary>
        ///     Returns true if the property of the entity that this property is ultimately part
        ///     of is set as modified.  If this is a property of an entity, then this method returns
        ///     true if the property is modified.  If this is a property of a complex object, then
        ///     this method returns true if the top-level complex property on the entity is modified.
        /// </summary>
        /// <returns> True if the entity property is modified. </returns>
        public abstract bool EntityPropertyIsModified();

        /// <summary>
        ///     Sets the property of the entity that this property is ultimately part of to modified.
        ///     If this is a property of an entity, then this method marks it as modified.
        ///     If this is a property of a complex object, then this method marks the top-level
        ///     complex property as modified.
        /// </summary>
        public abstract void SetEntityPropertyModified();

        /// <summary>
        ///     Rejects changes to this property.
        ///     If this is a property of a complex object, then this method rejects changes to the top-level
        ///     complex property.
        /// </summary>
        public abstract void RejectEntityPropertyChanges();

        /// <summary>
        ///     Walks the tree from a property of a complex property back up to the top-level
        ///     complex property and then checks whether or not DetectChanges still considers
        ///     the complex property to be modified. If it does not, then the complex property
        ///     is marked as Unchanged.
        /// </summary>
        public abstract void UpdateComplexPropertyState();

        #endregion

        #region Current and Original values

        /// <summary>
        ///     A delegate that reads the value of this property.
        ///     May be null if there is no way to set the value due to missing accessors on the type.
        /// </summary>
        public Func<object, object> Getter
        {
            get
            {
                if (!_getterIsCached)
                {
                    _getter = CreateGetter();
                    _getterIsCached = true;
                }
                return _getter;
            }
        }

        /// <summary>
        ///     A delegate that sets the value of this property.
        ///     May be null if there is no way to set the value due to missing accessors on the type.
        /// </summary>
        public Action<object, object> Setter
        {
            get
            {
                if (!_setterIsCached)
                {
                    _setter = CreateSetter();
                    _setterIsCached = true;
                }
                return _setter;
            }
        }

        /// <summary>
        ///     Gets or sets the original value.
        ///     Note that complex properties are returned as objects, not property values.
        /// </summary>
        public virtual object OriginalValue
        {
            get
            {
                ValidateNotDetachedAndInModel("OriginalValue");

                var parentOriginalValues = ParentOriginalValues;
                var value = parentOriginalValues == null ? null : parentOriginalValues[Name];

                var asValues = value as InternalPropertyValues;
                if (asValues != null)
                {
                    value = asValues.ToObject();
                }

                return value;
            }
            set
            {
                ValidateNotDetachedAndInModel("OriginalValue");
                CheckNotSettingComplexPropertyToNull(value);

                var parentOriginalValues = ParentOriginalValues;
                if (parentOriginalValues == null)
                {
                    Debug.Assert(ParentPropertyEntry != null, "Should only have null parent original values for nested properties.");

                    throw Error.DbPropertyValues_CannotSetPropertyOnNullOriginalValue(Name, ParentPropertyEntry.Name);
                }

                SetPropertyValueUsingValues(parentOriginalValues, value);
            }
        }

        /// <summary>
        ///     Gets or sets the current value.
        ///     Note that complex properties are returned as objects, not property values.
        ///     Also, for complex properties, the object returned is the actual complex object from the entity
        ///     and setting the complex object causes the actual object passed to be set onto the entity.
        /// </summary>
        /// <value> The current value. </value>
        public override object CurrentValue
        {
            get
            {
                // Attempt to get the property value directly from the CLR type
                if (Getter != null)
                {
                    return Getter(InternalEntityEntry.Entity);
                }

                // If that didn't work, then attempt to get the property from current values record
                if (!InternalEntityEntry.IsDetached
                    && EntryMetadata.IsMapped)
                {
                    var parentCurrentValues = ParentCurrentValues;
                    var value = parentCurrentValues == null ? null : parentCurrentValues[Name];

                    // If prop is complex, then create the complex object from the nested values
                    var asValues = value as InternalPropertyValues;
                    if (asValues != null)
                    {
                        value = asValues.ToObject();
                    }
                    return value;
                }

                // If prop isn't in the CLR type and current values record does not exist, then throw
                throw Error.DbPropertyEntry_CannotGetCurrentValue(Name, base.EntryMetadata.DeclaringType.Name);
            }
            set
            {
                CheckNotSettingComplexPropertyToNull(value);

                // If the entity is not tracked, or is Deleted, then just set the property value directly onto the CLR type.
                if (!EntryMetadata.IsMapped
                    || InternalEntityEntry.IsDetached
                    || InternalEntityEntry.State == EntityState.Deleted)
                {
                    if (!SetCurrentValueOnClrObject(value))
                    {
                        // If prop isn't in the CLR type and current values record does not exist, then throw
                        throw Error.DbPropertyEntry_CannotSetCurrentValue(Name, base.EntryMetadata.DeclaringType.Name);
                    }
                }
                else
                {
                    // The entity is tracked so attempt to set the property value using the underlying current values record
                    var parentCurrentValues = ParentCurrentValues;
                    if (parentCurrentValues == null)
                    {
                        Debug.Assert(ParentPropertyEntry != null, "Should only have null parent original values for nested properties.");

                        throw Error.DbPropertyValues_CannotSetPropertyOnNullCurrentValue(Name, ParentPropertyEntry.Name);
                    }

                    SetPropertyValueUsingValues(parentCurrentValues, value);

                    if (EntryMetadata.IsComplex)
                    {
                        // If the property was a complex property, then also set the complex object directly
                        // onto the CLR object if possible.
                        SetCurrentValueOnClrObject(value);
                    }
                }
            }
        }

        /// <summary>
        ///     Throws if the user attempts to set a complex property to null.
        /// </summary>
        /// <param name="value"> The value. </param>
        private void CheckNotSettingComplexPropertyToNull(object value)
        {
            if (value == null
                && EntryMetadata.IsComplex)
            {
                throw Error.DbPropertyValues_ComplexObjectCannotBeNull(Name, base.EntryMetadata.DeclaringType.Name);
            }
        }

        /// <summary>
        ///     Sets the given value directly onto the underlying entity object.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> True if the property had a setter that we could attempt to call; false if no setter was available. </returns>
        private bool SetCurrentValueOnClrObject(object value)
        {
            if (Setter == null)
            {
                return false;
            }

            if (Getter == null
                || !DbHelpers.KeyValuesEqual(value, Getter(InternalEntityEntry.Entity)))
            {
                Setter(InternalEntityEntry.Entity, value);
                if (EntryMetadata.IsMapped
                    &&
                    (InternalEntityEntry.State == EntityState.Modified
                     || InternalEntityEntry.State == EntityState.Unchanged))
                {
                    IsModified = true;
                }
            }
            return true;
        }

        /// <summary>
        ///     Sets the property value, potentially by setting individual nested values for a complex
        ///     property.
        /// </summary>
        /// <param name="value"> The value. </param>
        private void SetPropertyValueUsingValues(InternalPropertyValues internalValues, object value)
        {
            Debug.Assert(internalValues != null, "Expected to throw before calling this method.");

            var nestedValues = internalValues[Name] as InternalPropertyValues;
            if (nestedValues != null)
            {
                Debug.Assert(value != null, "Should already have thrown if complex object is null.");

                // Setting values from a derived type is allowed, but setting values from a base type is not.
                if (!nestedValues.ObjectType.IsAssignableFrom(value.GetType()))
                {
                    throw Error.DbPropertyValues_AttemptToSetValuesFromWrongObject(
                        value.GetType().Name, nestedValues.ObjectType.Name);
                }

                nestedValues.SetValues(value);
            }
            else
            {
                internalValues[Name] = value;
            }
        }

        #endregion

        #region Nested complex properties

        /// <summary>
        ///     Gets an internal object representing a scalar or complex property of this property,
        ///     which must be a mapped complex property.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="requestedType"> The type of object requested, which may be null or 'object' if any type can be accepted. </param>
        /// <param name="requireComplex">
        ///     if set to <c>true</c> then the found property must be a complex property.
        /// </param>
        /// <returns> The entry. </returns>
        public virtual InternalPropertyEntry Property(
            string property, Type requestedType = null, bool requireComplex = false)
        {
            DebugCheck.NotEmpty(property);

            Debug.Assert(
                EntryMetadata.IsMapped && EntryMetadata.IsComplex, "Should only be calling this from a DbComplexProperty instance.");

            return InternalEntityEntry.Property(this, property, requestedType ?? typeof(object), requireComplex);
        }

        #endregion

        #region IsModified

        /// <summary>
        ///     Gets or sets a value indicating whether this property is modified.
        /// </summary>
        public virtual bool IsModified
        {
            get
            {
                // If the entity is detached, then the property is not modified.
                if (InternalEntityEntry.IsDetached
                    || !EntryMetadata.IsMapped)
                {
                    return false;
                }

                return EntityPropertyIsModified();
            }
            set
            {
                ValidateNotDetachedAndInModel("IsModified");

                if (value)
                {
                    SetEntityPropertyModified();
                }
                else
                {
                    if (IsModified)
                    {
                        RejectEntityPropertyChanges();
                    }
                }
            }
        }

        #endregion

        #region Handling entries for detached entities

        /// <summary>
        ///     Validates that the owning entity entry is associated with an underlying
        ///     <see
        ///         cref="System.Data.Entity.Core.Objects.ObjectStateEntry" />
        ///     and
        ///     is not just wrapping a non-attached entity.
        /// </summary>
        private void ValidateNotDetachedAndInModel(string method)
        {
            if (!EntryMetadata.IsMapped)
            {
                throw Error.DbPropertyEntry_NotSupportedForPropertiesNotInTheModel(
                    method, base.EntryMetadata.MemberName, InternalEntityEntry.EntityType.Name);
            }

            if (InternalEntityEntry.IsDetached)
            {
                throw Error.DbPropertyEntry_NotSupportedForDetached(
                    method, base.EntryMetadata.MemberName, InternalEntityEntry.EntityType.Name);
            }
        }

        #endregion

        #region Property metadata access

        /// <summary>
        ///     Gets the property metadata.
        /// </summary>
        /// <value> The property metadata. </value>
        public new PropertyEntryMetadata EntryMetadata
        {
            get { return (PropertyEntryMetadata)base.EntryMetadata; }
        }

        #endregion

        #region DbMemberEntry factory methods

        /// <summary>
        ///     Creates a new non-generic <see cref="DbMemberEntry" /> backed by this internal entry.
        ///     The runtime type of the DbMemberEntry created will be <see cref="DbPropertyEntry" /> or a subtype of it.
        /// </summary>
        /// <returns> The new entry. </returns>
        public override DbMemberEntry CreateDbMemberEntry()
        {
            return EntryMetadata.IsComplex ? new DbComplexPropertyEntry(this) : new DbPropertyEntry(this);
        }

        /// <summary>
        ///     Creates a new generic <see cref="DbMemberEntry{TEntity,TProperty}" /> backed by this internal entry.
        ///     The runtime type of the DbMemberEntry created will be <see cref="DbPropertyEntry{TEntity,TProperty}" /> or a subtype of it.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <returns> The new entry. </returns>
        public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
        {
            return EntryMetadata.IsComplex
                       ? new DbComplexPropertyEntry<TEntity, TProperty>(this)
                       : new DbPropertyEntry<TEntity, TProperty>(this);
        }

        #endregion
    }
}
