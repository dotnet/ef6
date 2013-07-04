// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ComplexTypeDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_copy_namespace_from_entity()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.NamespaceName = "Foo";

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal("Foo", model.ComplexTypes.Single().NamespaceName);
        }

        [Fact]
        public void Apply_should_discover_complex_type_by_convention()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(0, model.AssociationTypes.Count());

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.ComplexTypes.Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
            Assert.Equal(1, model.ComplexTypes.Single().Properties.Count);
            Assert.Equal(1, declaringEntityType.DeclaredProperties.Count());
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_key()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.AddKeyMember(
                EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_base_type()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.BaseType = new EntityType("E", "N", DataSpace.CSpace);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_explicit_entity_configuration()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            complexEntityType.Annotations.SetConfiguration(
                new EntityTypeConfiguration(typeof(object))
                    {
                        IsExplicitEntity = true
                    });

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_with_outbound_nav_props()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace)),
                          TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace))
                      };
            complexEntityType.AddNavigationProperty("N", associationType);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_discover_complex_type_with_multiple_inbound_associations()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.AddItem(model.AssociationTypes.Single());

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(0, model.AssociationTypes.Count());

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.ComplexTypes.Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_has_a_constraint_defined()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;

            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);

            var associationType = model.AssociationTypes.Single();

            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            associationType.Constraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_has_explicit_configuration()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.AssociationTypes.Single().SetConfiguration(42);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_is_self_reference()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;

            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);

            var associationType = model.AssociationTypes.Single();

            associationType.SourceEnd
                = new AssociationEndMember("S", complexEntityType);
            associationType.TargetEnd
                = new AssociationEndMember("T", complexEntityType);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_the_inbound_association_other_end_is_not_optional()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            model.AssociationTypes.Single().TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_discover_complex_type_when_it_has_multiple_inbound_navigation_properties()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;

            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);

            declaringEntityType.AddNavigationProperty("E.C2", model.AssociationTypes.Single());

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(0, model.AssociationTypes.Count());

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(1, model.ComplexTypes.Count());
            Assert.Equal(0, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        [Fact]
        public void Apply_should_not_discover_entity_when_inbound_navigation_property_has_configuration()
        {
            EntityType declaringEntityType;
            EntityType complexEntityType;
            var model = CreateModelFixture(out declaringEntityType, out complexEntityType);
            declaringEntityType.NavigationProperties.Single().SetConfiguration(42);

            (new ComplexTypeDiscoveryConvention()).Apply(model, new DbModel(model, null));

            Assert.Equal(1, model.AssociationTypes.Count());

            Assert.Equal(2, model.EntityTypes.Count());
            Assert.Equal(0, model.ComplexTypes.Count());
            Assert.Equal(1, declaringEntityType.DeclaredNavigationProperties.Count);
        }

        private static EdmModel CreateModelFixture(
            out EntityType declaringEntityType, out EntityType complexEntityType)
        {
            var model = new EdmModel(DataSpace.CSpace);

            declaringEntityType = model.AddEntityType("E");
            complexEntityType = model.AddEntityType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexEntityType.AddMember(property);

            var associationType
                = model.AddAssociationType(
                    "A",
                    declaringEntityType, RelationshipMultiplicity.Many,
                    complexEntityType, RelationshipMultiplicity.ZeroOrOne);

            declaringEntityType.AddNavigationProperty("E.C", associationType);

            return model;
        }
    }
}
