// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using ModelEntity = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.EntityDesigner.Dialogs;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Refactoring;
    using Microsoft.Data.Entity.Design.UI;
    using Microsoft.Data.Entity.Design.UI.Util;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Model.Commands;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Immutability;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;
    using ComplexProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.ComplexProperty;
    using Diagram = Microsoft.Data.Entity.Design.Model.Designer.Diagram;
    using EntityDesignerSelection = Microsoft.Data.Entity.Design.UI.Views.EntityDesigner.EntityDesignerSelection;
    using Property = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Property;
    using ScalarProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.ScalarProperty;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class MicrosoftDataEntityDesignCommandSet : IEntityDesignCommandSet
    {
        internal enum ConnectorEnd
        {
            Source = 0,
            Target = 1
        }

        internal EntityDesignArtifact CurrentEntityDesignArtifact
        {
            get
            {
                var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                var modelManager = PackageManager.Package.ModelManager;
                var efArtifactSet = (EntityDesignArtifactSet)modelManager.GetArtifactSet(uri);
                return efArtifactSet.GetEntityDesignArtifact();
            }
        }

        protected override IList<MenuCommand> GetMenuCommands()
        {
            // Set the command set in the package
            var entityPackage = ServiceProvider as MicrosoftDataEntityDesignPackage;
            Debug.Assert(entityPackage != null, "We should have a MicrosoftDataEntityDesignPackage for this commandset");
            if (entityPackage != null)
            {
                entityPackage.CommandSet = this;
            }

            var commands = new List<MenuCommand>();

            // add "PageSetup", "Print" and "PrintPreview" commands from DSL
            var baseCommands = base.GetMenuCommands();
            foreach (var command in baseCommands)
            {
                if (command.CommandID == CommonModelingCommands.PageSetup
                    || command.CommandID == CommonModelingCommands.Print
                    || command.CommandID == CommonModelingCommands.PrintPreview)
                {
                    commands.Add(command);
                }
            }

            // Add menu command: "Delete"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusDelete)),
                    OnMenuDelete, StandardCommands.Delete));

            // Add menu command: "Add Entity"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddEntity)),
                    OnMenuAddEntity, MicrosoftDataEntityDesignCommands.AddEntity));

            // Add menu command: "Add Scalar Property"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddProperty)),
                    OnMenuAddProperty, MicrosoftDataEntityDesignCommands.AddScalarProperty));

            // Add menu command: "Add Complex Property"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddProperty)),
                    OnMenuAddProperty, MicrosoftDataEntityDesignCommands.AddComplexProperty));

            // Add menu command: "Add Association"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddAssociation)),
                    OnMenuAddAssociation, MicrosoftDataEntityDesignCommands.AddAssociation));

            // Add menu command: "Add Inheritance"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddInheritance)),
                    OnMenuAddInheritance, MicrosoftDataEntityDesignCommands.AddInheritance));

            // Add menu command: "Add Function Import"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddFunctionImport)),
                    OnMenuAddFunctionImport, MicrosoftDataEntityDesignCommands.AddFunctionImport));

            // Add menu command: "Add Navigation Property"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddNavigationProperty)), OnMenuAddNavigationProperty,
                    MicrosoftDataEntityDesignCommands.AddNavigationProperty));

            // Add menu command: "Rename"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusRename)),
                    OnMenuRename, MicrosoftDataEntityDesignCommands.Rename));

            // Add menu command: "Refactor Rename"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusRefactorRename)),
                    OnMenuRefactorRename, MicrosoftDataEntityDesignCommands.RefactorRename));

            // Add menu command: "Collapse"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCollapseEntityTypeShape)), OnMenuCollapseEntityTypeShape,
                    MicrosoftDataEntityDesignCommands.CollapseEntityTypeShape));

            // Add menu command: "Expand"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusExpandEntityTypeShape)), OnMenuExpandEntityTypeShape,
                    MicrosoftDataEntityDesignCommands.ExpandEntityTypeShape));

            // Add menu command: "Zoom In"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuZoomIn,
                    MicrosoftDataEntityDesignCommands.ZoomIn));

            // Add menu command: "Zoom Out"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuZoomOut,
                    MicrosoftDataEntityDesignCommands.ZoomOut));

            // Add menu command: "Zoom to Fit"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuZoomToFit,
                    MicrosoftDataEntityDesignCommands.ZoomToFit));

            // Add menu command: "Custom Zoom..."
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusZoomCustom)),
                    OnMenuZoomCustom, MicrosoftDataEntityDesignCommands.ZoomCustom));

            // Add ZoomToLevel menu commands
            for (var i = 0; i < MicrosoftDataEntityDesignCommands.ZoomLevels.Length; i++)
            {
                commands.Add(
                    new CommandZoomToLevel(
                        CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusZoomTo)),
                        OnMenuZoomTo, MicrosoftDataEntityDesignCommands.CommandZoomLevels[i],
                        MicrosoftDataEntityDesignCommands.ZoomLevels[i]));
            }

            // Add menu command: "Show Grid"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusShowGrid)),
                    OnMenuShowGrid, MicrosoftDataEntityDesignCommands.ShowGrid));

            // Add menu command: "Snap to Grid"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSnapToGrid)),
                    OnMenuSnapToGrid, MicrosoftDataEntityDesignCommands.SnapToGrid));

            // Add menu command: "Layout Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuLayoutDiagram,
                    MicrosoftDataEntityDesignCommands.LayoutDiagram));

            // Add menu command: "Export Diagram as Image..."
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuExportDiagramAsImage,
                    MicrosoftDataEntityDesignCommands.ExportDiagramAsImage));

            // Add menu command: "Collapse All"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)),
                    OnMenuCollapseAllEntityTypeShapes, MicrosoftDataEntityDesignCommands.CollapseAllEntityTypeShapes));

            // Add menu command: "Expand All"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuExpandAllEntityTypeShapes,
                    MicrosoftDataEntityDesignCommands.ExpandAllEntityTypeShapes));

            //// Add menu command: "Display Name"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusDisplayName)),
                    OnMenuDisplayName, MicrosoftDataEntityDesignCommands.DisplayName));

            // Add menu command: "Display Name and Type"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusDisplayNameAndType)),
                    OnMenuDisplayNameAndType, MicrosoftDataEntityDesignCommands.DisplayNameAndType));

            // Add menu command: "Show Mapping designer"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuShowMappingDesigner,
                    MicrosoftDataEntityDesignCommands.ShowMappingDesigner));

            // Add menu command: "Show EDM explorer"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(StatusEnableOnDiagramSelected)), OnMenuShowEdmExplorer,
                    MicrosoftDataEntityDesignCommands.ShowEdmExplorer));

            // Add menu command: "Table Mappings"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusTableOrSprocMappings)), OnMenuTableMappings,
                    MicrosoftDataEntityDesignCommands.TableMappings));

            // Add menu command: "Association Mappings"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAssociationMappings)), OnMenuShowMappingDesigner,
                    MicrosoftDataEntityDesignCommands.AssociationMappings));

            // Add menu command: "Stored Procedure Mappings"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusTableOrSprocMappings)), OnMenuSprocMappings,
                    MicrosoftDataEntityDesignCommands.SprocMappings));

            // Add menu command: "Show in EDM explorer"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusShowInEdmExplorer)),
                    OnMenuShowInEdmExplorer, MicrosoftDataEntityDesignCommands.ShowInEdmExplorer));

            // Add menu command: "Show in Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusShowInDiagram)),
                    OnMenuShowInDiagram, MicrosoftDataEntityDesignCommands.ShowInDiagram));

            // Add menu command: "Function Import Mapping"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusFunctionImportMapping)), OnMenuShowMappingDesigner,
                    MicrosoftDataEntityDesignCommands.FunctionImportMapping));

            // Add menu command: "Validate".  We allow this menu option when we are not designer-safe. 
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(OnStatusValidate), OnMenuValidate, MicrosoftDataEntityDesignCommands.Validate));

            // Add menu command: "Select All" (use our own instead one from the DslShell::CommandSet)
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAll)),
                    OnMenuSelectAll, MicrosoftDataEntityDesignCommands.SelectAll));
            // In order for Ctrl + A to work, we need to enable also SelectAll from StandardCommands (but don't place it in our context menu)
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAll)),
                    OnMenuSelectAll, StandardCommands.SelectAll));

            // Add menu command: "EntityKey"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusEntityKey)),
                    OnMenuEntityKey, MicrosoftDataEntityDesignCommands.EntityKey));

            // Add menu command: "Select" (for Association)
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAssociationEnd(ConnectorEnd.Source))),
                    OnMenuSelectAssociationEnd(ConnectorEnd.Source), MicrosoftDataEntityDesignCommands.SelectEntityEnd1));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAssociationProperty(ConnectorEnd.Source))),
                    OnMenuSelectAssociationProperty(ConnectorEnd.Source), MicrosoftDataEntityDesignCommands.SelectPropertyEnd1));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAssociationEnd(ConnectorEnd.Target))),
                    OnMenuSelectAssociationEnd(ConnectorEnd.Target), MicrosoftDataEntityDesignCommands.SelectEntityEnd2));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAssociationProperty(ConnectorEnd.Target))),
                    OnMenuSelectAssociationProperty(ConnectorEnd.Target), MicrosoftDataEntityDesignCommands.SelectPropertyEnd2));

            // Add menu command: "Select Association"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectAssociation)),
                    OnMenuSelectAssociation, MicrosoftDataEntityDesignCommands.SelectAssociation));

            // Add menu command: "Select" (for Inheritance)
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectInheritanceEnd)),
                    OnMenuSelectInheritanceEnd(ConnectorEnd.Source), MicrosoftDataEntityDesignCommands.SelectBaseType));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusSelectInheritanceEnd)),
                    OnMenuSelectInheritanceEnd(ConnectorEnd.Target), MicrosoftDataEntityDesignCommands.SelectSubtype));

            // Add menu command: "Refresh from Database"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusRefreshFromDatabase)), OnMenuRefreshFromDatabase,
                    MicrosoftDataEntityDesignCommands.RefreshFromDatabase));

            // Add menu command: "Generate Database from Model"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusGenerateDatabaseScriptFromModel)),
                    OnMenuGenerateDatabaseScriptFromModel, MicrosoftDataEntityDesignCommands.GenerateDatabaseScriptFromModel));

            // Add menu command: "Create Function Import"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCreateFunctionImport)), OnMenuCreateFunctionImport,
                    MicrosoftDataEntityDesignCommands.CreateFunctionImport));

            // Add menu command: "Edit"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusEdit)), OnMenuEdit,
                    MicrosoftDataEntityDesignCommands.Edit));

            // Add menu commands for Cut/Copy/Paste
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCutOrCopy)),
                    OnMenuCut, StandardCommands.Cut));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCutOrCopy)),
                    OnMenuCopy, StandardCommands.Copy));
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPaste)), OnMenuPaste,
                    StandardCommands.Paste));

            // Add menu command : "Add Complex Type"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddComplexType)),
                    OnMenuAddComplexType, MicrosoftDataEntityDesignCommands.AddComplexType));

            // Add menu command : "Go To Definition"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusGoToDefinition)),
                    OnMenuGoToDefinition, MicrosoftDataEntityDesignCommands.GoToDefinition));

            // Add menu commands : "Create Complex Type"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCreateComplexType)),
                    OnMenuCreateComplexType, MicrosoftDataEntityDesignCommands.CreateComplexType));

            // Add Explorer menu command : "Complex Types..."
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusExplorerComplexTypes)), OnMenuExplorerComplexTypes,
                    MicrosoftDataEntityDesignCommands.ExplorerComplexTypes));

            // Add menu command : "Add New Template
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(OnStatusAddNewTemplate), OnMenuAddNewTemplate,
                    MicrosoftDataEntityDesignCommands.AddNewTemplate));

            // Add menu commands : "Create Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(OnStatusAddNewDiagram), OnMenuAddNewDiagram,
                    MicrosoftDataEntityDesignCommands.AddNewDiagram));

            // Add menu commands : "Open Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(OnStatusOpenDiagram), OnMenuOpenDiagram,
                    MicrosoftDataEntityDesignCommands.OpenDiagram));

            // Add menu commands : "Add To Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddToDiagram)),
                    OnMenuAddToDiagram, MicrosoftDataEntityDesignCommands.AddToDiagram));

            // Add Menu commands: "Close Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusCloseDiagram)),
                    OnMenuCloseDiagram, MicrosoftDataEntityDesignCommands.CloseDiagram));

            // Add Menu commands: "Move To a New Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusMoveToNewDiagram)),
                    OnMenuMoveToNewDiagram, MicrosoftDataEntityDesignCommands.MoveToNewDiagram));

            // Add Menu commands: "Remove from Diagram"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusRemoveFromDiagram)),
                    OnMenuRemoveFromDiagram, MicrosoftDataEntityDesignCommands.RemoveFromDiagram));

            // Add Menu commands: "Add Related Entity Type"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusIncludeRelatedEntityType)),
                    OnMenuIncludeRelatedEntityType, MicrosoftDataEntityDesignCommands.IncludeRelatedEntityType));

            // Add Menu commands: "Move Diagrams To Separate File"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(
                        CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusMoveDiagramsToSeparateFile)),
                    OnMenuMoveDiagramsToSeparateFile, MicrosoftDataEntityDesignCommands.MoveDiagramsToSeparateFile));

            // Add Property Menu command: "Up"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMoveUp, MicrosoftDataEntityDesignCommands.MovePropertyUp));

            // Add Property Menu command: "Down"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMoveDown, MicrosoftDataEntityDesignCommands.MovePropertyDown));

            // Add Property Menu command: "Page Up"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMovePageUp, MicrosoftDataEntityDesignCommands.MovePropertyPageUp));

            // Add Property Menu command: "Page Down"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMovePageDown, MicrosoftDataEntityDesignCommands.MovePropertyPageDown));

            // Add Property Menu command: "To Top"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMoveTop, MicrosoftDataEntityDesignCommands.MovePropertyTop));

            // Add Property Menu command: "To Bottom"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusPropertyMove)),
                    OnMenuPropertyMoveBottom, MicrosoftDataEntityDesignCommands.MovePropertyBottom));

            // Add menu command : "Add Enum Type"
            commands.Add(
                CreateCommand(
                    CreateIsDiagramLockedStatusEventHandler(CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusAddEnumType)),
                    OnMenuAddEnumType, MicrosoftDataEntityDesignCommands.AddEnumType));

            // Add menu command : "Convert To Enum"
            commands.Add(
                CreateCommand(
                    CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(OnStatusConvertToEnum), OnMenuConvertToEnum,
                    MicrosoftDataEntityDesignCommands.ConvertToEnum));

            return commands;
        }

        private static DynamicStatusMenuCommand CreateCommand(EventHandler statusHandler, EventHandler invokeHandler, CommandID commandId)
        {
            return new DynamicStatusMenuCommand(statusHandler, invokeHandler, commandId);
        }

        public bool AddCommand(CommandID commandId, EntityDesignerCommand command, out DynamicStatusMenuCommand menuCommand)
        {
            menuCommand = null;

            EventHandler canExecute = (o, ea) =>
                {
                    var cmd = o as DynamicStatusMenuCommand;
                    Debug.Assert(cmd != null, "canExecute handler: cannot find OleMenuCommand to operate on");
                    if (cmd != null)
                    {
                        cmd.Visible = cmd.Enabled = false;
                        if (cmd.Properties.Contains(PackageConstants.guidEscherCmdSet))
                        {
                            var entityDesignerCommand = cmd.Properties[PackageConstants.guidEscherCmdSet] as EntityDesignerCommand;
                            cmd.Text = entityDesignerCommand.Name;

                            var selectedEFObject = SelectedEFObject;
                            if (selectedEFObject != null)
                            {
                                var canExecuteValue = entityDesignerCommand.CanExecute(
                                    selectedEFObject.XObject,
                                    CurrentMicrosoftDataEntityDesignDocView,
                                    SingleSelection,
                                    GetSelectedCompartment(true),
                                    IsSingleSelection());
                                cmd.Visible = canExecuteValue.Item1;
                                cmd.Enabled = canExecuteValue.Item2;
                                return;
                            }
                        }
                    }
                };

            EventHandler execute = (o, ea) =>
                {
                    var cmd = o as DynamicStatusMenuCommand;
                    Debug.Assert(cmd != null, "execute handler: cannot find OleMenuCommand to operate on");
                    if (cmd != null)
                    {
                        if (cmd.Properties.Contains(PackageConstants.guidEscherCmdSet))
                        {
                            var selectedEFObject = SelectedEFObject;
                            if (selectedEFObject != null)
                            {
                                var entityDesignerCommand = cmd.Properties[PackageConstants.guidEscherCmdSet] as EntityDesignerCommand;
                                entityDesignerCommand.Execute(
                                    selectedEFObject.XObject,
                                    CurrentMicrosoftDataEntityDesignDocView,
                                    SingleSelection,
                                    GetSelectedCompartment(true),
                                    IsSingleSelection());
                            }
                        }
                    }
                };

            var menuService = Services.OleMenuCommandService;
            Debug.Assert(menuService != null, "Command service must not be null");
            if (menuService != null)
            {
                // Add the command handler
                menuCommand = new DynamicStatusMenuCommand(canExecute, execute, commandId);
                menuCommand.Properties[PackageConstants.guidEscherCmdSet] = command;
                menuCommand.Text = command.Name;
                if (menuService.FindCommand(commandId) == null)
                {
                    menuService.AddCommand(menuCommand);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveCommand(CommandID commandIdNum)
        {
            var menuService = Services.OleMenuCommandService;
            Debug.Assert(menuService != null, "Command service must not be null");
            if (menuService != null)
            {
                // Remove the command
                var menuCommand = menuService.FindCommand(commandIdNum);
                if (menuCommand != null)
                {
                    menuService.RemoveCommand(menuCommand);
                    return true;
                }
            }
            return false;
        }

        #region Selection

        /// <summary>
        ///     Returns the ConceptualDiagram, null if the current selection is not a ConceptualDiagram
        /// </summary>
        /// <returns></returns>
        internal EntityDesignerDiagram GetDiagram()
        {
            if (SingleSelection != null)
            {
                var diagram = SingleSelection as EntityDesignerDiagram;
                if (null == diagram)
                {
                    if (CurrentMicrosoftDataEntityDesignDocView != null)
                    {
                        diagram = CurrentMicrosoftDataEntityDesignDocView.CurrentDiagram as EntityDesignerDiagram;
                    }
                }

                return diagram;
            }

            return null;
        }

        /// <summary>
        ///     check if our diagram is selected
        /// </summary>
        /// <returns></returns>
        private bool IsOurDiagramSelected()
        {
            return IsDiagramSelected() && GetDiagram() != null;
        }

        internal EFObject SelectedEFObject
        {
            get
            {
                if (CurrentEntityDesignArtifact != null)
                {
                    var editingContext = CurrentEntityDesignArtifact.EditingContext;
                    if (editingContext != null)
                    {
                        Selection selection = editingContext.Items.GetValue<EntityDesignerSelection>();
                        if (selection != null)
                        {
                            return selection.PrimarySelection;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Returns the EntityTypeShape currently selected, null otherwise
        /// </summary>
        /// <returns></returns>
        internal EntityTypeShape SelectedEntityTypeShape
        {
            get { return SingleSelection as EntityTypeShape; }
        }

        internal ICollection<EntityTypeShape> SelectedEntityTypeShapes
        {
            get
            {
                var entityTypeShapes = new List<EntityTypeShape>();
                foreach (var o in CurrentSelection)
                {
                    var ets = o as EntityTypeShape;
                    if (null != ets)
                    {
                        entityTypeShapes.Add(ets);
                    }
                }

                return entityTypeShapes;
            }
        }

        internal Property SelectedProperty
        {
            get { return SingleSelection as Property; }
        }

        internal ScalarProperty SelectedScalarProperty
        {
            get { return SingleSelection as ScalarProperty; }
        }

        internal ComplexProperty SelectedComplexProperty
        {
            get { return SingleSelection as ComplexProperty; }
        }

        // if the "Navigation Properties" compartment heading of a given entity type is selected
        // then return it. Otherwise return null
        internal ElementListCompartment SelectedNavigationPropertiesCompartment
        {
            get
            {
                var compartment = SingleSelection as ElementListCompartment;
                if (null != compartment
                    && EntityTypeShape.NavigationCompartmentName.Equals(compartment.Name, StringComparison.Ordinal))
                {
                    return compartment;
                }

                return null;
            }
        }

        internal NavigationProperty SelectedNavigationProperty
        {
            get { return SingleSelection as NavigationProperty; }
        }

        internal AssociationConnector SelectedAssociationConnector
        {
            get { return SingleSelection as AssociationConnector; }
        }

        internal ICollection<AssociationConnector> SelectedAssociationConnectors
        {
            get
            {
                var associationConnectors = new List<AssociationConnector>();
                foreach (var o in CurrentSelection)
                {
                    var assocConnector = o as AssociationConnector;
                    if (null != assocConnector)
                    {
                        associationConnectors.Add(assocConnector);
                    }
                }

                return associationConnectors;
            }
        }

        internal InheritanceConnector SelectedInheritanceConnector
        {
            get { return SingleSelection as InheritanceConnector; }
        }

        internal static EntityDesignExplorerFrame ExplorerFrame
        {
            get
            {
                var window = PackageManager.Package.ExplorerWindow;
                Debug.Assert(window != null, "Could not get the Explorer window");
                if (window != null)
                {
                    var info = window.CurrentExplorerInfo;
                    if (info != null
                        && info._explorerFrame != null)
                    {
                        var frame = info._explorerFrame as EntityDesignExplorerFrame;
                        if (frame != null)
                        {
                            return frame;
                        }
                    }
                }

                return null;
            }
        }

        internal static ExplorerEFElement SelectedExplorerItem
        {
            get
            {
                var window = PackageManager.Package.ExplorerWindow;
                Debug.Assert(window != null, "Could not get the explorer window");
                if (window != null)
                {
                    var info = window.CurrentExplorerInfo;
                    if (info != null
                        && info._explorerFrame != null)
                    {
                        return info._explorerFrame.GetSelectedExplorerEFElement();
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Return the selected Entity Type Shape compartment given a type.
        /// </summary>
        /// <param name="isPropertiesCompartment"></param>
        /// <returns></returns>
        private ElementListCompartment GetSelectedCompartment(bool isPropertiesCompartment)
        {
            ElementListCompartment result = null;

            var diagram = GetDiagram();
            if (diagram != null
                && diagram.ActiveDiagramView != null)
            {
                var view = diagram.ActiveDiagramView.DiagramClientView;
                if (view != null)
                {
                    var entityShape = view.Selection.PrimaryItem.Shape as EntityTypeShape;

                    // if EntityShape is null, the user might select the entity shape compartment.
                    if (entityShape == null)
                    {
                        var compartment = view.Selection.PrimaryItem.Shape as ElementListCompartment;
                        if (compartment != null)
                        {
                            entityShape = compartment.ParentShape as EntityTypeShape;
                        }
                    }
                    if (entityShape != null)
                    {
                        result = isPropertiesCompartment ? entityShape.PropertiesCompartment : entityShape.NavigationCompartment;
                        // Make sure what is returned is what the user requested.
                        Debug.Assert(
                            result != null,
                            "Could not retrieve compartment with isPropertiesComparment property value : " + isPropertiesCompartment);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Menu handlers

        /// <summary>
        ///     Common function to enable menu command if diagram is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void StatusEnableOnDiagramSelected(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = IsOurDiagramSelected();
            }
        }

        /// <summary>
        ///     Set AddNewEntity visibility and display text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddEntity(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "Unexpected command type. type:" + (sender == null ? "NULL" : sender.GetType().Name));

            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is EntityDesignExplorerWindow)
                {
                    var explorerSelection = SelectedExplorerItem;
                    var explorerTypes = explorerSelection as ExplorerTypes;
                    // This menu item should be available when the user right click on entity-type node or entity-type folder node.
                    if ((null != explorerTypes && explorerTypes.IsConceptualEntityTypesNode)
                        || explorerSelection is ExplorerEntityType)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        cmd.Text = Resources.AddEntityTypeCommand_ExplorerText;
                        return;
                    }
                }
                    // if the active window is not the explorer window, just check if an entity diagram is selected.
                else if (IsOurDiagramSelected())
                {
                    cmd.Enabled = cmd.Visible = true;
                    cmd.Text = Resources.AddEntityTypeCommand_DesignerText;
                    return;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Add Entity"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddEntity(object sender, EventArgs e)
        {
            var diagram = GetDiagram();

            // diagram is null when the focused element is not a diagram; in that case, try to get the diagram instance from the current doc view.
            if (diagram == null)
            {
                var docView = CurrentDocView as MicrosoftDataEntityDesignDocView;
                Debug.Assert(docView != null, "Why there is no current doc view?");
                if (docView != null)
                {
                    diagram = docView.CurrentDiagram as EntityDesignerDiagram;
                }
            }

            Debug.Assert(diagram != null, "Could not find an active diagram.");
            if (diagram != null)
            {
                diagram.AddNewEntityType(GetPositionForNewElements());
            }
        }

        /// <summary>
        ///     Status method to decide whether Add Scalar Property menu should be visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddProperty(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
            if (cmd != null)
            {
                cmd.Visible = cmd.Enabled = IsSingleSelection() && (GetSelectedCompartment(true /* Properties Compartment */) != null);
            }
        }

        /// <summary>
        ///     This method is called if the Add Scalar/Complex Property menu item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddProperty(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram or ActiveDiagramView");
                if (diagram != null
                    && diagram.ActiveDiagramView != null)
                {
                    var view = diagram.ActiveDiagramView.DiagramClientView;
                    if (view != null)
                    {
                        var compartment = GetSelectedCompartment(true /* Properties Compartment */);
                        Debug.Assert(compartment != null, "could not find properties compartment");
                        if (compartment != null)
                        {
                            var domainClassId = cmd.CommandID == MicrosoftDataEntityDesignCommands.AddComplexProperty
                                                    ? ComplexProperty.DomainClassId
                                                    : ScalarProperty.DomainClassId;
                            var domainClassInfo = diagram.Partition.DomainDataDirectory.GetDomainClass(domainClassId);
                            compartment.HandleNewListItemInsertion(view, domainClassInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Status handler for: "Add Association"
        ///     Visible only when user right-clicks on empty area of designer or EntityTypeShape
        ///     Enabled when there is at least one EntityTypeShape on a diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddAssociation(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                cmd.Visible = IsSingleSelection() && (IsOurDiagramSelected() || SelectedEntityTypeShape != null);
                cmd.Enabled = !EntityDesignerDiagram.IsEmptyDiagram(GetDiagram());
            }
        }

        /// <summary>
        ///     Invoke handler: "Add Association"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddAssociation(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (diagram != null)
            {
                diagram.AddNewAssociation(SelectedEntityTypeShape);
            }
        }

        /// <summary>
        ///     Status handler for: "Add Inheritance"
        ///     Visible only when user right-clicks on empty area of designer or EntityTypeShape
        ///     Enabled when there is at least one EntityTypeShape on a diagram and at least two EntityTypes in the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddInheritance(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Visible = IsSingleSelection() && (IsOurDiagramSelected() || SelectedEntityTypeShape != null);
                    cmd.Enabled = !EntityDesignerDiagram.IsEmptyDiagram(diagram) && diagram.ModelElement.EntityTypes.Count > 1;
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Add Inheritance"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddInheritance(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.AddNewInheritance(SelectedEntityTypeShape);
            }
        }

        /// <summary>
        ///     Status handler for: "Add Function Import"
        ///     Visible only when user right-clicks on empty area of designer or EntityTypeShape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddFunctionImport(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Visible = IsSingleSelection() && (IsOurDiagramSelected() || SelectedEntityTypeShape != null);
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Status handler for: "Add Navigation Property"
        ///     Visible only when user right-clicks on empty area of designer or EntityTypeShape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddNavigationProperty(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Visible = cmd.Enabled = IsSingleSelection() && (GetSelectedCompartment(false /* Navigation Compartment */) != null);
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Add Function Import"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddFunctionImport(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.AddNewFunctionImport(SelectedEntityTypeShape);
            }
        }

        /// <summary>
        ///     Invoke handler: "Add Navigation Property"
        /// </summary>
        internal void OnMenuAddNavigationProperty(object sender, EventArgs e)
        {
            // A user can Insert to add a navigation property outside of our command filter
            // so we need to ensure we also short-circuit here.
            if (sender is MenuCommand)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram");
                if (diagram != null
                    && diagram.ActiveDiagramView != null)
                {
                    var view = diagram.ActiveDiagramView.DiagramClientView;
                    if (view != null)
                    {
                        var compartment = GetSelectedCompartment(false /* Navigation Compartment */);
                        Debug.Assert(compartment != null, "could not find Navigation compartment");
                        if (compartment != null)
                        {
                            var domainClassId = NavigationProperty.DomainClassId;
                            var domainClassInfo = diagram.Partition.DomainDataDirectory.GetDomainClass(domainClassId);
                            compartment.HandleNewListItemInsertion(view, domainClassInfo);
                        }
                    }
                }
            }
        }

        internal void OnStatusDelete(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "Null MenuCommand");
            if (cmd != null)
            {
                cmd.Text = Resources.DeleteFromModelCommandText;
                cmd.Enabled = cmd.Visible = false;

                var diagram = GetDiagram();
                // this will check if currently selected window is the Designer window
                // NOTE: This check is repeated all over the document and is related to TFS#353470
                //       "Incorrect context menu when you right click in model browser
                //        immediately after right clicking on an any object in the designer"
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && diagram != null)
                {
                    if (OnlyDeletableObjectsSelected())
                    {
                        cmd.Enabled = cmd.Visible = true;
                    }
                }
                else
                {
                    var explorerSelection = SelectedExplorerItem;
                    if (explorerSelection != null)
                    {
                        if (explorerSelection is ExplorerFunctionImport)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerStorageEntityType = explorerSelection as ExplorerStorageEntityType;
                        if (null != explorerStorageEntityType)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerFunction = explorerSelection as ExplorerFunction;
                        if (null != explorerFunction)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerComplexType = explorerSelection as ExplorerComplexType;
                        if (explorerComplexType != null)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerNavigationProperty = explorerSelection as ExplorerNavigationProperty;
                        if (explorerNavigationProperty != null)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerConceptualProperty = explorerSelection as ExplorerConceptualProperty;
                        if (explorerConceptualProperty != null)
                        {
                            var property = explorerConceptualProperty.ModelItem as ModelEntity.Property;
                            if (property != null
                                && property.IsComplexTypeProperty)
                            {
                                cmd.Enabled = cmd.Visible = true;
                                return;
                            }
                        }

                        // Enable delete menu item if a diagram is selected in Model Browser window and there are more than 1 diagram.
                        var explorerDiagram = explorerSelection as ExplorerDiagram;
                        if (explorerDiagram != null)
                        {
                            Debug.Assert(explorerDiagram.Parent != null, "The selected explorer diagram's parent is null.");
                            if (explorerDiagram.Parent != null
                                && explorerDiagram.Parent.Children.Count() > 1)
                            {
                                cmd.Text = Resources.RemoveDiagramCommandText;
                                cmd.Enabled = cmd.Visible = true;
                            }
                            return;
                        }

                        var explorerEntityTypeShape = explorerSelection as ExplorerEntityTypeShape;
                        if (explorerEntityTypeShape != null)
                        {
                            cmd.Text = Resources.RemoveFromDiagramCommandText;
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerConceptualEntityType = explorerSelection as ExplorerConceptualEntityType;
                        if (explorerConceptualEntityType != null)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }

                        var explorerEnumType = explorerSelection as ExplorerEnumType;
                        if (explorerEnumType != null)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            return;
                        }
                    }
                }
            }
        }

        private bool AreAllSelectedItemsTypeof<T>()
        {
            if (CurrentSelection.Count == 0)
            {
                return false;
            }

            foreach (var o in CurrentSelection)
            {
                if (o.GetType() != typeof(T))
                {
                    return false;
                }
            }
            return true;
        }

        internal static string RegKeyConfirmDelete = "ShowConfirmDeleteDialog";

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)")]
        internal void OnMenuDelete(object sender, EventArgs e)
        {
            // confirm user wants to delete (see Accessibility bugs 457903 & 457904)
            bool displayConfirmDeleteDialog = true;
            var shouldConfirmDelete = EdmUtils.GetUserSetting(RegKeyConfirmDelete);
            if (!string.IsNullOrEmpty(shouldConfirmDelete))
            {
                bool.TryParse(shouldConfirmDelete, out displayConfirmDeleteDialog);
            }
            if (displayConfirmDeleteDialog
                && DismissibleWarningDialog.ShowWarningDialogAndSaveDismissOption(
                    DialogsResource.ConfirmDeleteDialog_Title,
                    DialogsResource.ConfirmDeleteDialog_DescriptionLabel_Text,
                    RegKeyConfirmDelete,
                    DismissibleWarningDialog.ButtonMode.YesNo))
            {
                    return;
            }

            var diagram = GetDiagram();
            if (diagram != null
                && ((diagram.Partition.GetLocks() & Locks.Delete) == Locks.Delete))
            {
                return;
            }

            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && diagram != null)
            {
                ProcessDesignerOnMenuDelete();
            }
            else
            {
                var element = SelectedExplorerItem;
                if (element != null)
                {
                    // accumulate the appropriate delete commands
                    ICollection<Command> commands = new List<Command>();
                    var functionImport = element.ModelItem as ModelEntity.FunctionImport;
                    var set = element.ModelItem as ModelEntity.StorageEntityType;
                    var func = element.ModelItem as ModelEntity.Function;
                    var complexType = element.ModelItem as ModelEntity.ComplexType;
                    var property = element.ModelItem as ModelEntity.Property;
                    var navigationProperty = element.ModelItem as ModelEntity.NavigationProperty;
                    var designerDiagram = element.ModelItem as Diagram;
                    var entityTypeShape = element.ModelItem as Model.Designer.EntityTypeShape;
                    var conceptualEntityType = element.ModelItem as ModelEntity.ConceptualEntityType;
                    var enumType = element.ModelItem as ModelEntity.EnumType;

                    if (functionImport != null)
                    {
                        var cmdFuncImpMapping = FunctionImportMapping.GetDeleteCommand(functionImport);
                        var cmdFuncImp = functionImport.GetDeleteCommand();
                        foreach (var c in cmdFuncImpMapping)
                        {
                            commands.Add(c);
                        }
                        commands.Add(cmdFuncImp);
                    }
                    else if (set != null)
                    {
                        commands.Add(set.GetDeleteCommand());
                    }
                    else if (func != null)
                    {
                        commands.Add(func.GetDeleteCommand());
                    }
                    else if (complexType != null)
                    {
                        commands.Add(complexType.GetDeleteCommand());
                    }
                    else if (property != null)
                    {
                        commands.Add(property.GetDeleteCommand());
                    }
                    else if (navigationProperty != null)
                    {
                        commands.Add(navigationProperty.GetDeleteCommand());
                    }
                    else if (designerDiagram != null)
                    {
                        if (designerDiagram.Parent.Children.Count() <= 1)
                        {
                            // TODO: display a message box here.
                            // (We don't currently allow deletion of the last diagram. See BUGBUG comment below).
                        }
                        else
                        {
                            // BUGBUG 811229: There is currently a non-trivial order-of-operations problem when deleting the last open diagram:
                            // The initiation of the deletion causes the frame to be closed, which causes the model to fully tear down, 
                            // despite that the deletion is still being processed. Per offline discussion, won't implement the proper fix for Beta.
                            // As a workaround, we open another diagram before deleting/closing the current one.
                            // (We open the next diagram in the list, unless we're deleting the last one, in which case we open the next-to-last one.)
                            // Note: If there is a single diagram for the model, there is no issue at the moment, because we don't currently allow deletion of the last diagram.
                            //
                            var canDelete = true;
                            if (DiagramManagerContextItem.DiagramManager.OpenDiagrams.Count() == 1)
                            {
                                var allDiagrams = element.Parent.Children.ToList();
                                var currentDiagramIndex = allDiagrams.IndexOf(element);
                                var diagramToOpen =
                                    (element == allDiagrams.Last()
                                         ? allDiagrams[allDiagrams.Count - 2]
                                         : allDiagrams[currentDiagramIndex + 1]) as ExplorerDiagram;
                                if (diagramToOpen != null)
                                {
                                    DiagramManagerContextItem.DiagramManager.OpenDiagram(diagramToOpen.DiagramMoniker, true);
                                }
                                else
                                {
                                    Debug.Fail(
                                        "We need to be able to open another diagram before the deletion. Either there was no other diagram available, or the next element was not an ExplorerDiagram");
                                    canDelete = false;
                                }
                            }
                            // END BUGBUG

                            if (canDelete)
                            {
                                commands.Add(designerDiagram.GetDeleteCommand());
                            }
                        }
                    }
                    else if (entityTypeShape != null)
                    {
                        commands.Add(entityTypeShape.GetDeleteCommand());
                    }
                    else if (conceptualEntityType != null)
                    {
                        AppendDeleteCommands(new List<EFElement> { conceptualEntityType }, commands);
                    }
                    else if (enumType != null)
                    {
                        commands.Add(enumType.GetDeleteCommand());
                    }

                    // having accumulated the delete commands, now invoke them
                    if (commands.Count > 0)
                    {
                        var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                        var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                        var cp =
                            new CommandProcessor(
                                editingContext,
                                EfiTransactionOriginator.ExplorerWindowOriginatorId,
                                String.Format(CultureInfo.CurrentCulture, Resources.Tx_Delete, element.ModelItem.DisplayName),
                                commands);
                        cp.Invoke();
                    }
                }
            }
        }

        /// <summary>
        ///     Method that will handle delete request in designer.
        ///     This method will fire DSL rules that deletes Escher model element.
        ///     It will also determine whether StorageEntitySets would be unmapped if the delete went ahead and provide the user a choice to proceed or not.
        /// </summary>
        internal void ProcessDesignerOnMenuDelete()
        {
            var diagram = GetDiagram();

            Debug.Assert(diagram != null, "diagram should not be null");
            if (diagram != null
                && OnlyDeletableObjectsSelected()) // should not process this command when nothing or only the diagram itself are selected
            {
                // determine what StorageEntitySets would be unmapped if this delete
                // went ahead. If there are any then offer the user the choice of
                // whether to delete them also.

                var selectedModelElements = new List<EFElement>();

                foreach (var ets in SelectedEntityTypeShapes)
                {
                    var viewModelEntityType = ets.TypedModelElement;
                    var cet =
                        viewModelEntityType.EntityDesignerViewModel.ModelXRef.GetExisting(viewModelEntityType) as
                        ModelEntity.ConceptualEntityType;
                    if (null != cet)
                    {
                        selectedModelElements.Add(cet);
                    }
                }

                foreach (var assocConnector in SelectedAssociationConnectors)
                {
                    var viewModelAssociation = assocConnector.ModelElement;
                    var assoc =
                        viewModelAssociation.SourceEntityType.EntityDesignerViewModel.ModelXRef.GetExisting(viewModelAssociation) as
                        ModelEntity.Association;
                    if (null != assoc)
                    {
                        selectedModelElements.Add(assoc);
                    }
                }

                // offer the user their choice of whether to delete the storage EntitySets
                // (this has the side-effect of adding a list of unmapped ones to the 
                // DeleteUnmappedStorageEntitySetsProperty)
                var result = diagram.ShouldDeleteUnmappedStorageEntitySets(selectedModelElements);
                if (DialogResult.Cancel == result)
                {
                    // user decided to cancel the whole operation
                    return;
                }

                // do the delete
                ProcessOnMenuDeleteCommand();
            }
        }

        private void AppendDeleteCommands(List<EFElement> toBeDeletedElements, ICollection<Command> commands)
        {
            // Get the active diagram from active doc view. We could not use GetDiagram() because there should be no focused diagram when this menu is launched.
            EntityDesignerDiagram entityDesignerDiagram = null;
            var docView = CurrentDocView as MicrosoftDataEntityDesignDocView;
            Debug.Assert(docView != null, "Why there is no active doc view?");
            if (docView != null)
            {
                entityDesignerDiagram = docView.CurrentDiagram as EntityDesignerDiagram;
            }

            Debug.Assert(entityDesignerDiagram != null, "Unable to find diagram instance.");
            if (entityDesignerDiagram != null)
            {
                var result = entityDesignerDiagram.ShouldDeleteUnmappedStorageEntitySets(toBeDeletedElements);
                if (DialogResult.Cancel != result)
                {
                    // Add delete command for the entity type to the queue.
                    foreach (var efElement in toBeDeletedElements)
                    {
                        commands.Add(efElement.GetDeleteCommand());
                    }

                    var vm = entityDesignerDiagram.ModelElement;
                    Debug.Assert(null != vm, "Diagram :" + entityDesignerDiagram.Title + " 's model element is null");

                    if (null != vm)
                    {
                        // check if the DeleteUnmappedStorageEntitySetsProperty property is set
                        if (vm.Store.PropertyBag.ContainsKey(EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty))
                        {
                            // if the property is set then unset it and add a command to remove these for each item in the master list
                            var unmappedMasterList =
                                vm.Store.PropertyBag[EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty] as
                                List<ICollection<ModelEntity.StorageEntitySet>>;
                            vm.Store.PropertyBag.Remove(EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty);

                            if (unmappedMasterList != null)
                            {
                                foreach (var unmappedEntitySets in unmappedMasterList)
                                {
                                    commands.Add(new DeleteUnmappedStorageEntitySetsCommand(unmappedEntitySets));
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void OnStatusRename(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    // Rename on the Designer
                    if (diagram.ActiveDiagramView != null)
                    {
                        var view = diagram.ActiveDiagramView.DiagramClientView;
                        if (view != null)
                        {
                            cmd.Visible = cmd.Enabled = view.Selection.CanEditValue(view);
                            return;
                        }
                    }
                }
                else
                {
                    // Rename on the Explorer
                    var element = SelectedExplorerItem;
                    if (element != null)
                    {
                        // Rename is only enabled in the Explorer on ComplexType, Property, ConceptualEntityType, Diagram and EntityTypeShape nodes.
                        var complexType = element.ModelItem as ModelEntity.ComplexType;
                        var property = element.ModelItem as ModelEntity.Property;
                        var conceptualEntityType = element.ModelItem as ModelEntity.ConceptualEntityType;
                        var designerDiagram = element.ModelItem as Diagram;
                        var entityTypeShape = element.ModelItem as Model.Designer.EntityTypeShape;

                        if (complexType != null)
                        {
                            cmd.Visible = cmd.Enabled = true;
                            return;
                        }
                        else if (property != null
                                 && property.IsComplexTypeProperty)
                        {
                            cmd.Visible = cmd.Enabled = true;
                            return;
                        }
                        else if (designerDiagram != null)
                        {
                            cmd.Visible = cmd.Enabled = true;
                            return;
                        }
                        else if (conceptualEntityType != null)
                        {
                            cmd.Visible = cmd.Enabled = true;
                            return;
                        }
                        else if (entityTypeShape != null)
                        {
                            cmd.Visible = cmd.Enabled = true;
                            return;
                        }
                    }
                }
            }

            // otherwise command is invisible and not enabled
            cmd.Visible = cmd.Enabled = false;
        }

        internal void OnMenuRename(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (diagram != null)
            {
                // Rename on the Designer
                Debug.Assert(
                    diagram.ActiveDiagramView != null && diagram.ActiveDiagramView.DiagramClientView != null,
                    "diagram.ActiveDiagramView and diagram.ActiveDiagramView.DiagramClientView should not be null");
                if (diagram.ActiveDiagramView != null)
                {
                    var view = diagram.ActiveDiagramView.DiagramClientView;
                    if (view != null)
                    {
                        view.Selection.EditValue(view);
                    }
                }
            }
            else
            {
                // Rename on the Explorer
                var frame = ExplorerFrame;
                if (frame != null)
                {
                    var selectedElement = SelectedExplorerItem;
                    if (selectedElement != null)
                    {
                        frame.PutElementInRenameMode(selectedElement);
                    }
                }
            }
        }

        internal void OnStatusRefactorRename(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null
                    && CurrentSelection.Count == 1)
                {
                    var modelObject = GetModelObjectFromSelectionForRefactor(diagram);
                    if (modelObject != null)
                    {
                        cmd.Visible = cmd.Enabled = true;
                        return;
                    }
                }

                // If we get down here we couldn't find a valid state for the command to execute, so change it to be invisible and not enabled
                cmd.Visible = cmd.Enabled = false;
            }
        }

        private EFObject GetModelObjectFromSelectionForRefactor(EntityDesignerDiagram diagram)
        {
            if (diagram.ActiveDiagramView != null
                && CurrentSelection.Count == 1)
            {
                var entityTypeShape = CurrentSelection.OfType<EntityTypeShape>().FirstOrDefault();
                if (entityTypeShape != null)
                {
                    var entityType =
                        diagram.GetRootViewModel().ModelXRef.GetExisting(entityTypeShape.ModelElement) as ModelEntity.EntityType;

                    if (entityType != null)
                    {
                        return entityType;
                    }
                }
                else
                {
                    var viewProperty = CurrentSelection.OfType<Property>().FirstOrDefault();
                    if (viewProperty != null)
                    {
                        var property = diagram.GetRootViewModel().ModelXRef.GetExisting(viewProperty) as ModelEntity.Property;

                        if (property != null)
                        {
                            return property;
                        }
                    }
                    else
                    {
                        var associationConnector = CurrentSelection.OfType<AssociationConnector>().FirstOrDefault();
                        if (associationConnector != null)
                        {
                            var association =
                                diagram.GetRootViewModel().ModelXRef.GetExisting(associationConnector.ModelElement) as
                                ModelEntity.Association;

                            if (association != null)
                            {
                                var artifact = association.Artifact as EntityDesignArtifact;
                                if (artifact != null)
                                {
                                    return association;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        internal void OnMenuRefactorRename(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (diagram != null)
            {
                var modelObject = GetModelObjectFromSelectionForRefactor(diagram);

                if (modelObject != null)
                {
                    RefactorEFObject.RefactorRenameElement(modelObject);
                }
            }
        }

        /// <summary>
        ///     Status handler for menu item to collapse an entity shape.
        ///     Visible when the selected entity shape is expanded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusCollapseEntityTypeShape(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Visible = IsSingleSelection() && (SelectedEntityTypeShape != null && SelectedEntityTypeShape.IsExpanded);
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler for "Collapse" menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuCollapseEntityTypeShape(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(
                diagram != null && IsSingleSelection() && (SelectedEntityTypeShape != null && SelectedEntityTypeShape.IsExpanded),
                "could not find diagram");
            if (diagram != null
                && SelectedEntityTypeShape != null)
            {
                diagram.CollapseEntityTypeShape(SelectedEntityTypeShape);
            }
        }

        /// <summary>
        ///     Status handler for menu item to expand an entity shape.
        ///     Visible when the selected entity shape is collapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusExpandEntityTypeShape(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Visible = IsSingleSelection() && (SelectedEntityTypeShape != null && !SelectedEntityTypeShape.IsExpanded);
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler for "Expand" menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuExpandEntityTypeShape(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(
                diagram != null && IsSingleSelection() && (SelectedEntityTypeShape != null && !SelectedEntityTypeShape.IsExpanded),
                "could not find diagram");
            if (diagram != null
                && SelectedEntityTypeShape != null)
            {
                diagram.ExpandEntityTypeShape(SelectedEntityTypeShape);
            }
        }

        /// <summary>
        ///     Invoke handler: "Zoom In"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuZoomIn(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.ZoomIn();
            }
        }

        /// <summary>
        ///     Invoke handler: "Zoom Out"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuZoomOut(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.ZoomOut();
            }
        }

        /// <summary>
        ///     Invoke handler: "Zoom to Fit"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuZoomToFit(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.ZoomToFit();
            }
        }

        internal void OnStatusZoomTo(object sender, EventArgs e)
        {
            StatusEnableOnDiagramSelected(sender, e);
            var cmd = sender as CommandZoomToLevel;
            Debug.Assert(cmd != null, "Command was null");
            if (cmd != null
                && cmd.Visible)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null, "Diagram was null");
                if (diagram != null)
                {
                    cmd.Checked = (diagram.ZoomLevel == cmd.ZoomLevel);
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Zoom to level"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuZoomTo(object sender, EventArgs e)
        {
            var cmd = sender as CommandZoomToLevel;
            Debug.Assert(cmd != null, "Command was null");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null, "Diagram was null");
                if (diagram != null)
                {
                    diagram.ZoomLevel = cmd.ZoomLevel;
                }
            }
        }

        internal void OnStatusZoomCustom(object sender, EventArgs e)
        {
            StatusEnableOnDiagramSelected(sender, e);
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "Command was null");
            if (cmd != null
                && cmd.Visible)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null, "Diagram was null");
                if (diagram != null)
                {
                    var custom = true;
                    foreach (var level in MicrosoftDataEntityDesignCommands.ZoomLevels)
                    {
                        if (diagram.ZoomLevel == level)
                        {
                            custom = false;
                            break;
                        }
                    }
                    cmd.Checked = custom;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Custom Zoom..."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuZoomCustom(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                using (var dlg = new CustomZoomDialog())
                {
                    dlg.ZoomPercent = GetDiagram().ZoomLevel;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        diagram.ZoomLevel = dlg.ZoomPercent;
                    }
                }
            }
        }

        /// <summary>
        ///     Status handler: "Show Grid"
        ///     Available only when user right-clicks on empty area of designer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusShowGrid(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Enabled = cmd.Visible = IsOurDiagramSelected();
                    cmd.Checked = diagram.ShowGrid;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Show Grid"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuShowGrid(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                var cmd = sender as MenuCommand;
                cmd.Checked = !cmd.Checked;
                diagram.ShowGrid = cmd.Checked;
                diagram.PersistShowGrid();
            }
        }

        /// <summary>
        ///     Status handler: "Snap to Grid"
        ///     Available only when user right-clicks on empty area of designer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusSnapToGrid(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Enabled = cmd.Visible = IsOurDiagramSelected();
                    cmd.Checked = diagram.SnapToGrid;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        /// <summary>
        ///     Invoke handler: "Snap to Grid"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuSnapToGrid(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                var cmd = sender as MenuCommand;
                cmd.Checked = !cmd.Checked;
                diagram.SnapToGrid = cmd.Checked;
                diagram.PersistSnapToGrid();
            }
        }

        /// <summary>
        ///     Invoke handler: "Layout Diagram"
        ///     Automatically lays out shapes on the designer surface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuLayoutDiagram(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.AutoLayoutDiagram();
            }
        }

        /// <summary>
        ///     Invoke handler: "Export Diagram as Image"
        ///     Exports the diagram as an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuExportDiagramAsImage(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                if (diagram.NestedChildShapes == null
                    || diagram.NestedChildShapes.Count == 0)
                {
                    VsUtils.ShowErrorDialog(String.Format(CultureInfo.CurrentCulture, Resources.Error_EmptyDiagram, diagram.Title));
                    return;
                }
                ModelUtils.ExportAsImage(diagram);
            }
        }

        /// <summary>
        ///     Invoke handler: "Collapse All".
        ///     Collapse all entity type shapes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuCollapseAllEntityTypeShapes(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.CollapseAllEntityTypeShapes();
            }
        }

        /// <summary>
        ///     Invoke handler: "Expand All".
        ///     Expand all entity type shapes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuExpandAllEntityTypeShapes(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.ExpandAllEntityTypeShapes();
            }
        }

        internal void OnStatusDisplayName(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "cmd is null");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Enabled = cmd.Visible = IsOurDiagramSelected();
                    cmd.Checked = !diagram.DisplayNameAndType;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        internal void OnMenuDisplayName(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.DisplayNameAndType = false;
            }
        }

        internal void OnStatusDisplayNameAndType(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Enabled = cmd.Visible = IsOurDiagramSelected();
                    cmd.Checked = diagram.DisplayNameAndType;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        internal void OnMenuDisplayNameAndType(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "could not find diagram");
            if (diagram != null)
            {
                diagram.DisplayNameAndType = true;
            }
        }

        internal void OnStatusTableOrSprocMappings(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                // this will check if currently selected window is the Designer window
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && diagram != null)
                {
                    if (!IsSingleSelection())
                    {
                        return;
                    }
                }

                // if ConceptualEntityType is selected from either diagram/designer or model browser window.
                if (SelectedEntityTypeShape != null
                    || SelectedProperty != null
                    // Fix bug where Table and Sproc mapping menu items are no longer shown when Property/ComplexProperty is selected post Dev10.
                    || SelectedExplorerItem is ExplorerConceptualEntityType
                    || SelectedExplorerItem is ExplorerEntityTypeShape)
                {
                    cmd.Visible = cmd.Enabled = true;

                    // if the selected entity type is abstract, Sproc Mapping menu item should be disabled.
                    var mdi = PackageManager.Package.MappingDetailsWindow.CurrentMappingDetailsInfo;
                    if (mdi != null
                        && mdi.ViewModel != null
                        && mdi.ViewModel.RootNode != null)
                    {
                        var entityType = mdi.ViewModel.RootNode.ModelItem as ModelEntity.ConceptualEntityType;
                        if (entityType != null
                            && entityType.IsAbstract
                            && cmd.CommandID == MicrosoftDataEntityDesignCommands.SprocMappings)
                        {
                            cmd.Enabled = false;
                        }
                    }
                }
            }
        }

        internal void OnStatusAssociationMappings(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                // this will check if currently selected window is the Designer window
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && diagram != null)
                {
                    if (!IsSingleSelection())
                    {
                        cmd.Enabled = cmd.Visible = false;
                        return;
                    }
                }

                var mdi = PackageManager.Package.MappingDetailsWindow.CurrentMappingDetailsInfo;
                if (mdi != null
                    && mdi.ViewModel != null
                    && mdi.ViewModel.RootNode != null
                    && mdi.ViewModel.RootNode.ModelItem is ModelEntity.Association)
                {
                    cmd.Enabled = cmd.Visible = true;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        internal void OnStatusShowInEdmExplorer(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;

                if (IsSingleSelection())
                {
                    cmd.Enabled = cmd.Visible = (SelectedEntityTypeShape != null)
                                                || (SelectedProperty != null)
                                                || (SelectedNavigationProperty != null)
                                                || (SelectedAssociationConnector != null);
                }
            }
        }

        internal void OnMenuShowInEdmExplorer(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (diagram != null)
            {
                EFObject efObject = null;
                if (SelectedEntityTypeShape != null)
                {
                    efObject = diagram.ModelElement.ModelXRef.GetExisting(SelectedEntityTypeShape.ModelElement);
                }
                else if (SelectedProperty != null)
                {
                    efObject = diagram.ModelElement.ModelXRef.GetExisting(SelectedProperty);
                }
                else if (SelectedNavigationProperty != null)
                {
                    efObject = diagram.ModelElement.ModelXRef.GetExisting(SelectedNavigationProperty);
                }
                else if (SelectedAssociationConnector != null)
                {
                    efObject = diagram.ModelElement.ModelXRef.GetExisting(SelectedAssociationConnector.ModelElement);
                }

                if (efObject != null)
                {
                    ExplorerNavigationHelper.NavigateTo(efObject);
                }
                else
                {
                    PackageManager.Package.ExplorerWindow.Show();
                }
            }
        }

        internal void OnStatusShowInDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                // We only show the menu item if the ExplorerItem is selected.
                if (SingleSelection == null
                    && SelectedExplorerItem != null)
                {
                    var dslDiagram = CurrentMicrosoftDataEntityDesignDocView.CurrentDiagram as EntityDesignerDiagram;
                    if (dslDiagram != null)
                    {
                        if (SelectedExplorerItem is ExplorerEntityTypeShape)
                        {
                            cmd.Enabled = cmd.Visible = true;
                        }
                        else
                        {
                            var modelDiagram = dslDiagram.ModelElement.ModelXRef.GetExisting(dslDiagram) as Diagram;
                            var efElement = SelectedExplorerItem.ModelItem;

                            if (efElement != null
                                && modelDiagram != null)
                            {
                                cmd.Enabled = cmd.Visible = (SelectedExplorerItem is ExplorerConceptualEntityType
                                                             || SelectedExplorerItem is ExplorerConceptualAssociation
                                                             || (SelectedExplorerItem is ExplorerConceptualProperty
                                                                 && efElement.Parent is ModelEntity.ConceptualEntityType)
                                                             || SelectedExplorerItem is ExplorerNavigationProperty
                                                             || SelectedExplorerItem is ExplorerEntitySet
                                                             || SelectedExplorerItem is ExplorerAssociationSet)
                                                            && modelDiagram.IsEFObjectRepresentedInDiagram(efElement);
                            }
                        }
                    }
                }
            }
        }

        internal void OnMenuShowInDiagram(object sender, EventArgs e)
        {
            Debug.Assert(
                SelectedExplorerItem is ExplorerConceptualEntityType
                || SelectedExplorerItem is ExplorerConceptualAssociation
                || SelectedExplorerItem is ExplorerConceptualProperty
                || SelectedExplorerItem is ExplorerNavigationProperty
                || SelectedExplorerItem is ExplorerEntitySet
                || SelectedExplorerItem is ExplorerAssociationSet
                || SelectedExplorerItem is ExplorerEntityTypeShape
                ,
                "Incorrect type of ExplorerItem, type is "
                + (SelectedExplorerItem == null ? "NULL" : SelectedExplorerItem.GetType().FullName));

            if (SelectedExplorerItem != null
                && SelectedExplorerItem.ModelItem != null
                && CurrentMicrosoftDataEntityDesignDocView != null)
            {
                var diagram = CurrentMicrosoftDataEntityDesignDocView.Diagram as IViewDiagram;

                if (diagram != null)
                {
                    if (SelectedExplorerItem is ExplorerEntityTypeShape)
                    {
                        // Selection "Show In Diagram" should behave the same as double clicking the explorer item in Explorer Window.
                        var window = PackageManager.Package.ExplorerWindow;
                        Debug.Assert(window != null, "Could not get the explorer window");
                        if (window != null)
                        {
                            var info = window.CurrentExplorerInfo;
                            if (info != null
                                && info._explorerFrame != null)
                            {
                                var frame = info._explorerFrame as EntityDesignExplorerFrame;
                                if (frame != null)
                                {
                                    frame.ExecuteActivate();
                                }
                            }
                        }
                    }
                    else
                    {
                        diagram.AddOrShowEFElementInDiagram(SelectedExplorerItem.ModelItem);
                    }
                }
            }
        }

        internal void OnStatusShowInTableDesigner(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                // only show this for Explorer
                if (diagram == null)
                {
                    if (SelectedExplorerItem != null)
                    {
                        var efElement = SelectedExplorerItem.ModelItem;
                        if (efElement != null)
                        {
                            cmd.Enabled = cmd.Visible =
                                          SelectedExplorerItem is ExplorerStorageEntityType
                                          || (SelectedExplorerItem is ExplorerStorageProperty
                                              && efElement.Parent is ModelEntity.StorageEntityType);
                            return;
                        }
                    }
                }

                cmd.Enabled = cmd.Visible = false;
            }
        }

        internal void OnStatusShowInEntityDesigner(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                // only show this for Explorer
                if (diagram == null)
                {
                    if (SelectedExplorerItem != null)
                    {
                        var efElement = SelectedExplorerItem.ModelItem;
                        if (efElement != null)
                        {
                            cmd.Enabled = cmd.Visible =
                                          SelectedExplorerItem is ExplorerConceptualEntityType
                                          || (SelectedExplorerItem is ExplorerConceptualProperty
                                              && efElement.Parent is ModelEntity.ConceptualEntityType);
                            return;
                        }
                    }
                }

                cmd.Enabled = cmd.Visible = false;
            }
        }

        internal void OnMenuShowMappingDesigner(object sender, EventArgs e)
        {
            PackageManager.Package.MappingDetailsWindow.Show();
        }

        internal void OnMenuTableMappings(object sender, EventArgs e)
        {
            OnMenuShowMappingDesigner(sender, e);
            var mdi = PackageManager.Package.MappingDetailsWindow.CurrentMappingDetailsInfo;
            if (mdi != null
                && mdi.EntityMappingMode != EntityMappingModes.Tables)
            {
                mdi.EntityMappingMode = EntityMappingModes.Tables;
                PackageManager.Package.MappingDetailsWindow.RefreshCurrentSelection();
            }
        }

        internal void OnMenuSprocMappings(object sender, EventArgs e)
        {
            OnMenuShowMappingDesigner(sender, e);
            var mdi = PackageManager.Package.MappingDetailsWindow.CurrentMappingDetailsInfo;
            if (mdi != null
                && mdi.EntityMappingMode != EntityMappingModes.Functions)
            {
                mdi.EntityMappingMode = EntityMappingModes.Functions;
                PackageManager.Package.MappingDetailsWindow.RefreshCurrentSelection();
            }
        }

        internal void OnMenuShowEdmExplorer(object sender, EventArgs e)
        {
            PackageManager.Package.ExplorerWindow.Show();
        }

        internal void OnStatusFunctionImportMapping(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView)
                {
                    cmd.Enabled = cmd.Visible = false;
                }
                else
                {
                    cmd.Enabled = cmd.Visible = SelectedExplorerItem is ExplorerFunctionImport;
                }
            }
        }

        internal void OnStatusValidate(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = true;
            }
        }

        internal void OnMenuValidate(object sender, EventArgs e)
        {
            var dte = PackageManager.Package.GetService(typeof(DTE)) as DTE;
            try
            {
                if (null != dte
                    && null != dte.StatusBar)
                {
                    dte.StatusBar.Text = Resources.StatusBarValidatingText;
                }
                Debug.Assert(CurrentDocData != null, "CurrentDocData is null");
                if (CurrentDocData != null)
                {
                    var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                    var modelManager = PackageManager.Package.ModelManager;
                    var modelListener = PackageManager.Package.ModelChangeEventListener;
                    var efArtifactSet = (EntityDesignArtifactSet)modelManager.GetArtifactSet(uri);
                    var efArtifact = efArtifactSet.GetEntityDesignArtifact();
                    VsUtils.EnsureProvider(efArtifact);

                    var project = VSHelpers.GetProjectForDocument(efArtifact.Uri.LocalPath, PackageManager.Package);
                    modelListener.OnBeforeValidateModel(project, efArtifact, false);

                    modelManager.ValidateAndCompileMappings(efArtifactSet, true);

                    var docData = VSHelpers.GetDocData(PackageManager.Package, efArtifact.Uri.LocalPath) as IEntityDesignDocData;
                    Debug.Assert(docData != null, "Unable to get docData for document");
                    var errors = efArtifactSet.GetAllErrorsForArtifact(efArtifact);
                    ErrorListHelper.AddErrorInfosToErrorList(errors, docData.Hierarchy, docData.ItemId, true);
                }
            }
            finally
            {
                if (null != dte
                    && null != dte.StatusBar)
                {
                    dte.StatusBar.Text = Resources.StatusBarValidationCompletedText;
                }
            }
        }

        internal void OnStatusRefreshFromDatabase(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Visible = true;
                if (IsArtifactInNonMiscFilesProject())
                {
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Enabled = false;
                }
            }
        }

        internal void OnStatusGenerateDatabaseScriptFromModel(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Visible = true;
                if (IsArtifactInNonMiscFilesProject())
                {
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Enabled = false;
                }
            }
        }

        internal void OnStatusCreateFunctionImport(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");

            cmd.Enabled = cmd.Visible = false;
            var explorerSelection = SelectedExplorerItem;
            if (explorerSelection != null)
            {
                var func = explorerSelection as ExplorerFunction;
                var funcImports = explorerSelection as ExplorerFunctionImports;

                if ((func != null && !((ModelEntity.Function)(func.ModelItem)).IsComposable.Value)
                    || funcImports != null)
                {
                    cmd.Enabled = cmd.Visible = true;
                }
            }
        }

        internal void OnStatusEdit(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            cmd.Enabled = cmd.Visible = false;
            var explorerSelection = SelectedExplorerItem;
            if (explorerSelection != null)
            {
                var funcImport = explorerSelection as ExplorerFunctionImport;
                var explorerEnumType = explorerSelection as ExplorerEnumType;
                if (funcImport != null
                    || explorerEnumType != null)
                {
                    cmd.Enabled = cmd.Visible = true;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void OnMenuRefreshFromDatabase(object sender, EventArgs e)
        {
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            ModelManager modelManager = PackageManager.Package.ModelManager;
            var efArtifactSet = modelManager.GetArtifactSet(uri);
            try
            {
                // cannot use foreach over Artifacts as the enumerator will
                // not work when you are altering each EFArtifact
                var artifacts = efArtifactSet.Artifacts.OfType<EntityDesignArtifact>().ToArray();
                for (var i = 0; i < artifacts.Length; i++)
                {
                    UpdateFromDatabaseEngine.UpdateModelFromDatabase(artifacts[i]);
                }
            }
            catch (Exception ex)
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Model.Resources.UpdateFromDatabaseExceptionMessage,
                        ex.GetType().FullName,
                        ex.Message));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void OnMenuGenerateDatabaseScriptFromModel(object sender, EventArgs e)
        {
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            var modelManager = PackageManager.Package.ModelManager;
            var efArtifactSet = modelManager.GetArtifactSet(uri);
            try
            {
                // cannot use foreach over Artifacts as the enumerator will
                // not work when you are altering each EFArtifact
                var artifacts = efArtifactSet.Artifacts.OfType<EntityDesignArtifact>().ToArray();
                for (var i = 0; i < artifacts.Length; i++)
                {
                    DatabaseGenerationEngine.GenerateDatabaseScriptFromModel(artifacts[i]);
                }
            }
            catch (Exception ex)
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Model.Resources.GenerateDatabaseScriptExceptionMessage,
                        ex.GetType().FullName,
                        ex.Message));
            }
        }

        internal void OnMenuCreateFunctionImport(object sender, EventArgs e)
        {
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            if (null == uri)
            {
                Debug.Fail("Could not find Uri for file name " + CurrentDocData.FileName);
                return;
            }
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
            if (null == editingContext)
            {
                Debug.Fail("Could not find Base.Context.EditingContext for uri " + uri.AbsoluteUri);
                return;
            }
            var info = editingContext.Items.GetValue<ExplorerWindow.ExplorerInfo>();
            if (null == info)
            {
                Debug.Fail("Could not find VisualStudio.Package.ExplorerWindow.ExplorerInfo for uri " + uri.AbsoluteUri);
                return;
            }
            var viewModelHelper = (EntityDesignExplorerViewModelHelper)info._explorerFrame.ExplorerViewModelHelper;
            if (null == viewModelHelper)
            {
                Debug.Fail("Could not find EntityDesignExplorerViewModelHelper for uri " + uri.AbsoluteUri);
                return;
            }

            // get SchemaVersion for current edmx file
            var artifactService = editingContext.GetEFArtifactService();
            if (null == artifactService)
            {
                Debug.Fail("Could not find EFArtifactService for uri " + uri.AbsoluteUri);
                return;
            }
            var artifact = artifactService.Artifact;
            if (null == artifact)
            {
                Debug.Fail("Could not find EFArtifact for uri " + uri.AbsoluteUri);
                return;
            }
            var schemaVersion = artifact.SchemaVersion;
            if (null == schemaVersion)
            {
                Debug.Fail("Could not determine Version for uri " + uri.AbsoluteUri);
                return;
            }

            var element = SelectedExplorerItem;
            if (element != null)
            {
                var function = element.ModelItem as ModelEntity.Function;
                if (function != null)
                {
                    viewModelHelper.CreateFunctionImport(function);
                }
                else
                {
                    viewModelHelper.CreateFunctionImport(null);
                }
            }
        }

        internal void OnMenuEdit(object sender, EventArgs e)
        {
            var window = PackageManager.Package.ExplorerWindow;
            Debug.Assert(window != null, "Could not get the explorer window");
            if (window != null)
            {
                var info = window.CurrentExplorerInfo;
                if (info != null
                    && info._explorerFrame != null)
                {
                    var frame = info._explorerFrame as EntityDesignExplorerFrame;
                    if (frame != null)
                    {
                        frame.ExecuteActivate();
                    }
                }
            }
        }

        internal void OnStatusSelectAll(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                var diagram = GetDiagram();
                // this will check if currently selected window is the Designer window
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && diagram != null)
                {
                    cmd.Visible = IsOurDiagramSelected();
                    cmd.Enabled = !EntityDesignerDiagram.IsEmptyDiagram(diagram);
                }
                else
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        internal void OnMenuSelectAll(object sender, EventArgs e)
        {
            base.ProcessOnMenuSelectAllCommand();
        }

        internal void OnStatusEntityKey(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                if (IsSingleSelection()
                    && SelectedScalarProperty != null)
                {
                    cmd.Visible = cmd.Enabled = true;
                    cmd.Checked = SelectedScalarProperty.EntityKey;

                    // We should disallow the user to set PK for a sub entity type's property or if the property type is not a "real" primitive type.
                    // but we should allow the user to clear the PK from a sub entity type if PK is set.
                    // Note: the "real" primitive types do not include the "quasi-primitive" types such as Geometry and Geography - which are not allowed as PK types
                    Debug.Assert(SelectedScalarProperty.EntityType != null, "SelectedScalarProperty.EntityType should not be null");
                    if (((null != SelectedScalarProperty.EntityType && SelectedScalarProperty.EntityType.HasBaseType)
                         || ModelHelper.AllPrimaryKeyPrimitiveTypes().Contains(SelectedScalarProperty.Type) == false)
                        && !cmd.Checked)
                    {
                        cmd.Enabled = false;
                    }
                }
                else
                {
                    cmd.Visible = cmd.Enabled = false;
                }
            }
        }

        internal void OnMenuEntityKey(object sender, EventArgs e)
        {
            Debug.Assert(SelectedScalarProperty != null, "SelectedScalarProperty should not be null");
            if (SelectedScalarProperty != null)
            {
                SelectedScalarProperty.ChangeEntityKey();
            }
        }

        internal EventHandler OnStatusSelectAssociationEnd(ConnectorEnd end)
        {
            return (sender, e) =>
                {
                    var cmd = sender as DynamicStatusMenuCommand;
                    Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
                    if (cmd != null)
                    {
                        if (IsSingleSelection()
                            && SelectedAssociationConnector != null)
                        {
                            cmd.Enabled = cmd.Visible = true;
                            var name = (end == ConnectorEnd.Source)
                                           ? SelectedAssociationConnector.ModelElement.SourceEntityType.Name
                                           : SelectedAssociationConnector.ModelElement.TargetEntityType.Name;
                            cmd.Text = String.Format(CultureInfo.CurrentCulture, Resources.SelectAssociationEndCommnadText, name);
                        }
                        else
                        {
                            cmd.Enabled = cmd.Visible = false;
                        }
                    }
                };
        }

        internal EventHandler OnMenuSelectAssociationEnd(ConnectorEnd end)
        {
            return (sender, args) =>
                {
                    Debug.Assert(SelectedAssociationConnector != null, "SelectedAssociationConnector should not be null");
                    if (SelectedAssociationConnector != null)
                    {
                        var diagram = GetDiagram();
                        Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram");
                        if (diagram != null
                            && diagram.ActiveDiagramView != null)
                        {
                            var shape = (end == ConnectorEnd.Source)
                                            ? SelectedAssociationConnector.FromShape
                                            : SelectedAssociationConnector.ToShape;
                            diagram.ActiveDiagramView.Selection.Set(new DiagramItem(shape));
                            diagram.EnsureSelectionVisible();
                        }
                    }
                };
        }

        internal EventHandler OnStatusSelectAssociationProperty(ConnectorEnd end)
        {
            return (sender, e) =>
                {
                    var cmd = sender as DynamicStatusMenuCommand;
                    Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
                    if (cmd != null)
                    {
                        cmd.Enabled = false;
                        cmd.Visible = false;

                        if (IsSingleSelection())
                        {
                            var theConnector = SelectedAssociationConnector;

                            if (theConnector != null)
                            {
                                var navProp = (end == ConnectorEnd.Source)
                                                  ? theConnector.ModelElement.SourceNavigationProperty
                                                  : theConnector.ModelElement.TargetNavigationProperty;

                                // A navigation property can be null
                                if (navProp != null)
                                {
                                    cmd.Enabled = cmd.Visible = true;
                                    cmd.Text = String.Format(
                                        CultureInfo.CurrentCulture, Resources.SelectAssociationPropertyCommandText, navProp.Name);
                                }
                            }
                        }
                    }
                };
        }

        internal EventHandler OnMenuSelectAssociationProperty(ConnectorEnd end)
        {
            return (sender, args) =>
                {
                    Debug.Assert(SelectedAssociationConnector != null, "SelectedAssociationConnector should not be null");
                    if (SelectedAssociationConnector != null)
                    {
                        var diagram = GetDiagram();
                        Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram");
                        if (diagram != null
                            && diagram.ActiveDiagramView != null)
                        {
                            EntityTypeShape entityShape = null;
                            NavigationProperty navProp = null;
                            if (end == ConnectorEnd.Source)
                            {
                                entityShape = SelectedAssociationConnector.FromShape as EntityTypeShape;
                                navProp = SelectedAssociationConnector.ModelElement.SourceNavigationProperty;
                            }
                            else
                            {
                                entityShape = SelectedAssociationConnector.ToShape as EntityTypeShape;
                                navProp = SelectedAssociationConnector.ModelElement.TargetNavigationProperty;
                            }

                            Debug.Assert(entityShape != null, "entityShape should not be null");
                            if (entityShape != null)
                            {
                                var index = entityShape.NavigationCompartment.Items.IndexOf(navProp);
                                if (index >= 0)
                                {
                                    var item = new DiagramItem(
                                        entityShape.NavigationCompartment, entityShape.NavigationCompartment.ListField,
                                        new ListItemSubField(index));
                                    diagram.ActiveDiagramView.Selection.Set(item);
                                    diagram.EnsureSelectionVisible();
                                }
                            }
                        }
                    }
                };
        }

        internal void OnStatusSelectAssociation(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Visible = (IsSingleSelection() && SelectedNavigationProperty != null);
                cmd.Enabled = (cmd.Visible && SelectedNavigationProperty != null && SelectedNavigationProperty.Association != null);
            }
        }

        internal void OnMenuSelectAssociation(object sender, EventArgs e)
        {
            Debug.Assert(SelectedNavigationProperty != null, "SelectedNavigationProperty should not be null");
            if (SelectedNavigationProperty != null)
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram");
                if (diagram != null
                    && diagram.ActiveDiagramView != null)
                {
                    ShapeElement shapeElement = null;
                    foreach (var presentationElement in PresentationViewsSubject.GetPresentation(SelectedNavigationProperty.Association))
                    {
                        var linkShape = presentationElement as LinkShape;
                        if (linkShape != null
                            && linkShape.Diagram == diagram)
                        {
                            shapeElement = linkShape;
                            break;
                        }
                    }

                    if (shapeElement != null)
                    {
                        diagram.ActiveDiagramView.Selection.Set(new DiagramItem(shapeElement));
                        diagram.EnsureSelectionVisible();
                    }
                }
            }
        }

        internal void OnStatusSelectInheritanceEnd(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = (IsSingleSelection() && SelectedInheritanceConnector != null);
            }
        }

        internal EventHandler OnMenuSelectInheritanceEnd(ConnectorEnd end)
        {
            return (sender, args) =>
                {
                    Debug.Assert(SelectedInheritanceConnector != null, "SelectedInheritanceConnector should not be null");
                    if (SelectedInheritanceConnector != null)
                    {
                        var diagram = GetDiagram();
                        Debug.Assert(diagram != null && diagram.ActiveDiagramView != null, "could not find diagram");
                        if (diagram != null
                            && diagram.ActiveDiagramView != null)
                        {
                            var shape = (end == ConnectorEnd.Source)
                                            ? SelectedInheritanceConnector.FromShape
                                            : SelectedInheritanceConnector.ToShape;
                            diagram.ActiveDiagramView.Selection.Set(new DiagramItem(shape));
                            diagram.EnsureSelectionVisible();
                        }
                    }
                };
        }

        internal void OnStatusCutOrCopy(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = CutAndCopyAreEnabled();
            }
        }

        private bool CutAndCopyAreEnabled()
        {
            var enable = false;
            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && GetDiagram() != null)
            {
                // designer case
                var selectedProperty = SelectedProperty;
                if (selectedProperty != null)
                {
                    // single or multi properties case
                    enable = true;
                    foreach (var obj in CurrentSelection)
                    {
                        // enable command only if all selected objects are Properties from same Entity
                        var property = obj as Property;
                        if (property == null
                            || property.EntityType != selectedProperty.EntityType)
                        {
                            enable = false;
                            break;
                        }
                    }
                }
                else
                {
                    // entity/entities with connectors case
                    foreach (var selected in CurrentDocumentSelection)
                    {
                        // we need at least one EntityTypeShape selected to enable command
                        if (selected is EntityTypeShape)
                        {
                            enable = true;
                        }
                        else if (!(selected is AssociationConnector)
                                 && !(selected is InheritanceConnector))
                        {
                            // if something other than EntityTypeShape or Connector is selected, disable command
                            enable = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                // explorer case
                if (SelectedExplorerItem != null)
                {
                    var explorerComplexType = SelectedExplorerItem as ExplorerComplexType;
                    var explorerProperty = SelectedExplorerItem as ExplorerProperty;
                    var explorerEnumType = SelectedExplorerItem as ExplorerEnumType;
                    if (explorerComplexType != null
                        ||
                        (explorerProperty != null && explorerProperty.Parent is ExplorerComplexType))
                    {
                        enable = true;
                    }
                    else if (explorerEnumType != null)
                    {
                        enable = true;
                    }
                }
            }

            return enable;
        }

        private void OnMenuCopy(object sender, EventArgs e)
        {
            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && GetDiagram() != null)
            {
                // designer case
                var selectedProperty = SelectedProperty;
                if (selectedProperty != null)
                {
                    // single or multi properties case
                    Debug.Assert(
                        selectedProperty.EntityType != null && selectedProperty.EntityType.EntityDesignerViewModel != null,
                        "cannot find view model");
                    var viewModel = selectedProperty.EntityType.EntityDesignerViewModel;
                    var modelProperties = new List<ModelEntity.Property>();
                    foreach (var obj in CurrentSelection)
                    {
                        var property = obj as Property;
                        Debug.Assert(
                            property != null && property.EntityType == selectedProperty.EntityType,
                            "Selected object is not a property or is from another EntityType");
                        if (property != null
                            && property.EntityType == selectedProperty.EntityType)
                        {
                            var modelProperty = viewModel.ModelXRef.GetExisting(property) as ModelEntity.Property;
                            Debug.Assert(modelProperty != null, "Selected Property is not mapped to a model Property");
                            modelProperties.Add(modelProperty);
                        }
                    }
                    CopyPasteUtils.CopyToClipboard(modelProperties);
                }
                else
                {
                    // single or multi entity and connectors case
                    var modelEntityTypeShapeCollection = new HashSet<Model.Designer.EntityTypeShape>();

                    // The CurrentSelection should only contains EntityTypeShapes since:
                    // - We don't allow the user to select EntityTypeShape and other types (AssociationConnector or InheritanceConnector) at the same time.
                    // - Copy menu item is no available if an EntityTypeShape is not selected.
                    foreach (var selected in CurrentSelection)
                    {
                        var entityShape = selected as EntityTypeShape;
                        if (entityShape != null)
                        {
                            var et = entityShape.ModelElement as EntityType;
                            var modelEntityShape =
                                et.EntityDesignerViewModel.ModelXRef.GetExisting(entityShape) as Model.Designer.EntityTypeShape;

                            if (modelEntityShape != null
                                && modelEntityTypeShapeCollection.Contains(modelEntityShape) == false)
                            {
                                modelEntityTypeShapeCollection.Add(modelEntityShape);
                            }
                            continue;
                        }

                        Debug.Fail("Selected object is not type of EntityTypeShape.");
                    }
                    Debug.Assert(modelEntityTypeShapeCollection.Count > 0, "No entities found for copy");

                    if (modelEntityTypeShapeCollection.Count > 0)
                    {
                        CopyPasteUtils.CopyToClipboard(modelEntityTypeShapeCollection);
                    }
                }
            }
            else
            {
                // explorer case
                if (SelectedExplorerItem != null)
                {
                    var explorerComplexType = SelectedExplorerItem as ExplorerComplexType;
                    if (explorerComplexType != null)
                    {
                        var complexType = explorerComplexType.ModelItem as ModelEntity.ComplexType;
                        Debug.Assert(complexType != null, "ModelItem is not a ComplexType");
                        if (complexType != null)
                        {
                            CopyPasteUtils.CopyToClipboard(complexType);
                        }
                        return;
                    }

                    var explorerProperty = SelectedExplorerItem as ExplorerProperty;
                    if (explorerProperty != null
                        && explorerProperty.Parent is ExplorerComplexType)
                    {
                        var property = explorerProperty.ModelItem as ModelEntity.Property;
                        Debug.Assert(property != null, "ModelItem is not a Property");
                        if (property != null)
                        {
                            CopyPasteUtils.CopyToClipboard(new[] { property });
                        }
                        return;
                    }

                    var explorerEnumType = SelectedExplorerItem as ExplorerEnumType;
                    if (explorerEnumType != null)
                    {
                        var enumType = explorerEnumType.ModelItem as ModelEntity.EnumType;
                        Debug.Assert(enumType != null, "ModelItem is not a EnumType");
                        if (enumType != null)
                        {
                            CopyPasteUtils.CopyToClipboard(enumType);
                        }
                        return;
                    }
                }
            }
        }

        private void OnMenuCut(object sender, EventArgs e)
        {
            // perform copy + delete
            OnMenuCopy(sender, e);

            // special case for EntityTypeShape on diagram surface.
            // "cut" performs "remove from diagram" instead of "delete from model"
            var diagram = GetDiagram();
            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && diagram != null)
            {
                if (AreAllSelectedItemsTypeof<EntityTypeShape>())
                {
                    OnMenuRemoveFromDiagram(sender, e);
                    return;
                }
            }

            OnMenuDelete(sender, e);
        }

        internal void OnStatusPaste(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && GetDiagram() != null)
                {
                    // designer case
                    // do not show Paste menu item if selected element is the NavigationProperties header or a NavigationProperty itself
                    if (SelectedNavigationPropertiesCompartment == null
                        && SelectedNavigationProperty == null
                        && (CopyPasteUtils.GetEntitiesFromClipboard() != null ||
                         (CopyPasteUtils.GetPropertiesFromClipboard() != null
                          && GetSelectedCompartment(true /* Properties Compartment */) != null)))
                    {
                        cmd.Enabled = cmd.Visible = true;
                        return;
                    }
                }
                else
                {
                    // explorer case
                    var pasteTarget = GetExplorerPasteTarget();
                    if (pasteTarget != null)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        return;
                    }
                }

                // Paste Command should not be enabled, but it may still be visible if
                // Cut/Copy are visible
                cmd.Enabled = false;
                cmd.Visible = CutAndCopyAreEnabled();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void OnMenuPaste(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && diagram != null)
            {
                // designer case
                var clipboardProperties = CopyPasteUtils.GetPropertiesFromClipboard();
                if (clipboardProperties != null)
                {
                    var propertiesCompartment = GetSelectedCompartment(true /* Properties Compartment */);
                    Debug.Assert(propertiesCompartment != null, "PropertiesCompartment not found");
                    if (propertiesCompartment != null)
                    {
                        var entityShape = propertiesCompartment.ParentShape as EntityTypeShape;
                        Debug.Assert(entityShape != null, "entityShape should not be null");
                        if (entityShape != null)
                        {
                            var entityType = entityShape.ModelElement as EntityType;
                            Debug.Assert(entityType != null, "entityType should not be null");
                            if (entityType != null)
                            {
                                var modelEntity =
                                    entityType.EntityDesignerViewModel.ModelXRef.GetExisting(entityType) as ModelEntity.EntityType;
                                Debug.Assert(modelEntity != null, "modelEntity should not be null");
                                if (modelEntity != null)
                                {
                                    var cpc = new CommandProcessorContext(
                                        diagram.ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId,
                                        Resources.Tx_Paste);

                                    // When a property is selected, that means the user wants to paste the property next to the selected property.
                                    ModelEntity.InsertPropertyPosition position = null;
                                    var vmProperty = SingleSelection as PropertyBase;
                                    // Check if there is only 1 property is selected, we will add the properties at the last position if there are multiple selected properties.
                                    if (IsSingleSelection()
                                        && vmProperty != null)
                                    {
                                        var modelProperty =
                                            entityType.EntityDesignerViewModel.ModelXRef.GetExisting(vmProperty) as ModelEntity.PropertyBase;
                                        Debug.Assert(
                                            modelProperty != null,
                                            "Could not find model property for DSL Property: " + vmProperty.Name + " from model xref.");
                                        if (modelProperty != null)
                                        {
                                            position = new ModelEntity.InsertPropertyPosition(modelProperty, false);
                                        }
                                    }

                                    var cmd = new CopyPropertiesCommand(clipboardProperties, modelEntity, position);
                                    var cp = new CommandProcessor(cpc, cmd);
                                    cp.Invoke();
                                    // Ensure that newly created properties are selected.
                                    // Since we don't support copy and past for navigation properties, we can safely assume pass the entity's PropertiesCompartment.
                                    SelectProperties(cmd.Properties, entityShape.PropertiesCompartment);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Get Model Diagram
                    var modelDiagram = diagram.GetModel().ModelXRef.GetExisting(diagram) as Diagram;
                    Debug.Assert(modelDiagram != null, "Could not find model diagram for diagram with title:" + diagram.Title);

                    if (modelDiagram != null)
                    {
                        var clipboardEntities = CopyPasteUtils.GetEntitiesFromClipboard();
                        // check to see if we have at least 1 EntityClipboardFormat in Clipboard.
                        if (clipboardEntities != null
                            && clipboardEntities.ClipboardEntities != null
                            && clipboardEntities.ClipboardEntities.Count > 0)
                        {
                            var cpc = new CommandProcessorContext(
                                diagram.ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Resources.Tx_Paste);
                            Command cmd = new CopyEntitiesCommand(modelDiagram, clipboardEntities, Command.ModelSpace.Conceptual);
                            var cp = new CommandProcessor(cpc, cmd);
                            diagram.Arranger.Start(GetPositionForNewElements());
                            cp.Invoke();
                            diagram.Arranger.End();
                        }
                    }
                }
                diagram.EnsureSelectionVisible();
            }
            else if (MonitorSelection != null
                     && MonitorSelection.CurrentWindow is EntityDesignExplorerWindow)
            {
                HandleOnMenuPasteInExplorerWindow();
            }
            // we only support copy and paste in Designer and Model Browser window, copy/paste on other windows should be a no-op.
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void HandleOnMenuPasteInExplorerWindow()
        {
            // explorer case
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
            var cpc = new CommandProcessorContext(editingContext, EfiTransactionOriginator.ExplorerWindowOriginatorId, Resources.Tx_Paste);

            var clipboardComplexType = CopyPasteUtils.GetComplexTypeFromClipboard();
            var clipboardEnumType = CopyPasteUtils.GetEnumTypeFromClipboard();

            EFObject newlyCreatedObject = null; // the newly created object from paste operation.

            if (clipboardComplexType != null)
            {
                var explorerComplexTypes = GetExplorerPasteTarget() as ExplorerComplexTypes;
                Debug.Assert(
                    explorerComplexTypes != null,
                    "Unexpected attempt to copy a clipboard with a complex type when the following Explorer object is selected: "
                    + SelectedExplorerItem.Name);
                if (explorerComplexTypes != null)
                {
                    var cmd = new CopyComplexTypeCommand(clipboardComplexType);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    newlyCreatedObject = cmd.ComplexType;
                }
            }
            else if (clipboardEnumType != null)
            {
                var explorerEnumTypes = GetExplorerPasteTarget() as ExplorerEnumTypes;
                Debug.Assert(
                    explorerEnumTypes != null,
                    "Unexpected attempt to copy a clipboard with a enum type when the following Explorer object is selected: "
                    + SelectedExplorerItem.Name);
                if (explorerEnumTypes != null)
                {
                    var cmd = new CopyEnumTypeCommand(clipboardEnumType);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    newlyCreatedObject = cmd.EnumType;
                }
            }
            else
            {
                var clipboardProperties = CopyPasteUtils.GetPropertiesFromClipboard();
                Debug.Assert(
                    clipboardProperties != null,
                    "The object(s) in the clipboard are of type neither ComplexTypeClipboardFormat nor PropertiesClipboardFormat");
                if (clipboardProperties != null)
                {
                    var explorerComplexType = GetExplorerPasteTarget() as ExplorerComplexType;
                    Debug.Assert(
                        explorerComplexType != null,
                        "Unexpected attempt to copy a clipboard containing a set of Property object(s) when the following Explorer object is selected: "
                        + SelectedExplorerItem.Name);
                    if (explorerComplexType != null)
                    {
                        var complexType = explorerComplexType.ModelItem as ModelEntity.ComplexType;
                        Debug.Assert(
                            complexType != null,
                            "When attempting to copy a clipboard containing a set of Property object(s), ModelItem has unexpected type "
                            + explorerComplexType.ModelItem.GetType().FullName);
                        if (complexType != null)
                        {
                            var cmd = new CopyPropertiesCommand(clipboardProperties, complexType);
                            CommandProcessor.InvokeSingleCommand(cpc, cmd);
                            newlyCreatedObject = cmd.Properties.FirstOrDefault();
                                // in this case just select the first property since we don't support multi select in model browser.
                        }
                    }
                }
            }

            if (newlyCreatedObject != null)
            {
                ExplorerNavigationHelper.NavigateTo(newlyCreatedObject);
            }
        }

        /// <summary>
        ///     Return the parent to which the currently selected Clipboard contents should be added
        ///     by a Paste Command based on the currently selected Explorer item
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static ExplorerEFElement GetExplorerPasteTarget()
        {
            var selectedExplorerItem = SelectedExplorerItem;
            if (selectedExplorerItem != null)
            {
                if (CopyPasteUtils.GetComplexTypeFromClipboard() != null)
                {
                    // Clipboard contains a ComplexType

                    if (selectedExplorerItem is ExplorerComplexTypes)
                    {
                        // user has selected the EnumTypes dummy node
                        // in this case the dummy node itself is the target
                        return selectedExplorerItem;
                    }
                    else if (selectedExplorerItem is ExplorerComplexType)
                    {
                        // user has selected another ComplexType node in the Explorer
                        // in this case the pasted ComplexType will be a sibling of the selected
                        // ComplexType and the target node is the parent ComplexTypes dummy node
                        var complexTypesExplorerElement = selectedExplorerItem.Parent as ExplorerComplexTypes;
                        Debug.Assert(
                            complexTypesExplorerElement != null,
                            "Parent of ComplexType node is of type " + selectedExplorerItem.Parent.GetType().FullName
                            + ". Should be ExplorerComplexTypes");
                        return complexTypesExplorerElement;
                    }
                }
                else if (CopyPasteUtils.GetEnumTypeFromClipboard() != null)
                {
                    // Clipboard contains an EnumType

                    if (selectedExplorerItem is ExplorerEnumTypes)
                    {
                        // user has selected the ComplexTypes dummy node
                        // in this case the dummy node itself is the target
                        return selectedExplorerItem;
                    }
                    else if (selectedExplorerItem is ExplorerEnumType)
                    {
                        // user has selected another EnumType node in the Explorer
                        // in this case the pasted ComplexType will be a sibling of the selected
                        // EnumType and the target node is the parent EnumTypes dummy node
                        var enumTypesExplorerElement = selectedExplorerItem.Parent as ExplorerEnumTypes;
                        Debug.Assert(
                            enumTypesExplorerElement != null,
                            "Parent of EnumType node is of type " + selectedExplorerItem.Parent.GetType().FullName
                            + ". Should be ExplorerEnumTypes");
                        return enumTypesExplorerElement;
                    }
                }
                else if (CopyPasteUtils.GetPropertiesFromClipboard() != null)
                {
                    // Clipboard contains a set of Property objects

                    if (selectedExplorerItem is ExplorerComplexType)
                    {
                        // user has selected the ComplexType node into which they would
                        // like to Paste - in this case the ComplexType node itself is the target
                        return selectedExplorerItem;
                    }

                    var explorerProperty = selectedExplorerItem as ExplorerConceptualProperty;
                    if (explorerProperty != null
                        && explorerProperty.Parent is ExplorerComplexType)
                    {
                        // user has selected a ComplexType Property node in the Explorer
                        // in this case the pasted ComplexType Properties will be siblings
                        // of the selected ComplexType Property so the target is the parent
                        // ComplexType
                        return explorerProperty.Parent;
                    }
                }
            }

            // clipboard contains objects which cannot be pasted on the Explorer
            // or the user has selected a target node which does not allow pasting
            return null;
        }

        private PointD GetPositionForNewElements()
        {
            if (CurrentMicrosoftDataEntityDesignDocView != null
                && CurrentMicrosoftDataEntityDesignDocView.IsContextMenuShowing)
            {
                return CurrentMicrosoftDataEntityDesignDocView.ContextMenuMousePosition;
            }

            return PointD.Empty;
        }

        private class CommandZoomToLevel : DynamicStatusMenuCommand
        {
            private readonly int _zoomLevel;

            public CommandZoomToLevel(EventHandler statusHandler, EventHandler invokeHandler, CommandID commandId, int zoomLevel)
                : base(statusHandler, invokeHandler, commandId)
            {
                _zoomLevel = zoomLevel;
            }

            public int ZoomLevel
            {
                get { return _zoomLevel; }
            }
        }

        private bool IsArtifactDesignerSafeAndEditSafe()
        {
            if (null != CurrentDocData)
            {
                var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                ModelManager modelManager = PackageManager.Package.ModelManager;
                var artifact = modelManager.GetArtifact(uri) as EntityDesignArtifact;

                // artifact may be null when we are shutting down
                if (artifact != null)
                {
                    return artifact.IsDesignerSafeAndEditSafe();
                }
            }
            return false;
        }

        private bool IsArtifactInNonMiscFilesProject()
        {
            if (null != CurrentDocData)
            {
                var project = VSHelpers.GetProjectForDocument(CurrentDocData.FileName, Services.ServiceProvider);
                if (project != null)
                {
                    if (!VsUtils.IsMiscellaneousProject(project))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void IsArtifactDesignerSafeAndEditSafeHandler(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                if (IsArtifactDesignerSafeAndEditSafe() == false)
                {
                    cmd.Enabled = cmd.Visible = false;
                }
            }
        }

        private void IsDiagramLockedHandler(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    if (diagram.Partition.GetLocks() == Locks.All)
                    {
                        cmd.Enabled = cmd.Visible = false;
                    }
                }
            }
        }

        private static EventHandler CombineEventHandler(params EventHandler[] eventHandlers)
        {
            var d = Delegate.Combine(eventHandlers);
            return d as EventHandler;
        }

        private EventHandler CreateDesignerSafeAndEditSafeOnlyStatusEventHandler(EventHandler eventHandler)
        {
            var eh = CombineEventHandler(eventHandler, IsArtifactDesignerSafeAndEditSafeHandler);
            return eh;
        }

        private EventHandler CreateIsDiagramLockedStatusEventHandler(EventHandler eventHandler)
        {
            var eh = CombineEventHandler(eventHandler, IsDiagramLockedHandler);
            return eh;
        }

        internal void OnStatusAddComplexType(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;

                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && GetDiagram() != null)
                {
                    if (IsOurDiagramSelected())
                    {
                        cmd.Enabled = cmd.Visible = true;
                        cmd.Text = Resources.AddComplexTypeCommand_DesignerText;
                        return;
                    }
                }
                else
                {
                    var explorerSelection = SelectedExplorerItem;
                    if (explorerSelection is ExplorerComplexTypes
                        || explorerSelection is ExplorerComplexType)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        cmd.Text = Resources.AddComplexTypeCommand_ExplorerText;
                        return;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void OnMenuAddComplexType(object sender, EventArgs e)
        {
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
            var cpc = new CommandProcessorContext(
                editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Design.Resources.Tx_AddComplexType);
            var complexType = CreateComplexTypeCommand.CreateComplexTypeWithDefaultName(cpc);
            Debug.Assert(complexType != null, "Creating ComplexType failed");
            if (complexType != null)
            {
                var frame = ExplorerFrame;
                if (frame != null)
                {
                    frame.NavigateToElementAndPutInRenameMode(complexType);
                }
            }
        }

        internal void OnStatusGoToDefinition(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                if (MonitorSelection != null
                    && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                    && diagram != null)
                {
                    if (SelectedComplexProperty != null)
                    {
                        cmd.Visible = true;
                        var complexProperty =
                            diagram.ModelElement.ModelXRef.GetExisting(SelectedComplexProperty) as ModelEntity.ComplexConceptualProperty;
                        if (complexProperty != null
                            && complexProperty.ComplexType.Status == BindingStatus.Known)
                        {
                            cmd.Enabled = true;
                        }
                    }
                }
                else
                {
                    if (SelectedExplorerItem != null)
                    {
                        var complexProperty = SelectedExplorerItem.ModelItem as ModelEntity.ComplexConceptualProperty;
                        if (complexProperty != null)
                        {
                            cmd.Visible = true;
                            if (complexProperty.ComplexType.Status == BindingStatus.Known)
                            {
                                cmd.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        internal void OnMenuGoToDefinition(object sender, EventArgs e)
        {
            ModelEntity.ComplexConceptualProperty complexProperty = null;
            var diagram = GetDiagram();
            if (diagram != null
                && SelectedComplexProperty != null)
            {
                complexProperty =
                    diagram.ModelElement.ModelXRef.GetExisting(SelectedComplexProperty) as ModelEntity.ComplexConceptualProperty;
            }
            else
            {
                if (SelectedExplorerItem != null)
                {
                    complexProperty = SelectedExplorerItem.ModelItem as ModelEntity.ComplexConceptualProperty;
                }
            }

            if (complexProperty != null
                && complexProperty.ComplexType.Status == BindingStatus.Known)
            {
                ExplorerNavigationHelper.NavigateTo(complexProperty.ComplexType.Target);
            }
        }

        internal void OnStatusCreateComplexType(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                var enable = false;
                var selectedProperty = SelectedProperty;
                if (selectedProperty != null)
                {
                    enable = true;
                    foreach (var obj in CurrentSelection)
                    {
                        // enable command only if all selected objects are non-key Properties from same Entity
                        var property = obj as Property;
                        var scalarProperty = property as ScalarProperty;
                        if (property == null
                            || property.EntityType != selectedProperty.EntityType
                            ||
                            (scalarProperty != null && scalarProperty.EntityKey))
                        {
                            enable = false;
                            break;
                        }
                    }
                }
                cmd.Visible = cmd.Enabled = enable;
            }
        }

        internal void OnMenuCreateComplexType(object sender, EventArgs e)
        {
            var selectedProperty = SelectedProperty;
            Debug.Assert(selectedProperty != null, "Unexpected object selected - selectedProperty is null");
            if (selectedProperty != null)
            {
                // gather all model properties from selected elements
                Debug.Assert(
                    selectedProperty.EntityType != null && selectedProperty.EntityType.EntityDesignerViewModel != null,
                    "could not find view model");
                var viewModel = selectedProperty.EntityType.EntityDesignerViewModel;
                var modelProperties = new List<ModelEntity.Property>();
                foreach (var obj in CurrentSelection)
                {
                    var property = obj as Property;
                    Debug.Assert(
                        property != null && property.EntityType == selectedProperty.EntityType,
                        "Selected object is not a property or is from another EntityType");
                    if (property != null
                        && property.EntityType == selectedProperty.EntityType)
                    {
                        var modelProperty = viewModel.ModelXRef.GetExisting(property) as ModelEntity.Property;
                        Debug.Assert(modelProperty != null, "Selected Property is not mapped to a model Property");
                        if (modelProperty != null)
                        {
                            modelProperties.Add(modelProperty);
                        }
                    }
                }

                // create ComplexType from selected properties
                var modelEntity = viewModel.ModelXRef.GetExisting(selectedProperty.EntityType) as ModelEntity.EntityType;
                Debug.Assert(modelEntity != null, "Couldn't find model EntityType");
                if (modelEntity != null
                    && modelProperties.Count > 0)
                {
                    var cpc = new CommandProcessorContext(
                        viewModel.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Resources.Tx_CreateComplexType);
                    var cmd = new CreateComplexTypeFromPropertiesCommand(modelEntity, modelProperties);
                    var cp = new CommandProcessor(cpc, cmd);
                    cp.Invoke();
                    var complexType = cmd.ComplexType;
                    Debug.Assert(complexType != null, "Creating ComplexType failed");
                    if (complexType != null)
                    {
                        var frame = ExplorerFrame;
                        if (frame != null)
                        {
                            frame.NavigateToElementAndPutInRenameMode(complexType);
                        }
                    }
                }
            }
        }

        internal void OnStatusExplorerComplexTypes(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                var explorerCT = SelectedExplorerItem as ExplorerComplexType;
                if (explorerCT != null
                    && explorerCT.ModelItem is ModelEntity.ComplexType)
                {
                    cmd.Enabled = cmd.Visible = true;
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void OnMenuExplorerComplexTypes(object sender, EventArgs e)
        {
            var explorerCT = SelectedExplorerItem as ExplorerComplexType;
            Debug.Assert(explorerCT != null, "Unexpected object selected");
            if (explorerCT != null)
            {
                var complexType = explorerCT.ModelItem as ModelEntity.ComplexType;
                Debug.Assert(complexType != null, "ModelItem is not ComplexType");
                if (complexType != null)
                {
                    var cModel = complexType.RuntimeModelRoot() as ModelEntity.ConceptualEntityModel;
                    Debug.Assert(cModel != null, "Conceptual model is null");
                    if (cModel != null)
                    {
                        using (var dialog = new ComplexTypePickerDialog(cModel, complexType))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                var returnType = dialog.ComplexType;
                                if (returnType != null)
                                {
                                    var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                                    var context = new EfiTransactionContext();
                                    var editingContext =
                                        PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                                    var cpc = new CommandProcessorContext(
                                        editingContext, EfiTransactionOriginator.ExplorerWindowOriginatorId,
                                        Design.Resources.Tx_CreateScalarProperty, null, context);
                                    var property = CreateComplexTypePropertyCommand.CreateDefaultProperty(cpc, complexType, returnType);
                                    if (property != null)
                                    {
                                        var frame = ExplorerFrame;
                                        if (frame != null)
                                        {
                                            frame.NavigateToElementAndPutInRenameMode(property);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void OnStatusAddNewTemplate(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Visible = true;
                if (IsArtifactInNonMiscFilesProject())
                {
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Enabled = false;
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRegisterNewDialogFilters.UnregisterAddNewItemDialogFilter(System.UInt32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRegisterNewDialogFilters.RegisterAddNewItemDialogFilter(Microsoft.VisualStudio.Shell.Interop.IVsFilterAddProjectItemDlg,System.UInt32@)")]
        internal void OnMenuAddNewTemplate(object sender, EventArgs e)
        {
            uint dwFilterCookie = 0;
            IVsRegisterNewDialogFilters registerNewDialogFilters = null;

            var project = VSHelpers.GetProjectForDocument(CurrentDocData.FileName, Services.ServiceProvider);
            Debug.Assert(
                project != null, "Cannot add AddNewItemDialogFilter. The project does not exist for file name " + CurrentDocData.FileName);
            if (project != null)
            {
                registerNewDialogFilters = ServiceProvider.GetService(typeof(SVsRegisterNewDialogFilters)) as IVsRegisterNewDialogFilters;
                Debug.Assert(registerNewDialogFilters != null, "couldn't find SvsRegisterNewDialogFilters service");
                if (registerNewDialogFilters != null)
                {
                    try
                    {
                        // Register my filter
                        registerNewDialogFilters.RegisterAddNewItemDialogFilter(new AddNewItemDialogFilter(), out dwFilterCookie);

                        var dte = (DTE)Services.ServiceProvider.GetService(typeof(DTE));

                        // Show the "Add...New...Item" dialog via DTE
                        AddArtifactGeneratorWizard.EdmxUri = Utils.FileName2Uri(CurrentDocData.FileName);

                        DbContextCodeGenerator.AddAndNestCodeGenTemplates(
                            VsUtils.GetProjectItemForDocument(CurrentDocData.FileName, Services.ServiceProvider),
                            () => dte.ExecuteCommand("Project.AddNewItem", String.Empty));
                    }
                    finally
                    {
                        if (dwFilterCookie != 0)
                        {
                            registerNewDialogFilters.UnregisterAddNewItemDialogFilter(dwFilterCookie);
                        }
                        AddArtifactGeneratorWizard.EdmxUri = null;
                    }
                }
            }
        }

        #endregion

        #region Diagram Menu items

        /// <summary>
        ///     Determine whether to show create diagram menu item. We only want to display it if the Diagram container is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusAddNewDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                // only show this if the ExplorerItem is in focus (GetDiagram should return null).
                if (diagram == null)
                {
                    var explorerSelection = SelectedExplorerItem;
                    if (explorerSelection is ExplorerDiagrams
                        || explorerSelection is ExplorerDiagram)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     Create a new diagram with a default name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddNewDiagram(object sender, EventArgs e)
        {
            var diagram = CreateDiagramWithDefaultName();
            Debug.Assert(diagram != null, "Creating Diagram failed");
            if (diagram != null)
            {
                var frame = ExplorerFrame;
                if (frame != null)
                {
                    // we want to navigate to newly created diagram node in mode browser and put the node to rename mode.
                    frame.NavigateToElementAndPutInRenameMode(diagram);
                    // Open the newly created diagram.
                    OnMenuOpenDiagram(sender, e);
                }
            }
        }

        private Diagram CreateDiagramWithDefaultName()
        {
            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
            var cpc = new CommandProcessorContext(
                editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Design.Resources.Tx_CreateDiagram);
            var diagram = CreateDiagramCommand.CreateDiagramWithDefaultName(cpc);
            return diagram;
        }

        /// <summary>
        ///     Show Open Diagram menu item only if the selected item in model browser is a Diagram.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusOpenDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                // only show this if the ExplorerItem is in focus (GetDiagram should return null).
                if (diagram == null)
                {
                    var explorerSelection = SelectedExplorerItem;
                    if (explorerSelection is ExplorerDiagram)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     Open the selected diagram in a new tab.
        ///     if the diagram is already opened, set the window frame to be visible and in focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuOpenDiagram(object sender, EventArgs e)
        {
            var contextItem = DiagramManagerContextItem;
            var explorerDiagram = SelectedExplorerItem as ExplorerDiagram;

            if (contextItem != null
                && explorerDiagram != null)
            {
                contextItem.DiagramManager.OpenDiagram(explorerDiagram.DiagramMoniker, true);
            }
        }

        internal void OnStatusAddToDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a MenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;
                // We only show the menu item if the ExplorerItem is selected.
                if (SingleSelection == null
                    && SelectedExplorerItem != null)
                {
                    var dslDiagram = CurrentMicrosoftDataEntityDesignDocView.CurrentDiagram as EntityDesignerDiagram;
                    if (dslDiagram != null)
                    {
                        var modelDiagram = dslDiagram.ModelElement.ModelXRef.GetExisting(dslDiagram) as Diagram;
                        var efElement = SelectedExplorerItem.ModelItem;

                        if (efElement != null
                            && modelDiagram != null)
                        {
                            cmd.Enabled = cmd.Visible = ((SelectedExplorerItem is ExplorerConceptualEntityType
                                                          || SelectedExplorerItem is ExplorerConceptualAssociation
                                                          || SelectedExplorerItem is ExplorerEntitySet
                                                          || SelectedExplorerItem is ExplorerAssociationSet)
                                                         && modelDiagram.IsEFObjectRepresentedInDiagram(efElement) == false);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Add or Show EFElement in the selected/Active diagram.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuAddToDiagram(object sender, EventArgs e)
        {
            if (SelectedExplorerItem != null
                && CurrentMicrosoftDataEntityDesignDocView != null)
            {
                var diagram = CurrentMicrosoftDataEntityDesignDocView.Diagram as IViewDiagram;

                if (diagram != null)
                {
                    diagram.AddOrShowEFElementInDiagram(SelectedExplorerItem.ModelItem);
                }
            }
        }

        /// <summary>
        ///     Show Close Diagram menu item only if the selected item in model browser is a Diagram.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusCloseDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                // only show this if the ExplorerItem is in focus (GetDiagram should return null).
                if (diagram == null)
                {
                    var explorerSelection = SelectedExplorerItem;
                    if (explorerSelection is ExplorerDiagram)
                    {
                        cmd.Enabled = cmd.Visible = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     Close selected diagram window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuCloseDiagram(object sender, EventArgs e)
        {
            var contextItem = DiagramManagerContextItem;
            var explorerDiagram = SelectedExplorerItem as ExplorerDiagram;
            if (contextItem != null
                && explorerDiagram != null)
            {
                contextItem.DiagramManager.CloseDiagram(explorerDiagram.DiagramMoniker);
            }
        }

        private DiagramManagerContextItem DiagramManagerContextItem
        {
            get
            {
                var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                var contextItem = editingContext.Items.GetValue<DiagramManagerContextItem>();
                Debug.Assert(contextItem != null, "Could not find DiagramManagerContextItem in editing context");
                return contextItem;
            }
        }

        /// <summary>
        ///     The menu item that allows the user to move entity-type-shapes from a diagram to another diagram.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusMoveToNewDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                // We only show the menu item if entity diagram is active (GetDiagram() will return non null value).
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    cmd.Enabled = cmd.Visible = AreAllSelectedItemsTypeof<EntityTypeShape>();
                }
            }
        }

        /// <summary>
        ///     The menu item that allows the user to move entity-type-shapes to another diagram.
        ///     It will do the following:
        ///     - Copy the selected shapes to clipboard.
        ///     - Remove the selected shapes from Current Diagram.
        ///     - Create a new diagram.
        ///     - Open the newly created diagram.
        ///     - Paste the shapes to the new diagram
        ///     - Select the new diagram node in model browser and put the node in rename mode.
        ///     TODO: MoveToNewDiagram will generate 3 transactions (delete entity-type-shapes, create new diagram, add entity-type-shapes),
        ///     it would be nice to be able to do all the above in a single transaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnMenuMoveToNewDiagram(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(
                diagram != null, "Entity designer diagram should be active and in focus when 'Move to New Diagram' menu item is called.");

            if (diagram != null)
            {
                var areSelectedItemsEntityTypeShape = AreAllSelectedItemsTypeof<EntityTypeShape>();
                Debug.Assert(
                    areSelectedItemsEntityTypeShape,
                    "The selected items must be type of entity type shape when 'Move to New Diagram' menu item is called");

                if (areSelectedItemsEntityTypeShape)
                {
                    // Copy the selected items to clipboard and delete the selected items from the diagram
                    OnMenuCut(sender, e);
                    // Create a new diagram with default names.
                    var modelDiagram = CreateDiagramWithDefaultName();
                    Debug.Assert(modelDiagram != null, "Model diagram is null. It should be created by now.");

                    if (modelDiagram != null)
                    {
                        // Open the newly created diagram.
                        Debug.Assert(DiagramManagerContextItem != null, "Diagram Manager Context item should not be null.");
                        if (DiagramManagerContextItem != null)
                        {
                            DiagramManagerContextItem.DiagramManager.OpenDiagram(modelDiagram.Id.Value, true);

                            // Check if the current active diagram is the same as the diagram that we opened in previous step.
                            Debug.Assert(
                                GetDiagram().DiagramId == modelDiagram.Id.Value,
                                "Expected the active diagram: " + modelDiagram.Name.Value + ", Actual active diagram:" + GetDiagram().Title);

                            // Paste the items in the clipboard to the new diagram.
                            OnMenuPaste(sender, e);

                            // The code below ensures that new diagram node is selected and in rename mode in model browser window.
                            var frame = ExplorerFrame;
                            Debug.Assert(frame != null, "Explorer frame is null");
                            if (frame != null)
                            {
                                // set the focus to the newly created diagram in model browser. This step must be done as the last step because other operation could cause focus to change.
                                frame.NavigateToElementAndPutInRenameMode(modelDiagram);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     The menu item to delete shapes (diagram items) and corresponding model items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStatusRemoveFromDiagram(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;

                // We only show the menu item if entity diagram is active. (GetDiagram() will return non null value).
                var diagram = GetDiagram();
                if (diagram != null
                    && OnlyDeletableObjectsSelected())
                {
                    if (AreAllSelectedItemsTypeof<EntityTypeShape>())
                    {
                        cmd.Enabled = cmd.Visible = true;
                        ProcessOnStatusDeleteCommand(cmd);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks to see whether only objects which are allowed to be deleted from the model are selected
        /// </summary>
        /// <returns></returns>
        private bool OnlyDeletableObjectsSelected()
        {
            if (CurrentSelection.Count == 0)
            {
                return false;
            }

            var allowedTypes = new List<Type>
                {
                    typeof(PropertyBase),
                    typeof(InheritanceConnector),
                    typeof(AssociationConnector),
                    typeof(EntityTypeShape)
                };

            foreach (var o in CurrentSelection)
            {
                var selectedType = o.GetType();
                if (!allowedTypes.Any(type => type.IsAssignableFrom(selectedType)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Event handler to delete shapes (diagram items) and corresponding model items.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void OnMenuRemoveFromDiagram(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            if (MonitorSelection != null
                && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                && diagram != null)
            {
                // For entity-type shape we only delete the diagram model items, not the model that the diagram items represent.
                // Note that this command is only available for Entity-Type-Shape. For any other types it should not be available.
                // We also update the behavior of what the user can select in the diagram.
                // The user cannot select Entity-Type-Shapes and other types at the same time.
                // Please see EntityDesignerDiagramSelectionRules class for more information.
                if (SelectedEntityTypeShapes.Count > 0)
                {
                    Debug.Assert(
                        AreAllSelectedItemsTypeof<EntityTypeShape>(),
                        "If an entity type shape is selected in the diagram, no other type should be selected.");

                    if (AreAllSelectedItemsTypeof<EntityTypeShape>())
                    {
                        IList<Command> deleteCommands = new List<Command>();
                        foreach (var ets in SelectedEntityTypeShapes)
                        {
                            // check if the EntityTypeShape model element is not null.
                            Debug.Assert(
                                ets.TypedModelElement != null, "DSL entity type shape (PEL) does not have the corresponding model element");
                            if (ets.TypedModelElement != null)
                            {
                                var viewModelEntityType = ets.TypedModelElement;
                                var modelEntityTypeShape =
                                    viewModelEntityType.EntityDesignerViewModel.ModelXRef.GetExisting(ets) as Model.Designer.EntityTypeShape;
                                Debug.Assert(
                                    modelEntityTypeShape != null,
                                    "Why DslModel-EscherModel XRef does not contain Escher model diagram item for DSL entity-type-shape: "
                                    + viewModelEntityType.Name);
                                if (modelEntityTypeShape != null)
                                {
                                    deleteCommands.Add(modelEntityTypeShape.GetDeleteCommand());
                                }
                            }
                        }

                        if (deleteCommands.Count > 0)
                        {
                            var transactionName = Resources.Tx_DeleteItems;
                            if (deleteCommands.Count == 1)
                            {
                                var ets = SelectedEntityTypeShapes.FirstOrDefault();
                                Debug.Assert(null != ets, "There should be a selected entity type.");
                                if (null != ets)
                                {
                                    transactionName = String.Format(
                                        CultureInfo.CurrentCulture, Resources.Tx_Delete, ets.TypedModelElement.Name);
                                }
                            }
                            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                            var cp = new CommandProcessor(
                                editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, transactionName, deleteCommands);
                            cp.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Menu item to import related entity type to an entityType.
        ///     This menu item is only shown if a single entity-type is selected in the diagram.
        /// </summary>
        internal void OnStatusIncludeRelatedEntityType(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                // We only show the menu item if entity diagram is active (GetDiagram() will return non null value).
                var diagram = GetDiagram();
                if (IsSingleSelection()
                    && diagram != null)
                {
                    cmd.Enabled = cmd.Visible = AreAllSelectedItemsTypeof<EntityTypeShape>();
                }
            }
        }

        /// <summary>
        ///     Event handler to create the related entity-types of the selected entity-type to a diagram.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void OnMenuIncludeRelatedEntityType(object sender, EventArgs e)
        {
            var diagram = GetDiagram();
            Debug.Assert(diagram != null, "There is no diagram selected");

            if (diagram != null)
            {
                var entityTypeShape = SingleSelection as EntityTypeShape;
                Debug.Assert(entityTypeShape != null, "EntityTypeShape is null.");

                if (entityTypeShape != null)
                {
                    var modelDiagram = diagram.ModelElement.ModelXRef.GetExisting(diagram) as Diagram;
                    Debug.Assert(modelDiagram != null, "Could not get the model diagram for diagram with title:" + diagram.Title);
                    var entityType = diagram.ModelElement.ModelXRef.GetExisting(entityTypeShape.ModelElement) as ModelEntity.EntityType;
                    Debug.Assert(entityType != null, "EntityType is null");

                    if (modelDiagram != null
                        && entityType != null)
                    {
                        var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                        var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                        var cpc = new CommandProcessorContext(
                            editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Resources.Tx_IncludeRelatedEntityTypeShape);

                        // Find all related entity types.
                        var relatedEntityTypes = ModelHelper.GetRelatedEntityTypes(entityType);
                        // Get all related entity types not in the diagram.
                        IList<ModelEntity.EntityType> relatedEntityTypesNotInDiagram =
                            relatedEntityTypes.Except(modelDiagram.EntityTypeShapes.Select(ets => ets.EntityType.SafeTarget)).ToList();

                        // if there is no related EntityTypes just return.
                        if (relatedEntityTypesNotInDiagram.Count == 0)
                        {
                            return;
                        }

                        try
                        {
                            diagram.Arranger.Start(PointD.Empty);
                            var delegateCommand = new DelegateCommand(
                                () =>
                                    {
                                        foreach (var et in relatedEntityTypesNotInDiagram)
                                        {
                                            CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                                                cpc, modelDiagram, et as ModelEntity.ConceptualEntityType, entityTypeShape.FillColor, false);
                                        }
                                    });
                            CommandProcessor.InvokeSingleCommand(cpc, delegateCommand);
                        }
                        finally
                        {
                            diagram.Arranger.End();
                            diagram.EnsureSelectionVisible();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Menu item to allow the user move diagram nodes to a separate file.
        /// </summary>
        internal void OnStatusMoveDiagramsToSeparateFile(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;

                // check whether the EDMX Project Item is a link.
                var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                var artifactProjectItem = VsUtils.GetProjectItemForDocument(uri.LocalPath, Services.ServiceProvider);

                if (artifactProjectItem != null
                    && VsUtils.IsLinkProjectItem(artifactProjectItem) == false)
                {
                    // Only show if the diagram artifact is not null
                    var modelManager = PackageManager.Package.ModelManager;
                    var artifact = modelManager.GetArtifact(uri) as EntityDesignArtifact;
                    Debug.Assert(artifact != null, "There is no EntityDesignArtifact with URI:" + uri.LocalPath + " in modelmanager.");

                    if (artifact != null
                        && artifact.DiagramArtifact == null)
                    {
                        cmd.Enabled = cmd.Visible = true;
                    }
                }
            }
        }

        internal void OnMenuMoveDiagramsToSeparateFile(object sender, EventArgs e)
        {
            var result = VsUtils.ShowMessageBox(
                Services.ServiceProvider, Resources.MoveDiagramNodesWarning
                , OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_WARNING);

            if (result == DialogResult.Yes)
            {
                var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                Debug.Assert(editingContext != null, "EditingContext for artifact with uri: " + uri + " is not available.");
                if (editingContext != null)
                {
                    var efArtifactService = editingContext.GetEFArtifactService();
                    var entityDesignArtifact = efArtifactService.Artifact as EntityDesignArtifact;

                    // Don't need to put the transaction name in resource string table since we clear the VS undo stack after the command is executed.
                    var cpc = new CommandProcessorContext(
                        editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, "MoveDiagrams");
                    MigrateDiagramInformationCommand.DoMigrate(cpc, entityDesignArtifact);
                    // Save the EDMX file.
                    var rdt = new RunningDocumentTable(Services.ServiceProvider);
                    rdt.SaveFileIfDirty(CurrentDocData.FileName);
                }
            }
        }

        #endregion

        #region Entity-type property reordering

        /// <summary>
        ///     Determine whether to show Property Move menu items or not
        ///     We only show menu items if:
        ///     - All the selected properties belong to a single entity.
        ///     AND
        ///     - All the selected properties are either Property (includes: ScalarProperty and ComplexProperty) or NavigationProperty.
        /// </summary>
        internal void OnStatusPropertyMove(object sender, EventArgs e)
        {
            var cmd = sender as MenuCommand;
            if (null != cmd)
            {
                cmd.Enabled = cmd.Visible = false;
                var diagram = GetDiagram();
                if (diagram != null)
                {
                    var properties = CurrentSelection.OfType<PropertyBase>().ToList();
                    // Check if all the selected items are properties.
                    if (CurrentSelection.Count == properties.Count
                        && properties.Count > 0)
                    {
                        var propertyTypes = new HashSet<Type>();
                        var entityTypes = new HashSet<EntityType>();
                        foreach (var property in properties)
                        {
                            var navProp = property as NavigationProperty;
                            var prop = property as Property;
                            if (navProp != null)
                            {
                                propertyTypes.Add(typeof(NavigationProperty));
                                entityTypes.Add(navProp.EntityType);
                            }
                            if (prop != null)
                            {
                                propertyTypes.Add(typeof(Property));
                                entityTypes.Add(prop.EntityType);
                            }
                        }
                        cmd.Enabled = cmd.Visible = (propertyTypes.Count == 1 && entityTypes.Count == 1);
                    }
                }
            }
        }

        internal void OnMenuPropertyMoveUp(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Up, 1);
        }

        internal void OnMenuPropertyMoveDown(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Down, 1);
        }

        internal void OnMenuPropertyMovePageUp(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Up, 5);
        }

        internal void OnMenuPropertyMovePageDown(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Down, 5);
        }

        internal void OnMenuPropertyMoveTop(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Up, Int32.MaxValue);
        }

        internal void OnMenuPropertyMoveBottom(object sender, EventArgs e)
        {
            DoSelectedPropertiesMove(MoveDirection.Down, Int32.MaxValue);
        }

        /// <summary>
        ///     The method will do the following:
        ///     - Loop through the selected DSL property and get the model property from ModelXref and add it to the list.
        ///     - Call command to move properties and passed in the list that is created from previous step.
        ///     - Ensure that the properties that are moved are still selected after the move.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void DoSelectedPropertiesMove(MoveDirection moveDirection, uint moveStep)
        {
            Debug.Assert(
                SelectedProperty != null || SelectedNavigationProperty != null,
                "There must be at least 1 selected property or navigation property.");

            if (SelectedProperty != null
                || SelectedNavigationProperty != null)
            {
                // Check whether navigation property or conceptual property are moved.
                // We don't support moving conceptual property and navigation property or moving properties that are belong to multiple entity type.
                var areNavigationPropertiesSelected = (SelectedProperty == null);

                var entityType = areNavigationPropertiesSelected ? SelectedNavigationProperty.EntityType : SelectedProperty.EntityType;
                Debug.Assert(entityType != null, "Unable to determine the entity-type of the selected properties.");

                if (entityType != null)
                {
                    var entityTypeShape = PresentationViewsSubject.GetPresentation(entityType).FirstOrDefault() as EntityTypeShape;
                    Debug.Assert(entityTypeShape != null, "Could not find the shape for entity-type: " + entityType.Name);

                    if (entityTypeShape != null)
                    {
                        var compartment = areNavigationPropertiesSelected
                                              ? entityTypeShape.NavigationCompartment
                                              : entityTypeShape.PropertiesCompartment;
                        var entityDesignerViewModel = entityType.EntityDesignerViewModel;
                        var modelProperties = new List<ModelEntity.PropertyBase>();

                        foreach (var property in CurrentSelection.OfType<PropertyBase>())
                        {
                            var modelProperty = entityDesignerViewModel.ModelXRef.GetExisting(property) as ModelEntity.PropertyBase;
                            Debug.Assert(
                                modelProperty != null, "Unable to get the model property for property : " + property.Name + "  from XRef.");
                            if (modelProperty != null)
                            {
                                modelProperties.Add(modelProperty);
                            }
                        }

                        if (modelProperties.Count > 0)
                        {
                            var uri = Utils.FileName2Uri(CurrentDocData.FileName);
                            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                            var cpc = new CommandProcessorContext(
                                editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, Resources.Tx_MoveProperty);
                            CommandProcessor.InvokeSingleCommand(cpc, new MovePropertiesCommand(modelProperties, moveDirection, moveStep));

                            // Restore the selections
                            SelectProperties(modelProperties, compartment);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Given the list of model Property, select the corresponding DSL Properties' shapes in the diagram.
        ///     The list contains:
        ///     - Properties that are belong to a single entity.
        ///     - Either conceptual properties or navigation properties. It cannot contain both.
        /// </summary>
        /// <param name="modelProperties"></param>
        /// <param name="compartment"></param>
        private void SelectProperties(List<ModelEntity.PropertyBase> modelProperties, ElementListCompartment compartment)
        {
            if (modelProperties.Count > 0)
            {
                // Check to see all the properties belong to a single entityType.
                var arePropertiesBelongToASingleEntityType = (modelProperties.Select(p => p.EntityType).Distinct().Count() == 1);
                Debug.Assert(arePropertiesBelongToASingleEntityType, "The properties are not belong to a single entity-type");

                if (arePropertiesBelongToASingleEntityType)
                {
                    var diagram = GetDiagram();
                    Debug.Assert(diagram != null, "Unable to get DSL diagram instance.");

                    if (diagram != null)
                    {
                        var entityDesignerViewModel = diagram.ModelElement;

                        var selections = new DiagramItemCollection();
                        // Loop through model property, find the DSL property from model xref.
                        // Given the DSL property find the index of the property in the ElementListCompartment.
                        foreach (var property in modelProperties)
                        {
                            var vmProperty = entityDesignerViewModel.ModelXRef.GetExisting(property) as PropertyBase;
                            Debug.Assert(vmProperty != null, "Unable to find DSL model property for property:" + property.DisplayName);

                            if (vmProperty != null)
                            {
                                var index = compartment.Items.IndexOf(vmProperty);
                                Debug.Assert(
                                    index >= 0, "Unable find the index of DSL property :" + vmProperty.Name + " in compartment list.");
                                if (index >= 0)
                                {
                                    selections.Add(new DiagramItem(compartment, compartment.ListField, new ListItemSubField(index)));
                                }
                            }
                        }

                        if (diagram != null
                            && diagram.ActiveDiagramView != null
                            && selections.Count > 0)
                        {
                            diagram.ActiveDiagramView.Selection.Set(selections);
                        }
                    }
                }
            }
        }

        #endregion

        internal void OnStatusAddEnumType(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
            if (cmd != null)
            {
                cmd.Enabled = cmd.Visible = false;

                if (EdmFeatureManager.GetEnumTypeFeatureState(CurrentEntityDesignArtifact).IsEnabled())
                {
                    if (MonitorSelection != null
                        && MonitorSelection.CurrentWindow is MicrosoftDataEntityDesignDocView
                        && GetDiagram() != null)
                    {
                        if (IsOurDiagramSelected())
                        {
                            cmd.Enabled = cmd.Visible = true;
                            cmd.Text = Resources.AddEnumTypeCommand_DesignerText;
                        }
                    }
                    else
                    {
                        var explorerSelection = SelectedExplorerItem;
                        if (explorerSelection != null
                            && (explorerSelection is ExplorerEnumTypes || explorerSelection is ExplorerEnumType))
                        {
                            cmd.Enabled = cmd.Visible = true;
                            cmd.Text = Resources.AddEnumTypeCommand_ExplorerText;
                        }
                    }
                }
            }
        }

        internal void OnMenuAddEnumType(object sender, EventArgs e)
        {
            var editingContext = CurrentEntityDesignArtifact.EditingContext;
            Debug.Assert(editingContext != null, "editingContext is null.");

            if (editingContext != null)
            {
                var enumType = EntityDesignViewModelHelper.AddNewEnumType(
                    null, editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId);
                if (enumType != null)
                {
                    ExplorerNavigationHelper.NavigateTo(enumType);
                }
            }
        }

        internal void OnStatusConvertToEnum(object sender, EventArgs e)
        {
            var cmd = sender as DynamicStatusMenuCommand;
            Debug.Assert(cmd != null, "could not cast sender as a DynamicStatusMenuCommand");
            if (cmd != null)
            {
                var selectedProperty = SelectedScalarProperty;
                // Command should only be enabled:
                // - Enum Type feature is supported in the current schema.
                // - The selected item is scalar property.
                // - There is only 1 selected item in designer.
                // - The property type is valid for enum's underlying type.
                var isEnabled = false;
                if (EdmFeatureManager.GetEnumTypeFeatureState(CurrentEntityDesignArtifact).IsEnabled()
                    && CurrentSelection.Count == 1
                    && selectedProperty != null
                    && ModelHelper.UnderlyingEnumTypes.Count(t => String.CompareOrdinal(t.Name, selectedProperty.Type) == 0) > 0)
                {
                    isEnabled = true;
                }

                cmd.Visible = cmd.Enabled = isEnabled;
            }
        }

        internal void OnMenuConvertToEnum(object sender, EventArgs e)
        {
            var editingContext = CurrentEntityDesignArtifact.EditingContext;
            Debug.Assert(editingContext != null, "editingContext is null.");

            if (editingContext != null)
            {
                var selectedProperty = SelectedScalarProperty;
                if (selectedProperty != null)
                {
                    var newlyCreatedEnumType = EntityDesignViewModelHelper.AddNewEnumType(
                        selectedProperty.Type, editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId);
                    if (newlyCreatedEnumType != null)
                    {
                        var rootViewModel = selectedProperty.GetRootViewModel();
                        Debug.Assert(rootViewModel != null, "Unable to find instance of EntityDesignerViewModel.");

                        if (rootViewModel != null)
                        {
                            var modelProperty = rootViewModel.ModelXRef.GetExisting(selectedProperty) as ModelEntity.ConceptualProperty;

                            Debug.Assert(modelProperty != null, "Unable to find model ConceptualProperty.");

                            if (modelProperty != null)
                            {
                                var cpc = new CommandProcessorContext(
                                    editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId
                                    , Design.Resources.Tx_UpdatePropertyType);
                                CommandProcessor.InvokeSingleCommand(
                                    cpc, new ChangePropertyTypeCommand(modelProperty, newlyCreatedEnumType.NormalizedNameExternal));
                            }
                        }
                    }
                }
            }
        }

        internal class AddNewItemDialogFilter : IVsFilterAddProjectItemDlg2
        {
            public int FilterListItemByCategory(ref Guid rguidProjectItemTemplates, string pszCategoryName, out int pfFilter)
            {
                pfFilter = 0;
                return VSConstants.S_OK;
            }

            public int FilterListItemByLocalizedName(ref Guid rguidProjectItemTemplates, string pszLocalizedName, out int pfFilter)
            {
                pfFilter = 0;
                return VSConstants.S_OK;
            }

            public int FilterListItemByTemplateFile(ref Guid rguidProjectItemTemplates, string pszTemplateFile, out int pfFilter)
            {
                pfFilter = 1; // exclude
                if (pszTemplateFile != null)
                {
                    var fi = new FileInfo(pszTemplateFile);
                    if (fi.Name.StartsWith("ADONETArtifactGenerator_", StringComparison.OrdinalIgnoreCase)
                        || fi.Name.StartsWith("DbContext_", StringComparison.OrdinalIgnoreCase))
                    {
                        pfFilter = 0; // include
                    }
                }
                return VSConstants.S_OK;
            }

            public int FilterTreeItemByCategory(ref Guid rguidProjectItemTemplates, string pszCategoryName, out int pfFilter)
            {
                pfFilter = 0;
                return VSConstants.S_OK;
            }

            public int FilterTreeItemByLocalizedName(ref Guid rguidProjectItemTemplates, string pszLocalizedName, out int pfFilter)
            {
                pfFilter = 0;
                return VSConstants.S_OK;
            }

            public int FilterTreeItemByTemplateDir(ref Guid rguidProjectItemTemplates, string pszTemplateDir, out int pfFilter)
            {
                pfFilter = 0;
                return VSConstants.S_OK;
            }
        }
    }
}
