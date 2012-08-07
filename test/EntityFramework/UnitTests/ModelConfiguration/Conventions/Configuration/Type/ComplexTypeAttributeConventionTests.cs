// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;

    public sealed class ComplexTypeAttributeConventionTests
    {
        [Fact]
        public void Apply_should_inline_type()
        {
            var mockType = new MockType();
            var modelConfiguration = new ModelConfiguration();

            new ComplexTypeAttributeConvention.ComplexTypeAttributeConventionImpl()
                .Apply(mockType, modelConfiguration, new ComplexTypeAttribute());

            Assert.True(modelConfiguration.IsComplexType(mockType));
        }
    }
}
