// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Used to configure a <see cref="Decimal" /> property of an entity type or
    ///     complex type.
    /// </summary>
    public class DecimalPropertyConfiguration : PrimitivePropertyConfiguration
    {
        /// <summary>
        ///     Gets or sets the precision of the property.
        /// </summary>
        public byte? Precision { get; set; }

        /// <summary>
        ///     Gets or sets the scale of the property.
        /// </summary>
        public byte? Scale { get; set; }

        /// <summary>
        ///     Initializes a new instance of the DecimalPropertyConfiguration class.
        /// </summary>
        public DecimalPropertyConfiguration()
        {
        }

        private DecimalPropertyConfiguration(DecimalPropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

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
                property.Precision = Precision;
            }

            if (Scale != null)
            {
                property.Scale = Scale;
            }
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);

            switch (facetDescription.FacetName)
            {
                case XmlConstants.PrecisionElement:
                    column.Precision = facetDescription.IsConstant ? null : Precision ?? column.Precision;
                    break;
                case XmlConstants.ScaleElement:
                    column.Scale = facetDescription.IsConstant ? null : Scale ?? column.Scale;
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

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
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

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var decRhs = other as DecimalPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var precisionIsCompatible = decRhs == null || IsCompatible(c => c.Precision, decRhs, ref errorMessage);
            var scaleIsCompatible = decRhs == null || IsCompatible(c => c.Scale, decRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible &&
                   scaleIsCompatible;
        }
    }
}
