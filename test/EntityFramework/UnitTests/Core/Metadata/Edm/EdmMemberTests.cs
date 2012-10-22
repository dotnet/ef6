// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EdmMemberTests
    {
        [Fact]
        public void Can_set_name_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            property.Name = "Foo";

            Assert.Equal("Foo", property.Name);
        }

        [Fact]
        public void IsPrimaryKeyColumn_should_return_true_when_parent_key_members_contains_member()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property = EdmProperty.Primitive("P", primitiveType);

            Assert.False(property.IsPrimaryKeyColumn);

            new EntityType().AddKeyMember(property);

            Assert.True(property.IsPrimaryKeyColumn);
        }
    }
}
