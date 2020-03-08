// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class MemberCollectionTests
    {
        [Fact]
        public void Item_setter_sets_member_of_base_type()
        {
            var baseEntityType = new EntityType("B", "N", DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            entityType.BaseType = baseEntityType;

            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);
            var property1 = EdmProperty.CreatePrimitive("P1", primitiveType);
            var property2 = EdmProperty.CreatePrimitive("P2", primitiveType);
            var property3 = EdmProperty.CreatePrimitive("P3", primitiveType);
            
            entityType.AddMember(property2);
            baseEntityType.AddMember(property1);

            Assert.Equal(1, baseEntityType.Members.Count);
            Assert.Equal(2, entityType.Members.Count);
            Assert.Equal(0, baseEntityType.Members.IndexOf(property1));
            Assert.Equal(0, entityType.Members.IndexOf(property1));
            Assert.Equal(1, entityType.Members.IndexOf(property2));

            entityType.Members.Source[0] = property3;

            Assert.False(baseEntityType.Members.Contains(property1));
            Assert.False(entityType.Members.Contains(property1));
            Assert.Equal(1, baseEntityType.Members.Count);
            Assert.Equal(2, entityType.Members.Count);
            Assert.Equal(0, baseEntityType.Members.IndexOf(property3));
            Assert.Equal(0, entityType.Members.IndexOf(property3));
            Assert.Equal(1, entityType.Members.IndexOf(property2));

            baseEntityType.Members.Source[0] = property1;

            Assert.False(baseEntityType.Members.Contains(property3));
            Assert.False(entityType.Members.Contains(property3));
            Assert.Equal(1, baseEntityType.Members.Count);
            Assert.Equal(2, entityType.Members.Count);
            Assert.Equal(0, baseEntityType.Members.IndexOf(property1));
            Assert.Equal(0, entityType.Members.IndexOf(property1));
            Assert.Equal(1, entityType.Members.IndexOf(property2));
        }
    }
}
