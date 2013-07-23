// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class NavigationPropertyNameForeignKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_discover_for_self_reference()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", associationType.SourceEnd.GetEntityType());

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("NavId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Foos", associationType);

            // Foo.Id == Foo.NavId

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("NavId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("NavId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;

            // Foo.Id == Bar.NavId

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("NavId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key_with_different_casing()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("ID", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("NavId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;

            // Foo.ID == Bar.NavId

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("NavId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_composite_matching_foreign_keys()
        {
            var associationType = CreateAssociationType();

            var pkProperty1 = EdmProperty.CreatePrimitive("Id1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var pkProperty2 = EdmProperty.CreatePrimitive("Id2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty1);
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty2);

            var fkProperty1 = EdmProperty.CreatePrimitive("NavId1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var fkProperty2 = EdmProperty.CreatePrimitive("NavId2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty1);
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty2);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;

            // Foo.Id1 == Bar.NavId1 && Foo.Id2 == Bar.NavId2

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal(2, associationType.Constraint.ToProperties.Count());
        }

        [Fact]
        public void Apply_should_discover_when_multiple_associations_exist()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("NavId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;

            // Foo.Id == Bar.NavId

            var model = new EdmModel(DataSpace.CSpace);
            model.AddItem(associationType);
            model.AddItem(associationType);

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(model, null));

            Assert.NotNull(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_property_types_are_incompatible()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("NavId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddNavigationProperty("Nav", associationType).ToEndMember = associationType.SourceEnd;

            // Foo.Id == Bar.NavId

            (new NavigationPropertyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        private static AssociationType CreateAssociationType()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            return associationType;
        }
    }
}
