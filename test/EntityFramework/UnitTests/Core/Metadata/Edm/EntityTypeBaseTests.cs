// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EntityTypeBaseTests
    {
        [Fact]
        public void Can_remove_member()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.Members);

            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);

            Assert.Equal(1, entityType.KeyMembers.Count);
            Assert.Equal(1, entityType.Members.Count);

            entityType.RemoveMember(property);

            Assert.Empty(entityType.KeyMembers);
            Assert.Empty(entityType.Members);
        }
    }
}
