// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    internal sealed class StateManagerMemberMetadata
    {
        private readonly EdmProperty _clrProperty; // may be null if shadowState
        private readonly EdmProperty _edmProperty;
        private readonly bool _isPartOfKey;
        private readonly bool _isComplexType;

        internal StateManagerMemberMetadata(ObjectPropertyMapping memberMap, EdmProperty memberMetadata, bool isPartOfKey)
        {
            // if memberMap is null, then this is a shadowstate
            Debug.Assert(null != memberMap, "shadowstate not supported");
            Debug.Assert(null != memberMetadata, "CSpace should never be null");
            _clrProperty = memberMap.ClrProperty;
            _edmProperty = memberMetadata;
            _isPartOfKey = isPartOfKey;
            _isComplexType = (Helper.IsEntityType(_edmProperty.TypeUsage.EdmType) ||
                              Helper.IsComplexType(_edmProperty.TypeUsage.EdmType));
        }

        internal string CLayerName
        {
            get { return _edmProperty.Name; }
        }

        internal Type ClrType
        {
            get
            {
                Debug.Assert(null != _clrProperty, "shadowstate not supported");
                return _clrProperty.TypeUsage.EdmType.ClrType;
                //return ((null != _clrProperty)
                //    ? _clrProperty.TypeUsage.EdmType.ClrType
                //    : (Helper.IsComplexType(_edmProperty)
                //        ? typeof(DbDataRecord)
                //        : ((PrimitiveType)_edmProperty.TypeUsage.EdmType).ClrEquivalentType));
            }
        }

        internal bool IsComplex
        {
            get { return _isComplexType; }
        }

        internal EdmProperty CdmMetadata
        {
            get { return _edmProperty; }
        }

        internal EdmProperty ClrMetadata
        {
            get
            {
                Debug.Assert(null != _clrProperty, "shadowstate not supported");
                return _clrProperty;
            }
        }

        internal bool IsPartOfKey
        {
            get { return _isPartOfKey; }
        }

        public object GetValue(object userObject) // wrapp it in cacheentry
        {
            Debug.Assert(null != _clrProperty, "shadowstate not supported");
            var dataObject = LightweightCodeGenerator.GetValue(_clrProperty, userObject);
            return dataObject;
        }

        public void SetValue(object userObject, object value) // if record , unwrapp to object, use materializer in cacheentry
        {
            Debug.Assert(null != _clrProperty, "shadowstate not supported");
            if (DBNull.Value == value)
            {
                value = null;
            }
            if (IsComplex && value == null)
            {
                throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(CLayerName));
            }
            LightweightCodeGenerator.SetValue(_clrProperty, userObject, value);
        }
    }
}
