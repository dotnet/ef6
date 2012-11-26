// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Mapping metadata for all OC member maps.
    /// </summary>
    internal class ObjectNavigationPropertyMapping : ObjectMemberMapping
    {
        /// <summary>
        ///     Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmNavigationProperty"> </param>
        /// <param name="clrNavigationProperty"> </param>
        internal ObjectNavigationPropertyMapping(NavigationProperty edmNavigationProperty, NavigationProperty clrNavigationProperty)
            :
                base(edmNavigationProperty, clrNavigationProperty)
        {
        }

        /// <summary>
        ///     return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get { return MemberMappingKind.NavigationPropertyMapping; }
        }
    }
}
