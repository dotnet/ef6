// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Validation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
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

            var model = new EdmModel();

            model.AddItem(parentEntity);

            var validationContext
                = new EdmModelValidationContext(true)
                      {
                          Model = model
                      };

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
    }
}
