namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Linq;
    using Xunit;

    public sealed class EdmEntityTypeExtensionsTests
    {
        [Fact]
        public void GetRootType_should_return_same_type_when_no_base_type()
        {
            var entityType = new EdmEntityType();

            Assert.Same(entityType, entityType.GetRootType());
        }

        [Fact]
        public void GetRootType_should_return_base_type_when_has_base_type()
        {
            var entityType = new EdmEntityType { BaseType = new EdmEntityType() };

            Assert.Same(entityType.BaseType, entityType.GetRootType());
        }

        [Fact]
        public void GetPrimitiveProperties_should_return_only_primitive_properties()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Foo");
            property.PropertyType.EdmType = EdmPrimitiveType.DateTime;
            entityType.AddComplexProperty("Bar", new EdmComplexType());

            Assert.Equal(1, entityType.GetDeclaredPrimitiveProperties().Count());
            Assert.True(entityType.GetDeclaredPrimitiveProperties().Contains(property));
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var entityType = new EdmEntityType();

            entityType.SetConfiguration(42);

            Assert.Equal(42, entityType.GetConfiguration());
        }

        [Fact]
        public void Can_get_and_set_clr_type_annotation()
        {
            var entityType = new EdmEntityType();

            Assert.Null(entityType.GetClrType());

            entityType.SetClrType(typeof(object));

            Assert.Equal(typeof(object), entityType.GetClrType());
        }

        [Fact]
        public void AddComplexProperty_should_create_and_add_complex_property()
        {
            var entityType = new EdmEntityType();

            var complexType = new EdmComplexType();
            var property = entityType.AddComplexProperty("Foo", complexType);

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.Same(complexType, property.PropertyType.ComplexType);
            Assert.True(entityType.DeclaredProperties.Contains(property));
        }

        [Fact]
        public void AddNavigationProperty_should_create_and_add_navigation_property()
        {
            var entityType = new EdmEntityType();
            var associationType = new EdmAssociationType();

            var navigationProperty = entityType.AddNavigationProperty("N", associationType);

            Assert.NotNull(navigationProperty);
            Assert.Equal("N", navigationProperty.Name);
            Assert.Same(associationType, navigationProperty.Association);
            Assert.True(entityType.NavigationProperties.Contains(navigationProperty));
        }

        [Fact]
        public void AddPrimitiveProperty_should_create_and_add_to_declared_properties()
        {
            var entityType = new EdmEntityType();

            var property = entityType.AddPrimitiveProperty("Foo");

            Assert.NotNull(property);
            Assert.Equal("Foo", property.Name);
            Assert.True(entityType.DeclaredProperties.Contains(property));
        }

        [Fact]
        public void GetPrimitiveProperty_should_return_correct_property()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Foo");
            property.PropertyType.EdmType = EdmPrimitiveType.Guid;

            var foundProperty = entityType.GetDeclaredPrimitiveProperty("Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void GetNavigationProperty_should_return_correct_property()
        {
            var entityType = new EdmEntityType();
            var associationType = new EdmAssociationType().Initialize();
            var property = entityType.AddNavigationProperty("Foo", associationType);

            var foundProperty = entityType.GetNavigationProperty("Foo");

            Assert.NotNull(foundProperty);
            Assert.Same(property, foundProperty);
        }

        [Fact]
        public void TypeHierarchyIterator_should_return_entity_types_in_depth_first_order()
        {
            var model = new EdmModel().Initialize();
            var entityTypeRoot = model.AddEntityType("Root");
            var derivedType1 = model.AddEntityType("DType1");
            var derivedType2 = model.AddEntityType("DType2");
            derivedType1.BaseType = entityTypeRoot;
            derivedType2.BaseType = entityTypeRoot;
            var derivedType1_1 = model.AddEntityType("DType1_1");
            var derivedType1_2 = model.AddEntityType("DType1_2");
            derivedType1_1.BaseType = derivedType1;
            derivedType1_2.BaseType = derivedType1;

            var typesVisited = new List<EdmEntityType>();
            foreach (var derivedType in entityTypeRoot.TypeHierarchyIterator(model))
            {
                typesVisited.Add(derivedType);
            }

            var oracle = new List<EdmEntityType> { entityTypeRoot, derivedType1, derivedType1_1, derivedType1_2, derivedType2 };
            Assert.Equal(true, oracle.SequenceEqual(typesVisited));
        }

        [Fact]
        public void IsAncestorOf_should_return_correct_answer()
        {
            var entityType1 = new EdmEntityType { Name = "E1" };
            var entityType2 = new EdmEntityType { Name = "E2" };
            entityType2.BaseType = entityType1;
            var entityType3 = new EdmEntityType { Name = "E3" };
            entityType3.BaseType = entityType1;
            var entityType4 = new EdmEntityType { Name = "E4" };
            entityType4.BaseType = entityType2;

            Assert.True(entityType1.IsAncestorOf(entityType4));
            Assert.False(entityType3.IsAncestorOf(entityType4));
        }
    }
}