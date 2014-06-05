// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    extern alias EntityDesignModel;
// Resharper wants to remove the below but do not - it causes build errors 
    using EntityDesignModel::System.Data.Entity.Core.Mapping;
    using EntityDesignModel::System.Data.Entity.Core.SchemaObjectModel;
// Resharper wants to remove the above but do not - it causes build errors 

    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;
    using ComplexType = Microsoft.Data.Entity.Design.Model.Entity.ComplexType;
    using ReferentialConstraint = Microsoft.Data.Entity.Design.Model.Entity.ReferentialConstraint;

    public class RuntimeMetadataValidatorTests
    {
        private readonly IDbDependencyResolver _resolver;

        public RuntimeMetadataValidatorTests()
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(SqlProviderServices.Instance);

            _resolver = mockResolver.Object;
        }

        [Fact]
        public void ValidateArtifactSet_does_not_validate_artifact_set_if_no_errors_and_forceValidation_false()
        {
            var mockModelManager =
                new Mock<ModelManager>(new Mock<IEFArtifactFactory>().Object, new Mock<IEFArtifactSetFactory>().Object);

            using (var modelManager = mockModelManager.Object)
            {
                var artifact =
                    new Mock<EFArtifact>(
                        modelManager, new Uri("http://tempuri"), new Mock<XmlModelProvider>().Object).Object;

                var mockArtifactSet = new Mock<EFArtifactSet>(artifact);

                new RuntimeMetadataValidator(modelManager, new Version(2, 0, 0, 0), _resolver)
                    .ValidateArtifactSet(mockArtifactSet.Object, forceValidation: false, validateMsl: false, runViewGen: false);

                mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Never());
            }
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_for_empty_conceptual_and_storage_models()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(2, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: false, validateMsl: false, runViewGen: false);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(2, errors.Count);
                        Assert.Equal(ErrorCodes.ErrorValidatingArtifact_StorageModelMissing, errors.First().ErrorCode);
                        Assert.Contains(Resources.ErrorValidatingArtifact_StorageModelMissing, errors.First().Message);
                        Assert.Equal(ErrorCodes.ErrorValidatingArtifact_ConceptualModelMissing, errors.Last().ErrorCode);
                        Assert.Contains(Resources.ErrorValidatingArtifact_ConceptualModelMissing, errors.Last().Message);
                        Assert.True(errors.All(e => ReferenceEquals(e.Item, artifactSet.GetEntityDesignArtifact())));

                        // these errors should not clear error class flags
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_All));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Exactly(2));
                    });
        }

        [Fact]
        public void
            ValidateArtifactSet_returns_errors_for_conceptual_and_storage_models_whose_version_is_greater_than_target_runtime_version()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" />",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(2, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: false, validateMsl: false, runViewGen: false);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(2, errors.Count);
                        Assert.Equal(
                            ErrorCodes.ErrorValidatingArtifact_InvalidSSDLNamespaceForTargetFrameworkVersion, errors.First().ErrorCode);
                        Assert.Contains(
                            Resources.ErrorValidatingArtifact_InvalidSSDLNamespaceForTargetFrameworkVersion, errors.First().Message);
                        Assert.Equal(artifactSet.GetEntityDesignArtifact().StorageModel, errors.First().Item);
                        Assert.Equal(
                            ErrorCodes.ErrorValidatingArtifact_InvalidCSDLNamespaceForTargetFrameworkVersion, errors.Last().ErrorCode);
                        Assert.Contains(
                            Resources.ErrorValidatingArtifact_InvalidCSDLNamespaceForTargetFrameworkVersion, errors.Last().Message);
                        Assert.Equal(artifactSet.GetEntityDesignArtifact().ConceptualModel, errors.Last().Item);

                        // these error should not clear error class flags
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_All));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Exactly(2));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_for_invalid_conceptual_and_storage_model()
        {
            // Both Csdl and Ssdl are missing the 'Namespace' attribute and therefore are invalid. 
            // We don't really care about what errors we will get as long as they are thrown by 
            // the runtime and one is for Csdl and the other one is for Ssdl.
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: false, validateMsl: false, runViewGen: false);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(2, errors.Count);
                        Assert.Equal(1, errors.Count(e => e.ErrorClass == ErrorClass.Runtime_CSDL));
                        Assert.Equal(1, errors.Count(e => e.ErrorClass == ErrorClass.Runtime_SSDL));

                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL | ErrorClass.Runtime_SSDL));
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_MSL | ErrorClass.Runtime_ViewGen));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Exactly(2));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_cached_when_reverse_engineering_db()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" Namespace=\"Model\">" +
                "  <EntityContainer Name=\"ModelContainer\" />" +
                "</Schema>",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\">"
                +
                "  <EntityContainer Name=\"StoreContainer\" />" +
                "</Schema>",
                "<Mapping Space=\"C-S\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/mapping/cs\">" +
                "  <EntityContainerMapping StorageEntityContainer=\"StoreContainer\" CdmEntityContainer=\"ModelContainer\" />" +
                "</Mapping>",
                (mockModelManager, mockArtifactSet) =>
                    {
                        var error = new EdmSchemaError("test", 42, EdmSchemaErrorSeverity.Error);

                        var artifactSet = mockArtifactSet.Object;
                        var mockArtifact = Mock.Get(artifactSet.GetEntityDesignArtifact());
                        mockArtifact
                            .Setup(m => m.GetModelGenErrors())
                            .Returns(new List<EdmSchemaError>(new[] { error }));

                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: true, runViewGen: true);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(1, errors.Count);
                        Assert.Equal(1, errors.Count(e => e.ErrorClass == ErrorClass.Runtime_SSDL));
                        Assert.Contains(error.Message, errors.Single().Message);

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Exactly(1));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_does_not_validate_mapping_if_mapping_validation_disabled()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" Namespace=\"Model\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                "<dummy />",
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: false, runViewGen: false);

                        Assert.Empty(artifactSet.GetAllErrors());

                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL | ErrorClass.Runtime_SSDL));
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_MSL | ErrorClass.Runtime_ViewGen));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_for_empty_mapping_model()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" Namespace=\"Model\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: true, runViewGen: false);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(1, errors.Count);
                        Assert.Equal(ErrorCodes.ErrorValidatingArtifact_MappingModelMissing, errors.First().ErrorCode);
                        Assert.Same(artifactSet.GetEntityDesignArtifact(), errors.First().Item);

                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Escher_CSDL | ErrorClass.Escher_SSDL));
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Escher_MSL | ErrorClass.Runtime_ViewGen));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Once());
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_for_mapping_models_whose_version_is_greater_than_target_runtime_version()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2008/09/edm\" Namespace=\"Model\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/02/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                "<Mapping Space=\"C-S\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/mapping/cs\" />",
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(2, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: true, runViewGen: false);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(1, errors.Count);
                        Assert.Equal(
                            ErrorCodes.ErrorValidatingArtifact_InvalidMSLNamespaceForTargetFrameworkVersion, errors.First().ErrorCode);
                        Assert.Contains(
                            Resources.ErrorValidatingArtifact_InvalidMSLNamespaceForTargetFrameworkVersion, errors.First().Message);
                        Assert.Equal(artifactSet.GetEntityDesignArtifact().MappingModel, errors.First().Item);

                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Escher_CSDL | ErrorClass.Escher_SSDL));
                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Escher_MSL | ErrorClass.Runtime_ViewGen));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Once());
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_for_invalid_mapping_model_if_mapping_validation_enabled()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" Namespace=\"Model\" />",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                "<dummy />",
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;

                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: true, runViewGen: true);

                        var errors = artifactSet.GetAllErrors();
                        Assert.Equal(1, errors.Count);
                        Assert.True(errors.All(e => e.ErrorClass == ErrorClass.Runtime_MSL));

                        Assert.False(
                            artifactSet.IsValidityDirtyForErrorClass(
                                ErrorClass.Runtime_CSDL | ErrorClass.Runtime_SSDL | ErrorClass.Runtime_MSL));

                        Assert.True(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_ViewGen));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_errors_if_validation_with_view_generation_fails()
        {
            // The mapping here is invalid - both C-Space properties are mapped to one S-Space property.
            // This condition is not detected when loading StorageMappingItemCollection but it make view gen
            // throw. In this case we don't really care about what error will be thrown as long as it is 
            // thrown by view gen
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" xmlns:cg=\"http://schemas.microsoft.com/ado/2006/04/codegeneration\" xmlns:store=\"http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator\" Namespace=\"Model\" Alias=\"Self\" xmlns:annotation=\"http://schemas.microsoft.com/ado/2009/02/edm/annotation\" annotation:UseStrongSpatialTypes=\"false\">"
                +
                "  <EntityContainer Name=\"ModelContainer\" annotation:LazyLoadingEnabled=\"true\">" +
                "      <EntitySet Name=\"Entities\" EntityType=\"Model.Entity\" />" +
                "  </EntityContainer>" +
                "  <EntityType Name=\"Entity\">" +
                "      <Key>" +
                "      <PropertyRef Name=\"Id\" />" +
                "      </Key>" +
                "      <Property Type=\"Int32\" Name=\"Id\" Nullable=\"false\" annotation:StoreGeneratedPattern=\"Identity\" />" +
                "      <Property Type=\"Int32\" Name=\"IntProperty\" Nullable=\"false\" />" +
                "  </EntityType>" +
                "</Schema>",
                "<Schema Namespace=\"Model.Store\" Alias=\"Self\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2008\" xmlns:store=\"http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\">"
                +
                "  <EntityContainer Name=\"ModelStoreContainer\">" +
                "    <EntitySet Name=\"Entity\" EntityType=\"Model.Store.Entity\" store:Type=\"Tables\" Schema=\"dbo\" />" +
                "  </EntityContainer>" +
                "  <EntityType Name=\"Entity\">" +
                "    <Key>" +
                "      <PropertyRef Name=\"Id\" />" +
                "    </Key>" +
                "    <Property Name=\"Id\" Type=\"int\" StoreGeneratedPattern=\"Identity\" Nullable=\"false\" />" +
                "    <Property Name=\"IntProperty\" Type=\"int\" Nullable=\"false\" />" +
                "  </EntityType>" +
                "</Schema>",
                "<Mapping Space=\"C-S\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/mapping/cs\">" +
                "  <EntityContainerMapping StorageEntityContainer=\"ModelStoreContainer\" CdmEntityContainer=\"ModelContainer\">" +
                "    <EntitySetMapping Name=\"Entities\">" +
                "      <EntityTypeMapping TypeName=\"IsTypeOf(Model.Entity)\">" +
                "        <MappingFragment StoreEntitySet=\"Entity\">" +
                "          <ScalarProperty Name=\"Id\" ColumnName=\"Id\" />" +
                "          <ScalarProperty Name=\"IntProperty\" ColumnName=\"Id\" />" +
                "        </MappingFragment>" +
                "      </EntityTypeMapping>" +
                "    </EntitySetMapping>" +
                "  </EntityContainerMapping>" +
                "</Mapping>",
                (mockModelManager, mockArtifactSet) =>
                    {
                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(mockArtifactSet.Object, forceValidation: true, validateMsl: true, runViewGen: true);

                        var errors = mockArtifactSet.Object.GetAllErrors();
                        Assert.Equal(1, errors.Count);
                        Assert.True(errors.All(e => e.ErrorClass == ErrorClass.Runtime_ViewGen));

                        Assert.False(mockArtifactSet.Object.IsValidityDirtyForErrorClass(ErrorClass.Runtime_All));
                    });
        }

        [Fact]
        public void ValidateArtifactSet_returns_no_errors_for_valid_artifacts()
        {
            SetupModelAndInvokeAction(
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\" Namespace=\"Model\">" +
                "  <EntityContainer Name=\"ModelContainer\" />" +
                "</Schema>",
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\">"
                +
                "  <EntityContainer Name=\"StoreContainer\" />" +
                "</Schema>",
                "<Mapping Space=\"C-S\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/mapping/cs\">" +
                "  <EntityContainerMapping StorageEntityContainer=\"StoreContainer\" CdmEntityContainer=\"ModelContainer\" />" +
                "</Mapping>",
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;

                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ValidateArtifactSet(artifactSet, forceValidation: true, validateMsl: true, runViewGen: true);

                        Assert.Empty(artifactSet.GetAllErrors());
                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_All));

                        mockArtifactSet.Verify(m => m.AddError(It.IsAny<ErrorInfo>()), Times.Never());
                    });
        }

        [Fact]
        public void Validate_calls_into_ValidateArtifactSet_with_correct_parameter_values()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var mockValidator =
                            new Mock<RuntimeMetadataValidator>(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver);

                        mockValidator.Object.Validate(mockArtifactSet.Object);

                        mockValidator
                            .Verify(m => m.ValidateArtifactSet(mockArtifactSet.Object, true, true, false), Times.Once());
                    });
        }

        [Fact]
        public void ValidateAndCompileMappings_calls_into_ValidateArtifactSet_with_correct_parameter_values()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var mockValidator =
                            new Mock<RuntimeMetadataValidator>(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver);

                        foreach (var validateMapping in new[] { true, false })
                        {
                            mockValidator.Object.ValidateAndCompileMappings(mockArtifactSet.Object, validateMapping);

                            mockValidator
                                .Verify(
                                    m =>
                                    m.ValidateArtifactSet(mockArtifactSet.Object, false, validateMapping, validateMapping),
                                    Times.Once());
                        }
                    });
        }

        [Fact]
        public void ProcessErrors_adds_errors_to_artifact_set()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifactSet = mockArtifactSet.Object;
                        var artifact = artifactSet.GetEntityDesignArtifact();
                        artifact.SetValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL, true);

                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ProcessErrors(
                                new List<EdmSchemaError>
                                    {
                                        new EdmSchemaError("abc", 42, EdmSchemaErrorSeverity.Error)
                                    },
                                artifact, ErrorClass.Runtime_CSDL);

                        Assert.Equal(1, artifactSet.GetAllErrors().Count);
                        var error = artifactSet.GetAllErrors().Single();
                        Assert.Contains("abc", error.Message);
                        Assert.Equal(42, error.ErrorCode);
                        Assert.Equal(ErrorClass.Runtime_CSDL, error.ErrorClass);
                        Assert.Equal(ErrorInfo.Severity.ERROR, error.Level);

                        Assert.False(artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL));
                    });
        }

        [Fact]
        public void ProcessErrors_changes_severity_to_warning_for_NotSpecifiedInstanceForEntitySetOrAssociationSet_if_store_model_empty()
        {
            SetupModelAndInvokeAction(
                null,
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        const int NotSpecifiedInstanceForEntitySetOrAssociationSet = 2062;

                        var artifactSet = mockArtifactSet.Object;
                        var artifact = artifactSet.GetEntityDesignArtifact();
                        Mock.Get(artifact.StorageModel)
                            .Setup(m => m.FirstEntityContainer)
                            .Returns(new Mock<StorageEntityContainer>(artifact.StorageModel, new XElement("dummy")).Object);

                        artifact.SetValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL, true);

                        new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                            .ProcessErrors(
                                new List<EdmSchemaError>
                                    {
                                        new EdmSchemaError(
                                            "abc", NotSpecifiedInstanceForEntitySetOrAssociationSet,
                                            EdmSchemaErrorSeverity.Error)
                                    },
                                artifact, ErrorClass.Runtime_CSDL);

                        Assert.Equal(1, artifactSet.GetAllErrors().Count);
                        var error = artifactSet.GetAllErrors().Single();
                        Assert.Contains("abc", error.Message);
                        Assert.Equal(NotSpecifiedInstanceForEntitySetOrAssociationSet, error.ErrorCode);
                        Assert.Equal(ErrorInfo.Severity.WARNING, error.Level);
                    });
        }

        [Fact]
        public void ProcessErrors_rewrites_NotQualifiedTypeErrorCode()
        {
            SetupModelAndInvokeAction(
                null,
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        const int NotQualifiedTypeErrorCode = 40;

                        var artifactSet = mockArtifactSet.Object;
                        var artifact = artifactSet.GetEntityDesignArtifact();
                        var complexType = new Mock<ComplexType>(null, new XElement("dummy")).Object;
                        var mockProperty = new Mock<ComplexConceptualProperty>(complexType, new XElement("dummy"))
                            {
                                CallBase = true
                            };
                        mockProperty.Setup(m => m.Artifact).Returns(artifact);
                        var property = mockProperty.Object;
                        property.ComplexType.SetXObject(new XAttribute("typeName", Resources.ComplexPropertyUndefinedType));

                        Mock.Get(artifact)
                            .Setup(m => m.FindEFObjectForLineAndColumn(It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(property);

                        artifact.SetValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL, true);

                        try
                        {
                            new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                                .ProcessErrors(
                                    new List<EdmSchemaError>
                                        {
                                            new EdmSchemaError(
                                                "abc", NotQualifiedTypeErrorCode,
                                                EdmSchemaErrorSeverity.Error)
                                        },
                                    artifact, ErrorClass.Runtime_CSDL);

                            Assert.Equal(1, artifactSet.GetAllErrors().Count);
                            var error = artifactSet.GetAllErrors().Single();
                            Assert.Contains(
                                string.Format(Resources.EscherValidation_UndefinedComplexPropertyType, string.Empty),
                                error.Message);
                            Assert.Equal(ErrorCodes.ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE, error.ErrorCode);
                            Assert.Equal(error.Item, property);
                            Assert.Equal(ErrorInfo.Severity.ERROR, error.Level);
                        }
                        finally
                        {
                            property.LocalName.Dispose();
                            property.ComplexType.Dispose();
                        }
                    });
        }

        [Fact]
        public void ProcessErrors_rewrites_NonValidAssociationSet_warning()
        {
            SetupModelAndInvokeAction(
                null,
                "<Schema xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\" Namespace=\"Model.Store\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\"/>",
                null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        const int NonValidAssociationSet = 2005;

                        var artifactSet = mockArtifactSet.Object;
                        var artifact = artifactSet.GetEntityDesignArtifact();

                        var mockAssociationSetMapping = new Mock<AssociationSetMapping>(null, new XElement("dummy"));
                        mockAssociationSetMapping
                            .Setup(m => m.Artifact)
                            .Returns(artifact);

                        try
                        {
                            Mock.Get(artifact)
                                .Setup(m => m.FindEFObjectForLineAndColumn(It.IsAny<int>(), It.IsAny<int>()))
                                .Returns(mockAssociationSetMapping.Object);

                            artifact.SetValidityDirtyForErrorClass(ErrorClass.Runtime_CSDL, true);

                            new RuntimeMetadataValidator(mockModelManager.Object, new Version(3, 0, 0, 0), _resolver)
                                .ProcessErrors(
                                    new List<EdmSchemaError>
                                        {
                                            new EdmSchemaError(
                                                "abc", NonValidAssociationSet,
                                                EdmSchemaErrorSeverity.Warning)
                                        },
                                    artifact, ErrorClass.Runtime_CSDL);

                            Assert.Equal(1, artifactSet.GetAllErrors().Count);
                            var error = artifactSet.GetAllErrors().Single();
                            Assert.Contains(
                                string.Format(Resources.EscherValidation_IgnoreMappedFKAssociation, string.Empty),
                                error.Message);
                            Assert.Equal(NonValidAssociationSet, error.ErrorCode);
                            Assert.Equal(ErrorInfo.Severity.WARNING, error.Level);
                        }
                        finally
                        {
                            mockAssociationSetMapping.Object.Name.Dispose();
                        }
                    });
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_non_runtime_errors()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, null, mockArtifactSet.Object.GetEntityDesignArtifact(),
                            42, ErrorClass.ParseError);
                        Assert.False(
                            RuntimeMetadataValidator.IsOpenInEditorError(error, mockArtifactSet.Object.Artifacts.First()));
                    });
        }

