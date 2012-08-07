// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Common;

    internal class FacetValues
    {
        private FacetValueContainer<bool?> _nullable;
        private FacetValueContainer<Int32?> _maxLength;
        private FacetValueContainer<bool?> _unicode;
        private FacetValueContainer<bool?> _fixedLength;
        private FacetValueContainer<byte?> _precision;
        private FacetValueContainer<byte?> _scale;

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

        internal bool TryGetFacet(FacetDescription description, out Facet facet)
        {
            if (description.FacetName
                == DbProviderManifest.NullableFacetName)
            {
                if (_nullable.HasValue)
                {
                    facet = Facet.Create(description, _nullable.GetValueAsObject());
                    return true;
                }
            }
            else if (description.FacetName
                     == DbProviderManifest.MaxLengthFacetName)
            {
                if (_maxLength.HasValue)
                {
                    facet = Facet.Create(description, _maxLength.GetValueAsObject());
                    return true;
                }
            }
            else if (description.FacetName
                     == DbProviderManifest.UnicodeFacetName)
            {
                if (_unicode.HasValue)
                {
                    facet = Facet.Create(description, _unicode.GetValueAsObject());
                    return true;
                }
            }
            else if (description.FacetName
                     == DbProviderManifest.FixedLengthFacetName)
            {
                if (_fixedLength.HasValue)
                {
                    facet = Facet.Create(description, _fixedLength.GetValueAsObject());
                    return true;
                }
            }
            else if (description.FacetName
                     == DbProviderManifest.PrecisionFacetName)
            {
                if (_precision.HasValue)
                {
                    facet = Facet.Create(description, _precision.GetValueAsObject());
                    return true;
                }
            }
            else if (description.FacetName
                     == DbProviderManifest.ScaleFacetName)
            {
                if (_scale.HasValue)
                {
                    facet = Facet.Create(description, _scale.GetValueAsObject());
                    return true;
                }
            }
            facet = null;
            return false;
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

                return values;
            }
        }
    }
}
