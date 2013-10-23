// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     type converter for a fixed list of values (such as an Enum type)
    /// </summary>
    internal abstract class FixedListConverter<T> : StringConverter
    {
        private SortedDictionary<T, string> _valueToDisplayMapping;
        private Dictionary<string, T> _displayToValueMapping;
        private bool _mappingDone;

        private SortedDictionary<T, string> ValueToDisplayMapping
        {
            get
            {
                EnsureMappingDone();

                Debug.Assert(_valueToDisplayMapping != null, "_valueDisplayMapping is null");

                return _valueToDisplayMapping;
            }
        }

        private Dictionary<string, T> DisplayToValueMapping
        {
            get
            {
                EnsureMappingDone();

                Debug.Assert(_displayToValueMapping != null, "_displayToValueMapping is null");

                return _displayToValueMapping;
            }
        }

        protected IEnumerable<T> Values
        {
            get
            {
                if (ValueToDisplayMapping != null)
                {
                    return ValueToDisplayMapping.Keys;
                }
                else
                {
                    return Enumerable.Empty<T>();
                }
            }
        }

        private void EnsureMappingDone()
        {
            if (!_mappingDone)
            {
                _valueToDisplayMapping = new SortedDictionary<T, string>();
                _displayToValueMapping = new Dictionary<string, T>();

                PopulateMapping();
                _mappingDone = true;
            }
        }

        protected abstract void PopulateMapping();

        protected void AddMapping(T value, string displayValue)
        {
            Debug.Assert(!_mappingDone);
            _valueToDisplayMapping[value] = displayValue;
            _displayToValueMapping[displayValue] = value;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(ValueToDisplayMapping.Keys);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)
                && ValueToDisplayMapping.ContainsKey((T)value))
            {
                return ValueToDisplayMapping[(T)value];
            }
            return null;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null
                && DisplayToValueMapping.ContainsKey(stringValue))
            {
                return DisplayToValueMapping[stringValue];
            }
            return null;
        }
    }
}
