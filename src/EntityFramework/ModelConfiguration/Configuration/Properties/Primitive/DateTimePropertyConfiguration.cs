namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

    internal class DateTimePropertyConfiguration : PrimitivePropertyConfiguration
    {
        public byte? Precision { get; set; }

        public DateTimePropertyConfiguration()
        {
        }

        private DateTimePropertyConfiguration(DateTimePropertyConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            Precision = source.Precision;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new DateTimePropertyConfiguration(this);
        }

        internal override void Configure(EdmProperty property)
        {
            base.Configure(property);

            if (Precision != null)
            {
                property.PropertyType.PrimitiveTypeFacets.Precision = Precision;
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
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var strConfigRhs = other as DateTimePropertyConfiguration;
            if (strConfigRhs != null)
            {
                Precision = strConfigRhs.Precision;
            }
        }

        public override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as DateTimePropertyConfiguration;
            if (strConfigRhs != null
                && Precision == null)
            {
                Precision = strConfigRhs.Precision;
            }
        }

        public override bool IsCompatible(PrimitivePropertyConfiguration other, bool InCSpace, out string errorMessage)
        {
            var dateRhs = other as DateTimePropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, InCSpace, out errorMessage);
            var precisionIsCompatible = dateRhs == null || IsCompatible(c => c.Precision, dateRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible;
        }
    }
}
