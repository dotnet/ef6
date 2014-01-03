// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.VisualStudio.Model.Commands;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    /// <summary>
    ///     The purpose of the tests are:
    ///     - MigrateDiagramInformationCommand works as expected.
    ///     - To ensure that basic designer functionalities are still working after diagrams nodes are moved to separate file.
    /// </summary>
    [TestClass]
    public class MigrateDiagramNodesTest
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

        public MigrateDiagramNodesTest()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
            _package = PackageManager.Package;
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SimpleAddEntity()
        {
            ExecuteMigrateDiagramNodesTest(
                "SimpleAddEntity",
                (artifact, commandProcessorContext) =>
                    {
                        var cet =
                            CreateEntityTypeCommand.CreateConceptualEntityTypeAndEntitySetAndProperty(
                                commandProcessorContext,
                                "entity1",
                                "entity1set",
                                true,
                                "id",
                                "String",
                                ModelConstants.StoreGeneratedPattern_Identity,
                                true);
                        Assert.IsNotNull(cet != null, "EntityType is not created");

                        // Verify that EntityTypeShape is created in diagram1.
                        Assert.IsNotNull(
                            artifact.DiagramArtifact.Diagrams.FirstDiagram.EntityTypeShapes.SingleOrDefault(
                                ets => ets.EntityType.Target == cet));
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SimpleDeleteEntity()
        {
            ExecuteMigrateDiagramNodesTest(
                "SimpleDeleteEntity",
                (artifact, commandProcessorContext) =>
                    {
                        var entity = artifact.ConceptualModel.EntityTypes().Single(et => et.Name.Value == "employee");
                        Assert.IsNotNull(entity, "Could not find Employee entity type.");

                        CommandProcessor.InvokeSingleCommand(commandProcessorContext, entity.GetDeleteCommand());
                        Assert.IsTrue(entity.IsDisposed);
                        Assert.IsTrue(
                            !artifact.DiagramArtifact.Diagrams.FirstDiagram.EntityTypeShapes.Any(
                                ets => ets.EntityType.Target.LocalName == entity.LocalName));
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SimpleUndoRedo()
        {
            ExecuteMigrateDiagramNodesTest(
                "SimpleUndoRedo",
                (artifact, commandProcessorContext) =>
                    {
                        var baseType =
                            (ConceptualEntityType)
                            CreateEntityTypeCommand.CreateEntityTypeAndEntitySetWithDefaultNames(commandProcessorContext);
                        var derivedType = CreateEntityTypeCommand.CreateDerivedEntityType(
                            commandProcessorContext, "SubType", baseType, true);

                        Dte.ExecuteCommandForOpenDocument(artifact.LocalPath(), "Edit.Undo");
                        Assert.IsTrue(!artifact.ConceptualModel.EntityTypes().Any(et => et.LocalName.Value == derivedType.LocalName.Value));

                        Dte.ExecuteCommandForOpenDocument(artifact.LocalPath(), "Edit.Undo");
                        Assert.IsTrue(!artifact.ConceptualModel.EntityTypes().Any(et => et.LocalName.Value == baseType.LocalName.Value));

                        Dte.ExecuteCommandForOpenDocument(artifact.LocalPath(), "Edit.Redo");
                        Dte.ExecuteCommandForOpenDocument(artifact.LocalPath(), "Edit.Redo");

                        Assert.IsNotNull(
                            artifact.ConceptualModel.EntityTypes().SingleOrDefault(et => et.LocalName.Value == baseType.LocalName.Value));

                        // Verify that derived type and inheritance are recreated.
                        derivedType =
                            (ConceptualEntityType)
                            artifact.ConceptualModel.EntityTypes().SingleOrDefault(et => et.LocalName.Value == derivedType.LocalName.Value);
                        Assert.IsNotNull(derivedType);
                        Assert.AreEqual(baseType.LocalName.Value, derivedType.BaseType.Target.LocalName.Value);
                    });
        }

        private void ExecuteMigrateDiagramNodesTest(string projectName, Action<EntityDesignArtifact, CommandProcessorContext> runTest)
        {
            var modelPath = Path.Combine(TestContext.DeploymentDirectory, @"TestData\Model\v3\PubSimple.edmx");

            UITestRunner.Execute(
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

                            Debug.Assert(entityDesignArtifact != null);
                            var editingContext =
                                _package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(entityDesignArtifact.Uri);

                            // Create TransactionContext to indicate that the transactions are done from first diagram.
                            // This is not used by MigrateDiagramInformationCommand but other commands in the callback methods.
                            var transactionContext = new EfiTransactionContext();
                            transactionContext.Add(
                                EfiTransactionOriginator.TransactionOriginatorDiagramId,
                                new DiagramContextItem(entityDesignArtifact.DesignerInfo.Diagrams.FirstDiagram.Id.Value));

                            var commandProcessorContext =
                                new CommandProcessorContext(
                                    editingContext, "MigrateDiagramNodesTest", projectName + "Txn", entityDesignArtifact, transactionContext);
                            MigrateDiagramInformationCommand.DoMigrate(commandProcessorContext, entityDesignArtifact);

                            Debug.Assert(entityDesignArtifact.DiagramArtifact != null);
                            Debug.Assert(
                                entityDesignArtifact.IsDesignerSafe,
                                "Artifact should not be in safe mode after MigrateDiagramInformationCommand is executed.");
                            Debug.Assert(
                                new Uri(entityDesignArtifact.Uri.LocalPath + EntityDesignArtifact.ExtensionDiagram)
                                == entityDesignArtifact.DiagramArtifact.Uri);

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
    }
}
