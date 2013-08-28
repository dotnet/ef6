// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class ComplexTypeMappingTests
    {
        [Fact]
        public void Can_add_and_remove_properties()
        {
            var complexTypeMapping = new ComplexTypeMapping(false);
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C"));

            Assert.Empty(complexTypeMapping.Properties);

            complexTypeMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, complexTypeMapping.Properties.Single());

            complexTypeMapping.RemoveProperty(scalarPropertyMapping);

            Assert.Empty(complexTypeMapping.Properties);
        }
    }
}
