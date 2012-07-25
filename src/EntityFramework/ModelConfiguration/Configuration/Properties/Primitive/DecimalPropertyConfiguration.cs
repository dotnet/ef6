// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

    internal class DecimalPropertyConfiguration : PrimitivePropertyConfiguration
    {
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }

        public DecimalPropertyConfiguration()
        {
        }

        private DecimalPropertyConfiguration(DecimalPropertyConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            Precision = source.Precision;
            Scale = source.Scale;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new DecimalPropertyConfiguration(this);
        }

        internal override void Configure(EdmProperty property)
        {
            base.Configure(property);

            if (Precision != null)
            {
                property.PropertyType.PrimitiveTypeFacets.Precision = Precision;
            }

            if (Scale != null)
            {
                property.PropertyType.PrimitiveTypeFacets.Scale = Scale;
            }
        }

        internal override void Configure(DbPrimitiveTypeFacets facets, FacetDescription facetDescription)
        {
            base.Configure(facets, facetDescription);

            switch (facetDescription.FacetName)
            {
                case SsdlConstants.Attribute_Precision:
                    facets.Precision = facetDescription.IsConstant ? null : Precision ?? facets.Precision;
                    break;
                case SsdlConstants.Attribute_Scale:
                    facets.Scale = facetDescription.IsConstant ? null : Scale ?? facets.Scale;
                    break;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var lenConfigRhs = other as DecimalPropertyConfiguration;
            if (lenConfigRhs != null)
            {
                Precision = lenConfigRhs.Precision;
                Scale = lenConfigRhs.Scale;
            }
        }

        public override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var lenConfigRhs = other as DecimalPropertyConfiguration;
            if (lenConfigRhs != null)
            {
                if (Precision == null)
                {
                    Precision = lenConfigRhs.Precision;
                }
                if (Scale == null)
                {
                    Scale = lenConfigRhs.Scale;
                }
            }
        }

        public override bool IsCompatible(PrimitivePropertyConfiguration other, bool InCSpace, out string errorMessage)
        {
            var decRhs = other as DecimalPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, InCSpace, out errorMessage);
            var precisionIsCompatible = decRhs == null || IsCompatible(c => c.Precision, decRhs, ref errorMessage);
            var scaleIsCompatible = decRhs == null || IsCompatible(c => c.Scale, decRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible &&
                   scaleIsCompatible;
        }
    }
}
