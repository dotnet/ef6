// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class ComplexTypeTests
    {
        [Fact]
        public void Properties_list_should_be_live_on_reread()
        {
            var complexType = new ComplexType("C");

            Assert.Empty(complexType.Properties);

            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexType.AddMember(property);

            Assert.Equal(1, complexType.Properties.Count);
        }
    }
}
