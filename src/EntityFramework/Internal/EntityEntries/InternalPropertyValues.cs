// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq.Expressions;
    using System.Reflection;

    // <summary>
    // The internal class used to implement <see cref="DbPropertyValues" />.
    // This internal class allows for a clean internal factoring without compromising the public API.
    // </summary>
    internal abstract class InternalPropertyValues
    {
        #region Fields and constructors

        private static readonly ConcurrentDictionary<Type, Func<object>> _nonEntityFactories =
            new ConcurrentDictionary<Type, Func<object>>();

        private readonly InternalContext _internalContext;
        private readonly Type _type;
        private readonly bool _isEntityValues;

        // <summary>
        // Initializes a new instance of the <see cref="InternalPropertyValues" /> class.
        // </summary>
        // <param name="internalContext"> The internal context with which the entity of complex object is associated. </param>
        // <param name="type"> The type of the entity or complex object. </param>
        // <param name="isEntityValues">
        // If set to <c>true</c> this is a dictionary for an entity, otherwise it is a dictionary for a complex object.
        // </param>
        protected InternalPropertyValues(InternalContext internalContext, Type type, bool isEntityValues)
        {
            DebugCheck.NotNull(internalContext);
            DebugCheck.NotNull(type);

            _internalContext = internalContext;
            _type = type;
            _isEntityValues = isEntityValues;
        }

        #endregion

        #region Abstract members

        // <summary>
        // Implemented by subclasses to get the dictionary item for a given property name.
        // Checking that the name is valid should happen before this method is called such
        // that subclasses do not need to perform the check.
        // </summary>
        // <param name="propertyName"> Name of the property. </param>
        // <returns> An item for the given name. </returns>
        protected abstract IPropertyValuesItem GetItemImpl(string propertyName);

        // <summary>
        // Gets the set of names of all properties in this dictionary as a read-only set.
        // </summary>
        // <value> The property names. </value>
        public abstract ISet<string> PropertyNames { get; }

        #endregion

        #region Copy to and from objects

        // <summary>
        // Creates an object of the underlying type for this dictionary and hydrates it with property
        // values from this dictionary.
        // </summary>
        // <returns> The properties of this dictionary copied into a new object. </returns>
        public object ToObject()
        {
            // Create an instance of the object either using the CreateObject method for an entity or
            // a compiled delegate call to the constructor for other types.
            var clone = CreateObject();
            var setters = DbHelpers.GetPropertySetters(_type);

            foreach (var propertyName in PropertyNames)
            {
                var value = GetItem(propertyName).Value;

                var asValues = value as InternalPropertyValues;
                if (asValues != null)
                {
                    value = asValues.ToObject();
                }

                // If the CLR type doesn't have a property with the given name, then we simply ignore it.
                // This cannot happen currently but will be possible when we have shadow state.
                Action<object, object> setterDelegate;
                if (setters.TryGetValue(propertyName, out setterDelegate))
                {
                    setterDelegate(clone, value);
                }
            }
            return clone;
        }

        // <summary>
        // Creates an instance of the underlying type for this dictionary, which may either be an entity type (in which
        // case CreateObject on the context is used) or a non-entity type (in which case the empty constructor is used.)
        // In either case, app domain cached compiled delegates are used to do the creation.
        // </summary>
        private object CreateObject()
        {
            if (_isEntityValues)
            {
                return _internalContext.CreateObject(_type);
            }

            Func<object> nonEntityFactory;
            if (!_nonEntityFactories.TryGetValue(_type, out nonEntityFactory))
            {
                var factoryExpression =
                    Expression.New(
                        _type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
                nonEntityFactory = Expression.Lambda<Func<object>>(factoryExpression, null).Compile();
                _nonEntityFactories.TryAdd(_type, nonEntityFactory);
            }
            return nonEntityFactory();
        }

        // <summary>
        // Sets the values of this dictionary by reading values out of the given object.
        // The given object must be of the type that this dictionary is based on.
        // </summary>
        // <param name="value"> The object to read values from. </param>
        public void SetValues(object value)
        {
            DebugCheck.NotNull(value);

            var getters = DbHelpers.GetPropertyGetters(value.GetType());

            foreach (var propertyName in PropertyNames)
            {
                // If the CLR type doesn't have a property with the given name, then we simply ignore it.
                // This cannot happen currently but will be possible when we have shadow state.
                Func<object, object> getterDelegate;
                if (getters.TryGetValue(propertyName, out getterDelegate))
                {
                    var propertyValue = getterDelegate(value);
                    var item = GetItem(propertyName);

                    // Cannot set values from a null complex property.
                    if (propertyValue == null
                        && item.IsComplex)
                    {
                        throw Error.DbPropertyValues_ComplexObjectCannotBeNull(propertyName, _type.Name);
                    }

                    var nestedValues = item.Value as InternalPropertyValues;
                    if (nestedValues == null)
                    {
                        SetValue(item, propertyValue);
                    }
                    else
                    {
                        nestedValues.SetValues(propertyValue);
                    }
                }
            }
        }

        #endregion

        #region Copy to and from property values

        // <summary>
        // Creates a new dictionary containing copies of all the properties in this dictionary.
        // Changes made to the new dictionary will not be reflected in this dictionary and vice versa.
        // </summary>
        // <returns> A clone of this dictionary. </returns>
        public InternalPropertyValues Clone()
        {
            return new ClonedPropertyValues(this);
        }

        // <summary>
        // Sets the values of this dictionary by reading values from another dictionary.
        // The other dictionary must be based on the same type as this dictionary, or a type derived
        // from the type for this dictionary.
        // </summary>
        // <param name="values"> The dictionary to read values from. </param>
        public void SetValues(InternalPropertyValues values)
        {
            DebugCheck.NotNull(values);

            // Setting values from a derived type is allowed, but setting values from a base type is not.
            if (!_type.IsAssignableFrom(values.ObjectType))
            {
                throw Error.DbPropertyValues_AttemptToSetValuesFromWrongType(values.ObjectType.Name, _type.Name);
            }

            foreach (var propertyName in PropertyNames)
            {
                var item = values.GetItem(propertyName);

                if (item.Value == null
                    && item.IsComplex)
                {
                    throw Error.DbPropertyValues_NestedPropertyValuesNull(propertyName, _type.Name);
                }

                this[propertyName] = item.Value;
            }
        }

        #endregion

        #region Property value access

        // <summary>
        // Gets or sets the value of the property with the specified property name.
        // The value may be a nested instance of this class.
        // </summary>
        // <param name="propertyName"> The property name. </param>
        // <value> The value of the property. </value>
        public object this[string propertyName]
        {
            get
            {
                DebugCheck.NotEmpty(propertyName);

                return GetItem(propertyName).Value;
            }
            set
            {
                DebugCheck.NotEmpty(propertyName);

                var asPropertyValues = value as DbPropertyValues;
                if (asPropertyValues != null)
                {
                    value = asPropertyValues.InternalPropertyValues;
                }

                var item = GetItem(propertyName);
                var nestedValues = item.Value as InternalPropertyValues;
                if (nestedValues == null)
                {
                    // Not a nested dictionary, so just set the value directly.
                    SetValue(item, value);
                }
                else
                {
                    // Check that the value passed is an InternalPropertyValues and not null
                    var valueAsValues = value as InternalPropertyValues;
                    if (valueAsValues == null)
                    {
                        throw Error.DbPropertyValues_AttemptToSetNonValuesOnComplexProperty();
                    }
                    nestedValues.SetValues(valueAsValues);
                }
            }
        }

        // <summary>
        // Gets the dictionary item for the property with the given name.
        // This method checks that the given name is valid.
        // </summary>
        // <param name="propertyName"> The property name. </param>
        // <returns> The item. </returns>
        public IPropertyValuesItem GetItem(string propertyName)
        {
            if (!PropertyNames.Contains(propertyName))
            {
                throw Error.DbPropertyValues_PropertyDoesNotExist(propertyName, _type.Name);
            }
            return GetItemImpl(propertyName);
        }

        // <summary>
        // Sets the value of the property only if it is different from the current value and is not
        // an invalid attempt to set a complex property.
        // </summary>
        private void SetValue(IPropertyValuesItem item, object newValue)
        {
            // Using KeyValuesEqual here to control setting the property to modified since the deep
            // comparison of binary values is more appropriate for all properties when used in an
            // N-Tier or concurrency situation.
            if (!DbHelpers.PropertyValuesEqual(item.Value, newValue))
            {
                if (item.Value == null
                    && item.IsComplex)
                {
                    throw Error.DbPropertyValues_NestedPropertyValuesNull(item.Name, _type.Name);
                }

                if (newValue != null
                    && !item.Type.IsAssignableFrom(newValue.GetType()))
                {
                    throw Error.DbPropertyValues_WrongTypeForAssignment(
                        newValue.GetType().Name, item.Name, item.Type.Name, _type.Name);
                }

                item.Value = newValue;
            }
        }

        #endregion

        #region Underlying dictionary state

        // <summary>
        // Gets the entity type of complex type that this dictionary is based on.
        // </summary>
        // <value> The type of the object underlying this dictionary. </value>
        public Type ObjectType
        {
            get { return _type; }
        }

        // <summary>
        // Gets the internal context with which the underlying entity or complex type is associated.
        // </summary>
        // <value> The internal context. </value>
        public InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        // <summary>
        // Gets a value indicating whether the object for this dictionary is an entity or a complex object.
        // </summary>
        // <value>
        // <c>true</c> if this this is a dictionary for an entity; <c>false</c> if it is a dictionary for a complex object.
        // </value>
        public bool IsEntityValues
        {
            get { return _isEntityValues; }
        }

        #endregion
    }
}
