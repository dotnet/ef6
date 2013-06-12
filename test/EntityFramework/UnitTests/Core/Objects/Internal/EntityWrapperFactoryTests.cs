// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using Moq;
    using Xunit;

    public class EntityWrapperFactoryTests
    {
        [Fact]
        public void Factory_sets_override_flag_appropriately_for_IPOCO_EntityObject_entities()
        {
            var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new Mock<EntityObject>().Object, new EntityKey());
            Assert.Same(typeof(LightweightEntityWrapper<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

            wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new Mock<EntityObjectWithEquals>().Object, new EntityKey());
            Assert.Same(typeof(LightweightEntityWrapper<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
        }

        public class EntityObjectWithEquals : EntityObject
        {
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Fact]
        public void Factory_sets_override_flag_appropriately_for_entities_with_relationships()
        {
            var mockWithRelationships = new Mock<IEntityWithRelationships>();
            mockWithRelationships.Setup(m => m.RelationshipManager).Returns(RelationshipManager.Create(mockWithRelationships.Object));
            var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(mockWithRelationships.Object, new EntityKey());
            Assert.Same(typeof(EntityWrapperWithRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

            wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new Mock<EntityWithRelationshipsAndEquals>().Object, new EntityKey());
            Assert.Same(typeof(EntityWrapperWithRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
        }

        public class EntityWithRelationshipsAndEquals : IEntityWithRelationships
        {
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public RelationshipManager RelationshipManager
            {
                get { return RelationshipManager.Create(this); }
            }
        }

        [Fact]
        public void Factory_sets_override_flag_appropriately_for_pure_POCO_entities()
        {
            var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new object(), new EntityKey());
            Assert.Same(typeof(EntityWrapperWithoutRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

            wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new Mock<PocoWithEquals>().Object, new EntityKey());
            Assert.Same(typeof(EntityWrapperWithoutRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
        }

        public class PocoWithEquals
        {
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
