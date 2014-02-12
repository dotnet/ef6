// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using ModelDesigner = Microsoft.Data.Entity.Design.Model.Designer;
using EntityDesignerViewModel = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
using EntityDesignerView = Microsoft.Data.Entity.Design.EntityDesigner.View;

namespace EFDesigner.InProcTests
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using EFDesigner.InProcTests.Extensions;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Package;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class EntityTypeShapeColorTest
    {
        private readonly IEdmPackage _package;

        private static DTE Dte
        {
            get { return VsIdeTestHostContext.Dte; }
        }

        private static IServiceProvider ServiceProvider
        {
            get { return VsIdeTestHostContext.ServiceProvider; }
        }

        public TestContext TestContext { get; set; }

        public EntityTypeShapeColorTest()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
            _package = PackageManager.Package;
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ChangeEntityTypeShapeFillColorTest()
        {
            var shapeColor = Color.Beige;
            ChangeEntityTypesFillColorTest(
                "ChangeShapeColor",
                shapeColor,
                (artifact, commandProcessor) =>
                    {
                        var entityDesignerDiagram = GetDocData(commandProcessor.EditingContext).GetEntityDesignerDiagram();
                        Assert.IsNotNull(entityDesignerDiagram, "Could not get an instance of EntityDesignerDiagram from editingcontext.");

                        foreach (var ets in entityDesignerDiagram.NestedChildShapes.OfType<EntityDesignerView.EntityTypeShape>())
                        {
                            var entityType = (EntityDesignerViewModel.EntityType)ets.ModelElement;
                            Assert.IsNotNull(entityType);
                            Assert.AreEqual(shapeColor, ets.FillColor);
                        }
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void CopyAndPasteInSingleDiagramTest()
        {
            var shapeColor = Color.Red;
            ChangeEntityTypesFillColorTest(
                "CopyAndPasteSingle",
                shapeColor,
                (artifact, commandProcessor) =>
                    {
                        var entityDesignerDiagram = GetDocData(commandProcessor.EditingContext).GetEntityDesignerDiagram();
                        Assert.IsNotNull(entityDesignerDiagram, "Could not get an instance of EntityDesignerDiagram from editingcontext.");

                        var author = entityDesignerDiagram.GetShape("author");
                        Assert.IsNotNull(author, "Could not get DSL entity type shape instance of 'author'.");

                        var titleAuthor = entityDesignerDiagram.GetShape("titleauthor");
                        Assert.IsNotNull(titleAuthor, "Could not get DSL entity type shape instance of 'titleauthor'.");

                        entityDesignerDiagram.SelectDiagramItems(new[] { author, titleAuthor });

                        DesignerUtilities.Copy(Dte);
                        DesignerUtilities.Paste(Dte);

                        var authorCopy = entityDesignerDiagram.GetShape("author1");
                        Assert.IsNotNull(authorCopy, "Entity: 'author1' should have been created.");
                        var authorCopyModel =
                            (EntityType)
                            authorCopy.TypedModelElement.EntityDesignerViewModel.ModelXRef.GetExisting(authorCopy.TypedModelElement);
                        Assert.IsNotNull(
                            authorCopyModel.GetAntiDependenciesOfType<AssociationEnd>().FirstOrDefault(),
                            "The association between author1 and titleauthor1 should have been created.");
                        Assert.AreEqual(shapeColor, authorCopy.FillColor);

                        var titleAuthorCopy = entityDesignerDiagram.GetShape("titleauthor1");
                        Assert.IsNotNull(titleAuthorCopy, "Entity: 'titleauthor1' should have been created.");
                        Assert.AreEqual(shapeColor, titleAuthorCopy.FillColor);
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void CopyAndPasteInMultipleDiagramsTest()
        {
            var shapeColor = Color.Brown;
            ChangeEntityTypesFillColorTest(
                "CopyAndPasteMulti",
                shapeColor,
                (artifact, commandProcessorContext) =>
                    {
                        var docData = GetDocData(commandProcessorContext.EditingContext);
                        var entityDesignerDiagram = docData.GetEntityDesignerDiagram();
                        Assert.IsNotNull(entityDesignerDiagram, "Could not get an instance of EntityDesignerDiagram from editingcontext.");

                        var author = entityDesignerDiagram.GetShape("author");
                        Assert.IsNotNull(author, "Could not get DSL entity type shape instance of 'author'.");

                        var titleAuthor = entityDesignerDiagram.GetShape("titleauthor");
                        Assert.IsNotNull(titleAuthor, "Could not get DSL entity type shape instance of 'titleauthor'.");

                        entityDesignerDiagram.SelectDiagramItems(new[] { author, titleAuthor });
                        DesignerUtilities.Copy(Dte);

                        var diagram = CreateDiagramCommand.CreateDiagramWithDefaultName(commandProcessorContext);
                        docData.OpenDiagram(diagram.Id.Value);

                        DesignerUtilities.Paste(Dte);

                        // Get the newly created diagram.
                        entityDesignerDiagram = docData.GetEntityDesignerDiagram(diagram.Id.Value);

                        author = entityDesignerDiagram.GetShape("author");
                        Assert.IsNotNull(author, "Entity: 'author' should exists in diagram:" + diagram.Name);

                        titleAuthor = entityDesignerDiagram.GetShape("titleauthor");
                        Assert.IsNotNull(titleAuthor, "Entity: 'titleauthor' should exists in diagram:" + diagram.Name);

                        var associationConnector =
                            entityDesignerDiagram.NestedChildShapes.OfType<EntityDesignerView.AssociationConnector>().FirstOrDefault();
                        Assert.IsNotNull(
                            associationConnector,
                            "There should have been association connector created between entities 'author' and 'titleauthor'.");

                        var entityDesignerViewModel = entityDesignerDiagram.ModelElement;
                        Assert.IsNotNull(entityDesignerViewModel, "Diagram's ModelElement is not a type of EntityDesignerViewModel");

                        var association = (Association)entityDesignerViewModel.ModelXRef.GetExisting(associationConnector.ModelElement);
                        Assert.IsNotNull(
                            association,
                            "Could not find association for associationConnector" + associationConnector.AccessibleName
                            + " from Model Xref.");

                        var entityTypesInAssociation = association.AssociationEnds().Select(ae => ae.Type.Target).Distinct().ToList();
                        Assert.AreEqual(2, entityTypesInAssociation.Count);
                        Assert.IsFalse(
                            entityTypesInAssociation.Any(et => et.LocalName.Value != "author" && et.LocalName.Value != "titleauthor"),
                            "The association between author and title-author is not created in diagram: " + diagram.Name);

                        Assert.AreEqual(shapeColor, author.FillColor);
                        Assert.AreEqual(shapeColor, titleAuthor.FillColor);
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void AddRelatedItemsTest()
        {
            var shapeColor = Color.Cyan;
            ChangeEntityTypesFillColorTest(
                "AddRelatedItems",
                shapeColor,
                (artifact, commandProcessorContext) =>
                    {
                        var diagram = CreateDiagramCommand.CreateDiagramWithDefaultName(commandProcessorContext);
                        var docData = GetDocData(commandProcessorContext.EditingContext);
                        docData.OpenDiagram(diagram.Id.Value);

                        CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                            commandProcessorContext,
                            diagram,
                            (ConceptualEntityType)artifact.ConceptualModel.EntityTypes().Single(et => et.LocalName.Value == "author"),
                            shapeColor, false);

                        var entityDesignerDiagram = docData.GetEntityDesignerDiagram(diagram.Id.Value);
                        var author = entityDesignerDiagram.GetShape("author");
                        Assert.IsNotNull(author, "Could not get DSL entity type shape instance of 'author'.");

                        entityDesignerDiagram.SelectDiagramItems(new[] { author });
                        Dte.ExecuteCommand("OtherContextMenus.MicrosoftDataEntityDesignContext.IncludeRelated");

                        var titleauthor = entityDesignerDiagram.GetShape("titleauthor");
                        Assert.IsNotNull(titleauthor, "Could not get DSL entity type shape instance of 'titleauthor'.");

                        Assert.AreEqual(shapeColor, author.FillColor);
                        Assert.AreEqual(shapeColor, titleauthor.FillColor);
                    });
        }

        private void ChangeEntityTypesFillColorTest(
            string projectName, Color fillColor, Action<EntityDesignArtifact, CommandProcessorContext> runTest)
        {
            var modelPath = Path.Combine(TestContext.DeploymentDirectory, @"TestData\Model\v3\PubSimple.edmx");

            UITestRunner.Execute(TestContext.TestName, 
                () =>
                    {
                        EntityDesignArtifact entityDesignArtifact = null;
                        Project project = null;
                        try
                        {
                            project = Dte.CreateProject(
                                TestContext.TestRunDirectory,
                                projectName,
                                DteExtensions.ProjectKind.Executable,
                                DteExtensions.ProjectLanguage.CSharp);

                            var projectItem = Dte.AddExistingItem(modelPath, project);
                            Dte.OpenFile(projectItem.FileNames[0]);

                            entityDesignArtifact =
                                (EntityDesignArtifact)new EFArtifactHelper(EFArtifactHelper.GetEntityDesignModelManager(ServiceProvider))
                                                          .GetNewOrExistingArtifact(TestUtils.FileName2Uri(projectItem.FileNames[0]));

                            var editingContext =
                                _package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(entityDesignArtifact.Uri);
                            var commandProcessorContext = new CommandProcessorContext(
                                editingContext, "DiagramTest", "DiagramTestTxn", entityDesignArtifact);

                            foreach (var ets in entityDesignArtifact.DesignerInfo.Diagrams.FirstDiagram.EntityTypeShapes)
                            {
                                CommandProcessor.InvokeSingleCommand(
                                    commandProcessorContext, new UpdateDefaultableValueCommand<Color>(ets.FillColor, fillColor));
                            }

                            runTest(entityDesignArtifact, commandProcessorContext);
                        }
                        finally
                        {
                            if (entityDesignArtifact != null)
                            {
                                entityDesignArtifact.Dispose();
                            }

                            if (project != null)
                            {
                                Dte.CloseSolution(false);
                            }
                        }
                    });
        }

        private static MicrosoftDataEntityDesignDocData GetDocData(EditingContext editingContext)
        {
            Debug.Assert(editingContext != null, "editingContext != null");

            var artifactService = editingContext.GetEFArtifactService();
            return (MicrosoftDataEntityDesignDocData)VSHelpers.GetDocData(ServiceProvider, artifactService.Artifact.Uri.LocalPath);
        }
    }
}
