// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    // This class represents the key of  constraint on values that a relation slot may have
    internal class ExtentKey : InternalBase
    {
        // effects: Creates a key object for an extent (present in each MemberPath)
        // with the fields corresponding to keyFields
        internal ExtentKey(IEnumerable<MemberPath> keyFields)
        {
            m_keyFields = new List<MemberPath>(keyFields);
        }

        // All the key fields in an entity set
        private readonly List<MemberPath> m_keyFields;

        internal IEnumerable<MemberPath> KeyFields
        {
            get { return m_keyFields; }
        }

        // effects: Determines all the keys (unique and primary for
        // entityType) for entityType and returns a key. "prefix" gives the
        // path of the extent or end of a relationship in a relationship set
        // -- prefix is prepended to the entity's key fields to get the full memberpath
        internal static List<ExtentKey> GetKeysForEntityType(MemberPath prefix, EntityType entityType)
        {
            // CHANGE_ADYA_MULTIPLE_KEYS: currently there is a single key only. Need to support
            // keys inside complex types + unique keys
            var key = GetPrimaryKeyForEntityType(prefix, entityType);

            var keys = new List<ExtentKey>();
            keys.Add(key);
            return keys;
        }

        // effects: Returns the key for entityType prefixed with prefix (for
        // its memberPath)
        internal static ExtentKey GetPrimaryKeyForEntityType(MemberPath prefix, EntityType entityType)
        {
            var keyFields = new List<MemberPath>();
            foreach (var keyMember in entityType.KeyMembers)
            {
                Debug.Assert(keyMember != null, "Bogus key member in metadata");
                keyFields.Add(new MemberPath(prefix, keyMember));
            }

            // Just have one key for now
            var key = new ExtentKey(keyFields);
            return key;
        }

        // effects: Returns a key correspnding to all the fields in different
        // ends of relationtype prefixed with "prefix"
        internal static ExtentKey GetKeyForRelationType(MemberPath prefix, AssociationType relationType)
        {
            var keyFields = new List<MemberPath>();

            foreach (var endMember in relationType.AssociationEndMembers)
            {
                var endPrefix = new MemberPath(prefix, endMember);
                var entityType = MetadataHelper.GetEntityTypeForEnd(endMember);
                var primaryKey = GetPrimaryKeyForEntityType(endPrefix, entityType);
                keyFields.AddRange(primaryKey.KeyFields);
            }
            var key = new ExtentKey(keyFields);
            return key;
        }

        internal string ToUserString()
        {
            var result = StringUtil.ToCommaSeparatedStringSorted(m_keyFields);
            return result;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.ToCommaSeparatedStringSorted(builder, m_keyFields);
        }
    }
}
