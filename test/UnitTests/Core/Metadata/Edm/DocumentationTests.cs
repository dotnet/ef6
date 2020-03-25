// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class DocumentationTests
    {
        [Fact]
        public void Can_create_instance_and_retrieve_properties()
        {
            var documentation = new Documentation("summary", "long description");

            Assert.Equal("summary", documentation.Summary);
            Assert.Equal("long description", documentation.LongDescription);
        }

        [Fact]
        public void Can_create_empty_instance()
        {
            var documentation = new Documentation(String.Empty, String.Empty);

            Assert.True(documentation.IsEmpty);

            documentation = new Documentation(null, null);

            Assert.True(documentation.IsEmpty);
            Assert.Equal(String.Empty, documentation.Summary);
            Assert.Equal(String.Empty, documentation.LongDescription);
        }
    }
}
