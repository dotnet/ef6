// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;

    public static class FacetValuesHelpers
    {
        internal static bool Equal<TValue>(FacetValues leftValues, FacetValues rightValues)
        {
            return Equal<string>(leftValues, rightValues, DbProviderManifest.CollationFacetName)
                   && Equal<CollectionKind>(leftValues, rightValues, EdmConstants.CollectionKind)
                   && Equal<ConcurrencyMode>(leftValues, rightValues, EdmProviderManifest.ConcurrencyModeFacetName)
                   && Equal<TValue>(leftValues, rightValues, DbProviderManifest.DefaultValueFacetName)
                   && Equal<bool>(leftValues, rightValues, DbProviderManifest.FixedLengthFacetName)
                   && Equal<bool>(leftValues, rightValues, DbProviderManifest.IsStrictFacetName)
                   && Equal<int>(leftValues, rightValues, DbProviderManifest.MaxLengthFacetName)
                   && Equal<bool>(leftValues, rightValues, DbProviderManifest.NullableFacetName)
                   && Equal<bool>(leftValues, rightValues, DbProviderManifest.UnicodeFacetName)
                   && Equal<byte>(leftValues, rightValues, DbProviderManifest.PrecisionFacetName)
                   && Equal<byte>(leftValues, rightValues, DbProviderManifest.ScaleFacetName)
                   && Equal<int>(leftValues, rightValues, DbProviderManifest.SridFacetName)
                   && Equal<StoreGeneratedPattern>(leftValues, rightValues, EdmProviderManifest.StoreGeneratedPatternFacetName);
        }

        private static bool Equal<T>(FacetValues leftValues, FacetValues rightValues, string facetName)
        {
            var description = FacetDescriptionHelpers.GetFacetDescription<T>(facetName);

            Facet leftFacet = null;
            var leftHasValue = leftValues.TryGetFacet(description, out leftFacet);
            Facet rightFacet = null;
            var rightHasValue = rightValues.TryGetFacet(description, out rightFacet);

            return (leftHasValue && rightHasValue
                    && EqualityComparer<T>.Default.Equals((T)leftFacet.Value, (T)rightFacet.Value))
                   || (!leftHasValue && !rightHasValue);
        }
    }
}
