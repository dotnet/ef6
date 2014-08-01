// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    // <summary>
    // Used to configure a <see cref="String" /> property of an entity type or
    // complex type.
    // </summary>
    internal class StringPropertyConfiguration : LengthPropertyConfiguration
    {
        // <summary>
        // Gets or sets a value indicating whether the property supports Unicode string
        // content.
        // </summary>
        public bool? IsUnicode { get; set; }

        // <summary>
        // Initializes a new instance of the StringPropertyConfiguration class.
        // </summary>
        public StringPropertyConfiguration()
        {
        }

        private StringPropertyConfiguration(StringPropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

            IsUnicode = source.IsUnicode;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new StringPropertyConfiguration(this);
        }

        protected override void ConfigureProperty(EdmProperty property)
        {
            base.ConfigureProperty(property);

            if (IsUnicode != null)
            {
                property.IsUnicode = IsUnicode;
            }
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);

            switch (facetDescription.FacetName)
            {
                case XmlConstants.UnicodeElement:
                    column.IsUnicode = facetDescription.IsConstant ? null : IsUnicode ?? column.IsUnicode;
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

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            base.MakeCompatibleWith(other, inCSpace);

            var stringPropertyConfiguration = other as StringPropertyConfiguration;

            if (stringPropertyConfiguration == null) return;
            if (stringPropertyConfiguration.IsUnicode != null) IsUnicode = null;
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
