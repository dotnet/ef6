// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests.EntityDesign.VisualStudio
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class EdmUpdateSolutionEventsTests
    {
        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_no_designer_info()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifact =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider).Object;

            Assert.True(EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifact));
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_designer_info_does_not_contain_options()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("root")))
            {
                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.True(EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
            }
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_designer_info_option_does_not_contain_ValidateOnBuild_property()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                designerInfoRoot
                    .AddDesignerInfo(
                        "Options",
                        new Mock<OptionsDesignerInfo>(designerInfoRoot, new XElement("_")).Object);

                entityDesignArtifactMock
                    .Setup(a => a.DesignerInfo)
                    .Returns(designerInfoRoot);

                Assert.True(EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
            }
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_designer_info_option_does_not_contain_ValidateOnBuild_value()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                using (var designerProperty = new DesignerProperty(null, new XElement("_")))
                {
                    var optionsDesignerInfoMock = new Mock<OptionsDesignerInfo>(designerInfoRoot, new XElement("_"));
                    optionsDesignerInfoMock
                        .Setup(o => o.ValidateOnBuild)
                        .Returns(designerProperty);

                    designerInfoRoot
                        .AddDesignerInfo(
                            "Options",
                            optionsDesignerInfoMock.Object);

                    entityDesignArtifactMock
                        .Setup(a => a.DesignerInfo)
                        .Returns(designerInfoRoot);

                    Assert.True(
                        EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
                }
            }
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_false_if_ValidateOnBuild_set_to_false()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                using (
                    var designerProperty =
                        new DesignerProperty(
                            null,
                            new XElement("_", new XAttribute("Value", "false"))))
                {
                    var optionsDesignerInfoMock = new Mock<OptionsDesignerInfo>(designerInfoRoot, new XElement("_"));
                    optionsDesignerInfoMock
                        .Setup(o => o.ValidateOnBuild)
                        .Returns(designerProperty);

                    designerInfoRoot
                        .AddDesignerInfo(
                            "Options",
                            optionsDesignerInfoMock.Object);

                    entityDesignArtifactMock
                        .Setup(a => a.DesignerInfo)
                        .Returns(designerInfoRoot);

                    Assert.False(
                        EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
                }
            }
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_ValidateOnBuild_set_to_true()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                using (
                    var designerProperty =
                        new DesignerProperty(
                            null,
                            new XElement("_", new XAttribute("Value", "true"))))
                {
                    var optionsDesignerInfoMock = new Mock<OptionsDesignerInfo>(designerInfoRoot, new XElement("_"));
                    optionsDesignerInfoMock
                        .Setup(o => o.ValidateOnBuild)
                        .Returns(designerProperty);

                    designerInfoRoot
                        .AddDesignerInfo(
                            "Options",
                            optionsDesignerInfoMock.Object);

                    entityDesignArtifactMock
                        .Setup(a => a.DesignerInfo)
                        .Returns(designerInfoRoot);

                    Assert.True(
                        EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
                }
            }
        }

        [Fact]
        public void ShouldValidateArtifactDuringBuild_returns_true_if_ValidateOnBuild_not_a_valid_bool()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var entityDesignArtifactMock =
                new Mock<EntityDesignArtifact>(modelManager, new Uri("urn:dummy"), modelProvider);

            using (var designerInfoRoot = new EFDesignerInfoRoot(entityDesignArtifactMock.Object, new XElement("_")))
            {
                using (
                    var designerProperty =
                        new DesignerProperty(
                            null,
                            new XElement("_", new XAttribute("Value", "abc"))))
                {
                    var optionsDesignerInfoMock = new Mock<OptionsDesignerInfo>(designerInfoRoot, new XElement("_"));
                    optionsDesignerInfoMock
                        .Setup(o => o.ValidateOnBuild)
                        .Returns(designerProperty);

                    designerInfoRoot
                        .AddDesignerInfo(
                            "Options",
                            optionsDesignerInfoMock.Object);

                    entityDesignArtifactMock
                        .Setup(a => a.DesignerInfo)
                        .Returns(designerInfoRoot);

                    Assert.True(
                        EdmUpdateSolutionEvents.ShouldValidateArtifactDuringBuild(entityDesignArtifactMock.Object));
                }
            }
        }
    }
}
