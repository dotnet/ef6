// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;
    using System.Linq;

    public class EntityTypeBaseTests
    {
        [Fact]
        public void Can_remove_member()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.Members);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);

            Assert.Equal(1, entityType.KeyMembers.Count);
            Assert.Equal(1, entityType.Members.Count);

            entityType.RemoveMember(property);

            Assert.Empty(entityType.KeyMembers);
            Assert.Empty(entityType.Members);
        }

        [Fact]
        public void Can_get_list_of_key_properties()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.KeyProperties);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.Equal(1, entityType.KeyProperties.Where(key => entityType.DeclaredMembers.Contains(key)).Count());

            entityType.RemoveMember(property);

            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            baseType.AddKeyMember(property);

            entityType.BaseType = baseType;

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.Empty(entityType.KeyProperties.Where(key => entityType.DeclaredMembers.Contains(key)));
            Assert.Equal(1, entityType.KeyMembers.Count);
        }
    }
}
