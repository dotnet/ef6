// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;

    // <summary>
    //     Migrate diagrams node from EDMX file to a separate file.
    // </summary>
    internal class MigrateDiagramInformationCommand : Command
    {
        private readonly EntityDesignArtifact _artifact;

        // Intentionally set this to private because we only want this command to be instantiated from static method and not to be combined with other command.
        private MigrateDiagramInformationCommand(EntityDesignArtifact artifact)
        {
            _artifact = artifact;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // First check if the diagram file exists, if yes quit immediately.
            var diagramFilePath = _artifact.Uri.LocalPath + EntityDesignArtifact.ExtensionDiagram;
            if (!File.Exists(diagramFilePath)
                && (_artifact != null))
            {
                // The diagram file should have same structure as the EDMX file but only contains diagrams node.
                // To accomplish that the method will the do the following:
                // - Load the EDMX file to XDocument.
                // - Store Diagrams root node.
                // - Remove all nodes under root node.
                // - Create and add Designer node under root node.
                // - Re-Add Diagrams node under Designer node.
                var document = XDocument.Parse(_artifact.XDocument.ToString(), LoadOptions.PreserveWhitespace);
                var rootDiagramsNode =
                    document.Descendants(XName.Get("Diagrams", SchemaManager.GetEDMXNamespaceName(_artifact.SchemaVersion)))
                        .FirstOrDefault();
                Debug.Assert(rootDiagramsNode != null, "Diagrams node does not exist in the EDMX file.");

                if (rootDiagramsNode != null)
                {
                    // Remove any nodes under root.
                    document.Root.RemoveNodes();
                    // Create an empty Designer node.
                    var element2 = new XElement(XName.Get("Designer", SchemaManager.GetEDMXNamespaceName(_artifact.SchemaVersion)));
                    element2.Add(rootDiagramsNode);
                    document.Root.Add(element2);
                    // Save the diagram file to disk.
                    document.Save(diagramFilePath, SaveOptions.None);

                    // Remove all diagram nodes under Diagrams node.
                    // Note that we want to still keep the Diagrams node for backward compatibility purpose. (The EDMX file could still be opened in the older Visual Studio).
                    _artifact.XDocument.Descendants(XName.Get("Diagrams", SchemaManager.GetEDMXNamespaceName(_artifact.SchemaVersion)))
                        .First()
                        .RemoveNodes();

                    // The code below is to ensure that Diagrams artifact in instantiated and initialized properly.
                    DiagramArtifact efArtifact = new VSDiagramArtifact(
                        _artifact.ModelManager, new Uri(diagramFilePath), _artifact.XmlModelProvider);
                    _artifact.DiagramArtifact = efArtifact;
                    _artifact.ModelManager.RegisterArtifact(efArtifact, _artifact.ArtifactSet);
                }
            }
        }

        internal static void DoMigrate(CommandProcessorContext cpc, EntityDesignArtifact artifact)
        {
            var xmlModelProvider = artifact.XmlModelProvider as VSXmlModelProvider;
            Debug.Assert(xmlModelProvider != null, "Artifact's model provider is not type of VSXmlModelProvider.");
            if ((xmlModelProvider != null)
                && (xmlModelProvider.UndoManager != null))
            {
                var undoManager = xmlModelProvider.UndoManager;
                try
                {
                    // We need to temporarily disable the Undo Manager because this operation is not undoable.
                    if (undoManager != null)
                    {
                        undoManager.Enable(0);
                    }

                    var command = new MigrateDiagramInformationCommand(artifact);
                    var processor = new CommandProcessor(cpc, shouldNotifyObservers: false);
                    processor.EnqueueCommand(command);
                    processor.Invoke();

                    Debug.Assert(artifact.DiagramArtifact != null, "Diagram artifact should have been created by now.");
                    if (artifact.DiagramArtifact != null)
                    {
                        // Ensure that diagram file is added to the project.
                        var service = PackageManager.Package.GetService(typeof(DTE)) as DTE;
                        service.ItemOperations.AddExistingItem(artifact.DiagramArtifact.Uri.LocalPath);

                        // Reload the artifacts.
                        artifact.ReloadArtifact();
                        artifact.IsDirty = true;

                        // The code below ensures mapping window and model-browser window are refreshed.
                        Debug.Assert(
                            PackageManager.Package.DocumentFrameMgr != null, "Could not find the DocumentFrameMgr for this package");
                        if (PackageManager.Package.DocumentFrameMgr != null)
                        {
                            PackageManager.Package.DocumentFrameMgr.SetCurrentContext(cpc.EditingContext);
                        }
                    }
                }
                finally
                {
                    if (undoManager != null)
                    {
                        undoManager.Enable(1);
                    }
                }
            }
        }
    }
}
