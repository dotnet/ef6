// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if VS12
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
#endif

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Commands;
    using Microsoft.Data.Entity.Design.UI.Util;
    using Microsoft.Data.Entity.Design.UI.ViewModels;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Key = System.Windows.Input.Key;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EntityDesignExplorerFrame : ExplorerFrame
    {
        private readonly DeferredRequest _putSelectedExplorerItemInRenameModeRequest;

        private delegate void ExecuteCmdHandler();

        private delegate bool CanExecuteCmdHandler();

        public EntityDesignExplorerFrame(EditingContext context)
            : base(context)
        {
            DefineCmd(WorkspaceCommands.Activate, ExecuteActivate, CanExecuteActivate);
            DefineCmd(WorkspaceCommands.PutInRenameMode, ExecutePutInRenameMode, CanExecutePutInRenameMode);
            _putSelectedExplorerItemInRenameModeRequest = new DeferredRequest(PutSelectedItemInRenameMode);
            Loaded += ExplorerFrameLoaded;

#if VS12
    // set bitmap scaling mode to most appropriate value based on text scaling
            RenderOptions.SetBitmapScalingMode(this, DpiHelper.BitmapScalingMode);
#endif
        }

        private void ExplorerFrameLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ExplorerFrameLoaded;

            if (ScrollViewer != null)
            {
                ScrollViewer.Style = FindResource(VsResourceKeys.ScrollViewerStyleKey) as Style;
            }
        }

        public bool CanExecuteActivate()
        {
            var result = false;
            var selectedExplorerEFElement = GetSelectedExplorerEFElement();
            if (selectedExplorerEFElement != null)
            {
                result = (selectedExplorerEFElement is ExplorerFunctionImport
                          || selectedExplorerEFElement is ExplorerFunction
                          || selectedExplorerEFElement is ExplorerDiagram
                          || selectedExplorerEFElement is ExplorerConceptualEntityType
                          || selectedExplorerEFElement is ExplorerConceptualAssociation
                          || (selectedExplorerEFElement is ExplorerConceptualProperty
                              && selectedExplorerEFElement.Parent is ExplorerConceptualEntityType)
                          || selectedExplorerEFElement is ExplorerNavigationProperty
                          || selectedExplorerEFElement is ExplorerAssociationSet
                          || selectedExplorerEFElement is ExplorerEntitySet
                          || selectedExplorerEFElement is ExplorerEntityTypeShape
                          || selectedExplorerEFElement is ExplorerEnumType);
            }

            return result;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void ExecuteActivate()
        {
            var viewModelHelper = ExplorerViewModelHelper as EntityDesignExplorerViewModelHelper;
            Debug.Assert(viewModelHelper != null, "ExplorerViewModelHelper is null or is not type of EntityDesignExplorerViewModelHelper");

            // Function-Import/Function  double-click behavior.
            var selectedExplorerEFElement = GetSelectedExplorerEFElement();
            var explorerFunctionImport = selectedExplorerEFElement as ExplorerFunctionImport;
            var explorerFunction = selectedExplorerEFElement as ExplorerFunction;
            var explorerDiagram = selectedExplorerEFElement as ExplorerDiagram;
            var explorerConceptualProperty = selectedExplorerEFElement as ExplorerConceptualProperty;
            var explorerEnumType = selectedExplorerEFElement as ExplorerEnumType;

            var diagramManagercontextItem = EditingContext.Items.GetValue<DiagramManagerContextItem>();
            Debug.Assert(diagramManagercontextItem != null, "Could not find instance of: DiagramManagerContextItem in editing context.");

            if (viewModelHelper != null
                && explorerFunctionImport != null)
            {
                viewModelHelper.EditFunctionImport(explorerFunctionImport.ModelItem as FunctionImport);
            }
                // if the user double-clicks on the function in the model browser.
            else if (viewModelHelper != null
                     && explorerFunction != null)
            {
                var function = explorerFunction.ModelItem as Function;
                Debug.Assert(function != null, "ExplorerFunction.ModelItem value is expected to be typeof Function and not null.");

                if (function != null)
                {
                    var schemaVersion = (function.Artifact == null ? null : function.Artifact.SchemaVersion);
                    if (function.IsComposable != null
                        && function.IsComposable.Value
                        && !EdmFeatureManager.GetComposableFunctionImportFeatureState(schemaVersion).IsEnabled())
                    {
                        // Composable Function Import for Version <= V2 - give warning message
                        VsUtils.ShowMessageBox(
                            PackageManager.Package,
                            string.Format(
                                CultureInfo.CurrentCulture, Design.Resources.FunctionImport_CannotCreateFromComposableFunction,
                                EntityFrameworkVersion.Version2),
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                            OLEMSGICON.OLEMSGICON_WARNING);
                    }
                    else
                    {
                        // first we detect whether there are existing FunctionImport(s) with the underlying function.
                        var itemBindings = function.GetDependentBindings();
                        var matchFunctionImportList = new List<FunctionImport>();
                        foreach (var itemBinding in itemBindings)
                        {
                            var functionImportMapping = itemBinding.GetParentOfType(typeof(FunctionImportMapping)) as FunctionImportMapping;
                            if (functionImportMapping != null
                                && functionImportMapping.FunctionImportName != null
                                && functionImportMapping.FunctionImportName.Target != null)
                            {
                                matchFunctionImportList.Add(functionImportMapping.FunctionImportName.Target);
                            }
                        }
                        // if we found function imports for the underlying function, navigate to the first function import in alphabetical sorted list
                        if (matchFunctionImportList.Count > 0)
                        {
                            matchFunctionImportList.Sort(EFElement.EFElementDisplayNameComparison);
                            ExplorerNavigationHelper.NavigateTo(matchFunctionImportList[0]);
                        }
                        else
                        {
                            // We could not find any function import for the underlying function, show new function import dialog
                            viewModelHelper.CreateFunctionImport(explorerFunction.ModelItem as Function);
                        }
                    }
                }
            }
            else if (explorerEnumType != null)
            {
                var enumType = explorerEnumType.ModelItem as EnumType;
                Debug.Assert(enumType != null, "ExplorerEnumType's model item is null or is not type of EnumType.");
                EntityDesignViewModelHelper.EditEnumType(
                    Context, EfiTransactionOriginator.ExplorerWindowOriginatorId, new EnumTypeViewModel(enumType));
            }
                // if the selected Explorer is type of ExplorerDiagram.
            else if (explorerDiagram != null
                     && diagramManagercontextItem != null)
            {
                diagramManagercontextItem.DiagramManager.OpenDiagram(explorerDiagram.DiagramMoniker, true);
            }
            else if (explorerConceptualProperty != null)
            {
                Debug.Assert(
                    explorerConceptualProperty.Parent is ExplorerConceptualEntityType,
                    "Only properties that belong to Entity type are supported.");
                if (explorerConceptualProperty.Parent is ExplorerConceptualEntityType)
                {
                    diagramManagercontextItem.DiagramManager.ActiveDiagram.AddOrShowEFElementInDiagram(explorerConceptualProperty.ModelItem);
                }
            }
                // if the selected Explorer is type of ExplorerConceptualEntityType, ExplorerConceptualAssociation, ExplorerNavigationProperty, ExplorerAssociationSet or ExplorerEntitySet
            else if (diagramManagercontextItem != null
                     && diagramManagercontextItem.DiagramManager != null
                     && diagramManagercontextItem.DiagramManager.ActiveDiagram != null)
            {
                Debug.Assert(
                    selectedExplorerEFElement is ExplorerConceptualEntityType
                    || selectedExplorerEFElement is ExplorerConceptualAssociation
                    || selectedExplorerEFElement is ExplorerNavigationProperty
                    || selectedExplorerEFElement is ExplorerAssociationSet
                    || selectedExplorerEFElement is ExplorerEntitySet
                    || selectedExplorerEFElement is ExplorerEntityTypeShape,
                    "The selected explorer type:" + selectedExplorerEFElement.GetType().Name + " is not supported.");

                if (selectedExplorerEFElement is ExplorerConceptualEntityType
                    || selectedExplorerEFElement is ExplorerConceptualAssociation
                    || selectedExplorerEFElement is ExplorerNavigationProperty
                    || selectedExplorerEFElement is ExplorerAssociationSet
                    || selectedExplorerEFElement is ExplorerEntitySet)
                {
                    diagramManagercontextItem.DiagramManager.ActiveDiagram.AddOrShowEFElementInDiagram(selectedExplorerEFElement.ModelItem);
                }
                else if (selectedExplorerEFElement is ExplorerEntityTypeShape)
                {
                    var entityTypeShape = selectedExplorerEFElement.ModelItem as EntityTypeShape;
                    Debug.Assert(entityTypeShape != null, "ExplorerEntityTypeShape does not contain instance of EntityTypeShape");
                    if (entityTypeShape != null)
                    {
                        diagramManagercontextItem.DiagramManager.OpenDiagram(entityTypeShape.Diagram.Id, true);
                        diagramManagercontextItem.DiagramManager.ActiveDiagram.AddOrShowEFElementInDiagram(
                            entityTypeShape.EntityType.Target);
                    }
                }
            }
        }

        public bool CanExecutePutInRenameMode()
        {
            var selectedExplorerEFElement = GetSelectedExplorerEFElement();
            if (selectedExplorerEFElement != null)
            {
                // Diagram nodes, ComplexType nodes and children of ComplexType nodes are the only ones with rename mode available
                if (selectedExplorerEFElement is ExplorerComplexType
                    || (selectedExplorerEFElement.Parent != null && selectedExplorerEFElement.Parent is ExplorerComplexType)
                    || selectedExplorerEFElement is ExplorerDiagram
                    || selectedExplorerEFElement is ExplorerEnumType
                    || selectedExplorerEFElement is ExplorerEntityTypeShape
                    || selectedExplorerEFElement is ExplorerConceptualEntityType)
                {
                    // compare previous selection to current selection. On first click on a given node these 
                    // selections will be different and so we will not trigger rename mode. But then we update 
                    // the previous selection so that the second time through this code (i.e. the second click)
                    // we will return true which will trigger rename mode.
                    var previousSelection = GetPreviousSelectedExplorerEFElement();
                    var result = (previousSelection == selectedExplorerEFElement);
                    if (!result)
                    {
                        _previousSelectedExplorerEFElement = selectedExplorerEFElement;
                    }
                    return result;
                }
            }

            return false;
        }

        public void ExecutePutInRenameMode()
        {
            var selectedExplorerEFElement = GetSelectedExplorerEFElement();
            if (selectedExplorerEFElement != null)
            {
                var treeViewItem = GetTreeViewItem(selectedExplorerEFElement, false);
                if (treeViewItem != null)
                {
                    var editableContentControl =
                        ExplorerUtility.GetTypeDescendents(treeViewItem, typeof(EditableContentControl)).FirstOrDefault() as
                        EditableContentControl;
                    if (editableContentControl != null)
                    {
                        // put the EditableContentControl into edit mode so user can change the name
                        editableContentControl.IsInEditMode = true;
                    }
                }
            }
        }

        protected override ExplorerViewModelHelper GetNewExplorerViewModelHelper()
        {
            return new EntityDesignExplorerViewModelHelper();
        }

        protected override ExplorerContent InitializeExplorerContent()
        {
#if VS12
        
            var content = FileResourceManager.GetElement("Resources/ExplorerContent_12.0.xaml") as ExplorerContent;
#else

            var content = FileResourceManager.GetElement("Resources/ExplorerContent_11.0.xaml") as ExplorerContent;
#endif

            //
            // DO NOT use a static resourceDictionary below.  For some reason, calling MergedDictionaries.Add()
            // will associate a pointer from the static resourceDictionary to _frameContent.  I wasn't able to make this reference
            // go away, even by calling MergedDictionaries.Remove().  The end result was the _frameContent would always stay in memory, 
            // and this would in turn keep the entire artifact and model in memory. 
            //
            var resourceDictionary = FileResourceManager.GetResourceDictionary("Resources/Styles.xaml");
            content.Resources.MergedDictionaries.Add(resourceDictionary);

            return content;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                // do not respond to F1 Help
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Insert)
            {
                var selected = GetSelectedExplorerEFElement();
                var context = new EfiTransactionContext();
                if (selected is ExplorerComplexTypes)
                {
                    var cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.ExplorerWindowOriginatorId,
                        Design.Resources.Tx_AddComplexType, null, context);
                    var complexType = CreateComplexTypeCommand.CreateComplexTypeWithDefaultName(cpc);
                    Debug.Assert(complexType != null, "Creating ComplexType failed");
                    NavigateToElementAndPutInRenameMode(complexType);
                    return;
                }

                var explorerComplexType = selected as ExplorerComplexType;
                if (explorerComplexType != null)
                {
                    var complexType = explorerComplexType.ModelItem as ComplexType;
                    if (complexType != null)
                    {
                        var cpc = new CommandProcessorContext(
                            Context,
                            EfiTransactionOriginator.ExplorerWindowOriginatorId,
                            Design.Resources.Tx_CreateScalarProperty, null, context);
                        var property = CreateComplexTypePropertyCommand.CreateDefaultProperty(
                            cpc, complexType,
                            ModelConstants.DefaultPropertyType);
                        Debug.Assert(property != null, "Creating Property failed");
                        NavigateToElementAndPutInRenameMode(property);
                    }
                    else
                    {
                        Debug.Fail("complexType shouldn't be null");
                    }
                    return;
                }

                // Event handler if the user presses 'Insert' button from the keyboard in the EnumTypes node.
                var explorerEnumTypes = selected as ExplorerEnumTypes;
                if (explorerEnumTypes != null)
                {
                    var entityModel = explorerEnumTypes.Parent.ModelItem as ConceptualEntityModel;
                    if (EdmFeatureManager.GetEnumTypeFeatureState(entityModel.Artifact).IsEnabled())
                    {
                        var cpc = new CommandProcessorContext(
                            Context,
                            EfiTransactionOriginator.ExplorerWindowOriginatorId,
                            Design.Resources.Tx_CreateScalarProperty, null, context);
                        var enumType = CreateEnumTypeCommand.CreateEnumTypeWithDefaultName(cpc);
                        Debug.Assert(enumType != null, "Creating Enum failed");
                        NavigateToElementAndPutInRenameMode(enumType);
                    }
                    return;
                }

                // Event handler if the user presses 'Insert' button from the keyboard in the diagrams node.
                var explorerDiagram = selected as ExplorerDiagrams;
                if (explorerDiagram != null)
                {
                    var diagrams = explorerDiagram.ModelItem as Diagrams;
                    if (diagrams != null)
                    {
                        var cpc = new CommandProcessorContext(
                            Context,
                            EfiTransactionOriginator.ExplorerWindowOriginatorId,
                            Design.Resources.Tx_CreateDiagram, null, context);
                        var diagram = CreateDiagramCommand.CreateDiagramWithDefaultName(cpc);
                        Debug.Assert(diagram != null, "The selected ExplorerEFElementItem is not type of diagram.");
                        NavigateToElementAndPutInRenameMode(diagram);
                        // Automatically open the diagram that we selected in the previous line.
                        var selectedExplorerEFElement = GetSelectedExplorerEFElement();
                        if (selectedExplorerEFElement is ExplorerDiagram)
                        {
                            ExecuteActivate();
                        }
                        else
                        {
                            Debug.Fail("The selected ExplorerEFElementItem is not type of diagram.");
                        }
                    }
                    else
                    {
                        Debug.Fail("diagram folder shouldn't be null");
                    }
                    return;
                }
            }

            base.OnKeyDown(e);
        }

        internal void NavigateToElementAndPutInRenameMode(EFElement element)
        {
            Debug.Assert(element != null, "element must not be null");
            if (element != null)
            {
                // first select the element
                ExplorerNavigationHelper.NavigateTo(element);

                // Invoke deferred request to put the selected item in rename mode.
                // This is necessary because the selected explorer item might not have been fully initialized yet.
                _putSelectedExplorerItemInRenameModeRequest.Request();
            }
        }

        // <summary>
        //     Adapter method to match DeferredRequest callback signature.
        //     The parameter is created to match callback signature and will be ignored.
        // </summary>
        private void PutSelectedItemInRenameMode(object o)
        {
            var explorerElement = GetSelectedExplorerEFElement();
            Debug.Assert(explorerElement != null, "selected ExplorerElement should not be null");
            if (explorerElement != null)
            {
                PutElementInRenameMode(explorerElement);
            }
        }

        internal void PutElementInRenameMode(ExplorerEFElement explorerElement)
        {
            Debug.Assert(explorerElement != null, "explorerElement must not be null");
            if (explorerElement != null)
            {
                // if ExplorerElement.IsEditableInline is false then there will be no EditableContentControl to find
                Debug.Assert(
                    explorerElement.IsEditableInline,
                    "explorerElement.IsEditableInline should be true but was false. ExplorerElement has name " + explorerElement.Name
                    + " and type " + explorerElement.GetType().FullName);
                if (explorerElement.IsEditableInline)
                {
                    var treeViewItem = GetTreeViewItem(explorerElement, false);
                    Debug.Assert(treeViewItem != null, "Could not find TreeViewItem for Explorer element named " + explorerElement.Name);
                    if (treeViewItem != null)
                    {
                        // Note: if the ItemContainerGenerator status is NotStarted, the UI Element is not created yet.
                        // This causes ExplorerUtility.GetTypeDescendents() call to return null.
                        if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.NotStarted)
                        {
                            treeViewItem.UpdateLayout();
                        }

                        var editableContentControl =
                            ExplorerUtility.GetTypeDescendents(treeViewItem, typeof(EditableContentControl)).FirstOrDefault() as
                            EditableContentControl;
                        Debug.Assert(
                            null != editableContentControl,
                            "Could not find EditableContentControl for Explorer element named " + explorerElement.Name);
                        if (null != editableContentControl)
                        {
                            // put the EditableContentControl into edit mode so user can change the name
                            editableContentControl.IsInEditMode = true;
                        }
                    }
                }
            }
        }

        private void DefineCmd(RoutedCommand cmdId, ExecuteCmdHandler executeCmd, CanExecuteCmdHandler canExecuteCmd)
        {
            var cmd = new CommandBinding(
                cmdId,
                delegate(object sender, ExecutedRoutedEventArgs e)
                    {
                        executeCmd();
                        e.Handled = true;
                    },
                delegate(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = canExecuteCmd(); });
            CommandBindings.Add(cmd);
        }

        // <summary>
        //     Entity Designer drag and drop support.
        //     Given an ExplorerEFElement constructs clipboard data-object containing collection of entity-types.
        //     Type:
        //     ExplorerEntitySet   -> collection of all entity-types in the entity set.
        //     AssociationSet      -> Participating entity-type in the associationset's association.
        // </summary>
        protected override DataObject GetClipboardObjectForExplorerItem(ExplorerEFElement explorerEFElement)
        {
            DataObject dataObject = null;

            Debug.Assert(explorerEFElement != null && explorerEFElement.ModelItem != null, "explorerEFElement parameter is null");
            if (explorerEFElement != null
                && explorerEFElement.ModelItem != null)
            {
                var entityTypes = new List<EntityType>();
                var associations = new List<Association>();

                // Check if the type is a storage type, we don't support drag and drop support for any storage type
                if (explorerEFElement.ModelItem.GetParentOfType(typeof(StorageEntityModel)) == null)
                {
                    var entityType = explorerEFElement.ModelItem as ConceptualEntityType;
                    var association = explorerEFElement.ModelItem as Association;
                    var associationSet = explorerEFElement.ModelItem as AssociationSet;
                    var entitySet = explorerEFElement.ModelItem as EntitySet;

                    // When the user is trying to drag an associationset, we want to treat as if the user drag the association in the set.
                    if (associationSet != null
                        && associationSet.Association.Status == BindingStatus.Known)
                    {
                        association = associationSet.Association.Target;
                    }

                    if (entityType != null)
                    {
                        entityTypes.Add(entityType);
                    }
                    else if (association != null)
                    {
                        foreach (var associationEnd in association.AssociationEnds())
                        {
                            if (associationEnd.Type.Status == BindingStatus.Known)
                            {
                                entityTypes.Add(associationEnd.Type.Target);
                            }
                        }
                        associations.Add(association);
                    }
                    else if (entitySet != null)
                    {
                        entityTypes.AddRange(entitySet.GetEntityTypesInTheSet());
                    }
                }

                // if entitytypes and association collections are both empty, that means drag and drop support are not supported for the ExplorerEFElement type.
                // Return an instance of dataobject that contains an empty object. This is so that we can display the appropriate "stop" icon in the diagram.
                if (entityTypes.Count == 0
                    && associations.Count == 0)
                {
                    dataObject = new DataObject(typeof(object).Name, new object());
                }
                else
                {
                    dataObject = new DataObject(
                        typeof(EntitiesClipboardFormat).Name,
                        new EntitiesClipboardFormat(entityTypes, associations, new Dictionary<EntityType, EntityType>()));
                }
            }
            return dataObject;
        }
    }
}
