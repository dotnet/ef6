// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Validation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EdmModelSemanticValidationRulesTests
    {
        [Fact]
        public void EdmNavigationProperty_BadNavigationPropertyBadFromRoleType()
        {
            var parentEntity = new EntityType("P", "N", DataSpace.CSpace);
            var targetEntity = new EntityType("T", "N", DataSpace.CSpace);
            var sourceEntity = new EntityType("S", "N", DataSpace.CSpace);

            var associationType
                = new AssociationType
                      {
                          SourceEnd = new AssociationEndMember("S", sourceEntity),
                          TargetEnd = new AssociationEndMember("T", targetEntity)
                      };

            var navigationProperty
                = new NavigationProperty("N", TypeUsage.Create(targetEntity))
                      {
                          RelationshipType = associationType
                      };

            parentEntity.AddMember(navigationProperty);

            var model = new EdmModel(DataSpace.CSpace);

            model.AddItem(parentEntity);

            var validationContext
                = new EdmModelValidationContext(model, true);

            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSemanticValidationRules
                .EdmNavigationProperty_BadNavigationPropertyBadFromRoleType
                .Evaluate(validationContext, navigationProperty);

            Assert.NotNull(errorEventArgs);
            Assert.Same(navigationProperty, errorEventArgs.Item);
            Assert.Equal(
                Strings.BadNavigationPropertyBadFromRoleType(
                    navigationProperty.Name,
                    sourceEntity.Name,
                    navigationProperty.GetFromEnd().Name,
                    navigationProperty.Association.Name,
                    parentEntity.Name),
                errorEventArgs.ErrorMessage);
        }

        [Fact]
        public void EdmEntityContainer_DuplicateEntitySetTable()
        {
            var model = new EdmModel(DataSpace.SSpace);

            model.Containers.Single().AddEntitySetBase(
                new EntitySet("Foo", "S", "T", null, new EntityType()));

            var duplicateEntitySet = new EntitySet("Bar", "S", "T", null, new EntityType());

            model.Containers.Single().AddEntitySetBase(duplicateEntitySet);

            var validationContext
                = new EdmModelValidationContext(model, true);

            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSemanticValidationRules
                .EdmEntityContainer_DuplicateEntitySetTable
                .Evaluate(validationContext, model.Containers.Single());

            Assert.NotNull(errorEventArgs);
            Assert.Same(duplicateEntitySet, errorEventArgs.Item);
            Assert.Equal(
                Strings.DuplicateEntitySetTable(
                    duplicateEntitySet.Name,
                    duplicateEntitySet.Schema,
                    duplicateEntitySet.Table),
                errorEventArgs.ErrorMessage);
        }
    }
}
