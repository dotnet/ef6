// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyDiscoveryConventionBaseTests
    {
        [Fact]
        public void Apply_should_not_discover_when_independent_constraint()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            associationType.MarkIndependent();

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_existing_constraint()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            var property = EdmProperty.CreatePrimitive("Fk", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var associationConstraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            associationType.Constraint = associationConstraint;

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Same(associationConstraint, associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_no_clear_principal()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            associationType.SourceEnd.RelationshipMultiplicity
                = associationType.TargetEnd.RelationshipMultiplicity
                  = RelationshipMultiplicity.Many;

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_no_key_properties()
        {
            var associationType = CreateAssociationType();

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_for_subset_key_match()
        {
            var associationType = CreateAssociationType();

            var pkProperty1 = EdmProperty.CreatePrimitive("Id1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var pkProperty2 = EdmProperty.CreatePrimitive("Id2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty1);
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty2);

            var fkProperty1 = EdmProperty.CreatePrimitive("FooId1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty1);

            // Foo.Id1 == Bar.FooId1 && Foo.Id2 == ?
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_property_types_incompatible()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_result_would_be_incorrectly_identifying()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddKeyMember(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_discover_when_fk_is_non_identifying_dependent_pk_component()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddKeyMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddKeyMember(
                EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_discover_optional_dependent_when_foreign_key_in_dependent_pk()
        {
            var associationType = CreateAssociationType();
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);
            associationType.TargetEnd.GetEntityType().AddKeyMember(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("FooId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_not_discover_optional_dependent_when_foreign_key_not_in_dependent_pk()
        {
            var associationType = CreateAssociationType();
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            var pkProperty = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.GetEntityType().Name = "Foo";

            (new TypeNameForeignKeyDiscoveryConvention())
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
