// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class EdmFeatureManagerTests
    {
        [Fact]
        public void FunctionImportReturningComplexType_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetFunctionImportReturningComplexTypeFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetFunctionImportReturningComplexTypeFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetFunctionImportReturningComplexTypeFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void EnumTypes_enabled_only_for_v3_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetEnumTypeFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetEnumTypeFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetEnumTypeFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void EnumTypes_enabled_only_for_v3_artifacts()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                IsFeatureEnabledForArtifact(EntityFrameworkVersion.Version1, EdmFeatureManager.GetEnumTypeFeatureState));

            Assert.Equal(
                FeatureState.VisibleButDisabled,
                IsFeatureEnabledForArtifact(EntityFrameworkVersion.Version2, EdmFeatureManager.GetEnumTypeFeatureState));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                IsFeatureEnabledForArtifact(EntityFrameworkVersion.Version3, EdmFeatureManager.GetEnumTypeFeatureState));
        }

        [Fact]
        public void FunctionImportColumnInformation_always_enabled()
        {
            foreach (var targetSchemaVersion in EntityFrameworkVersion.GetAllVersions())
            {
                Assert.Equal(
                    FeatureState.VisibleAndEnabled,
                    IsFeatureEnabledForArtifact(
                        targetSchemaVersion,
                        EdmFeatureManager.GetFunctionImportColumnInformationFeatureState));
            }
        }

        private static FeatureState IsFeatureEnabledForArtifact(Version schemaVersion, Func<EFArtifact, FeatureState> funcToTest)
        {
            using (var editingContext = new EditingContext())
            {
                var modelManager = new Mock<ModelManager>(null, null).Object;
                var modelProvider = new Mock<XmlModelProvider>().Object;

                var entityDesignArtifactMock =
                    new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

                editingContext.SetEFArtifactService(new EFArtifactService(entityDesignArtifactMock.Object));

                entityDesignArtifactMock.Setup(a => a.SchemaVersion).Returns(schemaVersion);
                entityDesignArtifactMock.Setup(a => a.EditingContext).Returns(editingContext);

                return funcToTest(entityDesignArtifactMock.Object);
            }
        }

        [Fact]
        public void FunctionImportMapping_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetFunctionImportMappingFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetFunctionImportMappingFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetFunctionImportMappingFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void ForeignKeys_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetForeignKeysInModelFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetForeignKeysInModelFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetForeignKeysInModelFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void UpdateViews_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetGenerateUpdateViewsFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetGenerateUpdateViewsFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetGenerateUpdateViewsFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void EntityContainerTypeAccess_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void LazyLoading_disabled_only_for_v1_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetLazyLoadingFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetLazyLoadingFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetLazyLoadingFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void ComposableFunctionImport_enabled_only_for_v3_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetComposableFunctionImportFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetComposableFunctionImportFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetComposableFunctionImportFeatureState(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void SpatialTypes_enabled_only_for_v3_schema_version()
        {
            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(EntityFrameworkVersion.Version1));

            Assert.Equal(
                FeatureState.VisibleButDisabled,
                EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(EntityFrameworkVersion.Version2));

            Assert.Equal(
                FeatureState.VisibleAndEnabled,
                EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(EntityFrameworkVersion.Version3));
        }
    }
}
