// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Moq;
    using Xunit;

    public sealed class KeyAttributeConventionTests
    {
        [Fact]
        public void Apply_should_find_single_key()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "Id");
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(object));

            new KeyAttributeConvention()
                .Apply(mockPropertyInfo, mockEntityTypeConfiguration.Object, new ModelConfiguration(), new KeyAttribute());

            mockEntityTypeConfiguration.Verify(e => e.Key(mockPropertyInfo, null, true));
        }
    }
}
