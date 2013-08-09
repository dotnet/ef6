// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    internal sealed class FieldNameLookup
    {
        private readonly Dictionary<string, int> _fieldNameLookup = new Dictionary<string, int>();

        // Original names for linear searches when exact matches fail
        private readonly string[] _fieldNames;

        public FieldNameLookup(ReadOnlyCollection<string> columnNames)
        {
            DebugCheck.NotNull(columnNames);

            var length = columnNames.Count;
            _fieldNames = new string[length];

            for (var i = 0; i < length; ++i)
            {
                _fieldNames[i] = columnNames[i];
                Debug.Assert(_fieldNames[i] != null);
            }

            GenerateLookup();
        }

        public FieldNameLookup(IDataRecord reader)
        {
            DebugCheck.NotNull(reader);

            var length = reader.FieldCount;
            _fieldNames = new string[length];

            for (var i = 0; i < length; ++i)
            {
                _fieldNames[i] = reader.GetName(i);
                Debug.Assert(_fieldNames[i] != null);
            }

            GenerateLookup();
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public int GetOrdinal(string fieldName)
        {
            Check.NotNull(fieldName, "fieldName");

            var index = IndexOf(fieldName);
            if (index == -1)
            {
                throw new IndexOutOfRangeException(fieldName);
            }

            return index;
        }

        private int IndexOf(string fieldName)
        {
            int index;
            if (!_fieldNameLookup.TryGetValue(fieldName, out index))
            {
                // Via case insensitive search, first match with lowest ordinal matches
                index = LinearIndexOf(fieldName, CompareOptions.IgnoreCase);

                if (index == -1)
                {
                    // Do the slow search now (kana, width insensitive comparison)
                    index = LinearIndexOf(fieldName, EntityUtil.StringCompareOptions);
                }
            }

            return index;
        }

        private int LinearIndexOf(string fieldName, CompareOptions compareOptions)
        {
            // Tried Array.FindIndex and various other options here; none seemed to be faster than this
            for (var i = 0; i < _fieldNames.Length; ++i)
            {
                if (CultureInfo.InvariantCulture.CompareInfo.Compare(fieldName, _fieldNames[i], compareOptions) == 0)
                {
                    _fieldNameLookup[fieldName] = i; // Add an exact match for the future
                    return i;
                }
            }
            return -1;
        }

        private void GenerateLookup()
        {
            // Via case sensitive search, first match with lowest ordinal matches
            for (var i = _fieldNames.Length - 1; 0 <= i; --i)
            {
                _fieldNameLookup[_fieldNames[i]] = i;
            }
        }
    }
}
