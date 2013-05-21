// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class EdmModelSyntacticValidationRulesTests
    {
        [Fact]
        public void EdmModel_NameIsTooLong_not_triggered_for_row_and_collection_types()
        {
            var intType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var properties = new EdmProperty[100];
            for (int i = 0; i < 100; i++)
            {
                properties[i] = EdmProperty.Primitive("Property" + i, intType);
            }

            var rowType = new RowType(properties);

            foreach (var type in new EdmType[] { rowType, rowType.GetCollectionType() })
            {
                var validationContext
                    = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);
                DataModelErrorEventArgs errorEventArgs = null;
                validationContext.OnError += (_, e) => errorEventArgs = e;

                EdmModelSyntacticValidationRules
                    .EdmModel_NameIsTooLong
                    .Evaluate(validationContext, type);

                Assert.Null(errorEventArgs);
            }
        }

        [Fact]
        public void EdmModel_NameIsNotAllowed_not_triggered_for_row_and_collection_types()
        {
            var rowType =
                new RowType(new[] { EdmProperty.Primitive("Property", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) });

            foreach (var type in new EdmType[] { rowType, rowType.GetCollectionType() })
            {
                var validationContext
                    = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);
                DataModelErrorEventArgs errorEventArgs = null;
                validationContext.OnError += (_, e) => errorEventArgs = e;

                EdmModelSyntacticValidationRules
                    .EdmModel_NameIsNotAllowed
                    .Evaluate(validationContext, type);

                Assert.Null(errorEventArgs);
            }
        }

        [Fact]
        public void EdmModel_NameIsNotAllowed_not_triggered_for_store_property_with_period()
        {
            var property = EdmProperty.Primitive("Property.With.Dots", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var validationContext
                = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);
            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSyntacticValidationRules
                .EdmModel_NameIsNotAllowed
                .Evaluate(validationContext, property);

            Assert.Null(errorEventArgs);
        }

        [Fact]
        public void EdmModel_NameIsNotAllowed_triggered_for_conceptual_property_with_period()
        {
            var property = EdmProperty.Primitive("Property.With.Dots", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var validationContext
                = new EdmModelValidationContext(new EdmModel(DataSpace.CSpace), true);
            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSyntacticValidationRules
                .EdmModel_NameIsNotAllowed
                .Evaluate(validationContext, property);

            Assert.NotNull(errorEventArgs);
        }

        [Fact]
        public void EdmModel_NameIsNotAllowed_not_triggered_for_store_entity_types_with_spaces()
        {
            var entityType = new EntityType("Entity With Spaces", "N", DataSpace.SSpace);

            var validationContext
                = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);
            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSyntacticValidationRules
                .EdmModel_NameIsNotAllowed
                .Evaluate(validationContext, entityType);

            Assert.Null(errorEventArgs);
        }

        [Fact]
        public void EdmModel_NameIsNotAllowed_triggered_for_conceptual_entity_types_with_spaces()
        {
            var entityType = new EntityType("Entity With Spaces", "N", DataSpace.CSpace);

            var validationContext
                = new EdmModelValidationContext(new EdmModel(DataSpace.CSpace), true);
            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSyntacticValidationRules
                .EdmModel_NameIsNotAllowed
                .Evaluate(validationContext, entityType);

            Assert.NotNull(errorEventArgs);
        }
    }
}
