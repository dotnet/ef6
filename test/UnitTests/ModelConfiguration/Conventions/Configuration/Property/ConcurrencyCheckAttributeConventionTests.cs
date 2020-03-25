// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;
    using PrimitivePropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;

    public sealed class ConcurrencyCheckAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_concurrency_token()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new ConcurrencyCheckAttributeConvention()
                .Apply(
                    new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new ConcurrencyCheckAttribute());

            Assert.Equal(ConcurrencyMode.Fixed, propertyConfiguration.ConcurrencyMode);
        }

        [Fact]
        public void Apply_should_ignore_attribute_if_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration
                {
                    ConcurrencyMode = ConcurrencyMode.None
                };

            new ConcurrencyCheckAttributeConvention()
                .Apply(
                    new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new ConcurrencyCheckAttribute());

            Assert.Equal(ConcurrencyMode.None, propertyConfiguration.ConcurrencyMode);
        }
    }
}
