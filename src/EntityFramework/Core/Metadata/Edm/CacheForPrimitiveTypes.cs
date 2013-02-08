// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Diagnostics;

    internal class CacheForPrimitiveTypes
    {
        // The primitive type kind is a list of enum which the EDM model 
        // Every specific instantiation of the model should map their 
        // primitive types to the edm primitive types.

        // In this class, primitive type is to be cached

        // Key for the cache: primitive type kind
        // Value for the cache: List<PrimitiveType>.  A list is used because there an be multiple types mapping to the
        // same primitive type kind.  For example, sqlserver has multiple string types.

        private readonly List<PrimitiveType>[] _primitiveTypeMap = new List<PrimitiveType>[EdmConstants.NumPrimitiveTypes];

        /// <summary>
        ///     Add the given primitive type to the primitive type cache
        /// </summary>
        /// <param name="type"> The primitive type to add </param>
        internal void Add(PrimitiveType type)
        {
            // Get to the list
            var primitiveTypes = EntityUtil.CheckArgumentOutOfRange(_primitiveTypeMap, (int)type.PrimitiveTypeKind, "primitiveTypeKind");

            // If there isn't a list for the given model type, create one and add it
            if (primitiveTypes == null)
            {
                primitiveTypes = new List<PrimitiveType>();
                primitiveTypes.Add(type);
                _primitiveTypeMap[(int)type.PrimitiveTypeKind] = primitiveTypes;
            }
            else
            {
                primitiveTypes.Add(type);
            }
        }

        /// <summary>
        ///     Try and get the mapped type for the given primitiveTypeKind in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind"> The primitive type kind of the primitive type to retrieve </param>
        /// <param name="facets"> The facets to use in picking the primitive type </param>
        /// <param name="type"> The resulting type </param>
        /// <returns> Whether a type was retrieved or not </returns>
        internal bool TryGetType(PrimitiveTypeKind primitiveTypeKind, IEnumerable<Facet> facets, out PrimitiveType type)
        {
            type = null;

            // Now, see if we have any types for this model type, if so, loop through to find the best matching one
            var primitiveTypes = EntityUtil.CheckArgumentOutOfRange(_primitiveTypeMap, (int)primitiveTypeKind, "primitiveTypeKind");
            if ((null != primitiveTypes)
                && (0 < primitiveTypes.Count))
            {
                if (primitiveTypes.Count == 1)
                {
                    type = primitiveTypes[0];
                    return true;
                }

                if (facets == null)
                {
                    var facetDescriptions = EdmProviderManifest.GetInitialFacetDescriptions(primitiveTypeKind);
                    if (facetDescriptions == null)
                    {
                        type = primitiveTypes[0];
                        return true;
                    }

                    Debug.Assert(facetDescriptions.Length > 0);
                    facets = CreateInitialFacets(facetDescriptions);
                }

                Debug.Assert(type == null, "type must be null here");
                var isMaxLengthSentinel = false;

                // Create a dictionary of facets for easy lookup
                foreach (var facet in facets)
                {
                    if ((primitiveTypeKind == PrimitiveTypeKind.String ||
                         primitiveTypeKind == PrimitiveTypeKind.Binary)
                        &&
                        facet.Value != null
                        &&
                        facet.Name == DbProviderManifest.MaxLengthFacetName
                        &&
                        Helper.IsUnboundedFacetValue(facet))
                    {
                        // MaxLength has the sentinel value. So this facet need not be added.
                        isMaxLengthSentinel = true;
                        continue;
                    }
                }

                var maxLength = 0;
                // Find a primitive type with the matching constraint
                foreach (var primitiveType in primitiveTypes)
                {
                    if (isMaxLengthSentinel)
                    {
                        if (type == null)
                        {
                            type = primitiveType;
                            maxLength =
                                Helper.GetFacet(primitiveType.FacetDescriptions, DbProviderManifest.MaxLengthFacetName).MaxValue.Value;
                        }
                        else
                        {
                            var newMaxLength =
                                Helper.GetFacet(primitiveType.FacetDescriptions, DbProviderManifest.MaxLengthFacetName).MaxValue.Value;
                            if (newMaxLength > maxLength)
                            {
                                type = primitiveType;
                                maxLength = newMaxLength;
                            }
                        }
                    }
                    else
                    {
                        type = primitiveType;
                        break;
                    }
                }

                Debug.Assert(type != null);
                return true;
            }

            return false;
        }

        private static Facet[] CreateInitialFacets(FacetDescription[] facetDescriptions)
        {
            Debug.Assert(facetDescriptions != null && facetDescriptions.Length > 0);

            var facets = new Facet[facetDescriptions.Length];

            for (var i = 0; i < facetDescriptions.Length; ++i)
            {
                switch (facetDescriptions[i].FacetName)
                {
                    case DbProviderManifest.MaxLengthFacetName:
                        facets[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultMaxLengthFacetValue);
                        break;

                    case DbProviderManifest.UnicodeFacetName:
                        facets[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultUnicodeFacetValue);
                        break;

                    case DbProviderManifest.FixedLengthFacetName:
                        facets[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultFixedLengthFacetValue);
                        break;

                    case DbProviderManifest.PrecisionFacetName:
                        facets[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultPrecisionFacetValue);
                        break;

                    case DbProviderManifest.ScaleFacetName:
                        facets[i] = Facet.Create(facetDescriptions[i], TypeUsage.DefaultScaleFacetValue);
                        break;

                    default:
                        Debug.Assert(false, "Unexpected facet");
                        break;
                }
            }

            return facets;
        }

        /// <summary>
        ///     Get the list of the primitive types for the given dataspace
        /// </summary>
        /// <returns> </returns>
        internal ReadOnlyCollection<PrimitiveType> GetTypes()
        {
            var primitiveTypes = new List<PrimitiveType>();
            foreach (var types in _primitiveTypeMap)
            {
                if (null != types)
                {
                    primitiveTypes.AddRange(types);
                }
            }
            return primitiveTypes.AsReadOnly();
        }
    }
}
