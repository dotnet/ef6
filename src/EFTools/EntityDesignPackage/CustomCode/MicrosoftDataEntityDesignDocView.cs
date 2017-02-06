// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DesignerModel = Microsoft.Data.Entity.Design.Model.Designer;
using DslDiagrams = Microsoft.VisualStudio.Modeling.Diagrams;
using DslModeling = Microsoft.VisualStudio.Modeling;
using SystemObjectModel = System.Collections.ObjectModel;

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.EntityDesigner;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Entity.Design.UI.Views.EntityDesigner;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell.Interop;
    using PropertyBase = Microsoft.Data.Entity.Design.Model.Entity.PropertyBase;

    /// <summary>
    ///     This file is adding support for our common selection model to the stock DocView that the DSL
    ///     Project Wizard gave us.  It's job is basically to interject itself inbetween DSL and the property window,
    ///     translating DSL classes into our model item descriptors.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class MicrosoftDataEntityDesignDocView
    {
        internal static string OpenViewTransactionContext = "openviewtransaction";
        private EditingContext _context;
        private readonly string _diagramId;
        private ModelToDesignerModelXRefItem _xRef;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public MicrosoftDataEntityDesignDocView(ModelingDocData docData, IServiceProvider serviceProvider, string diagramId)
            : base(docData, serviceProvider)
        {
            _diagramId = diagramId;
        }

        public EditingContext Context
        {
            get { return _context; }
        }

        public bool IsLoading { get; private set; }

        protected ModelToDesignerModelXRefItem XRef
        {
            get
            {
                if (_xRef == null)
                {
                    Debug.Assert(Diagram != null, "DocView Diagram is null");
                    _xRef = ModelToDesignerModelXRef.GetModelToDesignerModelXRefItem(Context, Diagram.ModelElement.Partition);
                }
                return _xRef;
            }
        }

        /// Ensures that DSL Diagram and Entity Diagram are in sync.
        /// This method also update the xref link between DSL Diagram and Model Diagram.
        private void ApplyLayoutInformationFromModelDiagram()
        {
            var artifact = Context.GetEFArtifactService().Artifact as EntityDesignArtifact;
            Debug.Assert(artifact != null, "Could not get the instance of EntityDesignArtifact from EditingContext.");

            if (artifact != null
                && artifact.IsDesignerSafe)
            {
                DesignerModel.Diagram modelDiagram = null;
                if (artifact.DesignerInfo != null
                    && artifact.DesignerInfo.Diagrams != null)
                {
                    modelDiagram = String.IsNullOrEmpty(_diagramId)
                                       ? artifact.DesignerInfo.Diagrams.FirstDiagram
                                       : artifact.DesignerInfo.Diagrams.GetDiagram(_diagramId);
                }

                // At this point DSL diagram should have been set.
                var diagram = Diagram as EntityDesignerDiagram;
                Debug.Assert(diagram != null, "Why does the DSL diagram is null?");

                if (modelDiagram != null
                    && diagram != null)
                {
                    EntityModelToDslModelTranslatorStrategy.TranslateDiagram(diagram, modelDiagram);
                    if (CurrentDesigner != null)
                    {
                        CurrentDesigner.ZoomAtViewCenter((float)modelDiagram.ZoomLevel.Value / 100);
                    }
                }
            }
        }

        /// <summary>
        ///     Helper method to retrieve/create DSL Diagram for the view.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private EntityDesignerDiagram GetNewOrExistingViewDiagram()
        {
            EntityDesignerDiagram diagram = null;

            // Check whether the EntityDesignerDiagram exists.
            var docData = DocData as MicrosoftDataEntityDesignDocDataBase;
            diagram = GetExistingViewDiagram(docData, _diagramId);

            // Create DSL diagram if the diagram has not been created yet.
            if (diagram == null
                && String.IsNullOrEmpty(_diagramId) == false)
            {
                var artifact = Context.GetEFArtifactService().Artifact as EntityDesignArtifact;
                Debug.Assert(artifact != null, "Could not find instance of EntityDesignArtifact in EditingContext.");
                if (artifact != null)
                {
                    if (artifact.IsDesignerSafe
                        && artifact.DesignerInfo != null
                        && artifact.DesignerInfo.Diagrams != null)
                    {
                        // Get the corresponding model diagram.
                        var modelDiagram = artifact.DesignerInfo.Diagrams.GetDiagram(_diagramId);
                        if (modelDiagram != null)
                        {
                            using (var transaction = docData.Store.TransactionManager.BeginTransaction("OpenView", true))
                            {
                                var evm =
                                    ModelTranslatorContextItem.GetEntityModelTranslator(Context)
                                        .TranslateModelToDslModel(modelDiagram, new DslModeling.Partition(docData.Store)) as
                                    EntityDesignerViewModel;
                                diagram = CreateDslDiagram(evm);
                                transaction.Commit();
                            }
                        }
                    }
                    else
                    {
                        using (var transaction = docData.Store.TransactionManager.BeginTransaction("OpenView", true))
                        {
                            var evm =
                                ModelTranslatorContextItem.GetEntityModelTranslator(Context)
                                    .TranslateModelToDslModel(null, new DslModeling.Partition(docData.Store)) as EntityDesignerViewModel;
                            diagram = CreateDslDiagram(evm);
                            transaction.Commit();
                        }
                    }
                }
            }
            return diagram;
        }

        internal static EntityDesignerDiagram GetExistingViewDiagram(MicrosoftDataEntityDesignDocDataBase docData, string diagramId)
        {
            EntityDesignerDiagram diagram = null;
            var diagrams = docData.GetDiagramPartition().ElementDirectory.FindElements<EntityDesignerDiagram>();
            if (diagrams.Count > 0)
            {
                if (String.IsNullOrEmpty(diagramId) == false)
                {
                    diagram = diagrams.Where(d => d.DiagramId == diagramId).FirstOrDefault();
                }
                else
                {
                    // If the diagramid is not set, just select the first diagram.
                    diagram = diagrams[0];
                }
            }
            return diagram;
        }

        private EntityDesignerDiagram CreateDslDiagram(EntityDesignerViewModel evm)
        {
            var docData = DocData as MicrosoftDataEntityDesignDocDataBase;
            EntityDesignerDiagram diagram = null;
            Debug.Assert(docData != null, "DocData is not a type of MicrosoftDataEntityDesignDocDataBase");
            if (docData != null)
            {
                diagram = MicrosoftDataEntityDesignSerializationHelper.Instance.CreateDiagramHelper(docData.GetDiagramPartition(), evm);
                // Set diagram name here otherwise DSL will assign one when diagram is loaded in the view which causes a transaction to be committed.
                // When the transaction is committed our custom DSL rules will get fired. At that point Debug.Assert might be fired because the xref might not properly set.
                // It doesn't matter if the name is the same across multiple diagram because we don't use it.
                diagram.Name = Path.GetFileNameWithoutExtension(DocData.FileName);
            }
            return diagram;
        }

        protected override bool LoadView()
        {
            var ret = false;

            var isDocDataDirty = 0;
            // Save IsDocDataDirty flag here to be set back later. 
            // This is because loading-view can cause the flag to be set since a new diagram could potentially created.
            DocData.IsDocDataDirty(out isDocDataDirty);
            IsLoading = true;
            try
            {
                var uri = Utils.FileName2Uri(DocData.FileName);
                _context = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                Debug.Assert(_context != null, "_context should not be null");

                // Set DSL Diagram instance and values.
                // Note: the code should be executed before we suspend rule notification. The diagram shapes will not created correctly if we don't.

                // When document is reloaded, a new diagram will be created; so we always need to check for the new view diagram every time LoadView is called.
                var currentDiagram = GetNewOrExistingViewDiagram();
                if (Diagram != currentDiagram)
                {
                    Diagram = currentDiagram;
                }

                // Ensure that cache _xRef is cleared.
                _xRef = null;

                // The only case where diagram is null at this point is that VS tries to open diagram that doesn't exist in our model.
                // One of the possibilities: the user creates multiple diagrams, open the diagrams in VS, then close the project without saving the document.
                // When the project is reopened in the same VS, VS remembers any opened windows and will try to reopen it.
                // In this case, we should close the frame.
                if (Diagram == null)
                {
                    // Return false will force the window frame to be closed.
                    return false;
                }

                ApplyLayoutInformationFromModelDiagram();

                Debug.Assert(DocData.Store.RuleManager.IsRuleSuspended == false, "The rule notification should not be suspended.");
                DocData.Store.RuleManager.SuspendRuleNotification();

                // We don't call base.LoadView() because the code assumes there is only 1 diagram (will assert otherwise),
                // and will always choose the first diagram.
                if (BaseLoadView())
                {
                    // Normally our toolbox items get populated only when the window is activated, but
                    // in certain circumstances we may switch the mode of our designer (normal/safe-mode/etc)
                    // when the window is already active. This is an expensive operation so we only need it when
                    // we reload the active document.
                    var escherDocData = DocData as MicrosoftDataEntityDesignDocData;
                    if (escherDocData != null
                        && escherDocData.IsHandlingDocumentReloaded)
                    {
                        ToolboxService.Refresh();
                    }
                    ret = true;
                }

                // Listen to Diagram Title change event so we can update our window caption with the information.
                var entityDesignerDiagram = Diagram as EntityDesignerDiagram;
                Debug.Assert(entityDesignerDiagram != null, "The diagram is not the type of EntityDesignerDiagram");
                if (entityDesignerDiagram != null)
                {
                    entityDesignerDiagram.OnDiagramTitleChanged += OnDiagramTitleChanged;
                }
                UpdateWindowFrameCaption();
            }
            finally
            {
                // After Diagram is set, DSL code enabled DSL Undo Manager, so the code below is to disable it.
                if (DocData.Store.UndoManager.UndoState == DslModeling.UndoState.Enabled)
                {
                    DocData.Store.UndoManager.UndoState = DslModeling.UndoState.Disabled;
                }

                if (DocData.Store.RuleManager.IsRuleSuspended)
                {
                    DocData.Store.RuleManager.ResumeRuleNotification();
                }
                DocData.SetDocDataDirty(isDocDataDirty);
                IsLoading = false;
            }
            return ret;
        }

        /// <summary>
        ///     Tack Diagram title information in Window caption.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.SetProperty(System.Int32,System.Object)")]
        private void UpdateWindowFrameCaption()
        {
            var entityDesignerDiagram = Diagram as EntityDesignerDiagram;
            if (entityDesignerDiagram != null)
            {
                if (!String.IsNullOrEmpty(entityDesignerDiagram.Title))
                {
                    Frame.SetProperty(
                        (int)__VSFPROPID.VSFPROPID_EditorCaption,
                        String.Format(CultureInfo.CurrentCulture, Resources.EditorCaptionFormat, entityDesignerDiagram.Title));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _context != null)
            {
                // we didn't create the context, so just release it as soon as we can
                _context = null;
                VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///     The implementation of VsShell.ISelectionContainer.CountObjects in
        ///     ModelingWindowPane calls either CountSelectedObjects() or CountAllObjects()
        ///     - base calls to ModelingWindowPane
        /// </summary>
        /// <returns></returns>
        protected override uint CountSelectedObjects()
        {
            if (!IsArtifactDesignerSafe()
                && !EdmUtils.ShouldShowByRefDebugHelpers())
            {
                return 0;
            }
            else
            {
                // we can just call the base class because the count of selected designer elements
                // and their corresponding ItemDescriptors should be the same
                return base.CountSelectedObjects();
            }
        }

        protected override bool HasSelectableObjects
        {
            get
            {
                if (IsArtifactDesignerSafe() == false)
                {
                    return EdmUtils.ShouldShowByRefDebugHelpers();
                }
                else
                {
                    return base.HasSelectableObjects;
                }
            }
        }

        /// <summary>
        ///     This allows us to disable the toolbox if the designer is in safe mode
        ///     See bug 556963 - SQL BU Defect Tracking
        /// </summary>
        protected override ICollection TargetToolboxItemFilterAttributes
        {
            get
            {
                if (IsArtifactDesignerAndEditSafe())
                {
                    return base.TargetToolboxItemFilterAttributes;
                }
                return null;
            }
        }

        /// <summary>
        ///     - base calls to DiagramDocView : ModelingDocView : ModelingWindowPane
        ///     if (this.CurrentDiagram != null) -> this.CountShapes
        ///     else calls to ModelingWindowPane
        /// </summary>
        /// <returns></returns>
        protected override uint CountAllObjects()
        {
            if (IsArtifactDesignerSafe() == false
                && EdmUtils.ShouldShowByRefDebugHelpers() == false)
            {
                return 0;
            }
            else
            {
                // the scope of this 'all' list should be the selectable shapes in the designer
                // so let the base class count these
                var count = base.CountAllObjects();
                if (count > (EntityDesignerDiagram.IMPLICIT_AUTO_LAYOUT_CEILING / 2))
                {
                    // if there are too many objects, just tell the system that we have 1 item
                    // which will cause only the selected item to show up in the drop-down list on top
                    // of the property window
                    return 1;
                }
                else
                {
                    return count;
                }
            }
        }

        /// <summary>
        ///     The implementation of VsShell.ISelectionContainer.GetObjects in
        ///     ModelingWindowPane calls either GetSelectedObjects() or GetAllObjects()
        ///     - base calls to ModelingWindowPane
        /// </summary>
        /// <returns></returns>
        protected override void GetSelectedObjects(uint count, object[] objects)
        {
            // call the base class version first; objects will be populated with a
            // collection of DSL ModelElements
            base.GetSelectedObjects(count, objects);

            if (IsArtifactDesignerSafe() == false)
            {
                if (EdmUtils.ShouldShowByRefDebugHelpers())
                {
                    if (Context != null)
                    {
                        var artifact = EditingContextManager.GetArtifact(Context) as EntityDesignArtifact;
                        if (artifact != null)
                        {
                            // Show at least the EFEntityModelDescriptor so we can see the by-reference property extensions
                            // for the hydrated model
                            var descriptor =
                                (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFEntityModelDescriptor), null, null);
                            descriptor.Initialize(artifact.ConceptualModel, Context, true);
                            objects[0] = descriptor;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < objects.Length; i++)
                    {
                        objects[i] = null;
                    }
                }
            }
                // only show properties for single selection
            else
            {
                // now change this into an array of our item descriptors.
                var selectedModelObjects = ConvertDslModelElementArrayToItemDescriptors(objects, false);

                // if there are more than 1 selected object, convert the item descriptors to linked-item-descriptors.
                // This is so that property update can be done in 1 transaction.
                if (selectedModelObjects.Count > 1)
                {
                    var tempSelectedModelObjects = new ArrayList();
                    var contextItem = new LinkedDescriptorContextItem();
                    foreach (var customTypeDescriptor in selectedModelObjects.ToArray().OfType<ICustomTypeDescriptor>())
                    {
                        tempSelectedModelObjects.Add(new LinkedPropertyTypeDescriptor(customTypeDescriptor, contextItem));
                    }
                    selectedModelObjects = tempSelectedModelObjects;
                }

                // if the lengths are not the same, then there are items (like compartments)
                // that are being selected, so don't change the object array
                if (objects.Length == selectedModelObjects.Count)
                {
                    selectedModelObjects.CopyTo(objects, 0);
                }
            }
        }

        /// <summary>
        ///     - base calls to DiagramDocView : ModelingDocView : ModelingWindowPane
        ///     if (this.CurrentDiagram != null) -> this.GrabObjects
        ///     else calls to ModelingWindowPane
        /// </summary>
        /// <param name="count"></param>
        /// <param name="objects"></param>
        protected override void GetAllObjects(uint count, object[] objects)
        {
            // call the base class version first; objects will be populated with a
            // collection of DSL ModelElements
            base.GetAllObjects(count, objects);

            // return null array if doc is not designer-safe
            if (IsArtifactDesignerSafe() == false)
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    objects[i] = null;
                }
            }
            else if (count > 0)
            {
                // now change this into an array of our item descriptors
                var allModelObjects = ConvertDslModelElementArrayToItemDescriptors(objects, true);

                // if the lengths are not the same, then there are items (like compartments)
                // in the current selection context, so don't change the object array
                if (objects.Length == allModelObjects.Count)
                {
                    allModelObjects.CopyTo(objects, 0);
                }
            }
        }

        /// <summary>
        ///     This uses the XRef in our context to find the Model item that is the basis for
        ///     each DSL object in the selection collection.  For each Model item, we get the correct
        ///     item descriptor and create a new selection array of these.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private ArrayList ConvertDslModelElementArrayToItemDescriptors(object[] objects, bool gettingAllObjects)
        {
            // save off this reference
            var selectedDesignerObjects = objects;

            // create a new array to hold the item descriptors
            var selectedModelObjects = new List<EFObject>();
            var selectedModelObjectDescriptors = new ArrayList();
            foreach (var o in selectedDesignerObjects)
            {
                DslModeling.ModelElement dslElem = null;
                var elemList = o as DslDiagrams.ElementListCompartment;
                var presElem = o as DslDiagrams.PresentationElement;
                if (elemList != null)
                {
                    // if the user selects a compartment element, we want to display the entity-type and the shape property.
                    if (elemList.IsNestedChild
                        && elemList.ParentShape != null)
                    {
                        presElem = elemList.ParentShape;
                        dslElem = presElem.ModelElement;
                    }
                    else
                    {
                        // they have selected one of the compartments, probably wanting to show 
                        // the right-click context menu
                        continue;
                    }
                }
                else if (presElem != null)
                {
                    // if this is a shape, gets it corresponding DSL object
                    dslElem = presElem.ModelElement;
                }
                else
                {
                    // o is a non-shape
                    dslElem = o as DslModeling.ModelElement;
                }

                // there might be no ModelElement corresponding to the selected object
                if (dslElem != null)
                {
                    EFObject modelElem;
                    // If an EntityType is selected in DSL canvas, we want to show the property of the EntityTypeShape.
                    if (dslElem is EntityType
                        && presElem != null)
                    {
                        modelElem = XRef.GetExisting(presElem);
                    }
                    else
                    {
                        modelElem = XRef.GetExisting(dslElem);
                    }
                    // model element might not be yet created
                    if (modelElem != null)
                    {
                        selectedModelObjects.Add(modelElem);
                        selectedModelObjectDescriptors.Add(PropertyWindowViewModel.GetObjectDescriptor(modelElem, Context, true));
                    }
                }
            }

            if (gettingAllObjects == false)
            {
                // update the selection for the Entity Designer in case anyone else is interested
                if (Context != null
                    && Context.Items != null)
                {
                    Context.Items.SetValue(new EntityDesignerSelection(selectedModelObjects));
                }
            }

            return selectedModelObjectDescriptors;
        }

        /// <summary>
        ///     The implementation of VsShell.ISelectionContainer.SelectObjects in
        ///     ModelingWindowPane calls DoSelectObjects()
        ///     - base calls to DiagramDocView : ModelingDocView : ModelingWindowPane
        ///     if (this.CurrentDesigner != null) -> sets selection in the current designer
        ///     else calls to ModelingWindowPane which is a no-op
        ///     This seems to only be called when the user chooses an item from the
        ///     drop down box at the top of the properties screen
        /// </summary>
        /// <returns></returns>
        protected override void DoSelectObjects(uint count, object[] objects, uint flags)
        {
            // We have to reverse the logic of the GetSelectX fn above; that is, convert the
            // array of our ItemDescriptors back into an array of Dsl ModelElements before 
            // calling the base impl; the base impl will then select the shape
            var objectsSelected = new ArrayList();
            var selectedModelObjects = new List<EFObject>();
            uint nonPropertyObjectsCount = 0;
            if (count > 0)
            {
                foreach (var o in objects)
                {
                    var typeDesc = o as ObjectDescriptor;
                    Debug.Assert(typeDesc != null, "Something unexpected was selected");
                    if (typeDesc != null)
                    {
                        selectedModelObjects.Add(typeDesc.WrappedItem);

                        // Properties do not have a corresponding ShapeElement; the instance of the DomainClass is just an item
                        // within the ElementListCompartment. Since this override only accepts a list of ShapeElements, we will
                        // have to use the DSLDesignerNavigationHelper to create a DiagramItem that uniquely identifies the
                        // list compartment item and select that.
                        if (typeDesc.WrappedItem is PropertyBase)
                        {
                            DSLDesignerNavigationHelper.NavigateTo(typeDesc.WrappedItem);
                        }
                        else
                        {
                            // For all other EFObjects, we can attempt to get a ShapeElement
                            nonPropertyObjectsCount++;
                            var dslElem = XRef.GetExisting(typeDesc.WrappedItem);
                            var presElem = dslElem as DslDiagrams.PresentationElement;
                            if (presElem == null)
                            {
                                var shapes =
                                    DslDiagrams.PresentationViewsSubject.GetPresentation(dslElem);

                                // just select the first shape for this item
                                if (shapes != null
                                    && shapes.Count > 0)
                                {
                                    presElem = shapes[0];
                                }
                            }

                            Debug.Assert(presElem != null, "Why couldn't we find the shape for this ModelElement?");

                            if (presElem != null)
                            {
                                objectsSelected.Add(presElem);
                            }
                        }
                    }
                }

                Debug.Assert(
                    nonPropertyObjectsCount == objectsSelected.Count,
                    "nonPropertyObjectsCount(" + nonPropertyObjectsCount + ") != objectsSelected.Count (" + objectsSelected.Count + ")");
            }

            // update the selection for the Entity Designer in case anyone else is interested
            Context.Items.SetValue(new EntityDesignerSelection(selectedModelObjects));

            if (nonPropertyObjectsCount > 0
                && nonPropertyObjectsCount == objectsSelected.Count)
            {
                // If we didn't encounter any property descriptors and the number of those descriptors equals the objects selected,
                // then we can pass this on to the base implementation to perform the selection
                var objectsToSelect = new object[objectsSelected.Count];
                objectsSelected.CopyTo(objectsToSelect, 0);
                base.DoSelectObjects(nonPropertyObjectsCount, objectsToSelect, flags);
            }
            else if (nonPropertyObjectsCount != objectsSelected.Count)
            {
                // We should have found objects to select from all the non-property descriptors. If for some reason we didn't,
                // then we pass this onto the base implementation
                base.DoSelectObjects(count, objects, flags);
            }
        }

        private bool IsArtifactDesignerSafe()
        {
            var artifact = EditingContextManager.GetArtifact(Context);

            // artifact may be null if the editing context has been disposed.
            if (artifact != null)
            {
                return IsArtifactDesignerSafe(artifact);
            }
            else
            {
                // No artifact.  This should never be, but return false just to be safe.
                return false;
            }
        }

        private static bool IsArtifactDesignerSafe(EFArtifact artifact)
        {
            // if the artifact needs to be reloaded, treat it as designer-unsafe for our purposes.
            // this should only be the case when editing in the xml editor, and our selection container
            // gets invoked.
            if (artifact.RequireDelayedReload)
            {
                return false;
            }
            return artifact.IsDesignerSafe;
        }

        private bool IsArtifactDesignerAndEditSafe()
        {
            var artifact = EditingContextManager.GetArtifact(Context) as EntityDesignArtifact;
            if (artifact != null)
            {
                return IsArtifactDesignerSafe(artifact) && artifact.IsDesignerSafeAndEditSafe();
            }
            else
            {
                return false;
            }
        }

        #region Event Handler

        /// <summary>
        ///     Event handler if Diagram Title has changed.
        ///     We will update windows caption.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiagramTitleChanged(object sender, EventArgs e)
        {
            UpdateWindowFrameCaption();
        }

        #endregion
    }
}
