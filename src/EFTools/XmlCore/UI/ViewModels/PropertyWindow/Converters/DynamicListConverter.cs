// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     type converter for a list of values that is determined programmatically
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TSelectedObj"></typeparam>
    internal abstract class DynamicListConverter<TValue, TSelectedObj> : StringConverter, IResettableConverter
        where TSelectedObj : class
    {
        protected Dictionary<TValue, string> _valueToDisplayMapping;
        protected SortedDictionary<string, TValue> _displayToValueMapping;
        protected ITypeDescriptorContext _context;
        protected object _displayValueForNull = null;

        void IResettableConverter.Reset()
        {
            _valueToDisplayMapping = null;
            _displayToValueMapping = null;
        }

        protected virtual bool ContainsMapping(string displayValue)
        {
            return _displayToValueMapping.ContainsKey(displayValue);
        }

        protected virtual void AddMapping(TValue value, string displayValue)
        {
            if (value != null)
            {
                if (_valueToDisplayMapping.ContainsKey(value))
                {
                    Debug.Fail(
                        GetType().Name + ".AddMapping(): existing _valueToDisplayMapping entry for value " + value + ", "
                        + _valueToDisplayMapping[value] + ", is about to be overwritten");
                    return;
                }
                _valueToDisplayMapping[value] = displayValue;
            }
            else
            {
                if (null != _displayValueForNull)
                {
                    Debug.Fail(
                        GetType().Name + ".AddMapping(): existing _displayValueForNull entry, " + _displayValueForNull
                        + ", is about to be overwritten");
                    return;
                }
                _displayValueForNull = displayValue;
            }

            if (_displayToValueMapping.ContainsKey(displayValue))
            {
                Debug.Fail(
                    GetType().Name + ".AddMapping(): existing _displayToValueMapping entry for displayValue " + displayValue + ", "
                    + _displayToValueMapping[displayValue] + ", is about to be overwritten");
                return;
            }
            _displayToValueMapping[displayValue] = value;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }
            return true;
        }

        protected abstract void PopulateMappingForSelectedObject(TSelectedObj selectedObject);

        protected virtual void PopulateMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }
            Debug.Assert(_context != null, "Should have a context for the PopulateMapping call.");

            foreach (var selectedObj in PropertyWindowViewModelHelper.GetObjectsFromSelection<TSelectedObj>(_context.Instance))
            {
                PopulateMappingForSelectedObject(selectedObj);
                break;
            }
        }

        protected virtual void InitializeMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }

            if (_valueToDisplayMapping == null)
            {
                Debug.Assert(_context != null, "Should have a context for the InitializeMapping call.");
                if (_context != null)
                {
                    _valueToDisplayMapping = new Dictionary<TValue, string>();
                    _displayToValueMapping = new SortedDictionary<string, TValue>();

                    PopulateMapping(_context);
                }
            }
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }
            Debug.Assert(_context != null, "Should have a context for the GetStandardValues call.");

            InitializeMapping(_context);
            return new StandardValuesCollection(_displayToValueMapping.Values);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context != null)
            {
                _context = context;
            }
            Debug.Assert(_context != null, "Should have a context for the ConvertTo call.");

            InitializeMapping(_context);
            if (value != null
                && destinationType == typeof(string))
            {
                string displayValue;
                if (!_valueToDisplayMapping.TryGetValue((TValue)value, out displayValue))
                {
                    displayValue = null;
                }
                return displayValue;
            }
            return _displayValueForNull;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (context != null)
            {
                _context = context;
            }
            Debug.Assert(_context != null, "Should have a context for the ConvertFrom call.");
            if (_context != null)
            {
                InitializeMapping(_context);
                var displayValue = value as string;
                Debug.Assert(displayValue != null);
                if (!string.IsNullOrEmpty(displayValue))
                {
                    TValue rawValue;
                    if (!_displayToValueMapping.TryGetValue(displayValue, out rawValue))
                    {
                        rawValue = default(TValue);
                    }
                    return rawValue;
                }
            }

            return null;
        }
    }
}
