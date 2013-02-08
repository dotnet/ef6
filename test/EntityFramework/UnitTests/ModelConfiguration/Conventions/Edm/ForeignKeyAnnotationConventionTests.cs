// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyAnnotationConventionTests
    {
        [Fact]
        public void Apply_is_noop_when_existing_constraint()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            var property = EdmProperty.Primitive("Fk", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var associationConstraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            associationType.Constraint = associationConstraint;

            var navigationProperty = new NavigationProperty("N", TypeUsage.Create(new EntityType()))
                                         {
                                             RelationshipType = associationType
                                         };

            ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, new EdmModel(DataSpace.CSpace));

            Assert.Same(associationConstraint, navigationProperty.Association.Constraint);
        }

        [Fact]
        public void Apply_is_noop_when_no_fk_annotation()
        {
            var navigationProperty = new NavigationProperty("N", TypeUsage.Create(new EntityType()))
                                         {
                                             RelationshipType = new AssociationType()
                                         };

            ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, new EdmModel(DataSpace.CSpace));

            Assert.Null(navigationProperty.Association.Constraint);
        }

        [Fact]
        public void Apply_is_noop_when_unknown_dependent()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            var navigationProperty = new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                                         {
                                             RelationshipType = associationType
                                         };
            var foreignKeyAnnotation = new ForeignKeyAttribute("AId");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_generates_constraint_when_simple_fk()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var associationType = model.AddAssociationType(
                "A",
                entityType, RelationshipMultiplicity.ZeroOrOne,
                entityType, RelationshipMultiplicity.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var property = EdmProperty.Primitive("Fk", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var fkProperty = property;
            var foreignKeyAnnotation = new ForeignKeyAttribute("Fk");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.NotNull(associationType.Constraint);
            Assert.True(associationType.Constraint.ToProperties.Contains(fkProperty));
        }

        [Fact]
        public void Apply_generates_constraint_when_composite_fk()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var associationType = model.AddAssociationType(
                "A",
                entityType, RelationshipMultiplicity.ZeroOrOne,
                entityType, RelationshipMultiplicity.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var property = EdmProperty.Primitive("Fk1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var fkProperty1 = property;
            var property1 = EdmProperty.Primitive("Fk2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var fkProperty2 = property1;
            var foreignKeyAnnotation = new ForeignKeyAttribute("Fk2,Fk1");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.NotNull(associationType.Constraint);
            Assert.True(new[] { fkProperty2, fkProperty1 }.SequenceEqual(associationType.Constraint.ToProperties));
        }

        [Fact]
        public void Apply_throws_when_cannot_find_foreign_key_properties()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var mockType = new MockType();

            entityType.Annotations.SetClrType(mockType);
            var associationType = model.AddAssociationType(
                "A",
                entityType, RelationshipMultiplicity.ZeroOrOne,
                entityType, RelationshipMultiplicity.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var foreignKeyAnnotation = new ForeignKeyAttribute("_Fk");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            Assert.Equal(
                Strings.ForeignKeyAttributeConvention_InvalidKey("N", mockType.Object, "_Fk", mockType.Object),
                Assert.Throws<InvalidOperationException>(
                    () => ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                              .Apply(navigationProperty, model)).Message);
        }

        [Fact]
        public void Apply_throws_with_empty_foreign_key_properties()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var mockType = new MockType();

            entityType.Annotations.SetClrType(mockType);
            var associationType = model.AddAssociationType(
                "A",
                entityType, RelationshipMultiplicity.ZeroOrOne,
                entityType, RelationshipMultiplicity.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var foreignKeyAnnotation = new ForeignKeyAttribute(",");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            Assert.Equal(
                Strings.ForeignKeyAttributeConvention_EmptyKey("N", mockType.Object),
                Assert.Throws<InvalidOperationException>(
                    () => ((IEdmConvention<NavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                              .Apply(navigationProperty, model)).Message);
        }
    }
}
