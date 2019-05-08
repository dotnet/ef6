// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using Moq;
    using Xunit;

    public class EntityWrapperFactoryTests
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(EntityProxyFactory.GetInterceptorDelegateMethod);
        }

        [Fact]
        public void Factory_sets_override_flag_appropriately_for_IPOCO_EntityObject_entities()
        {
            var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new EntityObjectWithoutEquals(), new EntityKey());
            Assert.Same(typeof(LightweightEntityWrapper<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

            wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new EntityObjectWithEquals(), new EntityKey());
            Assert.Same(typeof(LightweightEntityWrapper<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
        }

        public class EntityObjectWithoutEquals : EntityObject
        {
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
            var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new EntityWithRelationshipsButWithoutEquals(), new EntityKey());
            Assert.Same(typeof(EntityWrapperWithRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

            wrappedEntity = EntityWrapperFactory.CreateNewWrapper(new EntityWithRelationshipsAndEquals(), new EntityKey());
            Assert.Same(typeof(EntityWrapperWithRelationships<>), wrappedEntity.GetType().GetGenericTypeDefinition());
            Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
        }

        public class EntityWithRelationshipsButWithoutEquals : IEntityWithRelationships
        {
            public EntityWithRelationshipsButWithoutEquals()
            {
                RelationshipManager = RelationshipManager.Create(this);
            }

            public RelationshipManager RelationshipManager { get; }
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
