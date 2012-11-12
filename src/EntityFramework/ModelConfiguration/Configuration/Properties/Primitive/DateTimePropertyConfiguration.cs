// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Used to configure a <see cref="DateTime" /> property of an entity type or
    ///     complex type.
    /// </summary>
    public class DateTimePropertyConfiguration : PrimitivePropertyConfiguration
    {
        /// <summary>
        ///     Gets or sets the precision of the property.
        /// </summary>
        public byte? Precision { get; set; }

        /// <summary>
        ///     Initializes a new instance of the DateTimePropertyConfiguration class.
        /// </summary>
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
                property.Precision = Precision;
            }
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);

            switch (facetDescription.FacetName)
            {
                case SsdlConstants.Attribute_Precision:
                    column.Precision = facetDescription.IsConstant ? null : Precision ?? column.Precision;
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

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as DateTimePropertyConfiguration;
            if (strConfigRhs != null
                && Precision == null)
            {
                Precision = strConfigRhs.Precision;
            }
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var dateRhs = other as DateTimePropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var precisionIsCompatible = dateRhs == null || IsCompatible(c => c.Precision, dateRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible;
        }
    }
}
