// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal class StateManagerTypeMetadata
    {
        private readonly TypeUsage _typeUsage; // CSpace
        private readonly StateManagerMemberMetadata[] _members;
        private readonly Dictionary<string, int> _objectNameToOrdinal;
        private readonly Dictionary<string, int> _cLayerNameToOrdinal;
        private readonly DataRecordInfo _recordInfo;

        // For testing
        internal StateManagerTypeMetadata()
        {
        }

        internal StateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
        {
            DebugCheck.NotNull(edmType);
            Debug.Assert(
                Helper.IsEntityType(edmType) ||
                Helper.IsComplexType(edmType),
                "not Complex or EntityType");
            Debug.Assert(
                ReferenceEquals(mapping, null) ||
                ReferenceEquals(mapping.EdmType, edmType),
                "different EdmType instance");

            _typeUsage = TypeUsage.Create(edmType);
            _recordInfo = new DataRecordInfo(_typeUsage);

            var members = TypeHelpers.GetProperties(edmType);
            _members = new StateManagerMemberMetadata[members.Count];
            _objectNameToOrdinal = new Dictionary<string, int>(members.Count);
            _cLayerNameToOrdinal = new Dictionary<string, int>(members.Count);

            ReadOnlyMetadataCollection<EdmMember> keyMembers = null;
            if (Helper.IsEntityType(edmType))
            {
                keyMembers = ((EntityType)edmType).KeyMembers;
            }

            for (var i = 0; i < _members.Length; ++i)
            {
                var member = members[i];

                ObjectPropertyMapping memberMap = null;
                if (null != mapping)
                {
                    memberMap = mapping.GetPropertyMap(member.Name);
                    if (null != memberMap)
                    {
                        _objectNameToOrdinal.Add(memberMap.ClrProperty.Name, i); // olayer name
                    }
                }
                _cLayerNameToOrdinal.Add(member.Name, i); // clayer name

                // Determine whether this member is part of the identity of the entity.
                _members[i] = new StateManagerMemberMetadata(memberMap, member, ((null != keyMembers) && keyMembers.Contains(member)));
            }
        }

        internal TypeUsage CdmMetadata
        {
            get { return _typeUsage; }
        }

        internal DataRecordInfo DataRecordInfo
        {
            get { return _recordInfo; }
        }

        internal virtual int FieldCount
        {
            get { return _members.Length; }
        }

        internal Type GetFieldType(int ordinal)
        {
            return Member(ordinal).ClrType;
        }

        internal virtual StateManagerMemberMetadata Member(int ordinal)
        {
            if (unchecked((uint)ordinal < (uint)_members.Length))
            {
                return _members[ordinal];
            }
            throw new ArgumentOutOfRangeException("ordinal");
        }

        internal IEnumerable<StateManagerMemberMetadata> Members
        {
            get { return _members; }
        }

        internal string CLayerMemberName(int ordinal)
        {
            return Member(ordinal).CLayerName;
        }

        internal int GetOrdinalforOLayerMemberName(string name)
        {
            int ordinal;
            if (String.IsNullOrEmpty(name)
                || !_objectNameToOrdinal.TryGetValue(name, out ordinal))
            {
                ordinal = -1;
            }
            return ordinal;
        }

        internal int GetOrdinalforCLayerMemberName(string name)
        {
            int ordinal;
            if (String.IsNullOrEmpty(name)
                || !_cLayerNameToOrdinal.TryGetValue(name, out ordinal))
            {
                ordinal = -1;
            }
            return ordinal;
        }
    }
}
