// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal class FacetValues
    {
        private FacetValueContainer<bool?> _nullable;
        private FacetValueContainer<Int32?> _maxLength;
        private FacetValueContainer<bool?> _unicode;
        private FacetValueContainer<bool?> _fixedLength;
        private FacetValueContainer<byte?> _precision;
        private FacetValueContainer<byte?> _scale;
        private object _defaultValue;
        private FacetValueContainer<string> _collation;
        private FacetValueContainer<int?> _srid;
        private FacetValueContainer<bool?> _isStrict;
        private FacetValueContainer<StoreGeneratedPattern?> _storeGeneratedPattern;
        private FacetValueContainer<ConcurrencyMode?> _concurrencyMode;
        private FacetValueContainer<CollectionKind?> _collectionKind;

        internal FacetValueContainer<bool?> Nullable
        {
            set { _nullable = value; }
        }

        internal FacetValueContainer<Int32?> MaxLength
        {
            set { _maxLength = value; }
        }

        internal FacetValueContainer<bool?> Unicode
        {
            set { _unicode = value; }
        }

        internal FacetValueContainer<bool?> FixedLength
        {
            set { _fixedLength = value; }
        }

        internal FacetValueContainer<byte?> Precision
        {
            set { _precision = value; }
        }

        internal FacetValueContainer<byte?> Scale
        {
            set { _scale = value; }
        }

        internal object DefaultValue
        {
            set { _defaultValue = value; }
        }

        internal FacetValueContainer<string> Collation
        {
            set { _collation = value; }
        }

        internal FacetValueContainer<int?> Srid
        {
            set { _srid = value; }
        }

        internal FacetValueContainer<bool?> IsStrict
        {
            set { _isStrict = value; }
        }

        internal FacetValueContainer<StoreGeneratedPattern?> StoreGeneratedPattern
        {
            set { _storeGeneratedPattern = value; }
        }

        internal FacetValueContainer<ConcurrencyMode?> ConcurrencyMode
        {
            set { _concurrencyMode = value; }
        }

        internal FacetValueContainer<CollectionKind?> CollectionKind
        {
            set { _collectionKind = value; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal bool TryGetFacet(FacetDescription description, out Facet facet)
        {
            switch (description.FacetName)
            {
                case DbProviderManifest.NullableFacetName:
                    if (_nullable.HasValue)
                    {
                        facet = Facet.Create(description, _nullable.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.MaxLengthFacetName:
                    if (_maxLength.HasValue)
                    {
                        facet = Facet.Create(description, _maxLength.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.UnicodeFacetName:
                    if (_unicode.HasValue)
                    {
                        facet = Facet.Create(description, _unicode.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.FixedLengthFacetName:
                    if (_fixedLength.HasValue)
                    {
                        facet = Facet.Create(description, _fixedLength.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.PrecisionFacetName:
                    if (_precision.HasValue)
                    {
                        facet = Facet.Create(description, _precision.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.ScaleFacetName:
                    if (_scale.HasValue)
                    {
                        facet = Facet.Create(description, _scale.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.DefaultValueFacetName:
                    if (_defaultValue != null)
                    {
                        facet = Facet.Create(description, _defaultValue);
                        return true;
                    }
                    break;
                case DbProviderManifest.CollationFacetName:
                    if (_collation.HasValue)
                    {
                        facet = Facet.Create(description, _collation.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.SridFacetName:
                    if (_srid.HasValue)
                    {
                        facet = Facet.Create(description, _srid.GetValueAsObject());
                        return true;
                    }
                    break;
                case DbProviderManifest.IsStrictFacetName:
                    if (_isStrict.HasValue)
                    {
                        facet = Facet.Create(description, _isStrict.GetValueAsObject());
                        return true;
                    }
                    break;
                case EdmProviderManifest.StoreGeneratedPatternFacetName:
                    if (_storeGeneratedPattern.HasValue)
                    {
                        facet = Facet.Create(description, _storeGeneratedPattern.GetValueAsObject());
                        return true;
                    }
                    break;
                case EdmProviderManifest.ConcurrencyModeFacetName:
                    if (_concurrencyMode.HasValue)
                    {
                        facet = Facet.Create(description, _concurrencyMode.GetValueAsObject());
                        return true;
                    }
                    break;
                case EdmConstants.CollectionKind:
                    if (_collectionKind.HasValue)
                    {
                        facet = Facet.Create(description, _collectionKind.GetValueAsObject());
                        return true;
                    }
                    break;
                default:
                    Debug.Assert(false, "Unrecognized facet: " + description.FacetName);
                    break;
            }

            facet = null;
            return false;
        }

        public static FacetValues Create(IEnumerable<Facet> facets)
        {
            var facetValues = new FacetValues();
            foreach (var facet in facets)
            {
                var description = facet.Description;
                switch (description.FacetName)
                {
                    case DbProviderManifest.NullableFacetName:
                        facetValues.Nullable = (bool?)facet.Value;
                        break;
                    case DbProviderManifest.MaxLengthFacetName:
                        facetValues.MaxLength = (int?)facet.Value;
                        break;
                    case DbProviderManifest.UnicodeFacetName:
                        facetValues.Unicode = (bool?)facet.Value;
                        break;
                    case DbProviderManifest.FixedLengthFacetName:
                        facetValues.FixedLength = (bool?)facet.Value;
                        break;
                    case DbProviderManifest.PrecisionFacetName:
                        facetValues.Precision = (byte?)facet.Value;
                        break;
                    case DbProviderManifest.ScaleFacetName:
                        facetValues.Scale = (byte?)facet.Value;
                        break;
                    case DbProviderManifest.DefaultValueFacetName:
                        facetValues.DefaultValue = facet.Value;
                        break;
                    case DbProviderManifest.CollationFacetName:
                        facetValues.Collation = (string)facet.Value;
                        break;
                    case DbProviderManifest.SridFacetName:
                        facetValues.Srid = (int?)facet.Value;
                        break;
                    case DbProviderManifest.IsStrictFacetName:
                        facetValues.IsStrict = (bool?)facet.Value;
                        break;
                    case EdmProviderManifest.StoreGeneratedPatternFacetName:
                        facetValues.StoreGeneratedPattern = (StoreGeneratedPattern?)facet.Value;
                        break;
                    case EdmProviderManifest.ConcurrencyModeFacetName:
                        facetValues.ConcurrencyMode = (ConcurrencyMode?)facet.Value;
                        break;
                    case EdmConstants.CollectionKind:
                        facetValues.CollectionKind = (CollectionKind?)facet.Value;
                        break;
                    default:
                        Debug.Assert(false, "Unrecognized facet: " + description.FacetName);
                        break;
                }
            }

            return facetValues;
        }

        internal static FacetValues NullFacetValues
        {
            get
            {
                // null out everything except Nullable, and DefaultValue
                var values = new FacetValues();
                values.FixedLength = (bool?)null;
                values.MaxLength = (int?)null;
                values.Precision = (byte?)null;
                values.Scale = (byte?)null;
                values.Unicode = (bool?)null;
                values.Collation = (string)null;
                values.Srid = (int?)null;
                values.IsStrict = (bool?)null;
                values.ConcurrencyMode = (ConcurrencyMode?)null;
                values.StoreGeneratedPattern = (StoreGeneratedPattern?)null;
                values.CollectionKind = (CollectionKind?)null;

                return values;
            }
        }
    }
}
