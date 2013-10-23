// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using EFDesignerTestInfrastructure.VS;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Refactoring;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class RefactorRenameTests
    {
        public TestContext TestContext { get; set; }

        private readonly IEdmPackage _package;

        public RefactorRenameTests()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
            _package = PackageManager.Package;
        }

        private const string PubSimpleProgramText = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace {0}
{{
    class Program
    {{
        static void Main(string[] args)
        {{
            using (var c = new PUBSEntities())
            {{
                author author = new author() {{ au_id = ""foo"" }};
                c.AddToauthors(author);

                foreach (var i in c.authors)
                {{}}
            }}
        }}
    }}
}}";

        private const string RefactorRenameEntityResult = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorRenameEntity
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var c = new PUBSEntities())
            {
                renamedAuthor author = new renamedAuthor() { au_id = ""foo"" };
                c.AddTorenamedAuthor(author);

                foreach (var i in c.renamedAuthor)
                {}
            }
        }
    }
}";

        private const string RefactorRenamePropertyResult = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorRenameProperty
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var c = new PUBSEntities())
            {
                author author = new author() { renamedId = ""foo"" };
                c.AddToauthors(author);

                foreach (var i in c.authors)
                {}
            }
        }
    }
}";
        // [TestMethod, HostType("VS IDE")] http://entityframework.codeplex.com/workitem/992
        public void RefactorRenameEntity()
        {
            RefactorRenameTest(
                "RefactorRenameEntity", (artifact, cpc, programDocData) =>
                    {
                        var authorType = ModelHelper.FindEntityType(artifact.ConceptualModel, "author");
                        Assert.IsNotNull(authorType, "Could not find author type in the model");

                        RefactorEFObject.RefactorRenameElement(authorType, "renamedAuthor", false);

                        var textLines = VSHelpers.GetVsTextLinesFromDocData(programDocData);
                        Assert.IsNotNull(textLines, "Could not get VsTextLines for program DocData");

                        Assert.AreEqual(
                            RefactorRenameEntityResult, VSHelpers.GetTextFromVsTextLines(textLines), "Refactor results are incorrect");

                        var renamedAuthorType = ModelHelper.FindEntityType(artifact.ConceptualModel, "renamedAuthor");
                        Assert.IsNotNull(renamedAuthorType, "Could not find renamedAuthor type in the model");
                    });
        }

        // [TestMethod, HostType("VS IDE")] http://entityframework.codeplex.com/workitem/992
        public void RefactorRenameProperty()
        {
            RefactorRenameTest(
                "RefactorRenameProperty", (artifact, cpc, programDocData) =>
                    {
                        var authorType = ModelHelper.FindEntityType(artifact.ConceptualModel, "author");
                        Assert.IsNotNull(authorType, "Could not find author type in the model");

                        var idProperty = ModelHelper.FindProperty(authorType, "au_id");
                        Assert.IsNotNull(idProperty, "Could not find au_id property in the model");

                        RefactorEFObject.RefactorRenameElement(idProperty, "renamedId", false);

                        var textLines = VSHelpers.GetVsTextLinesFromDocData(programDocData);
                        Assert.IsNotNull(textLines, "Could not get VsTextLines for program DocData");

                        Assert.AreEqual(
                            RefactorRenamePropertyResult, VSHelpers.GetTextFromVsTextLines(textLines), "Refactor results are incorrect");

                        authorType = ModelHelper.FindEntityType(artifact.ConceptualModel, "author");
                        Assert.IsNotNull(authorType, "Could not find author type in the model");

                        var renamedIdProperty = ModelHelper.FindProperty(authorType, "renamedId");
                        Assert.IsNotNull(renamedIdProperty, "Could not find renamedId property in the model");
                    });
        }

        #region Helper Method

        private void RefactorRenameTest(string projectName, Action<EntityDesignArtifact, CommandProcessorContext, object> test)
        {
            var modelEdmxFilePath = Path.Combine(TestContext.DeploymentDirectory, @"TestData\Model\v3\PubSimple.edmx");
            var dte = VsIdeTestHostContext.Dte;
            var serviceProvider = VsIdeTestHostContext.ServiceProvider;

            UITestRunner.Execute(
                () =>
                    {
                        EntityDesignArtifact entityDesignArtifact = null;
                        try
                        {
                            var project = dte.CreateProject(
                                TestContext.TestRunDirectory,
                                projectName,
                                DteExtensions.ProjectKind.Executable,
                                DteExtensions.ProjectLanguage.CSharp);

                            var projectItem = dte.AddExistingItem(new FileInfo(modelEdmxFilePath).FullName, project);
                            dte.OpenFile(projectItem.FileNames[0]);
                            entityDesignArtifact =
                                (EntityDesignArtifact)new EFArtifactHelper(EFArtifactHelper.GetEntityDesignModelManager(serviceProvider))
                                                          .GetNewOrExistingArtifact(TestUtils.FileName2Uri(projectItem.FileNames[0]));

                            var editingContext =
                                _package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(entityDesignArtifact.Uri);
                            var cpc = new CommandProcessorContext(
                                editingContext, "DiagramTest" + projectName, "DiagramTestTxn" + projectName, entityDesignArtifact);

                            var programDocData = VSHelpers.GetDocData(
                                serviceProvider, Path.Combine(Path.GetDirectoryName(project.FullName), "Program.cs"));
                            Debug.Assert(programDocData != null, "Could not get DocData for program file");

                            var textLines = VSHelpers.GetVsTextLinesFromDocData(programDocData);
                            Debug.Assert(textLines != null, "Could not get VsTextLines for program DocData");

                            VsUtils.SetTextForVsTextLines(textLines, string.Format(PubSimpleProgramText, projectName));
                            test(entityDesignArtifact, cpc, programDocData);
                        }
                        catch (Exception ex)
                        {
                            TestContext.WriteLine(ex.ToString());
                            throw;
                        }
                        finally
                        {
                            if (entityDesignArtifact != null)
                            {
                                entityDesignArtifact.Dispose();
                            }

                            dte.CloseSolution(false);
                        }
                    });
        }

        #endregion
    }
}
