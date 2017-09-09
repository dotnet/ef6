// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class ModelHelperTests
    {
        [Fact]
        public void GetPrimitiveTypeFromString_returns_correct_type_for_valid_EDM_type_names()
        {
            Assert.Equal(PrimitiveTypeKind.Binary, ModelHelper.GetPrimitiveTypeFromString("Binary").PrimitiveTypeKind);
        }

        [Fact]
        public void GetPrimitiveTypeFromString_returns_null_for_invalid_EDM_type()
        {
            Assert.Null(ModelHelper.GetPrimitiveTypeFromString("Int23"));
        }

        [Fact]
        public void IsValidFacet_returns_true_for_valid_facet()
        {
            Assert.True(ModelHelper.IsValidModelFacet("String", "MaxLength"));
        }

        [Fact]
        public void IsValidFacet_returns_false_for_invalid_facet()
        {
            Assert.False(ModelHelper.IsValidModelFacet("Int32", "MaxLength"));
            Assert.False(ModelHelper.IsValidModelFacet("Int32", "42"));
        }

        [Fact]
        public void TryGetFacet_returns_true_and_facet_description_for_valid_facet()
        {
            FacetDescription facetDescription;

            Assert.True(
                ModelHelper.TryGetFacet(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), "MaxLength", out facetDescription));
            Assert.NotNull(facetDescription);
            Assert.Equal("MaxLength", facetDescription.FacetName);
        }

        [Fact]
        public void TryGetFacet_returns_false_and_null_for_invalid_facet()
        {
            FacetDescription facetDescription;

            Assert.False(
                ModelHelper.TryGetFacet(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), "MaxLength", out facetDescription));
            Assert.Null(facetDescription);
        }

        [Fact]
        public void TryGetFacet_returns_false_and_null_for_null_type()
        {
            FacetDescription facetDescription;

            Assert.False(ModelHelper.TryGetFacet(null, "MaxLength", out facetDescription));
            Assert.Null(facetDescription);
        }

        [Fact]
        public void GetAllPrimitiveTypes_returns_all_primitive_types_for_version()
        {
            foreach (var schemaVersion in EntityFrameworkVersion.GetAllVersions())
            {
                // remove geo spatial types for schema versions V1 and V2
                var expectedTypes =
                    PrimitiveType
                        .GetEdmPrimitiveTypes()
                        .Where(t => schemaVersion == EntityFrameworkVersion.Version3 || (!IsGeoSpatialType(t) && !IsHierardchyIdType(t)))
                        .Select(t => t.Name);

                Assert.Equal(expectedTypes, ModelHelper.AllPrimitiveTypes(schemaVersion));
            }
        }

        [Fact]
        public void CreateSetDesignerPropertyValueCommandFromArtifact_returns_non_null_for_existing_property_with_non_matching_value()
        {
            // this test just tests that CreateSetDesignerPropertyValueCommandFromArtifact() calls
            // CreateSetDesignerPropertyCommandInsideDesignerInfo(). The tests that the latter works
            // correctly are below.
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                const string designerPropertyName = "TestPropertyName";
                designerInfoRoot
                    .AddDesignerInfo(
                        "Options",
                        SetupOptionsDesignerInfo(designerPropertyName, "TestValue"));

                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.NotNull(
                    ModelHelper.CreateSetDesignerPropertyValueCommandFromArtifact(
                        entityDesignArtifactMock.Object, "Options", designerPropertyName, "NewValue"));
            }
        }

        [Fact]
        public void CreateSetDesignerPropertyCommandInsideDesignerInfo_returns_null_for_existing_property_with_matching_value()
        {
            const string designerPropertyName = "TestPropertyName";
            const string designerPropertyValue = "TestValue";
            using (var designerInfo = SetupOptionsDesignerInfo(designerPropertyName, designerPropertyValue))
            {
                Assert.Null(
                    ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        designerInfo,
                        designerPropertyName,
                        designerPropertyValue));
            }
        }

        [Fact]
        public void
            CreateSetDesignerPropertyCommandInsideDesignerInfo_returns_UpdateDefaultableValueCommand_for_existing_property_with_non_matching_value
            ()
        {
            const string designerPropertyName = "TestPropertyName";
            using (var designerInfo = SetupOptionsDesignerInfo(designerPropertyName, "TestValue"))
            {
                Assert.IsType<UpdateDefaultableValueCommand<string>>(
                    ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        designerInfo,
                        designerPropertyName,
                        "DifferentValue"));
            }
        }

        [Fact]
        public void CreateSetDesignerPropertyCommandInsideDesignerInfo_passed_null_value_returns_null_for_non_existent_property()
        {
            using (var designerInfo = SetupOptionsDesignerInfo(null, null))
            {
                Assert.Null(
                    ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        designerInfo,
                        "TestPropertyName",
                        null));
            }
        }

        [Fact]
        public void
            CreateSetDesignerPropertyCommandInsideDesignerInfo_passed_non_null_value_returns_ChangeDesignerPropertyCommand_for_non_existent_property
            ()
        {
            using (var designerInfo = SetupOptionsDesignerInfo(null, null))
            {
                Assert.IsType<ChangeDesignerPropertyCommand>(
                    ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        designerInfo, "TestPropertyName",
                        "NewValue"));
            }
        }

        private DesignerInfo SetupOptionsDesignerInfo(string designerPropertyName, string designerPropertyValue)
        {
            var designerInfo =
                new OptionsDesignerInfo(
                    null,
                    XElement.Parse(
                        "<Options xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
            var designerInfoPropertySet =
                new DesignerInfoPropertySet(
                    designerInfo,
                    XElement.Parse(
                        "<DesignerInfoPropertySet xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
            if (designerPropertyName != null)
            {
                var designerProperty =
                    new DesignerProperty(
                        designerInfoPropertySet,
                        XElement.Parse(
                            "<DesignerProperty Name='" + designerPropertyName + "' Value='" +
                            designerPropertyValue +
                            "' xmlns='http://schemas.microsoft.com/ado/2009/11/edmx' />"));
                designerInfoPropertySet.AddDesignerProperty(designerPropertyName, designerProperty);
            }

            designerInfo.PropertySet = designerInfoPropertySet;
            return designerInfo;
        }

        private static bool IsGeoSpatialType(PrimitiveType type)
        {
            return type.PrimitiveTypeKind >= PrimitiveTypeKind.Geometry &&
                   type.PrimitiveTypeKind <= PrimitiveTypeKind.GeographyCollection;
        }

        private static bool IsHierardchyIdType(PrimitiveType type)
        {
            return type.PrimitiveTypeKind == PrimitiveTypeKind.HierarchyId;
        }
    }
}
