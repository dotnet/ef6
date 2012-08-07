// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ComplexTypeDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_discover_complex_type_by_convention()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(0, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.GetEntityTypes().Count());
            Assert.Equal(1, model.GetComplexTypes().Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
            Assert.Equal(1, model.GetComplexTypes().Single().DeclaredProperties.Count);
            Assert.Equal(1, declaringEntityType.DeclaredProperties.Count());
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_key()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.DeclaredKeyProperties.Add(new EdmProperty().AsPrimitive());

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_base_type()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.BaseType = new EdmEntityType();

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_explicit_entity_configuration()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.SetConfiguration(
                new EntityTypeConfiguration(typeof(object))
                    {
                        IsExplicitEntity = true
                    });

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_outbound_nav_props()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.AddNavigationProperty("N", new EdmAssociationType().Initialize());

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_discover_complex_type_with_multiple_inbound_associations()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.Namespaces.Single().AssociationTypes.Add(model.GetAssociationTypes().Single());

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(0, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.GetEntityTypes().Count());
            Assert.Equal(1, model.GetComplexTypes().Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_has_a_constraint_defined()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.GetAssociationTypes().Single().Constraint = new EdmAssociationConstraint();

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_has_explicit_configuration()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.GetAssociationTypes().Single().SetConfiguration(42);

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_is_self_reference()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.GetAssociationTypes().Single().SourceEnd
                = model.GetAssociationTypes().Single().TargetEnd;

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_other_end_is_not_optional()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.GetAssociationTypes().Single().TargetEnd.EndKind = EdmAssociationEndKind.Required;

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_discover_complex_type_when_it_has_multiple_inbound_navigation_properties()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            declaringEntityType.DeclaredNavigationProperties.Add(declaringEntityType.NavigationProperties.Single());

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(0, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.GetEntityTypes().Count());
            Assert.Equal(1, model.GetComplexTypes().Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_inbound_navigation_property_has_configuration()
        {
            EdmEntityType declaringEntityType;
            EdmEntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            declaringEntityType.NavigationProperties.Single().SetConfiguration(42);

            ((IEdmConvention)new ComplexTypeDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(0, model.GetComplexTypes().Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        private static EdmModel CreateModelFixture(
            out EdmEntityType declaringEntityType, out EdmEntityType complexEntityType)
        {
            var model = new EdmModel().Initialize();

            declaringEntityType = model.AddEntityType("E");
            complexEntityType = model.AddEntityType("C");
            complexEntityType.AddPrimitiveProperty("P");

            var associationType
                = model.AddAssociationType(
                    "A",
                    declaringEntityType, EdmAssociationEndKind.Many,
                    complexEntityType, EdmAssociationEndKind.Optional);

            declaringEntityType.AddNavigationProperty("E.C", associationType);

            return model;
        }
    }
}
