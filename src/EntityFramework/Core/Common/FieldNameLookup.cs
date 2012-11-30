// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    internal sealed class FieldNameLookup
    {
        // hashtable stores the index into the _fieldNames, match via case-sensitive
        private Hashtable _fieldNameLookup;

        // original names for linear searches when exact matches fail
        private readonly string[] _fieldNames;

        // if _defaultLocaleID is -1 then _compareInfo is initialized with InvariantCulture CompareInfo
        // otherwise it is specified by the server? for the correct compare info
        private CompareInfo _compareInfo;
        private readonly int _defaultLocaleID;

        public FieldNameLookup(ReadOnlyCollection<string> columnNames, int defaultLocaleID)
        {
            var length = columnNames.Count;
            var fieldNames = new string[length];
            for (var i = 0; i < length; ++i)
            {
                fieldNames[i] = columnNames[i];
                Debug.Assert(null != fieldNames[i]);
            }
            _fieldNames = fieldNames;
            _defaultLocaleID = defaultLocaleID;
            GenerateLookup();
        }

        public FieldNameLookup(IDataRecord reader, int defaultLocaleID)
        {
            // V1.2.3300

            var length = reader.FieldCount;
            var fieldNames = new string[length];
            for (var i = 0; i < length; ++i)
            {
                fieldNames[i] = reader.GetName(i);
                Debug.Assert(null != fieldNames[i]);
            }
            _fieldNames = fieldNames;
            _defaultLocaleID = defaultLocaleID;
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public int GetOrdinal(string fieldName)
        {
            // V1.2.3300
            Check.NotNull(fieldName, "fieldName");

            var index = IndexOf(fieldName);
            if (-1 == index)
            {
                throw new IndexOutOfRangeException(fieldName);
            }

            return index;
        }

        public int IndexOf(string fieldName)
        {
            // V1.2.3300
            if (null == _fieldNameLookup)
            {
                GenerateLookup();
            }
            int index;
            var value = _fieldNameLookup[fieldName];
            if (null != value)
            {
                // via case sensitive search, first match with lowest ordinal matches
                index = (int)value;
            }
            else
            {
                // via case insensitive search, first match with lowest ordinal matches
                index = LinearIndexOf(fieldName, CompareOptions.IgnoreCase);
                if (-1 == index)
                {
                    // do the slow search now (kana, width insensitive comparison)
                    index = LinearIndexOf(fieldName, EntityUtil.StringCompareOptions);
                }
            }
            return index;
        }

        private int LinearIndexOf(string fieldName, CompareOptions compareOptions)
        {
            var compareInfo = _compareInfo;
            if (null == compareInfo)
            {
                if (-1 != _defaultLocaleID)
                {
                    compareInfo = CompareInfo.GetCompareInfo(_defaultLocaleID);
                }
                if (null == compareInfo)
                {
                    compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                }
                _compareInfo = compareInfo;
            }
            var length = _fieldNames.Length;
            for (var i = 0; i < length; ++i)
            {
                if (0 == compareInfo.Compare(fieldName, _fieldNames[i], compareOptions))
                {
                    _fieldNameLookup[fieldName] = i; // add an exact match for the future
                    return i;
                }
            }
            return -1;
        }

        // RTM common code for generating Hashtable from array of column names
        private void GenerateLookup()
        {
            var length = _fieldNames.Length;
            var hash = new Hashtable(length);

            // via case sensitive search, first match with lowest ordinal matches
            for (var i = length - 1; 0 <= i; --i)
            {
                var fieldName = _fieldNames[i];
                hash[fieldName] = i;
            }
            _fieldNameLookup = hash;
        }
    }
}
