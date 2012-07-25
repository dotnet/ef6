// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal sealed class StateManagerTypeMetadata
    {
        private readonly TypeUsage _typeUsage; // CSpace
        private readonly StateManagerMemberMetadata[] _members;
        private readonly Dictionary<string, int> _objectNameToOrdinal;
        private readonly Dictionary<string, int> _cLayerNameToOrdinal;
        private readonly DataRecordInfo _recordInfo;

        internal StateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
        {
            Debug.Assert(null != edmType, "null EdmType");
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

        internal int FieldCount
        {
            get { return _members.Length; }
        }

        internal Type GetFieldType(int ordinal)
        {
            return Member(ordinal).ClrType;
        }

        internal StateManagerMemberMetadata Member(int ordinal)
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

        internal bool IsMemberPartofShadowState(int ordinal)
        {
            // FUTURE_FEATURE:  When we add support for shadow state, fix this method.
            // The assert is okay for now because it's impossible for users to configure 
            // the system such that shadow state properties exist.
            Debug.Assert(
                Member(ordinal) != null,
                "The only case where Member(ordinal) can be null is if the property is in shadow state.  " +
                "When shadow state support is added, this assert should never fire.");
            return (null == Member(ordinal).ClrMetadata);
        }
    }
}
