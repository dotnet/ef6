// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;

    // <summary>
    // An implementation of <see cref="InternalPropertyValues" /> that represents a clone of another
    // dictionary.  That is, all the property values have been been copied into this dictionary.
    // </summary>
    internal class ClonedPropertyValues : InternalPropertyValues
    {
        #region Constructors and fields 

        private readonly ISet<string> _propertyNames;
        private readonly IDictionary<string, ClonedPropertyValuesItem> _propertyValues;

        // <summary>
        // Initializes a new instance of the <see cref="ClonedPropertyValues" /> class by copying
        // values from the given dictionary.
        // </summary>
        // <param name="original"> The dictionary to clone. </param>
        // <param name="valuesRecord"> If non-null, then the values for the new dictionary are taken from this record rather than from the original dictionary. </param>
        internal ClonedPropertyValues(InternalPropertyValues original, DbDataRecord valuesRecord = null)
            : base(original.InternalContext, original.ObjectType, original.IsEntityValues)
        {
            _propertyNames = original.PropertyNames;
            _propertyValues = new Dictionary<string, ClonedPropertyValuesItem>(_propertyNames.Count);

            foreach (var propertyName in _propertyNames)
            {
                var item = original.GetItem(propertyName);

                var value = item.Value;
                var asValues = value as InternalPropertyValues;
                if (asValues != null)
                {
                    var nestedValuesRecord = valuesRecord == null ? null : (DbDataRecord)valuesRecord[propertyName];
                    value = new ClonedPropertyValues(asValues, nestedValuesRecord);
                }
                else if (valuesRecord != null)
                {
                    value = valuesRecord[propertyName];
                    if (value == DBNull.Value)
                    {
                        value = null;
                    }
                }

                _propertyValues[propertyName] = new ClonedPropertyValuesItem(
                    propertyName, value, item.Type, item.IsComplex);
            }
        }

        #endregion

        #region Implementation of abstract members from base

        // <summary>
        // Gets the dictionary item for a given property name.
        // </summary>
        // <param name="propertyName"> Name of the property. </param>
        // <returns> An item for the given name. </returns>
        protected override IPropertyValuesItem GetItemImpl(string propertyName)
        {
            return _propertyValues[propertyName];
        }

        // <summary>
        // Gets the set of names of all properties in this dictionary as a read-only set.
        // </summary>
        // <value> The property names. </value>
        public override ISet<string> PropertyNames
        {
            get { return _propertyNames; }
        }

        #endregion
    }
}
