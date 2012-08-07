// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     An implementation of <see cref="InternalPropertyValues" /> that is based on an existing
    ///     <see cref="DbUpdatableDataRecord" /> instance.
    /// </summary>
    internal class DbDataRecordPropertyValues : InternalPropertyValues
    {
        #region Constructors and fields

        private readonly DbUpdatableDataRecord _dataRecord;
        private ISet<string> _names;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbDataRecordPropertyValues" /> class.
        /// </summary>
        /// <param name="internalContext"> The internal context. </param>
        /// <param name="type"> The type. </param>
        /// <param name="dataRecord"> The data record. </param>
        /// <param name="isEntityValues"> If set to <c>true</c> this is a dictionary for an entity, otherwise it is a dictionary for a complex object. </param>
        internal DbDataRecordPropertyValues(
            InternalContext internalContext, Type type, DbUpdatableDataRecord dataRecord, bool isEntity)
            : base(internalContext, type, isEntity)
        {
            Contract.Requires(dataRecord != null);

            _dataRecord = dataRecord;
        }

        #endregion

        #region Implementation of abstract members from base

        /// <summary>
        ///     Gets the dictionary item for a given property name.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> An item for the given name. </returns>
        protected override IPropertyValuesItem GetItemImpl(string propertyName)
        {
            var ordinal = _dataRecord.GetOrdinal(propertyName);
            var value = _dataRecord[ordinal];

            var asDataRecord = value as DbUpdatableDataRecord;
            if (asDataRecord != null)
            {
                value = new DbDataRecordPropertyValues(
                    InternalContext, _dataRecord.GetFieldType(ordinal), asDataRecord, isEntity: false);
            }
            else if (value == DBNull.Value)
            {
                value = null;
            }

            return new DbDataRecordPropertyValuesItem(_dataRecord, ordinal, value);
        }

        /// <summary>
        ///     Gets the set of names of all properties in this dictionary as a read-only set.
        /// </summary>
        /// <value> The property names. </value>
        public override ISet<string> PropertyNames
        {
            get
            {
                if (_names == null)
                {
                    var names = new HashSet<string>();
                    for (var i = 0; i < _dataRecord.FieldCount; i++)
                    {
                        names.Add(_dataRecord.GetName(i));
                    }
                    _names = new ReadOnlySet<string>(names);
                }
                return _names;
            }
        }

        #endregion
    }
}
