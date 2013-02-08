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
        public void Identity_uniquified_when_duplicate_name_set()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property1 = EdmProperty.Primitive("Foo", primitiveType);
            var property2 = EdmProperty.Primitive("Bar", primitiveType);

            var entityType = new EntityType("T","S",DataSpace.CSpace);

            entityType.AddMember(property1);
            entityType.AddMember(property2);

            property2.Name = "Foo";

            Assert.Equal("Foo1", property2.Identity);
            Assert.Equal(2, entityType.Properties.Count);
        }

        [Fact]
        public void Identity_synced_when_member_goes_readonly()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var property1 = EdmProperty.Primitive("Foo", primitiveType);
            var property2 = EdmProperty.Primitive("Bar", primitiveType);

            var entityType = new EntityType("T", "S", DataSpace.CSpace);

            entityType.AddMember(property1);
            entityType.AddMember(property2);

            property2.Name = "Foo";

            Assert.Equal("Foo1", property2.Identity);

            property2.SetReadOnly();

            Assert.Equal("Foo", property2.Identity);
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
