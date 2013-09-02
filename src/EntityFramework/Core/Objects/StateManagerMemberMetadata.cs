// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal class StateManagerMemberMetadata
    {
        private readonly EdmProperty _clrProperty;
        private readonly EdmProperty _edmProperty;
        private readonly bool _isPartOfKey;
        private readonly bool _isComplexType;

        // For testing
        internal StateManagerMemberMetadata()
        {
        }

        internal StateManagerMemberMetadata(ObjectPropertyMapping memberMap, EdmProperty memberMetadata, bool isPartOfKey)
        {
            DebugCheck.NotNull(memberMap);
            DebugCheck.NotNull(memberMetadata);
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
                Debug.Assert(null != _clrProperty);
                return _clrProperty.TypeUsage.EdmType.ClrType;
            }
        }

        internal virtual bool IsComplex
        {
            get { return _isComplexType; }
        }

        internal virtual EdmProperty CdmMetadata
        {
            get { return _edmProperty; }
        }

        internal EdmProperty ClrMetadata
        {
            get
            {
                Debug.Assert(null != _clrProperty);
                return _clrProperty;
            }
        }

        internal bool IsPartOfKey
        {
            get { return _isPartOfKey; }
        }

        public virtual object GetValue(object userObject) // wrapp it in cacheentry
        {
            Debug.Assert(null != _clrProperty);
            var dataObject = DelegateFactory.GetValue(_clrProperty, userObject);
            return dataObject;
        }

        public void SetValue(object userObject, object value) // if record , unwrapp to object, use materializer in cacheentry
        {
            Debug.Assert(null != _clrProperty);
            if (DBNull.Value == value)
            {
                value = null;
            }
            if (IsComplex && value == null)
            {
                throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(CLayerName));
            }
            DelegateFactory.SetValue(_clrProperty, userObject, value);
        }
    }
}
