// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using Xunit;

    public sealed class ConcurrencyCheckAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_concurrency_token()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new ConcurrencyCheckAttributeConvention.ConcurrencyCheckAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ConcurrencyCheckAttribute());

            Assert.Equal(EdmConcurrencyMode.Fixed, propertyConfiguration.ConcurrencyMode);
        }

        [Fact]
        public void Apply_should_ignore_attribute_if_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration
                                            {
                                                ConcurrencyMode = EdmConcurrencyMode.None
                                            };

            new ConcurrencyCheckAttributeConvention.ConcurrencyCheckAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ConcurrencyCheckAttribute());

            Assert.Equal(EdmConcurrencyMode.None, propertyConfiguration.ConcurrencyMode);
        }
    }
}
