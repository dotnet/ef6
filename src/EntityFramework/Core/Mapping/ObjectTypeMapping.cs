// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    // <summary>
    // Represents the metadata for OCObjectMapping.
    // </summary>
    internal class ObjectTypeMapping : MappingBase
    {
        // <summary>
        // Construct a new ObjectTypeMapping object
        // </summary>
        internal ObjectTypeMapping(EdmType clrType, EdmType cdmType)
        {
            Debug.Assert(clrType.BuiltInTypeKind == cdmType.BuiltInTypeKind, "BuiltInTypeKind must be the same for both types");
            m_clrType = clrType;
            m_cdmType = cdmType;
            identity = clrType.Identity + ObjectMslConstructs.IdentitySeperator + cdmType.Identity;

            if (Helper.IsStructuralType(cdmType))
            {
                m_memberMapping = new Dictionary<string, ObjectMemberMapping>(((StructuralType)cdmType).Members.Count);
            }
            else
            {
                m_memberMapping = EmptyMemberMapping;
            }
        }

        private readonly EdmType m_clrType; //type on the Clr side that is being mapped
        private readonly EdmType m_cdmType; //type on the Cdm side that is being mapped
        private readonly string identity;

        private readonly Dictionary<string, ObjectMemberMapping> m_memberMapping;
        //Indexes into the member mappings collection based on clr member name

        private static readonly Dictionary<string, ObjectMemberMapping> EmptyMemberMapping
            = new Dictionary<string, ObjectMemberMapping>(0);

        // <summary>
        // Gets the type kind for this item
        // </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.MetadataItem; }
        }

        // <summary>
        // The reference to the Clr type in Metadata
        // that participates in this mapping instance
        // </summary>
        internal EdmType ClrType
        {
            get { return m_clrType; }
        }

        // <summary>
        // The reference to the Cdm type in Metadata
        // that participates in this mapping instance
        // </summary>
        internal override MetadataItem EdmItem
        {
            get { return EdmType; }
        }

        // <summary>
        // The reference to the Cdm type in Metadata
        // that participates in this mapping instance
        // </summary>
        internal EdmType EdmType
        {
            get { return m_cdmType; }
        }

        // <summary>
        // Returns the Identity of ObjectTypeMapping.
        // The identity for an Object Type Map is the concatenation of
        // CLR Type Idntity + ':' + CDM Type Identity
        // </summary>
        internal override string Identity
        {
            get { return identity; }
        }

        // <summary>
        // get a MemberMap for the member name specified
        // </summary>
        // <param name="propertyName"> the name of the CDM member for which map needs to be retrieved </param>
        internal ObjectPropertyMapping GetPropertyMap(String propertyName)
        {
            var memberMapping = GetMemberMap(propertyName, false /*ignoreCase*/);

            if (memberMapping != null &&
                (
                    memberMapping.MemberMappingKind == MemberMappingKind.ScalarPropertyMapping
                    ||
                    memberMapping.MemberMappingKind == MemberMappingKind.ComplexPropertyMapping
                )
            )
            {
                return (ObjectPropertyMapping)memberMapping;
            }

            return null;
        }

        // <summary>
        // Add a member mapping as a child of this object mapping
        // </summary>
        // <param name="memberMapping"> child property mapping to be added </param>
        internal void AddMemberMap(ObjectMemberMapping memberMapping)
        {
            Debug.Assert(
                memberMapping.ClrMember.Name == memberMapping.EdmMember.Name,
                "Both clrmember and edmMember name must be the same");
            //Check to see if either the Clr member or the Cdm member specified in this 
            //type has already been mapped.
            Debug.Assert(!m_memberMapping.ContainsKey(memberMapping.EdmMember.Name));
            Debug.Assert(
                !ReferenceEquals(m_memberMapping, EmptyMemberMapping),
                "Make sure you don't add anything to the static emtpy member mapping");
            m_memberMapping.Add(memberMapping.EdmMember.Name, memberMapping);
        }

        // <summary>
        // Returns the member map for the given clr member
        // </summary>
        internal ObjectMemberMapping GetMemberMapForClrMember(string clrMemberName, bool ignoreCase)
        {
            return GetMemberMap(clrMemberName, ignoreCase);
        }

        // <summary>
        // returns the member mapping for the given member
        // </summary>
        private ObjectMemberMapping GetMemberMap(string propertyName, bool ignoreCase)
        {
            Check.NotEmpty(propertyName, "propertyName");
            ObjectMemberMapping memberMapping = null;

            if (!ignoreCase)
            {
                //First get the index of the member map from the clr indexs
                m_memberMapping.TryGetValue(propertyName, out memberMapping);
            }
            else
            {
                foreach (var keyValuePair in m_memberMapping)
                {
                    if (keyValuePair.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (memberMapping != null)
                        {
                            throw new MappingException(
                                Strings.Mapping_Duplicate_PropertyMap_CaseInsensitive(
                                    propertyName));
                        }
                        memberMapping = keyValuePair.Value;
                    }
                }
            }

            return memberMapping;
        }

        // <summary>
        // Overriding System.Object.ToString to provide better String representation
        // for this type.
        // </summary>
        public override string ToString()
        {
            return Identity;
        }
    }
}
