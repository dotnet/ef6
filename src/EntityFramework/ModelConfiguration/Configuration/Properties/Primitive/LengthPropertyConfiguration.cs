namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Data.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

    internal abstract class LengthPropertyConfiguration : PrimitivePropertyConfiguration
    {
        public bool? IsFixedLength { get; set; }
        public int? MaxLength { get; set; }
        public bool? IsMaxLength { get; set; }

        protected LengthPropertyConfiguration()
        {
        }

        protected LengthPropertyConfiguration(LengthPropertyConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            IsFixedLength = source.IsFixedLength;
            MaxLength = source.MaxLength;
            IsMaxLength = source.IsMaxLength;
        }

        internal override void Configure(EdmProperty property)
        {
            base.Configure(property);

            if (IsFixedLength != null)
            {
                property.PropertyType.PrimitiveTypeFacets.IsFixedLength = IsFixedLength;
            }

            if (MaxLength != null)
            {
                property.PropertyType.PrimitiveTypeFacets.MaxLength = MaxLength;
            }

            if (IsMaxLength != null)
            {
                property.PropertyType.PrimitiveTypeFacets.IsMaxLength = IsMaxLength;
            }
        }

        internal override void Configure(DbPrimitiveTypeFacets facets, FacetDescription facetDescription)
        {
            switch (facetDescription.FacetName)
            {
                case SsdlConstants.Attribute_FixedLength:
                    facets.IsFixedLength = facetDescription.IsConstant ? null : IsFixedLength ?? facets.IsFixedLength;
                    break;
                case SsdlConstants.Attribute_MaxLength:
                    facets.MaxLength = facetDescription.IsConstant ? null : MaxLength ?? facets.MaxLength;
                    facets.IsMaxLength = facetDescription.IsConstant ? null : IsMaxLength ?? facets.IsMaxLength;
                    break;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var lenConfigRhs = other as LengthPropertyConfiguration;
            if (lenConfigRhs != null)
            {
                IsFixedLength = lenConfigRhs.IsFixedLength;
                MaxLength = lenConfigRhs.MaxLength;
                IsMaxLength = lenConfigRhs.IsMaxLength;
            }
        }

        public override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var lenConfigRhs = other as LengthPropertyConfiguration;
            if (lenConfigRhs != null)
            {
                if (IsFixedLength == null)
                {
                    IsFixedLength = lenConfigRhs.IsFixedLength;
                }
                if (MaxLength == null)
                {
                    MaxLength = lenConfigRhs.MaxLength;
                }
                if (IsMaxLength == null)
                {
                    IsMaxLength = lenConfigRhs.IsMaxLength;
                }
            }
        }

        public override bool IsCompatible(PrimitivePropertyConfiguration other, bool InCSpace, out string errorMessage)
        {
            var lenRhs = other as LengthPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, InCSpace, out errorMessage);
            var isFixedLengthIsCompatible = lenRhs == null
                                            || IsCompatible(c => c.IsFixedLength, lenRhs, ref errorMessage);
            var isMaxLengthIsCompatible = lenRhs == null || IsCompatible(c => c.IsMaxLength, lenRhs, ref errorMessage);
            var maxLengthIsCompatible = lenRhs == null || IsCompatible(c => c.MaxLength, lenRhs, ref errorMessage);

            return baseIsCompatible &&
                   isFixedLengthIsCompatible &&
                   isMaxLengthIsCompatible &&
                   maxLengthIsCompatible;
        }
    }
}
