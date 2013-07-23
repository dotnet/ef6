// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public sealed class DecimalPropertyConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_decimal()
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal));

            (new DecimalPropertyConvention())
                .Apply(property, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal((byte)18, property.Precision);
            Assert.Equal((byte)2, property.Scale);
        }

        [Fact]
        public void Apply_should_not_set_defaults_for_configured_precision()
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal));
            property.Precision = 22;

            (new DecimalPropertyConvention())
                .Apply(property, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal((byte)22, property.Precision);
            Assert.Equal((byte)2, property.Scale);
        }

        [Fact]
        public void Apply_should_not_set_defaults_for_configured_scale()
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal));
            property.Scale = 4;

            (new DecimalPropertyConvention())
                .Apply(property, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal((byte)18, property.Precision);
            Assert.Equal((byte)4, property.Scale);
        }
    }
}
