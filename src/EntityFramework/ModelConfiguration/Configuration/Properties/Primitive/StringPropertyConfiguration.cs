// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

    /// <summary>
    /// Used to configure a <see cref="String" /> property of an entity type or
    /// complex type.
    /// </summary>
    public class StringPropertyConfiguration : LengthPropertyConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the property supports Unicode string
        /// content.
        /// </summary>
        public bool? IsUnicode { get; set; }

        /// <summary>
        /// Initializes a new instance of the StringPropertyConfiguration class.
        /// </summary>
        public StringPropertyConfiguration()
        {
        }

        private StringPropertyConfiguration(StringPropertyConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            IsUnicode = source.IsUnicode;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new StringPropertyConfiguration(this);
        }

        internal override void Configure(EdmProperty property)
        {
            base.Configure(property);

            if (IsUnicode != null)
            {
                property.PropertyType.PrimitiveTypeFacets.IsUnicode = IsUnicode;
            }
        }

        internal override void Configure(DbPrimitiveTypeFacets facets, FacetDescription facetDescription)
        {
            base.Configure(facets, facetDescription);

            switch (facetDescription.FacetName)
            {
                case SsdlConstants.Attribute_Unicode:
                    facets.IsUnicode = facetDescription.IsConstant ? null : IsUnicode ?? facets.IsUnicode;
                    break;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var strConfigRhs = other as StringPropertyConfiguration;
            if (strConfigRhs != null)
            {
                IsUnicode = strConfigRhs.IsUnicode;
            }
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as StringPropertyConfiguration;
            if (strConfigRhs != null
                && IsUnicode == null)
            {
                IsUnicode = strConfigRhs.IsUnicode;
            }
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var stringRhs = other as StringPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var isUnicodeIsCompatible = stringRhs == null || IsCompatible(c => c.IsUnicode, stringRhs, ref errorMessage);

            return baseIsCompatible &&
                   isUnicodeIsCompatible;
        }
    }
}
