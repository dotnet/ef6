// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using Moq;
    using Xunit;

    public class EntityTypeTests
    {
        [Fact]
        public void Properties_collection_is_live_until_entity_goes_readonly()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.False(entityType.IsReadOnly);
            Assert.NotSame(entityType.Properties, entityType.Properties);

            entityType.SetReadOnly();

            Assert.Same(entityType.Properties, entityType.Properties);
        }

        [Fact]
        public void Can_add_and_remove_foreign_key_builders()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var mockForeignKeyBuilder = new Mock<ForeignKeyBuilder>();

            entityType.AddForeignKey(mockForeignKeyBuilder.Object);

            Assert.Same(mockForeignKeyBuilder.Object, entityType.ForeignKeyBuilders.Single());

            mockForeignKeyBuilder.Verify(fk => fk.SetOwner(entityType));

            entityType.RemoveForeignKey(mockForeignKeyBuilder.Object);

            mockForeignKeyBuilder.Verify(fk => fk.SetOwner(null));
        }

        [Fact]
        public void Can_get_list_of_declared_navigation_properties()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.DeclaredNavigationProperties);

            var property = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)));

            entityType.AddMember(property);

            Assert.Equal(1, entityType.DeclaredNavigationProperties.Count);

            entityType.RemoveMember(property);

            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            baseType.AddMember(property);

            entityType.BaseType = baseType;

            Assert.Empty(entityType.DeclaredNavigationProperties);
            Assert.Equal(1, entityType.Members.Count);
        }

        [Fact]
        public void Can_get_list_of_declared_properties()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.DeclaredProperties);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);

            Assert.Equal(1, entityType.DeclaredProperties.Count);

            entityType.RemoveMember(property);

            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            baseType.AddMember(property);

            entityType.BaseType = baseType;

            Assert.Empty(entityType.DeclaredProperties);
            Assert.Equal(1, entityType.Members.Count);
        }

        [Fact]
        public void Can_get_list_of_declared_members()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.DeclaredMembers);

            var property1 = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)));
            var property2 = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            entityType.AddMember(property2);

            Assert.Equal(2, entityType.DeclaredMembers.Count);

            entityType.RemoveMember(property1);
            entityType.RemoveMember(property2);

            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            baseType.AddMember(property1);
            baseType.AddMember(property2);

            entityType.BaseType = baseType;

            Assert.Empty(entityType.DeclaredMembers);
            Assert.Equal(2, entityType.Members.Count);
        }

        [Fact]
        public void Properties_list_should_be_live_on_reread()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.Properties);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);

            Assert.Equal(1, entityType.Properties.Count);
        }

        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var entity =
                EntityType.Create(
                    "Customer",
                    "MyModel",
                    DataSpace.CSpace,
                    new[] { "Id" },
                    new[]
                        {
                            EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                            EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                        },
                    new[]
                        {
                            new MetadataProperty(
                                "TestProperty",
                                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                                "value"),
                        });

            Assert.Equal("MyModel.Customer", entity.FullName);
            Assert.Equal(DataSpace.CSpace, entity.DataSpace);
            Assert.True(new [] { "Id"}.SequenceEqual(entity.KeyMemberNames));
            Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
            Assert.True(entity.IsReadOnly);

            var metadataProperty = entity.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public void Declared_members_should_be_accessible_by_name()
        {
            var entityType =
                EntityType.Create(
                    "Blog",
                    "BlogModel",
                    DataSpace.CSpace,
                    new[] { "Id", "Title" },
                    new[] {
                        EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                        EdmProperty.CreatePrimitive("Title", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))},
                    null);

            Assert.NotNull(entityType.DeclaredMembers["Id"]);
            Assert.NotNull(entityType.DeclaredMembers["Title"]);
        }

        [Fact]
        public void NavigationProperties_is_thread_safe()
        {

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var property = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)));

            entityType.AddMember(property);

            const int cycles = 100;
            const int threadCount = 30;

            Action readNavigationProperties = () =>
            {
                for (var i = 0; i < cycles; ++i)
                {
                    var navProps = entityType.NavigationProperties;

                    //touching Members.Source triggers a reset to NavigationProperties
                    var sourceCount = entityType.Members.Source.Count;
                    Assert.True(sourceCount == 1);

                    var navigationPropertiesAfterReset = entityType.NavigationProperties;

                    Assert.True(navProps != null, "First reference to NavigationProperties should not be null");
                    Assert.True(navigationPropertiesAfterReset != null, "Second reference to NavigationProperties should not be null");
                    Assert.False(ReferenceEquals(navProps, navigationPropertiesAfterReset), "The NavigationProperties instances should be different");
                }
            };

            var tasks = new List<Thread>();
            for (var i = 0; i < threadCount; ++i)
            {
                tasks.Add(new Thread(new ThreadStart(readNavigationProperties)));
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Join());
        }

        [Fact]
        public void Create_entity_type_having_base_type_without_key_members()
        {
            var baseType =
                EntityType.Create(
                    "Base",
                    "MyModel",
                    DataSpace.CSpace,
                    null,
                    null,
                    null);

            var entity =
                EntityType.Create(
                    "Derived",
                    "MyModel",
                    DataSpace.CSpace,
                    baseType,
                    new[] { "Id" },
                    new[]
                    {
                        EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                        EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    },
                    new[]
                    {
                        new MetadataProperty(
                            "TestProperty",
                            TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                            "value")
                    });

            Assert.Same(baseType, entity.BaseType);
            Assert.Equal("MyModel.Derived", entity.FullName);
            Assert.Equal(DataSpace.CSpace, entity.DataSpace);
            Assert.True(new[] { "Id" }.SequenceEqual(entity.KeyMemberNames));
            Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
            Assert.True(entity.IsReadOnly);

            var metadataProperty = entity.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public void Create_entity_type_having_base_type_with_key_members()
        {
            var baseType =
                EntityType.Create(
                    "Base",
                    "MyModel",
                    DataSpace.CSpace,
                    new[] { "Id" },
                    new[]
                    {
                        EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                    },
                    null);

            var entity =
                EntityType.Create(
                    "Derived",
                    "MyModel",
                    DataSpace.CSpace,
                    baseType,
                    null,
                    new[]
                    {
                        EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    },
                    new[]
                    {
                        new MetadataProperty(
                            "TestProperty",
                            TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                            "value")
                    });

            Assert.Same(baseType, entity.BaseType);
            Assert.Equal("MyModel.Derived", entity.FullName);
            Assert.Equal(DataSpace.CSpace, entity.DataSpace);
            Assert.True(new[] { "Id" }.SequenceEqual(entity.KeyMemberNames));
            Assert.True(new[] { "Id", "Name" }.SequenceEqual(entity.Members.Select(m => m.Name)));
            Assert.True(entity.IsReadOnly);

            var metadataProperty = entity.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public void Create_entity_type_throws_if_key_members_defined_on_both_base_and_derived_types()
        {
            var baseType =
                EntityType.Create(
                    "Base",
                    "MyModel",
                    DataSpace.CSpace,
                    new[] { "Id" },
                    new[]
                    {
                        EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                    },
                    null);

            Assert.Equal(
                Strings.CannotDefineKeysOnBothBaseAndDerivedTypes,
                Assert.Throws<ArgumentException>(
                    () =>
                        EntityType.Create(
                            "Derived",
                            "MyModel",
                            DataSpace.CSpace,
                            baseType,
                            new[] { "Id" },
                            new[]
                            {
                                EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                EdmProperty.CreatePrimitive("Name", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                            },
                            null))
                    .Message);
        }

        [Fact]
        public void BaseType_setter_throws_if_entity_type_and_base_type_are_the_same()
        {
            var entity = new EntityType("ET", "MyModel", DataSpace.CSpace);

            Assert.Equal(
                Strings.CannotSetBaseTypeCyclicInheritance("ET", "ET"),
                Assert.Throws<ArgumentException>(
                    () => entity.BaseType = entity)
                    .Message);
        }

        [Fact]
        public void BaseType_setter_throws_if_cyclic_inheritance()
        {
            var entity1 = new EntityType("ET1", "MyModel", DataSpace.CSpace);
            var entity2 = new EntityType("ET2", "MyModel", DataSpace.CSpace);
            var entity3 = new EntityType("ET3", "MyModel", DataSpace.CSpace);
            entity2.BaseType = entity1;
            entity3.BaseType = entity2;

            Assert.Equal(
                Strings.CannotSetBaseTypeCyclicInheritance("ET3", "ET1"),
                Assert.Throws<ArgumentException>(
                    () => entity1.BaseType = entity3)
                    .Message);
        }
    }
}
