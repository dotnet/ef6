// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Mapping metadata for all OC member maps.
    /// </summary>
    internal abstract class ObjectMemberMapping
    {
        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        protected ObjectMemberMapping(EdmMember edmMember, EdmMember clrMember)
        {
            Debug.Assert(edmMember.BuiltInTypeKind == clrMember.BuiltInTypeKind, "BuiltInTypeKind must be the same");
            m_edmMember = edmMember;
            m_clrMember = clrMember;
        }

        private readonly EdmMember m_edmMember; //EdmMember metadata representing the Cdm member for which the mapping is specified
        private readonly EdmMember m_clrMember; //EdmMember metadata representing the Clr member for which the mapping is specified

        /// <summary>
        /// The PropertyMetadata object that represents the Cdm member for which mapping is being specified
        /// </summary>
        internal EdmMember EdmMember
        {
            get { return m_edmMember; }
        }

        /// <summary>
        /// The PropertyMetadata object that represents the Clr member for which mapping is being specified
        /// </summary>
        internal EdmMember ClrMember
        {
            get { return m_clrMember; }
        }

        /// <summary>
        /// Returns the member mapping kind
        /// </summary>
        internal abstract MemberMappingKind MemberMappingKind { get; }
    }
}