#if !VS14
        [Fact]
        public void IsOpenInEditorError_returns_false_for_recoverable_runtime_errors()
        {
            Assert.False(IsOpenInEditorError<EFObject>(-1));
        }
#endif

        [Fact]
        public void IsOpenInEditorError_returns_true_for_unrecoverable_errors_if_additional_conditions_not_met()
        {
            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifact = mockArtifactSet.Object.GetEntityDesignArtifact();

                        var mockEfObject = new Mock<EFObject>(null, new XElement("dummy"));
                        mockEfObject.Setup(m => m.Artifact).Returns(artifact);

                        var unrecoverableErrorCodes =
                            UnrecoverableRuntimeErrors
                                .SchemaObjectModelErrorCodes.Cast<int>()
                                .Concat(UnrecoverableRuntimeErrors.StorageMappingErrorCodes.Cast<int>());

                        foreach (var errorCode in unrecoverableErrorCodes)
                        {
                            var error = new ErrorInfo(
                                ErrorInfo.Severity.ERROR, null, mockEfObject.Object, errorCode, ErrorClass.Runtime_CSDL);

                            Assert.True(RuntimeMetadataValidator.IsOpenInEditorError(error, artifact));
                        }
                    });
        }

#if !VS14
        [Fact]
        public void IsOpenInEditorError_returns_false_for_SchemaValidationError_for_ModificationFunctionMapping()
        {
            Assert.False(
                IsOpenInEditorError<ModificationFunctionMapping>((int)MappingErrorCode.XmlSchemaValidationError));
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_XmlError_for_ReferentialConstraintRole()
        {
            Assert.False(
                IsOpenInEditorError<ReferentialConstraintRole>((int)ErrorCode.XmlError));
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_ConditionError_for_Condition()
        {
            Assert.False(
                IsOpenInEditorError<Condition>((int)MappingErrorCode.ConditionError));
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_InvalidPropertyInRelationshipConstraint_for_ReferentialConstraint()
        {
            Assert.False(
                IsOpenInEditorError<ReferentialConstraint>((int)ErrorCode.InvalidPropertyInRelationshipConstraint));
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE()
        {
            Assert.False(
                IsOpenInEditorError<EFObject>(ErrorCodes.ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE));
        }

        [Fact]
        public void IsOpenInEditorError_returns_false_for_NotInNamespace_for_ComplexConceptualProperty()
        {
            Assert.False(
                IsOpenInEditorError<ComplexConceptualProperty>((int)ErrorCode.NotInNamespace));
        }
#endif

        private static bool IsOpenInEditorError<T>(int errorCode) where T : EFObject
        {
            var openInEditor = true;

            SetupModelAndInvokeAction(
                null, null, null,
                (mockModelManager, mockArtifactSet) =>
                    {
                        var artifact = mockArtifactSet.Object.GetEntityDesignArtifact();

                        var mockEfObject =
                            new Mock<T>(null, new XElement("dummy"));
                        mockEfObject.Setup(m => m.Artifact).Returns(artifact);

                        Mock.Get(artifact)
                            .Setup(m => m.FindEFObjectForLineAndColumn(It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(mockEfObject.Object);

                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, null, mockEfObject.Object,
                            errorCode, ErrorClass.Runtime_CSDL);

                        openInEditor = RuntimeMetadataValidator.IsOpenInEditorError(error, artifact);
                    });

            return openInEditor;
        }

        private static void SetupModelAndInvokeAction(
            string conceptualModel, string storeModel, string mappingModel, Action<Mock<ModelManager>, Mock<EFArtifactSet>> action)
        {
            var tempUri = new Uri("http://tempuri");

            var mockModelManager =
                new Mock<ModelManager>(new Mock<IEFArtifactFactory>().Object, new Mock<IEFArtifactSetFactory>().Object);

            using (var modelManager = mockModelManager.Object)
            {
                var xmlModelProvider = new Mock<XmlModelProvider>();
                xmlModelProvider
                    .Setup(m => m.GetXmlModel(tempUri))
                    .Returns(new Mock<XmlModel>().Object);

                var mockArtifact =
                    new Mock<EntityDesignArtifact>(modelManager, tempUri, xmlModelProvider.Object);

                mockArtifact
                    .Setup(m => m.Artifact)
                    .Returns(mockArtifact.Object);

                mockArtifact
                    .Setup(m => m.FindEFObjectForLineAndColumn(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(mockArtifact.Object);

                mockArtifact
                    .Setup(m => m.Uri)
                    .Returns(tempUri);

                SetupConceptualModel(conceptualModel, mockArtifact);
                SetupStoreModel(storeModel, mockArtifact);
                SetupMappingModel(mappingModel, mockArtifact);

                mockArtifact.Object.SetValidityDirtyForErrorClass(ErrorClass.Runtime_All, true);

                var mockArtifactSet = new Mock<EFArtifactSet>(mockArtifact.Object) { CallBase = true };
                mockArtifact
                    .Setup(m => m.ArtifactSet)
                    .Returns(mockArtifactSet.Object);

                action(mockModelManager, mockArtifactSet);
            }
        }

        private static void SetupStoreModel(string storeModel, Mock<EntityDesignArtifact> mockArtifact)
        {
            if (storeModel != null)
            {
                var mockStorageModel =
                    new Mock<StorageEntityModel>(
                        mockArtifact.Object,
                        XElement.Parse(storeModel));

                mockStorageModel
                    .Setup(m => m.Artifact)
                    .Returns(mockArtifact.Object);

                mockArtifact
                    .Setup(m => m.StorageModel)
                    .Returns(mockStorageModel.Object);
            }
        }

        private static void SetupConceptualModel(string conceptualModel, Mock<EntityDesignArtifact> mockArtifact)
        {
            if (conceptualModel != null)
            {
                var mockConceptualModel =
                    new Mock<ConceptualEntityModel>(
                        mockArtifact.Object,
                        XElement.Parse(conceptualModel));

                mockConceptualModel
                    .Setup(m => m.Artifact)
                    .Returns(mockArtifact.Object);

                mockArtifact
                    .Setup(m => m.ConceptualModel)
                    .Returns(mockConceptualModel.Object);
            }
        }

        private static void SetupMappingModel(string mappingModel, Mock<EntityDesignArtifact> mockArtifact)
        {
            if (mappingModel != null)
            {
                var mockMappingModel =
                    new Mock<MappingModel>(
                        mockArtifact.Object,
                        XElement.Parse(mappingModel));

                mockMappingModel
                    .Setup(m => m.Artifact)
                    .Returns(mockArtifact.Object);

                mockArtifact
                    .Setup(m => m.MappingModel)
                    .Returns(mockMappingModel.Object);
            }
        }
    }
}
