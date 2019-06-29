// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VsErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VsShell = Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.Views.EntityDesigner;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    // <summary>
    //     Mapping details window
    // </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [Guid("CDBDEE54-B399-484b-B763-DB2C3393D646")]
    internal class MappingDetailsWindow : TreeGridDesignerToolWindow
    {
        private EditingContext _editingContext;
        private MappingDetailsInfo _currentMappingDetailsInfo;
        private TreeGridDesignerColumnDescriptor[] _defaultColumns;
        private TreeGridDesignerWatermarkInfo _watermarkInfo;
        private TreeGridDesignerWatermarkInfo _defaultWatermarkInfo;
        private EFObject _lastPrimarySelection;

        // <summary>
        //     Construct the Mapping Details window.
        // </summary>
        public MappingDetailsWindow(IServiceProvider sp)
            : base(sp)
        {
            _defaultColumns = new TreeGridDesignerColumnDescriptor[3];
            _defaultColumns[0] = new ColumnNameColumn();
            _defaultColumns[1] = new OperatorColumn();
            _defaultColumns[2] = new ValueColumn();

            // initialize the watermark info.
            SetUpDefaultWatermarkInfo();
        }

        internal MappingDetailsInfo CurrentMappingDetailsInfo
        {
            get { return _currentMappingDetailsInfo; }
        }

        // <summary>
        //     Initialize the Mapping Details window.
        // </summary>
        protected override void OnToolWindowCreate()
        {
            base.OnToolWindowCreate();

            // initialize watermark info again after the tool window as been created.
            SetUpDefaultWatermarkInfo();

            var vsFont = VSHelpers.GetVSFont(this);
            if (vsFont != null)
            {
                TreeControl.Font = vsFont;
            }
        }

        private void SetUpDefaultWatermarkInfo()
        {
            // set up default watermark info
            var watermarkText = String.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_Watermark, Resources.ExplorerWindowTitle);
            // container will only be null when we initialize this through the constructor.
            if (MappingDetailsWindowContainer != null)
            {
                var linkStart = watermarkText.IndexOf(Resources.ExplorerWindowTitle, StringComparison.Ordinal);
                var linkLength = Resources.ExplorerWindowTitle.Length;
                _defaultWatermarkInfo = new TreeGridDesignerWatermarkInfo(
                    watermarkText,
                    new TreeGridDesignerWatermarkInfo.LinkData(
                        linkStart, linkLength, MappingDetailsWindowContainer.watermarkLabel_LinkClickedShowExplorer));
            }
            else
            {
                _defaultWatermarkInfo = new TreeGridDesignerWatermarkInfo(watermarkText);
            }
            _watermarkInfo = _defaultWatermarkInfo;
        }

        // <summary>
        //     Override the base version and provide our own customized container (this adds the toolbar on the left)
        // </summary>
        protected override ITreeGridDesignerToolWindowContainer CreateContainer()
        {
            return new MappingDetailsWindowContainer(this, TreeControl);
        }

        // <summary>
        //     Get or Set the EditingContext into this object
        // </summary>
        public override EditingContext Context
        {
            get { return _editingContext; }
            set
            {
                // this is not the same one we have
                if (_editingContext != value)
                {
                    // if we have one, remove its event handlers
                    if (_editingContext != null)
                    {
                        _editingContext.Disposing -= OnEditingContextDisposing;
                        _editingContext.Reloaded -= OnEditingContextReloaded;
                        _editingContext.Items.Unsubscribe<EntityDesignerSelection>(OnEntityDesignerSelectionChanged);
                        _editingContext.Items.Unsubscribe<ExplorerSelection>(OnExplorerSelectionChanged);
                        PackageManager.Package.ModelManager.ModelChangesCommitted -= OnModelChangesCommitted;
                    }

                    _editingContext = value;
                    ContainerControl.HostContext = _editingContext;

                    // add our handlers
                    if (_editingContext != null)
                    {
                        _editingContext.Disposing += OnEditingContextDisposing;
                        _editingContext.Reloaded += OnEditingContextReloaded;
                        _editingContext.Items.Subscribe<EntityDesignerSelection>(OnEntityDesignerSelectionChanged);
                        _editingContext.Items.Subscribe<ExplorerSelection>(OnExplorerSelectionChanged);
                        PackageManager.Package.ModelManager.ModelChangesCommitted += OnModelChangesCommitted;
                    }

                    if (_editingContext == null)
                    {
                        // we need to clear out the last primary selection
                        // if we are clearing out the context.
                        _lastPrimarySelection = null;
                    }

                    InitializeView();
                }
            }
        }

        // <summary>
        //     Easy way to access the type-safe reference to our container
        // </summary>
        private MappingDetailsWindowContainer MappingDetailsWindowContainer
        {
            get { return ContainerControl as MappingDetailsWindowContainer; }
        }

        // <summary>
        //     If the EditingContext is going away, use our local setter
        // </summary>
        private void OnEditingContextDisposing(object sender, EventArgs e)
        {
            Debug.Assert(Context == sender, "incorrect context");
            Context = null;
        }

        // <summary>
        //     If the context is reloaded, just clear the screen
        // </summary>
        private void OnEditingContextReloaded(object sender, EventArgs e)
        {
            ClearToolWindowContents(true);
        }

        // <summary>
        //     This is our handler to watch for model changes
        // </summary>
        private void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        {
            if (e.ChangeGroup.Transaction.OriginatorId != EfiTransactionOriginator.MappingDetailsOriginatorId)
            {
                if (Context != null)
                {
                    var selection = Context.Items.GetValue<MappingDetailsSelection>();
                    RefreshCurrentSelection();
                    if (e.ChangeGroup.Transaction.OriginatorId == EfiTransactionOriginator.PropertyWindowOriginatorId
                        &&
                        selection != null)
                    {
                        // if the change originated from Property Window we need to restore previous selection
                        // otherwise the Property Window will become empty
                        Context.Items.SetValue(selection);
                    }
                }
                else
                {
                    InitializeView();
                }
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            UpdateSelection();
        }

        // <summary>
        //     Set ourselves back to what one of the controlling views is currently selecting
        // </summary>
        internal void RefreshCurrentSelection()
        {
            Debug.Assert(Context != null, "You cannot refresh selection until you have an EditingContext");
            if (Context != null)
            {
                Selection previousSelection = null;

                // see if the designer has a selection
                if (_currentMappingDetailsInfo != null
                    &&
                    _currentMappingDetailsInfo.SelectionSource == EntityMappingSelectionSource.EntityDesigner)
                {
                    previousSelection = Context.Items.GetValue<EntityDesignerSelection>();
                }

                // see if the explorer has the selection
                if (_currentMappingDetailsInfo != null
                    &&
                    _currentMappingDetailsInfo.SelectionSource == EntityMappingSelectionSource.ModelBrowser)
                {
                    previousSelection = Context.Items.GetValue<ExplorerSelection>();
                }

                // clear the contents
                ClearToolWindowContents(true);

                // if we found one, show this selection
                if (previousSelection != null)
                {
                    ProcessSelectionFromOtherWindows(previousSelection);
                }
            }
        }

        // <summary>
        //     As long as we have an EditingContext, we can create our MappingDetailsInfo instance
        // </summary>
        private void InitializeView()
        {
            if (_editingContext == null)
            {
                _currentMappingDetailsInfo = null;
            }
            else
            {
                _currentMappingDetailsInfo = _editingContext.Items.GetValue<MappingDetailsInfo>();
                if (_currentMappingDetailsInfo.SelectionContainer == null)
                {
                    _currentMappingDetailsInfo.SetMappingDetailsInfo(
                        this,
                        _editingContext,
                        new EntityDesignSelectionContainer<MappingDetailsSelection>(this, _editingContext)
                        );
                }
            }

            ClearToolWindowContents(true);
        }

        internal void UpdateSelection()
        {
            var newSelection = new HashSet<EFObject>();
            if (SelectedModelItem != null
                && TreeControl.CurrentColumn < TreeControl.Columns.Length
                && TreeControl.Columns[TreeControl.CurrentColumn] != null)
            {
                // use the current column index to find what type of column we have
                var currentColumnType = TreeControl.Columns[TreeControl.CurrentColumn].GetType();

                // we have to find the "descriptable" EFElements for the MappingEFElements here
                EFObject modelItemForDescriptor = null;
                if (SelectedModelItem is ScalarProperty)
                {
                    var scalarProperty = SelectedModelItem as ScalarProperty;
                    if (currentColumnType.IsAssignableFrom(typeof(ValueColumn))
                        || currentColumnType.IsAssignableFrom(typeof(PropertyColumn)))
                    {
                        modelItemForDescriptor = scalarProperty.Name.Target;
                    }
                    else if (currentColumnType.IsAssignableFrom(typeof(ColumnNameColumn)))
                    {
                        modelItemForDescriptor = scalarProperty.ColumnName.Target;
                    }
                }
                else if (SelectedModelItem is FunctionScalarProperty)
                {
                    var functionScalarProperty = SelectedModelItem as FunctionScalarProperty;
                    if (currentColumnType.IsAssignableFrom(typeof(PropertyColumn)))
                    {
                        modelItemForDescriptor = functionScalarProperty.Name.Target;
                    }
                    else if (currentColumnType.IsAssignableFrom(typeof(ParameterColumn)))
                    {
                        modelItemForDescriptor = functionScalarProperty.ParameterName.Target;
                    }
                }

                // if we were able to find a corresponding descriptable EFElement (if we allow the property window to
                // display the properties) then add the EFElement to a new MappingDetailsSelection and update the context
                if (modelItemForDescriptor != null)
                {
                    newSelection.Add(modelItemForDescriptor);
                }
            }
            if (_editingContext != null
                && _editingContext.Items != null)
            {
                _editingContext.Items.SetValue(new MappingDetailsSelection(newSelection));
            }
        }

        // <summary>
        //     Proxy method to provide access to VS global services, such as ITrackSelection
        // </summary>
        protected override object GetService(Type serviceType)
        {
            var serviceProvider = ServiceProvider;
            if (serviceProvider != null)
            {
                return serviceProvider.GetService(serviceType);
            }

            return base.GetService(serviceType);
        }

        // <summary>
        //     Dispose the window
        // </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_defaultColumns != null)
                    {
                        for (var i = 0; i < _defaultColumns.Length; i++)
                        {
                            _defaultColumns[i].Dispose();
                            _defaultColumns[i] = null;
                        }
                        _defaultColumns = null;
                    }

                    Context = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // <summary>
        //     This will work with an item selected in either the Designer or the Explorer.
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ProcessSelectionFromOtherWindows(Selection selection)
        {
            _lastPrimarySelection = selection.PrimarySelection;

            // we might be called before we are fully initialized
            if (_currentMappingDetailsInfo == null)
            {
                return;
            }

            if (selection.PrimarySelection != null)
            {
                var property = selection.PrimarySelection as Property;
                var navProp = selection.PrimarySelection as NavigationProperty;
                var entityTypeShape = selection.PrimarySelection as EntityTypeShape;

                MappingDetailsWindowContainer.SetHintColor(Color.Transparent);

                if (entityTypeShape != null)
                {
                    MappingDetailsWindowContainer.SetHintColor(entityTypeShape.FillColor.Value);
                }

                if (_currentMappingDetailsInfo.ViewModel != null
                    &&
                    _currentMappingDetailsInfo.ViewModel.RootNode.ModelItem.Identity == selection.PrimarySelection.Identity)
                {
                    // same item we are showing, don't do anything
                    return;
                }
                else if (_currentMappingDetailsInfo.ViewModel != null
                         && selection.PrimarySelection.Parent != null
                         && ((property != null && property.Parent == _currentMappingDetailsInfo.ViewModel.RootNode.ModelItem) ||
                             (navProp != null && navProp.Relationship.Target == _currentMappingDetailsInfo.ViewModel.RootNode.ModelItem))
                    )
                {
                    // we are selecting a child for the parent we are showing (like property and entity), don't do anything
                    return;
                }
                else if (_currentMappingDetailsInfo.ViewModel != null
                         && entityTypeShape != null
                         && entityTypeShape.EntityType != null
                         && _currentMappingDetailsInfo.ViewModel.RootNode.ModelItem == entityTypeShape.EntityType.Target)
                {
                    // if the current entity type that we are showing is as same as what the shape refers, do nothing (we try to show the same thing).
                    return;
                }

                // reset the watermark to the default (it may have been set to an error message about hand-edited MSL)
                SetWatermarkInfo(_defaultWatermarkInfo);

                // set the screen if the user selects an entity or property
                // if they select a property, then resolve up to parent entity
                // if they select an EntityTypeShape, then resolve up the referred entity.
                ConceptualEntityType entityType = null;
                if (property != null
                    && property.EntityType != null
                    && property.EntityType.EntityModel.IsCSDL)
                {
                    entityType = property.EntityType as ConceptualEntityType;
                    Debug.Assert(property.EntityType != null ? entityType != null : true, "EntityType is not ConceptualEntityType");
                }
                else if (entityTypeShape != null
                         && entityTypeShape.EntityType != null
                         && entityTypeShape.EntityType.Target != null)
                {
                    entityType = entityTypeShape.EntityType.Target as ConceptualEntityType;
                    Debug.Assert(entityType != null, "Why does EntityTypeShape's EntityType is not ConceptualEntityType");
                }
                else
                {
                    entityType = selection.PrimarySelection as ConceptualEntityType;
                }

                if (entityType != null
                    && CanEditMappingsForEntityType(entityType))
                {
                    // reset our view model XRef
                    Context.Items.SetValue(new ModelToMappingModelXRef());

                    // if our last view was not an entity or the entity is abstract, start out in Tables mode
                    if (_currentMappingDetailsInfo.EntityMappingMode == EntityMappingModes.None
                        || entityType.Abstract.Value)
                    {
                        _currentMappingDetailsInfo.EntityMappingMode = EntityMappingModes.Tables;
                    }

                    // create the view model
                    _currentMappingDetailsInfo.ViewModel = MappingViewModelHelper.CreateViewModel(Context, entityType);

                    // show the view model in the tool window
                    Debug.Assert(
                        _currentMappingDetailsInfo.ViewModel != null && _currentMappingDetailsInfo.ViewModel.RootNode != null,
                        "Failed to correctly create the mapping details ViewModel for the selected entity");
                    if (_currentMappingDetailsInfo.ViewModel != null
                        && _currentMappingDetailsInfo.ViewModel.RootNode != null)
                    {
                        DoShowMappingDetailsForElement(_currentMappingDetailsInfo.ViewModel.RootNode.ModelItem);
                    }

                    // update the toolbar buttons
                    MappingDetailsWindowContainer.UpdateToolbar();

                    return;
                }

                // see if user selected a navigation property, and treat the mapping view as if the user selected an association
                var association = GetAssociationFromLastPrimarySelection();

                if (association != null
                    && association.EntityModel.IsCSDL
                    && CanEditMappingsForAssociation(association, false))
                {
                    SetUpAssociationDisplay();

                    return;
                }

                // set the screen if the user selected a FunctionImport
                var fi = selection.PrimarySelection as FunctionImport;
                if (fi != null
                    && fi.FunctionImportMapping != null)
                {
                    // only show mappings for FuntionImports that ReturnType is either EntityType or ComplexType
                    var isReturnTypeEntityOrComplexType = fi.IsReturnTypeEntityType || fi.IsReturnTypeComplexType;

                    if (isReturnTypeEntityOrComplexType && CanEditMappingsForFunctionImport(fi))
                    {
                        // reset our view model XRef
                        Context.Items.SetValue(new ModelToMappingModelXRef());

                        // set our mode to None since we aren't mapping entities
                        _currentMappingDetailsInfo.EntityMappingMode = EntityMappingModes.None;
                        _currentMappingDetailsInfo.ViewModel = MappingViewModelHelper.CreateViewModel(Context, fi.FunctionImportMapping);

                        // show the view model in the tool window
                        Debug.Assert(
                            _currentMappingDetailsInfo.ViewModel != null && _currentMappingDetailsInfo.ViewModel.RootNode != null,
                            "Failed to correctly create the mapping details ViewModel for the selected association");
                        if (_currentMappingDetailsInfo.ViewModel != null
                            &&
                            _currentMappingDetailsInfo.ViewModel.RootNode != null)
                        {
                            DoShowMappingDetailsForElement(_currentMappingDetailsInfo.ViewModel.RootNode.ModelItem);
                        }

                        // update the toolbar buttons
                        MappingDetailsWindowContainer.UpdateToolbar();

                        return;
                    }
                }
            }

            // the user clicked on something other than an entity or association
            ClearToolWindowContents(false);

            // clear out our selection source
            _currentMappingDetailsInfo.SelectionSource = EntityMappingSelectionSource.None;
        }

        internal Association GetAssociationFromLastPrimarySelection()
        {
            // see if user selected a navigation property, and treat the mapping view as if the user selected an association
            var navProp = _lastPrimarySelection as NavigationProperty;
            Association association = null;
            if (navProp != null)
            {
                association = navProp.Relationship.Target;
            }

            // set the screen if the user selected an association or a navigation property
            if (association == null)
            {
                association = _lastPrimarySelection as Association;
            }
            return association;
        }

        internal void SetUpAssociationDisplay()
        {
            var association = GetAssociationFromLastPrimarySelection();
            if (association != null)
            {
                // reset our view model XRef
                Context.Items.SetValue(new ModelToMappingModelXRef());

                // set our mode to None since we aren't mapping entities
                _currentMappingDetailsInfo.EntityMappingMode = EntityMappingModes.None;
                _currentMappingDetailsInfo.ViewModel = MappingViewModelHelper.CreateViewModel(Context, association);

                // show the view model in the tool window
                Debug.Assert(
                    _currentMappingDetailsInfo.ViewModel != null && _currentMappingDetailsInfo.ViewModel.RootNode != null,
                    "Failed to correctly create the mapping details ViewModel for the selected association");
                if (_currentMappingDetailsInfo.ViewModel != null
                    &&
                    _currentMappingDetailsInfo.ViewModel.RootNode != null)
                {
                    DoShowMappingDetailsForElement(_currentMappingDetailsInfo.ViewModel.RootNode.ModelItem);
                }

                // update the toolbar buttons
                MappingDetailsWindowContainer.UpdateToolbar();

                MappingDetailsWindowContainer.SetHintColor(Color.Transparent);
            }
        }

        // <summary>
        //     Clear our state and revert back the default watermark
        // </summary>
        private void ClearToolWindowContents(bool resetWatermark)
        {
            // clear any names from title bar
            UpdateTitleBar(null);

            // reset the watermark to the default (it may have been set to an error message about hand-edited MSL)
            if (resetWatermark &&
                string.Compare(_watermarkInfo.WatermarkText, _defaultWatermarkInfo.WatermarkText, StringComparison.CurrentCulture) != 0)
            {
                SetWatermarkInfo(_defaultWatermarkInfo);
            }

            // clear our view model
            if (_currentMappingDetailsInfo != null)
            {
                _currentMappingDetailsInfo.ViewModel = null;
            }

            // empty the toolwindow (cause watermark to show)
            DoSelectionChanged((object)null);

            // update the toolbar buttons
            if (MappingDetailsWindowContainer != null)
            {
                MappingDetailsWindowContainer.UpdateToolbar();
            }
        }

        // <summary>
        //     Calls a validation method to check if this entity's MSL is editable by the designer
        // </summary>
        private bool CanEditMappingsForEntityType(ConceptualEntityType entityType)
        {
            var errorMessage = string.Empty;
            if (MappingViewModelHelper.CanEditMappingsForEntityType(entityType, ref errorMessage))
            {
                return true;
            }
            else
            {
                SetWatermarkInfo(string.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral, errorMessage));
                return false;
            }
        }

        // <summary>
        //     Calls a validation method to check if this association's MSL is editable by the designer
        // </summary>
        internal bool CanEditMappingsForAssociation(Association association, bool allowAssociationMappingEditWithFKs)
        {
            if (association == null)
            {
                throw new ArgumentNullException("association");
            }

            TreeGridDesignerWatermarkInfo watermarkInfo = null;

            if (MappingViewModelHelper.CanEditMappingsForAssociation(
                association, MappingDetailsWindowContainer, ref watermarkInfo, allowAssociationMappingEditWithFKs))
            {
                return true;
            }
            else
            {
                Debug.Assert(watermarkInfo != null, "watermark should not be null");
                if (watermarkInfo != null)
                {
                    SetWatermarkInfo(watermarkInfo);
                }
                return false;
            }
        }

        // <summary>
        //     Calls a validation method to check if this association's MSL is editable by the designer
        // </summary>
        private bool CanEditMappingsForFunctionImport(FunctionImport fi)
        {
            var errorMessage = string.Empty;
            // check whether we should lit up the function import mapping.
            if (EdmFeatureManager.GetFunctionImportMappingFeatureState(fi.Artifact.SchemaVersion).IsEnabled() == false)
            {
                SetWatermarkInfo(Resources.MappingDetails_ErrMappingNotSupported);
                return false;
            }
            else if (MappingViewModelHelper.CanEditMappingsForFunctionImport(fi, ref errorMessage))
            {
                return true;
            }
            else
            {
                SetWatermarkInfo(string.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral, errorMessage));
                return false;
            }
        }

        // <summary>
        //     Redirects to our command selection handler
        // </summary>
        private void OnExplorerSelectionChanged(ExplorerSelection selection)
        {
            if (_currentMappingDetailsInfo != null)
            {
                _currentMappingDetailsInfo.SelectionSource = EntityMappingSelectionSource.ModelBrowser;
            }
            ProcessSelectionFromOtherWindows(selection);
        }

        // <summary>
        //     Redirects to our command selection handler
        // </summary>
        private void OnEntityDesignerSelectionChanged(EntityDesignerSelection selection)
        {
            if (_currentMappingDetailsInfo != null)
            {
                _currentMappingDetailsInfo.SelectionSource = EntityMappingSelectionSource.EntityDesigner;
            }
            ProcessSelectionFromOtherWindows(selection);
        }

        // <summary>
        //     This toolwindow is only usable when one of our documents is shown
        // </summary>
        protected override bool IsDocumentSupported(DocData docData)
        {
            return docData is IEntityDesignDocData;
        }

        // <summary>
        //     Caption displayed in the title bar of the window
        // </summary>
        public override string WindowTitle
        {
            get { return Resources.MappingDetails_WindowTitle; }
        }

        // <summary>
        //     Does the work of actually showing the view model in the Trid.
        // </summary>
        private void DoShowMappingDetailsForElement(EFElement element)
        {
            Debug.Assert(element != null, "A null element was passed to DoShowMappingDetailsForElement()");
            if (element != null)
            {
                var xref = Context.Items.GetValue<ModelToMappingModelXRef>();
                var mappingElement = xref.GetExisting(element);

                Debug.Assert(mappingElement != null, "Failed to get ViewModel element for the passed in Model element");
                if (mappingElement != null)
                {
                    TreeControl.BatchDrawItem = true;
                    try
                    {
                        var invalidate = DoSelectionChanged(mappingElement);
                        if (invalidate)
                        {
                            // local shortcuts
                            var tree = TreeProvider;
                            var rootBranch = tree.Root;

                            if (rootBranch != null
                                && !WatermarkVisible)
                            {
                                // expand all nodes
                                for (var i = 0; i < tree.VisibleItemCount; i++)
                                {
                                    if (tree.IsExpandable(i, 0)
                                        && !tree.IsExpanded(i, 0))
                                    {
                                        tree.ToggleExpansion(i, 0);
                                    }
                                }

                                // scroll tree to top
                                TreeControl.CurrentIndex = 0;

                                UpdateTitleBar(mappingElement.Name);
                            }
                        }

                        // if the watermark is showing and selection changed, update the title bar
                        if (WatermarkVisible && invalidate)
                        {
                            UpdateTitleBar(null);
                        }
                    }
                    finally
                    {
                        TreeControl.BatchDrawItem = false;
                    }
                }
            }
        }

        internal bool NavigateTo(EFObject item)
        {
            var foundObject = false;
            var foundObjectInRow = -1;

            IBranch lastBranch = null;
            var branchRow = -1;

            var tree = TreeProvider;
            if (tree.Root != null
                && !WatermarkVisible)
            {
                // from the tree's point of view, there is a set of rows from 0 to n
                for (var row = 0; row < tree.VisibleItemCount; row++)
                {
                    // for a given row, it will be governed by a Branch
                    var info = tree.GetItemInfo(row, 0, true);

                    if (info.Branch != null)
                    {
                        // branches are only aware of the items they process, from 0 to n
                        // so as long as we in the same branch as the previous iteration we just increment
                        if (lastBranch == info.Branch)
                        {
                            branchRow++;
                        }
                        else
                        {
                            // we switched branches, so reset the local index
                            lastBranch = info.Branch;
                            branchRow = 0;
                        }

                        // get the object for this branch at this location: we always send 0 for the
                        // column because we only care about getting the branch for the row, which is
                        // same for every column in a given row
                        var options = 0;
                        var obj = info.Branch.GetObject(branchRow, 0, TreeGridDesignerBranch.BrowsingObject, ref options);

                        // if this is a mapping element, then check to see if its managing the passed in EFObject
                        var mel = obj as MappingEFElement;
                        if (mel != null
                            && mel.ModelItem != null
                            && mel.ModelItem.Equals(item))
                        {
                            foundObjectInRow = row;
                            foundObject = true;
                            break;
                        }
                    }
                }
            }

            // we found one, so move the tree's selection
            if (foundObject
                && foundObjectInRow >= 0
                && foundObjectInRow < tree.VisibleItemCount)
            {
                TreeControl.CurrentIndex = foundObjectInRow;
            }

            return foundObject;
        }

        internal MappingEFElement SelectedViewModelItem
        {
            get
            {
                var info = TreeControl.SelectedItemInfo;
                if (info.Branch != null)
                {
                    // get the object for this branch at this location
                    var options = 0;
                    var obj = info.Branch.GetObject(info.Row, info.Column, TreeGridDesignerBranch.BrowsingObject, ref options);

                    // return it if we found a view model element (may be null)
                    var mel = obj as MappingEFElement;
                    return mel;
                }

                return null;
            }
        }

        internal EFObject SelectedModelItem
        {
            get
            {
                // if we found a view model element, returns its underlying model element (may be null)
                var mel = SelectedViewModelItem;
                if (mel != null)
                {
                    return mel.ModelItem;
                }

                return null;
            }
        }

        // <summary>
        //     Updates the title bar to include the item's name that we are editing
        // </summary>
        private void UpdateTitleBar(string name)
        {
            var fullName = Resources.MappingDetails_WindowTitle;
            if (!string.IsNullOrEmpty(name))
            {
                fullName = String.Format(CultureInfo.CurrentCulture, "{0} - {1}", fullName, name);
            }

            var frame = Frame;
            if (frame != null)
            {
                // set window title based on selection
                // this will return E_UNEXPECTED if the frame is disposed, ignore that as there's no need to set the title in that case.
                VsErrorHandler.ThrowOnFailure(
                    frame.SetProperty((int)VsShell.__VSFPROPID.VSFPROPID_Caption, fullName), VSConstants.E_UNEXPECTED);
            }
        }

        // <summary>
        //     Returns the window's AccessibilityName
        // </summary>
        protected override string AccessibilityName
        {
            get { return Resources.MappingDetails_WindowTitle; }
        }

        // <summary>
        //     Returns the CommandID of the context menu to be shown for this tool window.
        // </summary>
        protected override CommandID ContextMenuId
        {
            get { return null; }
        }

        // <summary>
        //     Allows derived classes to specify default watermark text
        // </summary>
        protected override TreeGridDesignerWatermarkInfo WatermarkInfo
        {
            get { return _watermarkInfo; }
        }

        // NOTE: This is currently only used by the API Test framework
        internal TreeGridDesignerWatermarkInfo GetWatermarkInfo()
        {
            return _watermarkInfo;
        }

        public void SetWatermarkInfo(string text)
        {
            var wm = new TreeGridDesignerWatermarkInfo(text);
            SetWatermarkInfo(wm);
        }

        public void SetWatermarkInfo(TreeGridDesignerWatermarkInfo watermarkInfo)
        {
            Debug.Assert(watermarkInfo != null, "watermark info is null!");
            if (watermarkInfo != null)
            {
                _watermarkInfo = watermarkInfo;
                ForceWatermarkTextChange();
            }
        }

        // <summary>
        //     Returns the collection of columns that the Trid will show
        // </summary>
        protected override ICollection DefaultColumns
        {
            get { return _defaultColumns; }
        }

        // <summary>
        //     Mapping details window bitmap resource id
        // </summary>
        protected override int BitmapResource
        {
            get { return 104; }
        }

        // <summary>
        //     Mapping details window bitmap index
        // </summary>
        protected override int BitmapIndex
        {
            get { return 0; }
        }
    }
}
