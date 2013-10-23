// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class DiagramArtifactTests
    {
        [Fact]
        public void DetermineIfArtifactIsDesignerSafe_sets_IsDesignerSafe_to_true_for_valid_diagram_edmx()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;

            using (var diagramArtifact = new DiagramArtifact(modelManager, new Uri("urn:dummy"), modelProvider))
            {
                diagramArtifact.SetXObject(
                    XDocument.Parse("<Edmx Version=\"3.0\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />"));
                diagramArtifact.DetermineIfArtifactIsDesignerSafe();

                Assert.True(diagramArtifact.IsDesignerSafe);
            }
        }

        [Fact]
        public void DetermineIfArtifactIsDesignerSafe_sets_IsDesignerSafe_to_false_for_xml_that_is_not_valid_diagram_edmx()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;

            using (var diagramArtifact = new DiagramArtifact(modelManager, new Uri("urn:dummy"), modelProvider))
            {
                //missing Version attribute makes the Xml below invalid accoring to the edmx schema
                diagramArtifact.SetXObject(
                    XDocument.Parse("<Edmx xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\" />"));
                diagramArtifact.DetermineIfArtifactIsDesignerSafe();

                Assert.False(diagramArtifact.IsDesignerSafe);
            }
        }

        [Fact]
        public void DetermineIfArtifactIsDesignerSafe_sets_IsDesignerSafe_to_false_for_xml_that_is_not_using_edmx_namespace()
        {
            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;

            using (var diagramArtifact = new DiagramArtifact(modelManager, new Uri("urn:dummy"), modelProvider))
            {
                diagramArtifact.SetXObject(
                    XDocument.Parse("<Edmx xmlns=\"bar\" />"));
                diagramArtifact.DetermineIfArtifactIsDesignerSafe();

                Assert.False(diagramArtifact.IsDesignerSafe);
            }
        }
    }
}
