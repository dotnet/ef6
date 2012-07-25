// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class Helper
    {
        /// <summary>
        /// Searches for Facet Description with the name specified. 
        /// </summary>
        /// <param name="facetCollection">Collection of facet description</param>
        /// <param name="facetName">name of the facet</param>
        /// <returns></returns>
        internal static FacetDescription GetFacet(IEnumerable<FacetDescription> facetCollection, string facetName)
        {
            foreach (var facetDescription in facetCollection)
            {
                if (facetDescription.FacetName == facetName)
                {
                    return facetDescription;
                }
            }

            return null;
        }

        internal static bool IsUnboundedFacetValue(Facet facet)
        {
            // TODO: vamshikb
            // Use return object.ReferenceEquals(facet.Value, EdmConstants.UnboundedValue);
            // when EdmConstants.UnboundedValue is made public.
            return (null == facet.Value || facet.IsUnbounded);
        }
    }
}
