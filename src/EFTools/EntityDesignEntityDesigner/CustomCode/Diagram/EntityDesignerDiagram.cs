// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomCode.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Util;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;
    using Microsoft.VisualStudio.Modeling.Immutability;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using EntityDesignerResources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;
    using ModelAssociation = Microsoft.Data.Entity.Design.Model.Entity.Association;
    using ModelDiagram = Microsoft.Data.Entity.Design.Model.Designer.Diagram;
    using ViewModelEntityType = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityType;
    using ViewModelNavigationProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.NavigationProperty;
    using ViewModelProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Property;
    using ViewModelPropertyBase = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.PropertyBase;

    partial class EntityDesignerDiagram : IViewDiagram
    {
        private const int undefinedZoomLevel = -1;
        internal const int IMPLICIT_AUTO_LAYOUT_CEILING = 1000;

        [NonSerialized]
        internal AutoArrangeHelper Arranger = new AutoArrangeHelper();

        private bool _displayNameAndType;
        private DeferredRequest _deferredInitialSelection;
        private bool _disableFixUpDiagramSelection;
        private EntitiesClipboardFormat _clipboardObjects;

        // indicates the action to perform when a watermark link is clicked.  "View Toolbox" is not here as it is indicated via a tool window GUID in the LinkData  
        private enum LinkAction
        {
            Unknown,
            XmlEditor,
            Upgrade,
            ShowModelBrowser
        };

        internal event EventHandler OnDiagramTitleChanged;

        private readonly EmphasizedShapes _emphasizedShapes = new EmphasizedShapes();

        internal bool DisableFixUpDiagramSelection
        {
            get { return _disableFixUpDiagramSelection; }
            set { _disableFixUpDiagramSelection = value; }
        }

        /// <summary>
        ///     Updates the selection during FixUpDiagram.  If _disableFixUpDiagramSelection is false, the behavior is to select
        ///     the newChildShape on the active diagram view if there is one, or on all
        ///     views if there is no active view.
        /// </summary>
        /// <param name="newChildShape">The new child shape that is added by FixUpDiagram</param>
        public override IList FixUpDiagramSelection(ShapeElement newChildShape)
        {
            if (_disableFixUpDiagramSelection)
            {
                return new ArrayList(0);
            }
            else
            {
                return base.FixUpDiagramSelection(newChildShape);
            }
        }

        /// <summary>
        ///     When diagram is finished initialized, it will automatically select a shape that is closest to origin (coordinate 0,0).
        ///     This property allows the client to specify which diagram item to select.
        /// </summary>
        internal DiagramItemCollection InitialDiagramItemSelection { get; set; }

        #region IViewDiagram interface

        public void AddOrShowEFElementInDiagram(EFElement efElement)
        {
            var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
            if (modelDiagram.IsEFObjectRepresentedInDiagram(efElement))
            {
                /// Navigate to the "most-appropriate" DSL node for the given EFObject in the diagram.
                DSLDesignerNavigationHelper.NavigateToDSLNodeInDiagram(this, efElement);
            }
            else
            {
                var efElementList = new List<EFElement>();
                efElementList.Add(efElement);
                var cpc = new CommandProcessorContext(
                    ModelElement.EditingContext,
                    EfiTransactionOriginator.EntityDesignerOriginatorId,
                    EntityDesignerResources.Tx_DropItems);
                CommandProcessor.InvokeSingleCommand(cpc, new CreateDiagramItemForEFElementsCommand(efElementList, modelDiagram, false));
            }
            EnsureSelectionVisible();
        }

        #endregion

        #region Drag & Drop support

        /// <summary>
        ///     The event handler when objects are dropped to the designer.
        ///     If clipboard object is not null, we will go through the objects in the clipboard object and create appropriate Escher objects.
        /// </summary>
        /// <param name="e"></param>
        public override void OnDragDrop(DiagramDragEventArgs e)
        {
            try
            {
                Arranger.Start(e.IsDropLocationUserSpecified ? e.MousePosition : PointD.Empty);
                if (_clipboardObjects != null)
                {
                    var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
                    var cpc = new CommandProcessorContext(
                        ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_DropItems);
                    CommandProcessor.InvokeSingleCommand(
                        cpc, 
                        new CreateDiagramItemForEFElementsCommand(
                            GetEFElementNotInDiagramFromClipboardObject(_clipboardObjects),
                            modelDiagram, e.Shift || e.Control));
                }
                else
                {
                    base.OnDragDrop(e);
                }
            }
            finally
            {
                _clipboardObjects = null;
                Arranger.End();
                if (e.IsDropLocationUserSpecified == false)
                {
                    EnsureSelectionVisible();
                }
            }
        }

        /// <summary>
        ///     The even handler when objects are dragged over the designer.
        /// </summary>
        /// <param name="e"></param>
        public override void OnDragEnter(DiagramDragEventArgs e)
        {
            // Depending on type of objects are being dragged, display the appropriate icons.
            _clipboardObjects = GetClipboardObjectFromDragEventArgs(e);
            if (_clipboardObjects != null)
            {
                e.Effect = GetDropEffects();
                e.Handled = true;
            }
            else
            {
                base.OnDragEnter(e);
            }
        }

        /// <summary>
        ///     OnDragLeave ensure that the _clipboardObject is cleared.
        /// </summary>
        /// <param name="e"></param>
        public override void OnDragLeave(DiagramPointEventArgs e)
        {
            _clipboardObjects = null;
            base.OnDragLeave(e);
        }

        /// <summary>
        ///     Handler when EFObjects are dragged over designer.
        ///     Set the appropriate drag effect.
        /// </summary>
        /// <param name="e"></param>
        public override void OnDragOver(DiagramDragEventArgs e)
        {
            e.Effect = GetDropEffects();
            if (e.Effect != DragDropEffects.None)
            {
                e.Handled = true;
            }
            else
            {
                base.OnDragOver(e);
            }
        }

        /// <summary>
        ///     Return DragDropEffects.Move if there are EFObjects should be represented in diagram but is not.
        /// </summary>
        /// <returns></returns>
        private DragDropEffects GetDropEffects()
        {
            if (_clipboardObjects != null)
            {
                if (GetEFElementNotInDiagramFromClipboardObject(_clipboardObjects).Any())
                {
                    return DragDropEffects.Move;
                }
            }
            return DragDropEffects.None;
        }

        /// <summary>
        ///     Retrieves the instance of EntitiesClipboardFormat from ClipboardData object.
        ///     return null if none exists.
        /// </summary>
        private static EntitiesClipboardFormat GetClipboardObjectFromDragEventArgs(DiagramDragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(EntitiesClipboardFormat).Name, false))
            {
                return e.Data.GetData(typeof(EntitiesClipboardFormat).Name, false) as EntitiesClipboardFormat;
            }
            return null;
        }

        /// <summary>
        ///     Loop through Clipboard objects and determine whether there are EFObject that should be in the diagram but are not.
        /// </summary>
        /// <param name="clipboardObjects"></param>
        /// <returns></returns>
        private IList<EFElement> GetEFElementNotInDiagramFromClipboardObject(EntitiesClipboardFormat clipboardObjects)
        {
            var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
            var artifactSet = modelDiagram.Artifact.ArtifactSet;

            IList<EFElement> efElements = new List<EFElement>();

            // We are only interested in entity-types all the associations will be automatically created when entity-types are added to diagram.
            foreach (var clipboardObject in clipboardObjects.ClipboardEntities)
            {
                var efElement = artifactSet.LookupSymbol(clipboardObject.NormalizedName);
                if (efElement != null
                    && modelDiagram.IsEFObjectRepresentedInDiagram(efElement) == false)
                {
                    efElements.Add(efElement);
                }
            }

            return efElements;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_deferredInitialSelection != null)
                    {
                        _deferredInitialSelection.Dispose();
                        _deferredInitialSelection = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public bool DisplayNameAndType
        {
            get { return _displayNameAndType; }
            set
            {
                _displayNameAndType = value;
                Invalidate(true);
                if (!ModelUtils.IsSerializing(Store))
                {
                    PersistDisplayType();
                }
            }
        }

        public new EntityDesignerViewModel ModelElement
        {
            get { return base.ModelElement as EntityDesignerViewModel; }
            set { base.ModelElement = value; }
        }

        /// <summary>
        ///     Returns the model associated with this diagram
        /// </summary>
        internal EntityDesignerViewModel GetModel()
        {
            return ModelElement;
        }

        /// <summary>
        ///     Returns true if this is an empty diagram, false otherwise
        /// </summary>
        /// <param name="diagram"></param>
        /// <returns></returns>
        internal static bool IsEmptyDiagram(EntityDesignerDiagram diagram)
        {
            return (diagram != null) && (diagram.NestedChildShapes.Count == 0);
        }

        protected override void InitializeInstanceResources()
        {
            base.InitializeInstanceResources();
            ShowGrid = false;
            SnapToGrid = true;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            // select the shape that is closest to the origin, but defer until after the view is fully rendered
            if (_deferredInitialSelection != null)
            {
                _deferredInitialSelection.Dispose();
            }

            _deferredInitialSelection = new DeferredRequest(SetInitialSelectionCallback);
            _deferredInitialSelection.Request();
        }

        private void SetInitialSelectionCallback(object o)
        {
            if (ActiveDiagramView != null)
            {
                if (InitialDiagramItemSelection == null)
                {
                    // seed our "hypotenuse" with the length from upper-left to the bottom-right corner of the canvas rectangle
                    // (we don't need to Sqrt on the result since we are just comparing them, not interested in actual length)
                    var clientRectangle = ActiveDiagramView.ClientRectangle;
                    double hyp = ((clientRectangle.Width * clientRectangle.Width) + (clientRectangle.Height * clientRectangle.Height));

                    // find the entity closest to the origin
                    EntityTypeShape upperLeft = null;
                    foreach (var shape in NestedChildShapes)
                    {
                        var entityShape = shape as EntityTypeShape;
                        if (entityShape != null)
                        {
                            // calculate the distance from 0,0 to the shape's upper left corner
                            // (we don't need to Sqrt on the result since we are just comparing them, not interested in actual length)
                            var entityBounds = entityShape.AbsoluteBounds;
                            var entityHyp = ((entityBounds.X * entityBounds.X) + (entityBounds.Y * entityBounds.Y));

                            // if this is closer to the origin, remember it
                            if (entityHyp < hyp)
                            {
                                upperLeft = entityShape;
                                hyp = entityHyp;
                            }
                        }
                    }

                    // if we found one, select it and move it into view
                    if (upperLeft != null)
                    {
                        ActiveDiagramView.Focus();
                        var selectMe = new DiagramItem(upperLeft);
                        ActiveDiagramView.Selection.Set(selectMe);
                        EnsureSelectionVisible();
                    }
                }
                else
                {
                    ActiveDiagramView.Focus();
                    ActiveDiagramView.Selection.Set(InitialDiagramItemSelection);
                    EnsureSelectionVisible();
                    InitialDiagramItemSelection = null;
                }
            }
        }

        // disable the default auto placement, since this is handled
        // by the AutoArrangeHelper class.
        public override bool ShouldAutoPlaceChildShapes
        {
            get { return false; }
        }

        /// <summary>
        ///     This is called before a transaction has committed,
        ///     This tests to see if the transaction was a drag & drop, and if
        ///     so the new elements can be auto-arranged before the transaction has finished.
        ///     See Also: ShapeAddedToDiagramRule
        /// </summary>
        public override void OnTransactionCommitting(TransactionCommitEventArgs e)
        {
            base.OnTransactionCommitting(e);

            Arranger.TransactionCommit(this);
        }

        internal void EnsureSelectionVisible()
        {
            if (ActiveDiagramView != null
                && ActiveDiagramView.DiagramClientView != null)
            {
                // ActiveDiagramView.Selection.EnsureVisible() method does not work correctly for straight Connector shapes, so use this instead
                ActiveDiagramView.DiagramClientView.EnsureVisible(ActiveDiagramView.Selection.BoundingBox);
            }
        }

        #region Expand/Collapse

        /// <summary>
        ///     Collapse the passed in entity type shape.
        /// </summary>
        /// <param name="entityTypeShape">Entity type shape to be collapsed.</param>
        internal void CollapseEntityTypeShape(EntityTypeShape entityTypeShape)
        {
            SetEntityShapesExpanded(new[] { entityTypeShape }, false);
        }

        /// <summary>
        ///     Collapse all entity type shapes in the diagram.
        /// </summary>
        internal void CollapseAllEntityTypeShapes()
        {
            SetEntityShapesExpanded(NestedChildShapes, false);
        }

        /// <summary>
        ///     Expand the passed in entity type shape.
        /// </summary>
        /// <param name="entityTypeShape"></param>
        internal void ExpandEntityTypeShape(EntityTypeShape entityTypeShape)
        {
            SetEntityShapesExpanded(new[] { entityTypeShape }, true);
        }

        /// <summary>
        ///     Expand all entity type shapes in the diagram.
        /// </summary>
        internal void ExpandAllEntityTypeShapes()
        {
            SetEntityShapesExpanded(NestedChildShapes, true);
        }

        /// <summary>
        ///     Helper function to expand or collapse entity type shape.
        /// </summary>
        /// <param name="shapeElements">Shape elements collection.</param>
        /// <param name="isExpanded">a flag that indicates whether the shape to be expanded or not.</param>
        private void SetEntityShapesExpanded(IList<ShapeElement> shapeElements, bool isExpanded)
        {
            var entityShapeChanged = false;
            // Put up an hourglass because this may take a while
            using (new VsUtils.HourglassHelper())
            {
                using (var t = Store.TransactionManager.BeginTransaction(EntityDesignerResources.Tx_SetEntityTypeIsExpandedProperty))
                {
                    t.Context.Add(EfiTransactionOriginator.TransactionOriginatorDiagramId, DiagramId);

                    foreach (var shape in shapeElements)
                    {
                        var entityShape = shape as EntityTypeShape;
                        if (entityShape != null)
                        {
                            if (isExpanded != entityShape.IsExpanded)
                            {
                                entityShape.IsExpanded = isExpanded;
                                entityShapeChanged = true;
                            }
                        }
                    }
                    // only commit if there is a change.
                    if (entityShapeChanged)
                    {
                        t.Commit();
                    }
                }
            }
        }

        #endregion

        #region Watermark

        public override string WatermarkText
        {
            get
            {
                if (GetModel() != null
                    && GetModel().EditingContext != null
                    && GetModel().EditingContext.GetEFArtifactService() != null)
                {
                    var artifact = GetModel().EditingContext.GetEFArtifactService().Artifact as VSArtifact;
                    if (artifact != null)
                    {
                        var project = VSHelpers.GetProjectForDocument(artifact.Uri.LocalPath, PackageManager.Package);
                        Debug.Assert(project != null);

                        if (!VsUtils.EntityFrameworkSupportedInProject(
                            project, Services.ServiceProvider, allowMiscProject: true))
                        {
                            return string.Format(
                                CultureInfo.CurrentCulture,
                                EntityDesignerResources.DesignerWatermark_EDMNotSupported,
                                EntityDesignerResources.DesignerWatermarkXmlEditorLink).Replace(@"\n", "\n");
                        }

                        if (!artifact.IsStructurallySafe)
                        {
                            return string.Format(
                                CultureInfo.CurrentCulture,
                                EntityDesignerResources.DesignerWatermarkSafeModeErrorText,
                                EntityDesignerResources.DesignerWatermarkXmlEditorLink).Replace(@"\n", "\n");
                        }
                        // if the schema version in the document is not valid for the current target framework.
                        if (!artifact.IsVersionSafe)
                        {
                            return string.Format(
                                CultureInfo.CurrentCulture,
                                EntityDesignerResources.DesignerWatermarkUpgradeErrorText,
                                EntityDesignerResources.DesignerWatermarkXmlEditorLink,
                                EntityDesignerResources.DesignerWatermarkUpgradeLink).Replace(@"\n", "\n");
                        }
                    }
                }

                return string.Format(
                    CultureInfo.CurrentCulture,
                    EntityDesignerResources.DesignerWatermarkText,
                    EntityDesignerResources.DesignerWatermarkToolboxLink,
                    EntityDesignerResources.DesignerWatermarkModelBrowserLink).Replace(@"\n", "\n");
            }
        }

        protected override void OnAssociated(DiagramAssociationEventArgs e)
        {
            base.OnAssociated(e);

            // Initialize and add LinkLabels to diagram watermark
            if (e.DiagramView != null)
            {
                // OnAssociated gets called when the document gets loaded as well as reloaded. This means that
                // we need to reset the watermark before we refresh the link labels associated with them. 
                // Resetting the watermark does the appropriate steps to check if the artifact is designer safe, etc.
                // to determine what watermark to display.
                if (e.DiagramView.Watermark != null)
                {
                    ResetWatermark(e.DiagramView);
                }

                Debug.Assert(e.DiagramView.Selection != null, "Why DiagramView's Selection property is null?");
                if (e.DiagramView.Selection != null)
                {
                    e.DiagramView.Selection.ShapeSelectionChanged += diagramView_OnShapeSelectionChanged;
                }
            }
        }

        /// <summary>
        ///     Unsubscribe to the shape selection changed event when view is about to be disposed.
        /// </summary>
        protected override void OnDisassociated(DiagramAssociationEventArgs e)
        {
            if (e.DiagramView != null
                && e.DiagramView.Selection != null)
            {
                e.DiagramView.Selection.ShapeSelectionChanged -= diagramView_OnShapeSelectionChanged;
            }
            base.OnDisassociated(e);
        }

        /// <summary>
        ///     Event Handler when selections in the Diagram are changed.
        ///     When the selection changes in diagram, we build the list of the diagram items that will be emphasized and we ask the diagram item's shapes to be redrawn.
        ///     When the shape is redrawn, each shape determines whether it is in the list; if yes, it will draw the emphasis shape.
        /// </summary>
        private void diagramView_OnShapeSelectionChanged(object sender, EventArgs e)
        {
            if (ActiveDiagramView != null
                && ActiveDiagramView.Selection != null)
            {
                var shapesToBeEmphasized = new DiagramItemCollection();
                var selectedShapes = ActiveDiagramView.Selection;

                // For each DiagramItem in the Selection
                // - Get the corresponding model element.
                // - See if model element implement IContainRelatedElementsToEmphasizeWhenSelected.
                // - Get related model elements.
                // - For each related model elements, instantiates DiagramItem for the ModelElement.
                foreach (DiagramItem diagramItem in selectedShapes)
                {
                    IContainRelatedElementsToEmphasizeWhenSelected relatedModelElementToEmphasize = null;
                    if (diagramItem.Shape != null
                        && diagramItem.Shape.ModelElement != null)
                    {
                        relatedModelElementToEmphasize = diagramItem.Shape.ModelElement as IContainRelatedElementsToEmphasizeWhenSelected;
                    }
                    else
                    {
                        // DiagramItem.Shape.ModelElement is null when the selected item is not a ShapeElement(for example: Property/NavigationProperty).
                        // Fortunately, we can retrieve the information from RepresentedElements property.
                        relatedModelElementToEmphasize =
                            diagramItem.RepresentedElements.OfType<ModelElement>().FirstOrDefault() as
                            IContainRelatedElementsToEmphasizeWhenSelected;
                    }

                    if (relatedModelElementToEmphasize != null)
                    {
                        // For each ModelElement get the corresponding diagram item.
                        foreach (var emphasizedModelElement in relatedModelElementToEmphasize.RelatedElementsToEmphasizeOnSelected)
                        {
                            DiagramItem emphasizedDiagramItem = null;

                            // if ModelElement is a Property, we could not just instantiate a DiagramItem and pass in the property's PresentationElement to the constructor
                            // since property's PresentationElement is not a ShapeElement.
                            var propertyBase = emphasizedModelElement as ViewModelPropertyBase;
                            if (propertyBase != null)
                            {
                                ViewModelEntityType et = null;
                                var navigationProperty = propertyBase as ViewModelNavigationProperty;
                                var property = propertyBase as ViewModelProperty;
                                if (navigationProperty != null)
                                {
                                    et = navigationProperty.EntityType;
                                }
                                else if (property != null)
                                {
                                    et = property.EntityType;
                                }
                                else
                                {
                                    Debug.Fail("Unexpected property type. Type name:" + propertyBase.GetType().Name);
                                }

                                Debug.Assert(et != null, "Could not get EntityType for property: " + propertyBase.Name);
                                if (et != null)
                                {
                                    Debug.Assert(
                                        PresentationViewsSubject.GetPresentation(et).Count() <= 1,
                                        "There should be at most 1 EntityTypeShape for EntityType:" + et.Name);
                                    var ets = PresentationViewsSubject.GetPresentation(et).FirstOrDefault() as EntityTypeShape;
                                    emphasizedDiagramItem = ets.GetDiagramItemForProperty(propertyBase);
                                }
                            }
                            else
                            {
                                var relatedPresentationElementToEmphasize =
                                    PresentationViewsSubject.GetPresentation(emphasizedModelElement).FirstOrDefault();
                                if (relatedPresentationElementToEmphasize != null)
                                {
                                    var relatedShapeElementToEmphasize = relatedPresentationElementToEmphasize as ShapeElement;

                                    if (relatedShapeElementToEmphasize != null)
                                    {
                                        emphasizedDiagramItem = new DiagramItem(relatedShapeElementToEmphasize);
                                    }
                                }
                            }

                            // Only add if the DiagramItem hasn't been added to the list and the diagram item is not a member of selected diagram item list.
                            if (emphasizedDiagramItem != null
                                && shapesToBeEmphasized.Contains(emphasizedDiagramItem) == false
                                && selectedShapes.Contains(emphasizedDiagramItem) == false)
                            {
                                shapesToBeEmphasized.Add(emphasizedDiagramItem);
                            }
                        }
                    }
                }
                EmphasizedShapes.Set(shapesToBeEmphasized);
            }
        }

        internal void ResetWatermark(DiagramView diagramView)
        {
            // The way the DSL code is implemented, we need to set HasWatermark to false and then to true to get the watermark text to change.
            // if we are in the middle of creating diagram, ActiveDiagramView will be null. 
            if (diagramView != null)
            {
                diagramView.HasWatermark = false;
                diagramView.HasWatermark = true;

                RefreshWatermarkLinks(diagramView);
            }
        }

        internal void RefreshWatermarkLinks(DiagramView diagramView)
        {
            diagramView.Watermark.Links.Clear();
            AddToolboxWatermarkLink(diagramView);
            AddModelBrowserWatermarkLink(diagramView);
            AddXmlEditorWatermarkLink(diagramView);
            AddUpgradeDocumentWatermarkLink(diagramView);

            // ensure the colors for the watermark LinkLabel are correct by VS UX
            VSHelpers.AssignLinkLabelColor(diagramView.Watermark);

            diagramView.Watermark.LinkClicked += diagramWatermark_LinkClicked;
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "link")]
        private static void AddToolboxWatermarkLink(DiagramView diagramView)
        {
            var link = AddLink(diagramView, EntityDesignerResources.DesignerWatermarkToolboxLink, StandardToolWindows.Toolbox);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "link")]
        private static void AddXmlEditorWatermarkLink(DiagramView diagramView)
        {
            var link = AddLink(diagramView, EntityDesignerResources.DesignerWatermarkXmlEditorLink, LinkAction.XmlEditor);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "link")]
        private static void AddUpgradeDocumentWatermarkLink(DiagramView diagramView)
        {
            var link = AddLink(diagramView, EntityDesignerResources.DesignerWatermarkUpgradeLink, LinkAction.Upgrade);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "link")]
        private static void AddModelBrowserWatermarkLink(DiagramView diagramView)
        {
            var link = AddLink(diagramView, EntityDesignerResources.DesignerWatermarkModelBrowserLink, LinkAction.ShowModelBrowser);
        }

        private static LinkLabel.Link AddLink(DiagramView diagramView, string linkText, object linkData)
        {
            LinkLabel.Link link = null;
            diagramView.Watermark.ForeColor = SystemColors.WindowText;
            var diagramWatermark = diagramView.Watermark;

            var waterMarkBegin = diagramWatermark.Text.IndexOf(linkText, StringComparison.OrdinalIgnoreCase);

            if (waterMarkBegin > 0)
            {
                var waterMarkLength = linkText.Length;
                link = new LinkLabel.Link(waterMarkBegin, waterMarkLength);
                link.Name = linkText;
                link.LinkData = linkData;

                // Note that even if the link has the same bounds and link data, the standard Contains()
                // operation will return FALSE based on the hashcode. Thus we need to add a key. The following
                // debug-time check will verify that if there is a link associated with the key, it has the same
                // bounds as the incoming link; i.e. we can't have two 'Toolbox' links in a watermark.
#if DEBUG
                if (diagramWatermark.Links.ContainsKey(linkText))
                {
                    var linkAlreadyInWatermark = diagramWatermark.Links[linkText];
                    Debug.Assert(
                        linkAlreadyInWatermark != null,
                        "Attempted to do debug-time verification of link '" + linkText
                        + "' in watermark but couldn't find the link even though there is a key for it");
                    if (linkAlreadyInWatermark != null)
                    {
                        // Verify that the bounds and link data are the same
                        Debug.Assert(
                            linkAlreadyInWatermark.Start == link.Start && linkAlreadyInWatermark.Length == link.Length,
                            "We found a link in the watermark associated with '" + linkText
                            + "' but it has different bounds than the one we are trying to add. This is not allowed.");
                        Debug.Assert(
                            linkAlreadyInWatermark.LinkData == link.LinkData,
                            "We found a link in the watermark associated with '" + linkText
                            + "' but it has different link data than the one we are trying to add. This is not allowed.");
                    }
                }
#endif
                if (false == diagramWatermark.Links.ContainsKey(linkText))
                {
                    diagramWatermark.Links.Add(link);
                }
            }

            return link;
        }

        /// <summary>
        ///     Event invoked when user clicks on a link in the diagram watermark
        ///     We show the appropriate tool window
        /// </summary>
        /// <param name="sender">LinkLabel</param>
        /// <param name="e">LinkLabelLinkClickedEventArgs</param>
        private void diagramWatermark_ToolboxLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = sender as LinkLabel;
            if (linkLabel != null)
            {
                if (e.Link.LinkData is Guid)
                {
                    var toolWindow = (Guid)e.Link.LinkData;
                    if (toolWindow != Guid.Empty)
                    {
                        var uiService = Services.ServiceProvider.GetService(typeof(IUIService)) as IUIService;
                        if (uiService != null)
                        {
                            uiService.ShowToolWindow(toolWindow);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Event invoked when user clicks on a link in the diagram watermark
        ///     We show the appropriate tool window
        /// </summary>
        /// <param name="sender">LinkLabel</param>
        /// <param name="e">LinkLabelLinkClickedEventArgs</param>
        private void diagramWatermark_OpenInXmlEditorLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = sender as LinkLabel;
            if (linkLabel != null)
            {
                var a = GetModel().EditingContext.GetEFArtifactService().Artifact;

                IServiceProvider sp = PackageManager.Package;
                if (sp != null)
                {
                    // Open the referenced document using our editor.
                    IVsWindowFrame frame;
                    IVsUIHierarchy hierarchy;
                    uint itemid;
                    VsShellUtilities.OpenDocumentWithSpecificEditor(
                        sp, a.Uri.LocalPath, CommonPackageConstants.xmlEditorGuid, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemid,
                        out frame);
                    if (frame != null)
                    {
                        NativeMethods.ThrowOnFailure(frame.Show());
                    }
                }
            }
        }

        private void diagramWatermark_UpgradeLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Display hourglass since the operation may take some time especially for a large model.
            using (new VsUtils.HourglassHelper())
            {
                var editingContext = GetModel().EditingContext;
                var entityDesignArtifact = editingContext.GetEFArtifactService().Artifact as EntityDesignArtifact;
                Debug.Assert(entityDesignArtifact != null, "EFArtifact is not an instance of EntityDesignArtifact");
                if (entityDesignArtifact != null)
                {
                    var targetSchemaVersion = EdmUtils.GetEntityFrameworkVersion(
                        VSHelpers.GetProjectForDocument(entityDesignArtifact.Uri.LocalPath, PackageManager.Package),
                        PackageManager.Package, useLatestIfNoEF: false);

                    ReversionModel(PackageManager.Package, editingContext, entityDesignArtifact, targetSchemaVersion);
                }
            }
        }

        //internal for testing
        internal static void ReversionModel(
            IEdmPackage package, EditingContext editingContext, EntityDesignArtifact entityDesignArtifact, Version targetSchemaVersion)
        {
            var cpc = new CommandProcessorContext(
                editingContext, EfiTransactionOriginator.EntityDesignerOriginatorId,
                EntityDesignerResources.RetargetDocumentFromWatermarkTransactionName, entityDesignArtifact);

            RetargetXmlNamespaceCommand.RetargetArtifactXmlNamespaces(cpc, entityDesignArtifact, targetSchemaVersion);

            // The code below ensure that our mapping window works property after the command is executed.
            Debug.Assert(package.DocumentFrameMgr != null, "Could not find the DocumentFrameMgr for this package");
            if (package.DocumentFrameMgr != null)
            {
                package.DocumentFrameMgr.SetCurrentContext(editingContext);
            }
        }

        private void diagramWatermark_ShowModelBrowserLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Debug.Assert(PackageManager.Package != null, "Entity Designer Package is null.");
            if (PackageManager.Package != null)
            {
                Debug.Assert(PackageManager.Package.ExplorerWindow != null, "Unable to get instance of Model Browser window from package.");
                if (PackageManager.Package.ExplorerWindow != null)
                {
                    PackageManager.Package.ExplorerWindow.Show();
                }
            }
        }

        private void diagramWatermark_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.LinkData is LinkAction)
            {
                var linkAction = LinkAction.Unknown;
                linkAction = (LinkAction)e.Link.LinkData;
                if (linkAction == LinkAction.XmlEditor)
                {
                    diagramWatermark_OpenInXmlEditorLinkClicked(sender, e);
                }
                else if (linkAction == LinkAction.Upgrade)
                {
                    diagramWatermark_UpgradeLinkClicked(sender, e);
                }
                else if (linkAction == LinkAction.ShowModelBrowser)
                {
                    diagramWatermark_ShowModelBrowserLinkClicked(sender, e);
                }
                else
                {
                    Debug.Fail("unexpected LinkAction value in LinkData value");
                }
            }
            else if (e.Link.LinkData is Guid)
            {
                diagramWatermark_ToolboxLinkClicked(sender, e);
            }
            else
            {
                Debug.Fail("unexpected data for LinkData");
            }
        }

        #endregion Watermark

        #region Zoom & Layout

        /// <summary>
        ///     Performs an AutoLayoutDiagram command on all child shapes
        /// </summary>
        public void AutoLayoutDiagram()
        {
            AutoLayoutDiagram(NestedChildShapes);
        }

        /// <summary>
        ///     Performs an AutoLayoutDiagram command on the list of shapes passed in
        /// </summary>
        public void AutoLayoutDiagram(IList shapes)
        {
            // Put up an hourglass because this may take a while
            using (new VsUtils.HourglassHelper())
            {
                // Inheritance lines need to be placed using a different styling
                // so that the lines join at the same point. Sort out which shapes we have
                var inheritanceLinks = new List<ShapeElement>();
                var inheritanceShapes = new List<ShapeElement>();
                var otherShapes = new List<ShapeElement>();

                foreach (ShapeElement shape in shapes)
                {
                    // Fix a bug when the user tries to layout a diagram that contains inheritance classes multiple times, the shapes are moved to the bottom of the screen.
                    // The fix is to include both base class and derived class in the inheritance shapes list; before the fix we only includes the derived class in the list.
                    var isInheritanceClass = false;

                    var entityTypeShape = shape as EntityTypeShape;
                    // get a list of all lines leading to/from the shape
                    if (entityTypeShape != null)
                    {
                        var allLinks = new ArrayList(entityTypeShape.FromRoleLinkShapes);
                        allLinks.AddRange(entityTypeShape.ToRoleLinkShapes);

                        foreach (LinkShape link in allLinks)
                        {
                            if (link is InheritanceConnector)
                            {
                                isInheritanceClass = true;
                                break;
                            }
                        }
                    }

                    if (isInheritanceClass)
                    {
                        inheritanceShapes.Add(shape);
                    }
                    else if (shape is InheritanceConnector)
                    {
                        inheritanceLinks.Add(shape);
                    }
                    else
                    {
                        otherShapes.Add(shape);
                    }
                }

                // These flags are based on AutoLayout code from the ClassDesigner
                // Flags we set so the graph object ignores a shape (treats that shape as invisible) when doing layout
                const VGNodeFixedStates ignoreShapeFlags =
                    VGNodeFixedStates.FixedPlace | // don't consider for placement
                    VGNodeFixedStates.PermeablePlace; // place on top if desired (ignore for placement purposes)

                // Flags we set so the graph object recognizes but doesn't move a shape when doing layout
                const VGNodeFixedStates noMoveShapeFlags = VGNodeFixedStates.FixedPlace;

                // Perform the auto layout
                using (var t = Store.TransactionManager.BeginTransaction(EntityDesignerResources.Tx_LayoutDiagram))
                {
                    t.Context.Add(EfiTransactionOriginator.TransactionOriginatorDiagramId, DiagramId);

                    using (new SaveLayoutFlags(inheritanceShapes, ignoreShapeFlags))
                    {
                        // Since the inheritance shapes will be moved later,
                        // tell this layout to ignore them and place other objects on
                        // top if necessary
                        AutoLayoutShapeElements(
                            shapes,
                            VGRoutingStyle.VGRouteNetwork,
                            PlacementValueStyle.VGPlaceWE,
                            false);
                    }

                    using (new SaveLayoutFlags(otherShapes, noMoveShapeFlags))
                    {
                        // DD 40487: Move any classes that have inheritance, while keeping
                        // the others in place. Use org chart and PlaceSN so that parent
                        // classes appear above child ones
                        AutoLayoutShapeElements(
                            shapes,
                            VGRoutingStyle.VGRouteOrgChartNS,
                            PlacementValueStyle.VGPlaceSN,
                            false);
                    }

                    using (new SaveLayoutFlags(shapes, noMoveShapeFlags))
                    {
                        // DD 40516: Make inheritance lines connect at single point and
                        // don't move anything else
                        AutoLayoutShapeElements(
                            inheritanceLinks,
                            VGRoutingStyle.VGRouteRightAngle,
                            PlacementValueStyle.VGPlaceUndirected,
                            false);
                    }

                    Reroute();

                    t.Commit();
                }
            } // restore cursor
        }

        /// <summary>
        ///     This class saves the VGNodeFixedStates of the objects
        ///     passed, and restores them on the disposed. It can also
        ///     be used to change all the flags to the same value.
        /// </summary>
        private sealed class SaveLayoutFlags : IDisposable
        {
            private readonly IList _elements;
            private VGNodeFixedStates[] _savedFlags;

            public SaveLayoutFlags(IList elements)
            {
                _elements = elements;
                Save();
            }

            public SaveLayoutFlags(IList elements, VGNodeFixedStates flags)
                : this(elements)
            {
                SetFlags(flags);
            }

            public void Dispose()
            {
                Restore();
            }

            // Sets all the flags on the elements stored to this value
            public void SetFlags(VGNodeFixedStates flags)
            {
                for (var i = 0; i < _elements.Count; i++)
                {
                    var node = _elements[i] as NodeShape;
                    if (node != null)
                    {
                        node.LayoutObjectFixedFlags = flags;
                    }
                }
            }

            // Keeps a copy of the old values of all these flags
            private void Save()
            {
                _savedFlags = new VGNodeFixedStates[_elements.Count];
                for (var i = 0; i < _elements.Count; i++)
                {
                    var node = _elements[i] as NodeShape;
                    if (node != null)
                    {
                        _savedFlags[i] = node.LayoutObjectFixedFlags;
                    }
                }
            }

            // Restores the old flag values
            private void Restore()
            {
                for (var i = 0; i < _elements.Count; i++)
                {
                    var node = _elements[i] as NodeShape;
                    if (node != null)
                    {
                        node.LayoutObjectFixedFlags = _savedFlags[i];
                    }
                }
            }
        }

        /// <summary>
        ///     Zooms the diagram to fit all the contents
        /// </summary>
        public void ZoomToFit()
        {
            ActiveDiagramView.ZoomToFit();
        }

        /// <summary>
        ///     ZoomIn
        /// </summary>
        public void ZoomIn()
        {
            ActiveDiagramView.ZoomIn();
        }

        /// <summary>
        ///     ZoomOut
        /// </summary>
        public void ZoomOut()
        {
            ActiveDiagramView.ZoomOut();
        }

        /// <summary>
        ///     Gets/Sets the zoom level in percent values
        /// </summary>
        public int ZoomLevel
        {
            get
            {
                if (ActiveDiagramView != null)
                {
                    return (int)(ActiveDiagramView.ZoomFactor * 100);
                }
                else
                {
                    return undefinedZoomLevel;
                }
            }
            set
            {
                if (ActiveDiagramView != null)
                {
                    ActiveDiagramView.ZoomAtViewCenter((float)value / 100);
                }
            }
        }

        #endregion Zoom & Layout

        // Accessibility name of the diagram will be the schema's namespace
        public override string AccessibleName
        {
            get
            {
                if (ModelElement == null)
                {
                    return base.AccessibleName;
                }

                return ModelElement.Namespace;
            }
        }

        public override string AccessibleDescription
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    EntityDesignerResources.AccDesc_EntityDesignerViewModel,
                    ModelElement.GetType().Name);
            }
        }

        internal void AddNewEntityType(PointD dropPoint)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            var model = ModelElement.ModelXRef.GetExisting(ModelElement) as ConceptualEntityModel;
            Debug.Assert(model != null);

            using (var dialog = new NewEntityDialog(model))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Arranger.Start(dropPoint);
                        Store.RuleManager.DisableRule(typeof(EntityType_AddRule));

                        using (var t = Store.TransactionManager.BeginTransaction(EntityDesignerResources.Tx_AddEntityType))
                        {
                            t.Context.Add(EfiTransactionOriginator.TransactionOriginatorDiagramId, DiagramId);
                            ViewModelChangeContext.GetNewOrExistingContext(t).ViewModelChanges.Add(new EntityType_AddFromDialog(dialog));
                            t.Commit();
                        }
                    }
                    finally
                    {
                        Store.RuleManager.EnableRule(typeof(EntityType_AddRule));
                        Arranger.End();
                        EnsureSelectionVisible();
                    }
                }
            }
        }

        /// <summary>
        ///     Creates new association for given EntityTypeShape
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void AddNewAssociation(EntityTypeShape entityShape)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            // If user selected an EntityTypeShape, use corresponding EntityType
            // else default to first EntityType in the model
            ViewModelEntityType end1 = null;
            if (entityShape != null)
            {
                end1 = entityShape.ModelElement as ViewModelEntityType;
            }
            if (end1 == null)
            {
                end1 = ModelElement.EntityTypes[0];
            }

            // Pick something for the second end (defaulting to the next EntityType in the model)
            ViewModelEntityType end2 = null;
            var index = ModelElement.EntityTypes.IndexOf(end1) + 1;
            if (ModelElement.EntityTypes.Count <= index)
            {
                index = 0;
            }
            end2 = ModelElement.EntityTypes[index];

            var modelEnd1Entity = ModelElement.ModelXRef.GetExisting(end1) as Model.Entity.EntityType;
            var modelEnd2Entity = ModelElement.ModelXRef.GetExisting(end2) as Model.Entity.EntityType;
            Debug.Assert(modelEnd1Entity != null && modelEnd2Entity != null);
            var model = modelEnd1Entity.Parent as ConceptualEntityModel;
            Debug.Assert(model != null);

            using (var dialog = new NewAssociationDialog(model.EntityTypes(), modelEnd1Entity, modelEnd2Entity))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Store.RuleManager.DisableRule(typeof(Association_AddRule));
                        using (var t = Store.TransactionManager.BeginTransaction(EntityDesignerResources.Tx_AddAssociation))
                        {
                            t.Context.Add(EfiTransactionOriginator.TransactionOriginatorDiagramId, DiagramId);
                            ViewModelChangeContext.GetNewOrExistingContext(t).ViewModelChanges.Add(new Association_AddFromDialog(dialog));
                            t.Commit();
                        }
                    }
                    finally
                    {
                        Store.RuleManager.EnableRule(typeof(Association_AddRule));
                    }
                }
            }
        }

        /// <summary>
        ///     Creates new inheritance for given EntityTypeShape
        /// </summary>
        /// <param name="entityShape"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        internal void AddNewInheritance(EntityTypeShape entityShape)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            // If user selected an EntityTypeShape, use corresponding EntityType
            // else default to first EntityType in the model
            ViewModelEntityType baseEntity = null;
            ConceptualEntityType modelEntity = null;
            if (entityShape != null)
            {
                baseEntity = entityShape.ModelElement as ViewModelEntityType;
                modelEntity = ModelElement.ModelXRef.GetExisting(baseEntity) as ConceptualEntityType;
                Debug.Assert(modelEntity != null);
            }

            var model = ModelElement.ModelXRef.GetExisting(ModelElement) as ConceptualEntityModel;
            Debug.Assert(model != null);

            var cets = new List<ConceptualEntityType>(model.EntityTypes().Cast<ConceptualEntityType>());

            using (var dialog = new NewInheritanceDialog(modelEntity, cets))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var t = Store.TransactionManager.BeginTransaction(EntityDesignerResources.Tx_AddInheritance))
                    {
                        t.Context.Add(EfiTransactionOriginator.TransactionOriginatorDiagramId, DiagramId);
                        ViewModelChangeContext.GetNewOrExistingContext(t).ViewModelChanges.Add(new Inheritance_AddFromDialog(dialog));
                        t.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a function import; if there is a selected entity type, it will use that for the return type.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void AddNewFunctionImport(EntityTypeShape entityShape)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            ViewModelEntityType baseEntity = null;
            Model.Entity.EntityType modelEntity = null;
            if (entityShape != null)
            {
                baseEntity = entityShape.TypedModelElement;
                Debug.Assert(baseEntity != null);
                modelEntity = ModelElement.ModelXRef.GetExisting(baseEntity) as Model.Entity.EntityType;
                Debug.Assert(modelEntity != null);
            }

            // get the necessary conceptual model elements
            var cModel = ModelElement.ModelXRef.GetExisting(ModelElement) as ConceptualEntityModel;
            Debug.Assert(cModel != null, "Could not find a conceptual entity model associated with this artifact");
            ConceptualEntityContainer cContainer = null;
            if (cModel != null)
            {
                cContainer = cModel.FirstEntityContainer as ConceptualEntityContainer;
                Debug.Assert(cContainer != null, "There is no conceptual entity container in the conceptual entity model");
            }

            // get the necessary storage model elements and mapping model elements
            var artifactService = ModelElement.EditingContext.GetEFArtifactService();
            Debug.Assert(
                artifactService != null && artifactService.Artifact != null,
                "There is no artifact service/artifact associated with this editing context");
            StorageEntityModel sModel = null;
            EntityContainerMapping ecMapping = null;
            if (artifactService != null
                && artifactService.Artifact != null)
            {
                sModel = artifactService.Artifact.StorageModel();
                Debug.Assert(sModel != null, "Could not find a storage entity model associated with this artifact");
                var mModel = artifactService.Artifact.MappingModel();
                Debug.Assert(mModel != null, "Could not find a mapping model associated with this artifact");
                if (mModel != null)
                {
                    ecMapping = mModel.FirstEntityContainerMapping;
                    Debug.Assert(
                        ecMapping != null,
                        "There must be an entity container mapping in the mapping model to create the function import mapping");
                }
            }

            if (cModel != null
                && sModel != null
                && cContainer != null
                && ecMapping != null)
            {
                var schemaVersion = artifactService.Artifact.SchemaVersion;
                if (null == schemaVersion)
                {
                    Debug.Assert(
                        false,
                        typeof(EntityDesignerDiagram).Name + " could not determine Version for path "
                        + artifactService.Artifact.Uri.LocalPath);
                    return;
                }

                EntityDesignViewModelHelper.CreateFunctionImport(
                    ModelElement.EditingContext,
                    artifactService.Artifact,
                    null,
                    sModel,
                    cModel,
                    cContainer,
                    modelEntity,
                    EfiTransactionOriginator.EntityDesignerOriginatorId);
            }
        }

        internal void PersistZoomLevel()
        {
            // make sure that we have ActiveDiagramView to get the current ZoomLevel from
            if (ActiveDiagramView != null
                && !ModelUtils.IsSerializing(Store))
            {
                var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
                Debug.Assert(modelDiagram != null, "Model Diagram should be present");
                if (modelDiagram != null)
                {
                    if (undefinedZoomLevel != ZoomLevel
                        && modelDiagram.ZoomLevel.Value != ZoomLevel)
                    {
                        var cpc = new CommandProcessorContext(
                            ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_SetZoom);
                        var cmd = new UpdateDefaultableValueCommand<int>(modelDiagram.ZoomLevel, ZoomLevel);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        internal void PersistShowGrid()
        {
            var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
            Debug.Assert(modelDiagram != null, "Model Diagram should be present");
            if (modelDiagram != null)
            {
                if (modelDiagram.ShowGrid.Value != ShowGrid)
                {
                    var cpc = new CommandProcessorContext(
                        ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_SetGridVisibility);
                    var cmd = new UpdateDefaultableValueCommand<bool>(modelDiagram.ShowGrid, ShowGrid);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal void PersistSnapToGrid()
        {
            var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
            Debug.Assert(modelDiagram != null, "Model Diagram should be present");
            if (modelDiagram != null)
            {
                if (modelDiagram.SnapToGrid.Value != SnapToGrid)
                {
                    var cpc = new CommandProcessorContext(
                        ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_SetSnapToGrid);
                    var cmd = new UpdateDefaultableValueCommand<bool>(modelDiagram.SnapToGrid, SnapToGrid);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal void PersistDisplayType()
        {
            var modelDiagram = ModelElement.ModelXRef.GetExisting(this) as ModelDiagram;
            Debug.Assert(modelDiagram != null, "Model Diagram should be present");
            if (modelDiagram != null)
            {
                if (modelDiagram.DisplayType.Value != DisplayNameAndType)
                {
                    var cpc = new CommandProcessorContext(
                        ModelElement.EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_SetMemberFormatValue);
                    var cmd = new UpdateDefaultableValueCommand<bool>(modelDiagram.DisplayType, DisplayNameAndType);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public override DiagramSelectionRules SelectionRules
        {
            get
            {
                return new EntityDesignerDiagramSelectionRules(this);
            }
        }

        public class EntityDesignerDiagramSelectionRules : DiagramSelectionRules
        {
            private readonly EntityDesignerDiagram _diagram;

            public EntityDesignerDiagramSelectionRules(EntityDesignerDiagram diagram)
            {
                _diagram = diagram;
            }

            /// <summary>
            ///     Called by the design surface to allow selection filtering
            /// </summary>
            /// <param name="currentSelection">[in] The current selection before any ShapeElements are added or removed.</param>
            /// <param name="proposedItemsToAdd">[in/out] The proposed DiagramItems to be added to the selection.</param>
            /// <param name="proposedItemsToRemove">[in/out] The proposed DiagramItems to be removed from the selection.</param>
            /// <param name="primaryItem">
            ///     [in/out] The proposed DiagramItem to become the primary DiagramItem of the selection.
            ///     A null value signifies that the last DiagramItem in the resultant selection should be assumed as the
            ///     primary DiagramItem.
            /// </param>
            /// <returns>
            ///     true if some or all of the selection was accepted; false if the entire selection proposal
            ///     was rejected. If false, appropriate feedback will be given to the user to indicate that the
            ///     selection was rejected.
            /// </returns>
            public override bool GetCompliantSelection(
                SelectedShapesCollection currentSelection, DiagramItemCollection proposedItemsToAdd,
                DiagramItemCollection proposedItemsToRemove, DiagramItem primaryItem)
            {
                base.GetCompliantSelection(currentSelection, proposedItemsToAdd, proposedItemsToRemove, primaryItem);

                var originalProposedItemsToAdd = new DiagramItem[proposedItemsToAdd.Count];
                proposedItemsToAdd.CopyTo(originalProposedItemsToAdd, 0);

                // we only perform this with selection rectangles, in which case the focused item will be the diagram 
                // and the user clicks on the "Properties" or "Navigation Properties" compartment on an entity
                if (currentSelection.FocusedItem != null
                    && currentSelection.FocusedItem.Shape == _diagram)
                {
                    foreach (var item in originalProposedItemsToAdd)
                    {
                        if (item.Shape != null
                            && item.Shape is ElementListCompartment)
                        {
                            // only perform this if we have selected the "Scalar Properties" ListCompartment or
                            // the "Navigation Properties" ListCompartment
                            var elListCompartment = item.Shape as ElementListCompartment;
                            if (elListCompartment.DefaultCreationDomainClass.Id == ViewModelProperty.DomainClassId
                                || elListCompartment.DefaultCreationDomainClass.Id == ViewModelNavigationProperty.DomainClassId)
                            {
                                // we don't perform this if a Property or NavigationProperty was selected
                                var representedShapeElements = item.RepresentedElements.OfType<ShapeElement>();
                                if (representedShapeElements.Count() == 1
                                    && representedShapeElements.Contains(item.Shape))
                                {
                                    // find the parent EntityTypeShape that houses this ListCompartment
                                    var entityTypeShape = elListCompartment.ParentShape as EntityTypeShape;
                                    Debug.Assert(
                                        entityTypeShape != null, "Why isn't the parent of the list compartment an EntityTypeShape?");
                                    var entityTypeShapeDiagramItem = new DiagramItem(entityTypeShape);

                                    // add the parent EntityTypeShape if it doesn't already exist in the collection
                                    if (!currentSelection.Contains(entityTypeShapeDiagramItem)
                                        && proposedItemsToAdd.Contains(entityTypeShapeDiagramItem) == false)
                                    {
                                        proposedItemsToAdd.Add(entityTypeShapeDiagramItem);
                                    }

                                    proposedItemsToAdd.Remove(item);
                                }
                            }
                        }
                    }
                }

                // The code below is responsible to enforce the following diagram items selection rule. (see Multiple Diagram spec).
                // - if an entity-type-shape is selected, no diagram-item can be selected in the diagram.
                // - if diagram-item that is not an entity-type-shape is selected, no entity-type-shape can be selected.
                var firstDiagramItem = FirstSelectionItem(currentSelection, proposedItemsToAdd, proposedItemsToRemove);
                if (firstDiagramItem != null
                    && proposedItemsToAdd.Count > 0)
                {
                    var isFirstDiagramItemEntityTypeShape = firstDiagramItem.Shape is EntityTypeShape;

                    // For Multi selection rules, we can only select EntityTypeShapes or else but not both.
                    foreach (var item in proposedItemsToAdd.ToList())
                    {
                        if (isFirstDiagramItemEntityTypeShape && ((item.Shape is EntityTypeShape) == false))
                        {
                            RemoveSelectedDiagramItem(item, currentSelection, proposedItemsToAdd, proposedItemsToRemove);
                        }
                        else if (isFirstDiagramItemEntityTypeShape == false
                                 && (item.Shape is EntityTypeShape))
                        {
                            RemoveSelectedDiagramItem(item, currentSelection, proposedItemsToAdd, proposedItemsToRemove);
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        ///     Helper method to cancel a diagram item to be selected.
        /// </summary>
        /// <param name="diagramItem"></param>
        /// <param name="currentSelection"></param>
        /// <param name="proposedItemsToAdd"></param>
        /// <param name="proposedItemsToRemove"></param>
        private static void RemoveSelectedDiagramItem(
            DiagramItem diagramItem, SelectedShapesCollection currentSelection, DiagramItemCollection proposedItemsToAdd,
            DiagramItemCollection proposedItemsToRemove)
        {
            if (currentSelection.Contains(diagramItem) == false
                && proposedItemsToAdd.Contains(diagramItem))
            {
                proposedItemsToAdd.Remove(diagramItem);
            }
            if (currentSelection.Contains(diagramItem)
                && proposedItemsToRemove.Contains(diagramItem) == false)
            {
                proposedItemsToRemove.Add(diagramItem);
            }
        }

        /// <summary>
        ///     Returns the first selection item in the provided selection.
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <param name="proposedShapesToAdd"></param>
        /// <param name="proposedShapesToRemove"></param>
        /// <returns></returns>
        private static DiagramItem FirstSelectionItem(
            SelectedShapesCollection currentSelection, DiagramItemCollection proposedShapesToAdd,
            DiagramItemCollection proposedShapesToRemove)
        {
            // Temporary list that stores what the selections will look like.
            var actualSelection = new DiagramItemCollection();

            // Add current selection items to the list.
            foreach (DiagramItem item in currentSelection)
            {
                if (item != null
                    && actualSelection.Contains(item) == false)
                {
                    actualSelection.Add(item);
                }
            }

            // Include additional diagram items that will be selected.
            foreach (var item in proposedShapesToAdd)
            {
                if (item != null
                    && actualSelection.Contains(item) == false)
                {
                    actualSelection.Add(item);
                }
            }

            // Remove diagram items that will be unselected.
            foreach (var item in proposedShapesToRemove)
            {
                if (item != null
                    && actualSelection.Contains(item))
                {
                    actualSelection.Remove(item);
                }
            }

            // Return the first item in the list.
            if (actualSelection.Count > 0)
            {
                return actualSelection[0];
            }
            return null;
        }

        /// <summary>
        ///     Asks the user if they would like to also delete any
        ///     storage EntitySets which will be unmapped. If the answer
        ///     is Yes, also has the side-effect of adding the list of
        ///     unmapped sets to DeleteUnmappedStorageEntitySetsProperty
        ///     in the view-model's Store PropertyBag
        /// </summary>
        /// <param name="selectedModelElements">C-side objects to be deleted</param>
        /// <returns>
        ///     Result of asking the user, or No if never asked;
        ///     possible results are Yes, No, or Cancel (No means just delete the  C-side
        ///     objects, Yes means also delete the S-side objects that will become unmapped,
        ///     Cancel means don't delete anything
        /// </returns>
        internal DialogResult ShouldDeleteUnmappedStorageEntitySets(List<EFElement> selectedModelElements)
        {
            // if there are no selected elements then return 'No'
            if (null == selectedModelElements
                || 0 >= selectedModelElements.Count)
            {
                return DialogResult.No;
            }

            // find which S-side EntitySets would be deleted
            var unmappedStorageEntitySets =
                DeleteUnmappedStorageEntitySetsCommand.UnmappedStorageEntitySetsIfDelete(selectedModelElements);

            // only offer user choice if there are any S-side EntitySets to delete
            if (0 >= unmappedStorageEntitySets.Count)
            {
                return DialogResult.No;
            }

            // show dialog giving user the choice to (a) delete only the 
            // C-side objects selected, (b) to also delete any StorageEntitySets
            // which will end up unmapped, or (c) cancel the whole operation
            using (var dialog =
                new DeleteStorageEntitySetsDialog(unmappedStorageEntitySets))
            {
                var result = dialog.ShowDialog();
                if (DialogResult.Yes == result)
                {
                    // user decided to also delete the unmapped StorageEntitySets;
                    // the property below is checked in the EntityDesignerViewModel when committing the changes
                    var viewModel = GetModel();
                    Debug.Assert(null != viewModel, "viewModel should not be null");
                    if (null != viewModel)
                    {
                        List<ICollection<StorageEntitySet>> unmappedMasterList = null;
                        if (viewModel.Store.PropertyBag.ContainsKey(EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty))
                        {
                            unmappedMasterList =
                                viewModel.Store.PropertyBag[EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty] as
                                List<ICollection<StorageEntitySet>>;
                        }

                        if (unmappedMasterList == null)
                        {
                            unmappedMasterList = new List<ICollection<StorageEntitySet>>();
                            viewModel.Store.PropertyBag[EntityDesignerViewModel.DeleteUnmappedStorageEntitySetsProperty] =
                                unmappedMasterList;
                        }

                        unmappedMasterList.Add(unmappedStorageEntitySets);
                    }
                }

                return result;
            }
        }

        internal void AddMissingEntityTypeShapes(EntityDesignArtifact efArtifact, out bool addedMissingShapes)
        {
            addedMissingShapes = false;
            Debug.Assert(efArtifact != null, "EFArtifact is null");
            if (efArtifact != null)
            {
                // Now, for all entitytypes, ensure that an entitytype shape is created.
                var dslDiagram = Diagram as EntityDesignerDiagram;
                if (efArtifact.ConceptualModel() != null
                    && dslDiagram != null)
                {
                    var entityTypesMaterializedAsShapes = new HashSet<Model.Entity.EntityType>();
                    foreach (
                        var designerEntityTypeShape in
                            efArtifact.DesignerInfo.Diagrams.Items.Where(d => d.Id.Value == DiagramId).SelectMany(d => d.EntityTypeShapes))
                    {
                        // First check if there's a ModelElement for this.
                        var entityTypeShape = dslDiagram.ModelElement.ModelXRef.GetExisting(designerEntityTypeShape) as EntityTypeShape;
                        if (entityTypeShape != null
                            && designerEntityTypeShape.EntityType.Target != null
                            && !entityTypesMaterializedAsShapes.Contains(designerEntityTypeShape.EntityType.Target))
                        {
                            entityTypesMaterializedAsShapes.Add(designerEntityTypeShape.EntityType.Target);
                        }
                    }

                    var cpc = new CommandProcessorContext(
                        efArtifact.EditingContext,
                        EfiTransactionOriginator.EntityDesignerOriginatorId,
                        "Restore Excluded Elements");
                    var cp = new CommandProcessor(cpc, shouldNotifyObservers: true);

                    foreach (var entityType in efArtifact.ConceptualModel().EntityTypes())
                    {
                        if (!entityTypesMaterializedAsShapes.Contains(entityType))
                        {
                            var modelDiagram = dslDiagram.ModelElement.ModelXRef.GetExisting(dslDiagram) as ModelDiagram;
                            if (modelDiagram != null)
                            {
                                cp.EnqueueCommand(new CreateEntityTypeShapeCommand(modelDiagram, entityType));
                            }
                        }
                    }

                    if (cp.CommandCount > 0)
                    {
                        cp.Invoke();
                        addedMissingShapes = true;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void AddMissingAssociationConnectors(EntityDesignArtifact efArtifact, out bool addedMissingShapes)
        {
            addedMissingShapes = false;
            Debug.Assert(efArtifact != null, "EFArtifact is null");
            if (efArtifact != null)
            {
                // Now, for all associations, ensure that an association connector is created.
                var dslDiagram = Diagram as EntityDesignerDiagram;
                if (efArtifact.ConceptualModel() != null
                    && dslDiagram != null)
                {
                    var associationsMaterializedAsConnectors = new HashSet<ModelAssociation>();
                    foreach (
                        var connector in
                            efArtifact.DesignerInfo.Diagrams.Items.Where(d => d.Id.Value == DiagramId)
                                .SelectMany(d => d.AssociationConnectors))
                    {
                        // First check if there's a ModelElement for this.
                        var associationConnector = dslDiagram.ModelElement.ModelXRef.GetExisting(connector) as AssociationConnector;
                        if (associationConnector != null
                            && connector.Association.Target != null
                            && !associationsMaterializedAsConnectors.Contains(connector.Association.Target))
                        {
                            associationsMaterializedAsConnectors.Add(connector.Association.Target);
                        }
                    }

                    var cpc = new CommandProcessorContext(
                        efArtifact.EditingContext,
                        EfiTransactionOriginator.EntityDesignerOriginatorId,
                        "Restore Excluded Elements");
                    var cp = new CommandProcessor(cpc, shouldNotifyObservers: true);

                    foreach (var association in efArtifact.ConceptualModel().Associations())
                    {
                        if (!associationsMaterializedAsConnectors.Contains(association))
                        {
                            var shouldAddConnector = true;
                            var entityTypesOnEnds = association.AssociationEnds().Select(ae => ae.Type.Target);
                            foreach (var entityType in entityTypesOnEnds)
                            {
                                if (entityType != null
                                    && entityType.GetAntiDependenciesOfType<Model.Designer.EntityTypeShape>()
                                           .Any(ets => ets.Diagram.Id == DiagramId) == false)
                                {
                                    shouldAddConnector = false;
                                    break;
                                }
                            }

                            if (shouldAddConnector)
                            {
                                var modelDiagram = dslDiagram.ModelElement.ModelXRef.GetExisting(dslDiagram) as ModelDiagram;
                                if (modelDiagram != null)
                                {
                                    cp.EnqueueCommand(new CreateAssociationConnectorCommand(modelDiagram, association));
                                }
                            }
                        }
                    }

                    if (cp.CommandCount > 0)
                    {
                        cp.Invoke();
                        addedMissingShapes = true;
                    }
                }
            }
        }

        public new string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;
                    if (OnDiagramTitleChanged != null)
                    {
                        OnDiagramTitleChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        ///     Contains the list of related shapes which emphasis shapes will be drawn around them.
        /// </summary>
        public EmphasizedShapes EmphasizedShapes
        {
            get { return _emphasizedShapes; }
        }

        protected override void InitializeResources(StyleSet classStyleSet)
        {
            base.InitializeResources(classStyleSet);
            // Set themable colors for all instances.
            SetColorTheme(classStyleSet);
            // Subscribe to theme changed event to update themable colors for all instances if necessary.
            VSColorTheme.ThemeChanged += e => SetColorTheme(classStyleSet);
        }

        private static void SetColorTheme(StyleSet styleSet)
        {
            // TODO: Without this some test in ViewModel.Tests.csproj are failing because apparently they are non runing in VS. We should fix the tests and shouldn't need this anymore. 
            if (!IsThemeServiceAvailable())
            {
                return;
            }
            // Override brush settings for this diagram background.
            styleSet.OverrideBrushColor(
                DiagramBrushes.DiagramBackground, VSColorTheme.GetThemedColor(EnvironmentColors.DesignerBackgroundColorKey));
            // Override lasso color for thumbnail view.
            styleSet.OverridePenColor(DiagramPens.ZoomLasso, VSColorTheme.GetThemedColor(EnvironmentColors.ClassDesignerLassoColorKey));
            // Notify EntityTypeShape that changes will require updating themable colors on next painting.
            EntityTypeShape.IsColorThemeSet = false;
            // Notify AssociationConnector that changes will require updating themable colors on next painting.
            AssociationConnector.IsColorThemeSet = false;
        }

        /// <summary>
        ///     Check whether theme services are available, i.e. we are running inside VS
        /// </summary>
        private static bool IsThemeServiceAvailable()
        {
            return null != Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell5;
        }
    }
}
