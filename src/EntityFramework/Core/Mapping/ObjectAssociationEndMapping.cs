// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Mapping metadata for all OC member maps.
    /// </summary>
    internal class ObjectAssociationEndMapping : ObjectMemberMapping
    {
        /// <summary>
        ///     Constrcut a new AssociationEnd member mapping metadata object
        /// </summary>
        /// <param name="edmAssociationEnd"> </param>
        /// <param name="clrAssociationEnd"> </param>
        internal ObjectAssociationEndMapping(AssociationEndMember edmAssociationEnd, AssociationEndMember clrAssociationEnd)
            : base(edmAssociationEnd, clrAssociationEnd)
        {
        }

        /// <summary>
        ///     return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get { return MemberMappingKind.AssociationEndMapping; }
        }
    }
}
