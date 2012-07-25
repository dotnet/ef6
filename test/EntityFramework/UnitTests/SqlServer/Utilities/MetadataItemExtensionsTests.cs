// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class MetadataItemExtensionsTests
    {
        [Fact]
        public void GetMetadataPropertyValue_returns_the_value_of_the_property_with_the_given_name()
        {
            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.Name).Returns("DefiningQuery");
            mockProperty.Setup(m => m.Identity).Returns("DefiningQuery");
            mockProperty.Setup(m => m.Value).Returns("I am defined.");

            var mockSet = new Mock<EntitySetBase>();
            mockSet.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockProperty.Object }));

            Assert.Equal("I am defined.", mockSet.Object.GetMetadataPropertyValue<string>("DefiningQuery"));
        }

        [Fact]
        public void GetMetadataPropertyValue_returns_null_if_the_property_is_not_set()
        {
            var mockSet = new Mock<EntitySetBase>();
            mockSet.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new MetadataProperty[0]));

            Assert.Equal(null, mockSet.Object.GetMetadataPropertyValue<string>("DefiningQuery"));
        }
    }
}