// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class StucturalTypeTests
    {
        [Fact]
        public void Can_remove_member()
        {
            var entityType = new EntityType();

            Assert.Empty(entityType.Members);

            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);

            Assert.Equal(1, entityType.Members.Count);

            entityType.RemoveMember(property);

            Assert.Empty(entityType.Members);
        }
    }
}
