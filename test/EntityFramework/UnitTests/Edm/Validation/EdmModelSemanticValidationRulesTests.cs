// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EdmModelSemanticValidationRulesTests
    {
        [Fact]
        public void EdmFunction_DuplicateParameterName()
        {
            var validationContext
                = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);

            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            var parameter1
                = new FunctionParameter(
                    "P",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            var parameter2
                = new FunctionParameter(
                    "P2",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);
            
            var function
                = new EdmFunction(
                    "F", "N", DataSpace.SSpace,
                    new EdmFunctionPayload
                        {
                            Parameters = new[] { parameter1, parameter2 }
                        });

            parameter2.Name = "P";

            EdmModelSemanticValidationRules
                .EdmFunction_DuplicateParameterName
                .Evaluate(validationContext, function);

            Assert.NotNull(errorEventArgs);
            Assert.Same(parameter2, errorEventArgs.Item);
            Assert.Equal(
                Strings.ParameterNameAlreadyDefinedDuplicate("P"),
                errorEventArgs.ErrorMessage);
        }

        [Fact]
        public void EdmNavigationProperty_BadNavigationPropertyBadFromRoleType()
        {
            var parentEntity = new EntityType("P", "N", DataSpace.CSpace);
            var targetEntity = new EntityType("T", "N", DataSpace.CSpace);
            var sourceEntity = new EntityType("S", "N", DataSpace.CSpace);

            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
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
                new EntitySet("Foo", "S", "T", null, new EntityType("E", "N", DataSpace.CSpace)));

            var duplicateEntitySet = new EntitySet("Bar", "S", "T", null, new EntityType("E", "N", DataSpace.CSpace));

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

        [Fact]
        public void EdmType_SystemNamespaceEncountered_not_triggered_for_row_and_collection_types()
        {
            var rowType =
                new RowType(new[] { EdmProperty.Primitive("Property", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) });

            foreach (var type in new EdmType[] { rowType, rowType.GetCollectionType() })
            {
                var validationContext
                    = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);
                DataModelErrorEventArgs errorEventArgs = null;
                validationContext.OnError += (_, e) => errorEventArgs = e;

                EdmModelSemanticValidationRules
                    .EdmType_SystemNamespaceEncountered
                    .Evaluate(validationContext, type);

                Assert.Null(errorEventArgs);
            }
        }

        [Fact]
        public void EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2()
        {
            var errorEventArgs = EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2_runner(1.0);
            Assert.NotNull(errorEventArgs);
            Assert.IsType<EdmFunction>(errorEventArgs.Item);
            Assert.Equal(
                errorEventArgs.ErrorMessage,
                Strings.EdmModel_Validator_Semantic_ComposableFunctionImportsNotSupportedForSchemaVersion);

            errorEventArgs = EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2_runner(2.0);
            Assert.NotNull(errorEventArgs);
            Assert.IsType<EdmFunction>(errorEventArgs.Item);
            Assert.Equal(
                errorEventArgs.ErrorMessage,
                Strings.EdmModel_Validator_Semantic_ComposableFunctionImportsNotSupportedForSchemaVersion);
        }

        [Fact]
        public void EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2_not_thrown_for_non_composable_function_imports()
        {
            Assert.Null(
                EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2_runner(
                    1.0, 
                    isFunctionImport: true, 
                    isComposable: false));
        }

        private static DataModelErrorEventArgs EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2_runner(double schemaVersion, bool isFunctionImport = true, bool isComposable = true)
        {
            var functionImport = new EdmFunction(
                "f", "Ns", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsComposable = isComposable,
                    IsFunctionImport = isFunctionImport
                });

            var model = new EdmModel(DataSpace.CSpace, schemaVersion);

            var validationContext
                = new EdmModelValidationContext(model, true);

            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSemanticValidationRules
                .EdmFunction_ComposableFunctionImportsNotAllowed_V1_V2
                .Evaluate(validationContext, functionImport);

            return errorEventArgs;
        }

        [Fact]
        public void EdmAssociationType_ValidateReferentialConstraint_invalid_for_non_fkey_references_in_CSpace()
        {
            var errorEventArgs = ValidateAssociationTypeWithNonFkeyReference(DataSpace.CSpace);
            Assert.NotNull(errorEventArgs);
            Assert.Equal(
                Strings.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint("C", "ns.P", "ns.AT"), 
                errorEventArgs.ErrorMessage);
        }

        [Fact]
        public void EdmAssociationType_ValidateReferentialConstraint_valid_for_non_fkey_references_in_SSpace()
        {
            Assert.Null(ValidateAssociationTypeWithNonFkeyReference(DataSpace.SSpace));
        }

        private DataModelErrorEventArgs ValidateAssociationTypeWithNonFkeyReference(DataSpace dataSpace)
        {
            var model = new EdmModel(dataSpace, 1.0);

            var intType =
                dataSpace == DataSpace.CSpace
                    ? PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)
                    : FakeSqlProviderServices.Instance.GetProviderManifest("2008").GetStoreTypes().Single(t => t.Name == "int");

            var principal = 
                new EntityType("P", "ns", dataSpace, new [] {"Id"}, new[] { EdmProperty.Primitive("Id", intType) });
            var dependent = 
                new EntityType("P", "ns", dataSpace, new [] {"Id"},
                    new[] { EdmProperty.Primitive("Id", intType), EdmProperty.Primitive("NonKeyProperty", intType) });

            foreach (var property in principal.Properties.Concat(dependent.Properties))
            {
                property.Nullable = false;
            }

            var associationType =
                new AssociationType("AT", "ns", false, dataSpace)
                    {
                        Constraint = new ReferentialConstraint(
                                new AssociationEndMember("P", principal.GetReferenceType(), RelationshipMultiplicity.One),
                                new AssociationEndMember("C", dependent.GetReferenceType(), RelationshipMultiplicity.Many),
                                principal.KeyProperties,
                                dependent.Properties.Where(p => p.Name == "NonKeyProperty"))
                    };

            model.AddAssociationType(associationType);

            var validationContext = new EdmModelValidationContext(model, true);

            DataModelErrorEventArgs errorEventArgs = null;
            validationContext.OnError += (_, e) => errorEventArgs = e;

            EdmModelSemanticValidationRules
                .EdmAssociationType_ValidateReferentialConstraint
                .Evaluate(validationContext, model.AssociationTypes.Single());

            return errorEventArgs;
        }
    }
}
