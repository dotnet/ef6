// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public sealed class EntityTypeExtensionsTests
    {
        [Fact]
        public void GetRootType_should_return_same_type_when_no_base_type()
        {
            var entityType = new EntityType();

            Assert.Same(entityType, entityType.GetRootType());
        }

        [Fact]
        public void GetRootType_should_return_base_type_when_has_base_type()
        {
            var entityType = new EntityType
                                 {
                                     BaseType = new EntityType()
                                 };

            Assert.Same(entityType.BaseType, entityType.GetRootType());
        }

        [Fact]
        public void GetPrimitiveProperties_should_return_only_primitive_properties()
        {
            var entityType = new EntityType();
            var property1 = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            entityType.AddComplexProperty("Bar", new ComplexType("C"));

            Assert.Equal(1, entityType.GetDeclaredPrimitiveProperties().Count());
            Assert.True(entityType.GetDeclaredPrimitiveProperties().Contains(property));
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var entityType = new EntityType();

            entityType.Annotations.SetConfiguration(42);

            Assert.Equal(42, entityType.GetConfiguration());
        }

        [Fact]
        public void Can_get_and_set_clr_type_annotation()
        {
            var entityType = new EntityType();

            Assert.Null(entityType.GetClrType());

            var type = typeof(object);

            entityType.Annotations.SetClrType(type);

            Assert.Equal(typeof(object), entityType.GetClrType());
        }

        [Fact]
        public void AddComplexProperty_should_create_and_add_complex_property()
        {
            var entityType = new EntityType();

            var complexType = new ComplexType("C");
            var property = entityType.AddComplexProperty("Foo", complexType);

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.Same(complexType, property.ComplexType);
            Assert.True(entityType.DeclaredProperties.Contains(property));
        }

        [Fact]
        public void AddNavigationProperty_should_create_and_add_navigation_property()
        {
            var entityType = new EntityType();
            var associationType
                = new AssociationType
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType()),
                          TargetEnd = new AssociationEndMember("T", new EntityType().GetReferenceType(), RelationshipMultiplicity.Many)
                      };

            var navigationProperty = entityType.AddNavigationProperty("N", associationType);

            Assert.NotNull(navigationProperty);
            Assert.Equal("N", navigationProperty.Name);
            Assert.Same(associationType, navigationProperty.Association);
            Assert.Equal(BuiltInTypeKind.CollectionType, navigationProperty.TypeUsage.EdmType.BuiltInTypeKind);
            Assert.Same(associationType.SourceEnd, navigationProperty.FromEndMember);
            Assert.Same(associationType.TargetEnd, navigationProperty.ToEndMember);
            Assert.True(entityType.NavigationProperties.Contains(navigationProperty));
        }

        [Fact]
        public void AddPrimitiveProperty_should_create_and_add_to_declared_properties()
        {
            var entityType = new EntityType();

            var property1 = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.True(entityType.DeclaredProperties.Contains(property));
        }

        [Fact]
        public void GetPrimitiveProperty_should_return_correct_property()
        {
            var entityType = new EntityType();
            var property1 = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            var foundProperty = entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void GetNavigationProperty_should_return_correct_property()
        {
            var entityType = new EntityType();
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            var property = entityType.AddNavigationProperty("Foo", associationType);

            var foundProperty = entityType.NavigationProperties.SingleOrDefault(np => np.Name == "Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void TypeHierarchyIterator_should_return_entity_types_in_depth_first_order()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityTypeRoot = model.AddEntityType("Root");
            var derivedType1 = model.AddEntityType("DType1");
            var derivedType2 = model.AddEntityType("DType2");
            derivedType1.BaseType = entityTypeRoot;
            derivedType2.BaseType = entityTypeRoot;
            var derivedType1_1 = model.AddEntityType("DType1_1");
            var derivedType1_2 = model.AddEntityType("DType1_2");
            derivedType1_1.BaseType = derivedType1;
            derivedType1_2.BaseType = derivedType1;

            var typesVisited = new List<EntityType>();
            foreach (var derivedType in entityTypeRoot.TypeHierarchyIterator(model))
            {
                typesVisited.Add(derivedType);
            }

            var oracle = new List<EntityType>
                             {
                                 entityTypeRoot,
                                 derivedType1,
                                 derivedType1_1,
                                 derivedType1_2,
                                 derivedType2
                             };
            Assert.Equal(true, oracle.SequenceEqual(typesVisited));
        }

        [Fact]
        public void IsAncestorOf_should_return_correct_answer()
        {
            var entityType1 = new EntityType
                                  {
                                      Name = "E1"
                                  };
            var entityType2 = new EntityType
                                  {
                                      Name = "E2"
                                  };
            entityType2.BaseType = entityType1;
            var entityType3 = new EntityType
                                  {
                                      Name = "E3"
                                  };
            entityType3.BaseType = entityType1;
            var entityType4 = new EntityType
                                  {
                                      Name = "E4"
                                  };
            entityType4.BaseType = entityType2;

            Assert.True(entityType1.IsAncestorOf(entityType4));
            Assert.False(entityType3.IsAncestorOf(entityType4));
        }
    }
}
