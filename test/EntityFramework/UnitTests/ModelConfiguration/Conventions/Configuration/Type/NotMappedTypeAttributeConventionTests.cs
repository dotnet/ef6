// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Xunit;

    public sealed class NotMappedTypeAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_ignore_type()
        {
            var mockType = new MockType();
            var modelConfiguration = new ModelConfiguration();

            new NotMappedTypeAttributeConvention()
                .Apply(new LightweightTypeConfiguration(mockType, modelConfiguration), new NotMappedAttribute());

            Assert.True(modelConfiguration.IsIgnoredType(mockType));
        }
    }
}
