// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using EFDesignerTestInfrastructure.VS;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class SafeModeTestsRemote
    {
        private readonly EFArtifactHelper _efArtifactHelper =
            new EFArtifactHelper(EFArtifactHelper.GetEntityDesignModelManager(VsIdeTestHostContext.ServiceProvider));

        public TestContext TestContext { get; set; }

        private string ModelsDirectory
        {
            get { return Path.Combine(TestContext.DeploymentDirectory, @"TestData\InProc"); }
        }

        private string ModelValidationDirectory
        {
            get { return Path.Combine(TestContext.DeploymentDirectory, @"TestData\Model\ValidationSamples"); }
        }

        public SafeModeTestsRemote()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsModelWithXmlParserErrors()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "SafeModeTests.PubsModelWithXmlParserErrors.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsModelWithXSDErrors()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "SafeModeTests.PubsModelWithXSDErrors.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsModelWithEdmxXSDErrors()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "SafeModeTests.PubsModelWithEdmxXSDErrors.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsMinimalWithFunctionMapping()
        {
            Assert.IsTrue(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "PubsMinimalWithFunctionMapping.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsMinimalWithFunctionMappingV2()
        {
            Assert.IsTrue(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "PubsMinimalWithFunctionMappingV2.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PubsMinimalWithFunctionMappingAndEdmxXsdError()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "SafeModeTests.PubsMinimalWithFunctionMappingAndEDMXSchemaError.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void UndefinedComplexPropertyType()
        {
            Assert.IsTrue(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelsDirectory, "UndefinedComplexPropertyType.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void CircularInheritance()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelValidationDirectory, "CircularInheritance.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void EntityTypeWithNoEntitySet()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelValidationDirectory, "EntityTypeWithNoEntitySet.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void MultipleEntitySetsPerType()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelValidationDirectory, "MultipleEntitySetsPerType.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void AssociationWithoutAssociationSet()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    Path.Combine(ModelValidationDirectory, "AssociationWithoutAssociationSet.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedEdmxTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}Edmx",
                        "NonQualifiedEdmxTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedDesignerTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}Designer",
                        "NonQualifiedDesignerTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedRuntimeTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}Runtime",
                        "NonQualifiedRuntimeTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedMappingsTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}Mappings",
                        "NonQualifiedMappingsTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedConceptualModelsTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}ConceptualModels",
                        "NonQualifiedConceptualModelsTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedStorageModelsTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2007/06/edmx}StorageModels",
                        "NonQualifiedStorageModelsTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedMappingTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{urn:schemas-microsoft-com:windows:storage:mapping:CS}Mapping",
                        "NonQualifiedMappingTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedConceptualSchemaTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2006/04/edm}Schema",
                        "NonQualifiedConceptualSchemaTag.edmx")));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NonQualifiedStorageSchemaTag()
        {
            Assert.IsFalse(
                IsArtifactDesignerSafe(
                    GenerateInvalidEdmx(
                        "{http://schemas.microsoft.com/ado/2006/04/edm/ssdl}Schema",
                        "NonQualifiedStorageSchemaTag.edmx")));
        }

        private string GenerateInvalidEdmx(XName elementName, string destinationPath)
        {
            var sourceModel = XDocument.Load(Path.Combine(ModelsDirectory, "PubsMinimal.edmx"));
            var elementToChange = sourceModel.Descendants(elementName).Single();
            elementToChange.Name = "{http://tempuri.org}" + elementToChange.Name.LocalName;
            sourceModel.Save(destinationPath);

            return destinationPath;
        }

        /* TODO: How much value these add? Maybe just need to be removed
        [TestMethod, HostType("VS IDE")]
        public void LoadInvalidSampleFiles()
        {
            UITestRunner(delegate()
            {
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "TwoThree.EmptySet.cs.edmx", false); // contains error 2063
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "EdmRally.edmx", false); // contains error 3023
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "AbstractSimpleMappingError1.edmx", false); // contains error 2078
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "ComplexType.Condition.edmx", true); // contains error 2016
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "EntitySplitting_Same_EdmProperty_Maps_Different_Store_Type.edmx", false); // contains error 2039
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "EntitySplitting_Same_EdmProperty_Maps_Same_Store_Type_Non_Promotable_Facets.edmx", false); // contains error 2039
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "ExtraIllegalElement.edmx", true); // contains error 102, 2025
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "MemberTypeMismatch.CS.edmx", false); // contains error 2007,2063
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "NullableComplexType.edmx", true); // contains error 157, 2002
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "TwoThree.InvalidXml2.edmx", true); // contains error 102, 2025
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "TwoThree.PartialMapping.StorageEntityContainerMismatch1.edmx", true); // contains error 2007, 2063, also has a MaxLength schema error
                LoadFileTest(TestContextEntityDesigner.GeneratedEdmxInvalidSamplesDirectory, "UnparsableQueryView.edmx", true); // contains error 2068
            });
        }
        */

        private bool IsArtifactDesignerSafe(string fileName)
        {
            var isArtifactDesignerSafe = true;

            UITestRunner.Execute(TestContext.TestName, 
                () =>
                    {
                        var artifactUri = TestUtils.FileName2Uri(fileName);

                        try
                        {
                            VsIdeTestHostContext.Dte.OpenFile(artifactUri.LocalPath);
                            isArtifactDesignerSafe = _efArtifactHelper.GetNewOrExistingArtifact(artifactUri).IsDesignerSafe;
                        }
                        finally
                        {
                            VsIdeTestHostContext.Dte.CloseDocument(fileName, false);
                        }
                    });

            return isArtifactDesignerSafe;
        }
    }
}
