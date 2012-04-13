namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Edm;
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
            var associationConstraint = new EdmAssociationConstraint();
            var navigationProperty = new EdmNavigationProperty
                {
                    Association = new EdmAssociationType
                        {
                            Constraint = associationConstraint
                        }
                };

            ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, new EdmModel());

            Assert.Same(associationConstraint, navigationProperty.Association.Constraint);
        }

        [Fact]
        public void Apply_is_noop_when_no_fk_annotation()
        {
            var navigationProperty = new EdmNavigationProperty
                {
                    Association = new EdmAssociationType()
                };

            ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, new EdmModel());

            Assert.Null(navigationProperty.Association.Constraint);
        }

        [Fact]
        public void Apply_is_noop_when_unknown_dependent()
        {
            var model = new EdmModel().Initialize();
            var associationType = new EdmAssociationType().Initialize();
            var navigationProperty = new EdmNavigationProperty { Association = associationType };
            var foreignKeyAnnotation = new ForeignKeyAttribute("AId");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_generates_constraint_when_simple_fk()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var associationType = model.AddAssociationType("A",
                entityType, EdmAssociationEndKind.Optional,
                entityType, EdmAssociationEndKind.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var fkProperty = entityType.AddPrimitiveProperty("Fk");
            var foreignKeyAnnotation = new ForeignKeyAttribute("Fk");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.NotNull(associationType.Constraint);
            Assert.True(associationType.Constraint.DependentProperties.Contains(fkProperty));
        }

        [Fact]
        public void Apply_generates_constraint_when_composite_fk()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var associationType = model.AddAssociationType("A",
                entityType, EdmAssociationEndKind.Optional,
                entityType, EdmAssociationEndKind.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var fkProperty1 = entityType.AddPrimitiveProperty("Fk1");
            var fkProperty2 = entityType.AddPrimitiveProperty("Fk2");
            var foreignKeyAnnotation = new ForeignKeyAttribute("Fk2,Fk1");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                .Apply(navigationProperty, model);

            Assert.NotNull(associationType.Constraint);
            Assert.True(new[] { fkProperty2, fkProperty1 }.SequenceEqual(associationType.Constraint.DependentProperties));
        }

        [Fact]
        public void Apply_throws_when_cannot_find_foreign_key_properties()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var mockType = new MockType();
            entityType.SetClrType(mockType);
            var associationType = model.AddAssociationType("A",
                entityType, EdmAssociationEndKind.Optional,
                entityType, EdmAssociationEndKind.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var foreignKeyAnnotation = new ForeignKeyAttribute("_Fk");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            Assert.Equal(Strings.ForeignKeyAttributeConvention_InvalidKey("N", mockType.Object, "_Fk", mockType.Object), Assert.Throws<InvalidOperationException>(() => ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                                                                                                                                                                                  .Apply(navigationProperty, model)).Message);
        }

        [Fact]
        public void Apply_throws_with_empty_foreign_key_properties()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var mockType = new MockType();
            entityType.SetClrType(mockType);
            var associationType = model.AddAssociationType("A",
                entityType, EdmAssociationEndKind.Optional,
                entityType, EdmAssociationEndKind.Many);
            var navigationProperty = entityType.AddNavigationProperty("N", associationType);
            var foreignKeyAnnotation = new ForeignKeyAttribute(",");
            navigationProperty.Annotations.SetClrAttributes(new[] { foreignKeyAnnotation });

            Assert.Equal(Strings.ForeignKeyAttributeConvention_EmptyKey("N", mockType.Object), Assert.Throws<InvalidOperationException>(() => ((IEdmConvention<EdmNavigationProperty>)new ForeignKeyNavigationPropertyAttributeConvention())
                                                                                                                                                        .Apply(navigationProperty, model)).Message);
        }
    }
}