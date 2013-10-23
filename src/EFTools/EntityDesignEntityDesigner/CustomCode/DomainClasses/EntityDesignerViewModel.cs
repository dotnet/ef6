// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;
using ModelDesigner = Microsoft.Data.Entity.Design.Model.Designer;
using ModelDiagram = Microsoft.Data.Tools.Model.Diagram;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Resources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class EntityDesignerViewModel : IDisposable
    {
        internal static int EntityShapeLocationSeed = 0;
        internal static bool RespondToModelChanges = true;

        //this Guid will be stored in Transaction that should be rolled back
        //public static readonly Guid TransactionCancelledGuid = new Guid("E6A95EB6-2E6D-4fc9-91C3-36C164F13545");
        internal static readonly string InternalTransactionIdPrefix = "EntityDesignerViewModel_";
        public static readonly string DeleteUnmappedStorageEntitySetsProperty = "DeleteUnmappedStorageEntitySets";

        private EditingContext _editingContext;
        private bool _reloading;
        private bool _loggedFatalError;
        private bool _shouldClearAndReloadDiagram;
        private bool _isDisposed;

        internal bool Reloading
        {
            get { return _reloading; }
        }

        /// <summary>
        ///     Returns true if the model has elements, false otherwise.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal bool ModelHasElements()
        {
            return (EntityTypes.Count > 0);
        }

        /// <summary>
        ///     Returns the diagram associated with this model
        /// </summary>
        /// <param name="diagram"></param>
        /// <returns></returns>
        internal EntityDesignerDiagram GetDiagram()
        {
            EntityDesignerDiagram diagram = null;

            foreach (ShapeElement shape in PresentationViewsSubject.GetPresentation(this))
            {
                if (shape.Diagram != null)
                {
                    diagram = shape.Diagram as EntityDesignerDiagram;
                    break;
                }
            }

            return diagram;
        }

        /// <summary>
        ///     The diagram id association with this DSL Model.
        ///     The assumption is that there will be only 1 diagram per view model.
        /// </summary>
        internal string DiagramId
        {
            get
            {
                var diagram = GetDiagram();
                Debug.Assert(diagram != null, "Diagram is null");
                if (diagram != null)
                {
                    return diagram.DiagramId;
                }
                return string.Empty;
            }
        }

        internal EditingContext EditingContext
        {
            get { return _editingContext; }
            set
            {
                if (_editingContext != value)
                {
                    SetContext(value);
                }
            }
        }

        internal ModelToDesignerModelXRefItem ModelXRef
        {
            get
            {
                if (_editingContext == null)
                {
                    return null;
                }
                else
                {
                    return ModelToDesignerModelXRef.GetModelToDesignerModelXRefItem(_editingContext, Partition);
                }
            }
        }

        private HashSet<EFArtifact> CurrentArtifactsInView
        {
            get
            {
                var artifacts = new HashSet<EFArtifact>();
                var service = EditingContext.GetEFArtifactService();
                var entityDesignArtifact = service.Artifact as EntityDesignArtifact;

                Debug.Assert(
                    entityDesignArtifact != null,
                    "The active artifact is not type of: " + typeof(EntityDesignArtifact) + ", Actual:" + service.Artifact.GetType());
                if (entityDesignArtifact != null)
                {
                    artifacts.Add(entityDesignArtifact);

                    if (entityDesignArtifact.DiagramArtifact != null)
                    {
                        artifacts.Add(entityDesignArtifact.DiagramArtifact);
                    }
                }
                return artifacts;
            }
        }

        internal EFObject SelectedEFObject
        {
            get
            {
                var dslWindowPane = Services.IMonitorSelectionService.CurrentSelectionContainer as ModelingWindowPane;
                if (dslWindowPane != null)
                {
                    var modelElement = dslWindowPane.PrimarySelection as ModelElement;
                    if (modelElement == null)
                    {
                        var presentationElement = dslWindowPane.PrimarySelection as PresentationElement;
                        modelElement = presentationElement.ModelElement;
                    }
                    return ModelXRef.GetExisting(modelElement);
                }
                return null;
            }
        }

        private void SetContext(EditingContext context)
        {
            // unregister from old context
            UnregisterEventDelegates();

            // register to new context
            _editingContext = context;
            RegisterEventDelegates();
        }

        private void UnregisterEventDelegates()
        {
            if (_editingContext != null)
            {
                _editingContext.Disposing -= OnContextDisposing;
                _editingContext.Reloaded -= OnContextReloaded;
            }
            PackageManager.Package.ModelManager.ModelChangesCommitted -= OnModelChangesCommitted;
        }

        private void RegisterEventDelegates()
        {
            if (_editingContext != null)
            {
                PackageManager.Package.ModelManager.ModelChangesCommitted += OnModelChangesCommitted;
                _editingContext.Disposing += OnContextDisposing;
                _editingContext.Reloaded += OnContextReloaded;
            }
        }

        private void OnContextDisposing(object sender, EventArgs e)
        {
            var context = (EditingContext)sender;
            Debug.Assert(_editingContext == context);
            SetContext(null);
        }

        private void OnContextReloaded(object sender, EventArgs e)
        {
            ClearAndReloadDiagram();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ClearAndReloadDiagram()
        {
            var diagram = GetDiagram();
            try
            {
                _reloading = true;

                if (diagram != null
                    && diagram.ActiveDiagramView != null)
                {
                    diagram.ActiveDiagramView.HasWatermark = false;
                }
                MicrosoftDataEntityDesignSerializationHelper.ClearModel(this);
                MicrosoftDataEntityDesignSerializationHelper.ClearDiagram(this);
                MicrosoftDataEntityDesignSerializationHelper.Instance.ReloadModel(this);
                MicrosoftDataEntityDesignSerializationHelper.ReloadDiagram(this);
            }
            catch (Exception e)
            {
                var artifact = EditingContextManager.GetArtifact(EditingContext);

                if (artifact != null
                    && !_loggedFatalError)
                {
                    VsUtils.LogStandardError(
                        string.Format(CultureInfo.CurrentCulture, Resources.Error_DiagramShouldBeReloaded, e.Message),
                        artifact.Uri.LocalPath, 0, 0);

                    _loggedFatalError = true;
                }
            }
            finally
            {
                _reloading = false;
                if (diagram != null
                    && diagram.ActiveDiagramView != null)
                {
                    diagram.ActiveDiagramView.HasWatermark = true;
                }
            }
        }

        /// <summary>
        ///     This is our EventHandler which modifies the diagram view whenever the underlying Model changes.
        ///     Any undo/redo changes will be processed through this event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        {
            if (e.ChangeGroup.Transaction != null
                && e.ChangeGroup.Transaction.OriginatorId == EfiTransactionOriginator.MappingDetailsOriginatorId)
            {
                // if its an edit from the mapping designer, there is nothing to process
                return;
            }

            if (RespondToModelChanges == false)
            {
                return;
            }

            ProcessModelChanges(e.ChangeGroup);

            if (e.ChangeGroup.Transaction != null
                && e.ChangeGroup.Transaction.OriginatorId == EfiTransactionOriginator.XmlEditorOriginatorId)
            {
                // if this transaction came from the Xml Editor, then the user is typing and
                // we are going to be creating and deleting lots of items, so just try and 
                // layout the shapes
                try
                {
                    _reloading = true;
                    MicrosoftDataEntityDesignSerializationHelper.ReloadDiagram(this);
                }
                catch (Exception)
                {
                    // no-op, at some point, they might have changed the file so much that we can't
                    // lay things out anymore
                }
                finally
                {
                    _reloading = false;
                }
            }
        }

        /// <summary>
        ///     This method iterates through model changes and update the corresponding DSL model and presentation elements.
        ///     The method consist of 2 steps:
        ///     1. The first step makes sure that DSL model elements are created and in sync with our Escher model.
        ///     2. The second step is to apply layout information from Diagram-Model to DSL PEL.
        ///     We also update Escher and DSL model Xref information.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ProcessModelChanges(EfiChangeGroup changeGroup)
        {
            EntityDesignerDiagram diagram = null;
            diagram = GetDiagram();
            var disableFixUpDiagramSelection = false;

            // If diagram is null it might have been deleted, in this case just return.
            if (diagram == null)
            {
                return;
            }

            // Store the value of the disableFixUpDiagramSelection.
            disableFixUpDiagramSelection = diagram.DisableFixUpDiagramSelection;
            try
            {
                //
                // This turns off layout during "serialization" and fixes bad perf during update-model scenarios.
                // We set this to true here, and then set it false in the finally block because there were some problems with 
                // the DSL fix not drawing self-associations correctly when this is set to true all the time. 
                //
                if (changeGroup.Transaction != null
                    && (changeGroup.Transaction.OriginatorId == EfiTransactionOriginator.UpdateModelFromDatabaseId ||
                        changeGroup.Transaction.OriginatorId == EfiTransactionOriginator.UndoRedoOriginatorId))
                {
                    Store.PropertyBag["WorkaroundFixSerializationTransaction"] = true;
                }
                    // If coming from property window, we don't want the selection to change.
                else if (changeGroup.Transaction.OriginatorId == EfiTransactionOriginator.PropertyWindowOriginatorId
                         && diagram != null)
                {
                    diagram.DisableFixUpDiagramSelection = true;
                }

                // sort changes and discover the artifact that has changed
                var extraElementsToProcess = new HashSet<EFObject>();
                var changes = changeGroup.SortChangesForProcessing(new ChangeComparer());

                var artifacts = CurrentArtifactsInView;

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // FIRST STEP: updating DSL Model Elements.
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // tx name is not shown anywhere since we don't use the DSL undo manager
                using (var t = Store.TransactionManager.BeginTransaction(
                        InternalTransactionIdPrefix + changeGroup.Transaction.OriginatorId, true))
                {
                    // start auto-arranger for arranging new elements
                    // don't do this for undo/redo because we will translate the new diagram EFObject information into the DSL ShapeElements
                    // don't do this for update model because we want to set up an entirely new transaction for auto-layout with the new shapes
                    if (changeGroup.Transaction.OriginatorId != EfiTransactionOriginator.UndoRedoOriginatorId
                        && changeGroup.Transaction.OriginatorId != EfiTransactionOriginator.UpdateModelFromDatabaseId)
                    {
                        if (diagram != null)
                        {
                            diagram.Arranger.Start(PointD.Empty);
                        }
                    }

                    var hasChangesForCurrentArtifactInView = false;

                    foreach (var change in changes)
                    {
                        if (artifacts.Contains(change.Changed.Artifact))
                        {
                            // only process changes if the artifact is designer-safe.  
                            if (change.Changed.Artifact.IsDesignerSafe)
                            {
                                hasChangesForCurrentArtifactInView = true;

                                if (change.Changed is ModelDiagram.BaseDiagramObject)
                                {
                                    ProcessSingleDiagramModelChange(change, extraElementsToProcess);
                                }
                                else if (ProcessSingleModelChange(change))
                                {
                                    break;
                                }
                            }
                        }
                    }

                    foreach (var efobject in extraElementsToProcess)
                    {
                        var et = efobject as ConceptualEntityType;
                        if (et != null)
                        {
                            foreach (var p in et.Properties())
                            {
                                OnEFObjectCreatedOrUpdated(p, ModelXRef);
                            }
                            foreach (var np in et.NavigationProperties())
                            {
                                OnEFObjectCreatedOrUpdated(np, ModelXRef);
                            }

                            if (et.BaseType.Target != null)
                            {
                                OnEFObjectCreatedOrUpdated(et.BaseType, ModelXRef);
                            }
                        }
                        else
                        {
                            Debug.Fail("Unexpected type of model node in extraElementsToProcess");
                        }
                    }

                    if (hasChangesForCurrentArtifactInView & t.HasPendingChanges)
                    {
                        t.Commit();
                    }

                    if (changeGroup.Transaction.OriginatorId != EfiTransactionOriginator.UndoRedoOriginatorId
                        && changeGroup.Transaction.OriginatorId != EfiTransactionOriginator.UpdateModelFromDatabaseId)
                    {
                        Debug.Assert(diagram != null, "Should have discovered the diagram if autoArrange == true");
                        if (diagram != null)
                        {
                            diagram.Arranger.End();
                        }
                    }

                    if (ModelHasElements() == false)
                    {
                        var edd = GetDiagram();
                        if (edd != null)
                        {
                            edd.ResetWatermark(edd.ActiveDiagramView);
                        }
                    }
                }

                // we need to keep track of the entity shapes that needed to be auto layout.
                var shapesToAutoLayout = new List<ShapeElement>();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // SECOND STEP: updating DSL Presentation Elements.
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // Given the changegroup and any transaction that originates from the model, see if there were any diagram EFObjects were changed. 
                // Go through each one, find the corresponding ShapeElement (DSL presentation element) that was created, and add it to the view model xref.
                if (changeGroup.Transaction != null)
                {
                    using (var t = Store.TransactionManager.BeginTransaction(
                            InternalTransactionIdPrefix + changeGroup.Transaction.OriginatorId, true))
                    {
                        var hasDiagramObjectChanges = false;
                        var moveDiagram2Shapes =
                            (changeGroup.Transaction.OriginatorId == EfiTransactionOriginator.UndoRedoOriginatorId ||
                             changeGroup.Transaction.OriginatorId == EfiTransactionOriginator.UpdateModelFromDatabaseId);

                        foreach (var change in changes)
                        {
                            if (artifacts.Contains(change.Changed.Artifact))
                            {
                                if (change.Changed.Artifact.IsDesignerSafe)
                                {
                                    if (change.Changed is DefaultableValue
                                        && change.Changed.Parent is ModelDesigner.Diagram
                                        && ModelXRef.ContainsKey(change.Changed.Parent))
                                    {
                                        hasDiagramObjectChanges = true;
                                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                            .SynchronizeSingleDslModelElement(this, change.Changed.Parent);
                                    }
                                    else
                                    {
                                        // this will set the ShapeElements to their original position as well as add them to the XRef
                                        ModelDiagram.BaseDiagramObject modelDiagramObject = null;
                                        var currentNode = change.Changed;
                                        while (currentNode != null)
                                        {
                                            var baseDiagram = currentNode as ModelDiagram.BaseDiagramObject;
                                            if (baseDiagram != null)
                                            {
                                                modelDiagramObject = baseDiagram;
                                                break;
                                            }
                                            currentNode = currentNode.Parent;
                                        }

                                        // Call TranslateDiagramObject to sync Escher Designer Model element and DSL PEL (Presentation Element).
                                        // We need to call this even when the change type is 'delete' because it might affect DSL PEL.
                                        // (For example: when you change entity type shape's FillColor to a default color, the FillColor attribute will be deleted since it is not needed,
                                        // in that case, we still need to update DSL PEL).
                                        if (modelDiagramObject != null
                                            && modelDiagramObject.Diagram != null
                                            && modelDiagramObject.Diagram.Id == DiagramId)
                                        {
                                            hasDiagramObjectChanges = true;
                                            EntityModelToDslModelTranslatorStrategy.TranslateDiagramObject(
                                                this, modelDiagramObject, moveDiagram2Shapes, shapesToAutoLayout);
                                        }
                                    }
                                }
                            }
                        }

                        if (hasDiagramObjectChanges && t.HasPendingChanges)
                        {
                            t.Commit();
                        }
                    }
                }

                if (changeGroup.Transaction != null
                    && shapesToAutoLayout.Count > 0)
                {
                    diagram.AutoLayoutDiagram(shapesToAutoLayout);
                }
            }
            finally
            {
                Store.PropertyBag["WorkaroundFixSerializationTransaction"] = false;
                if (diagram != null)
                {
                    diagram.DisableFixUpDiagramSelection = disableFixUpDiagramSelection;
                }
            }

            if (_shouldClearAndReloadDiagram)
            {
                try
                {
                    ClearAndReloadDiagram();
                }
                finally
                {
                    _shouldClearAndReloadDiagram = false;
                }
            }
        }

        /// <summary>
        ///     Return true if processing this change caused a reload of the entire doc, false otherwise.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool ProcessSingleModelChange(EfiChange change)
        {
            var xref = ModelXRef;

            if ((change.Type == EfiChange.EfiChangeType.Create ||
                 change.Type == EfiChange.EfiChangeType.Delete)
                && (change.Changed is EFArtifact ||
                 change.Changed is ConceptualEntityModel))
            {
                // the artifact or model has changed on us, reload the whole thing
                ClearAndReloadDiagram();
                return true;
            }
            else
            {
                try
                {
                    switch (change.Type)
                    {
                        case EfiChange.EfiChangeType.Update:
                            OnEFObjectUpdated(change, xref);
                            break;

                        case EfiChange.EfiChangeType.Create:
                            // Adding association, entity-type or inheritance is no longer translate to the addition of the corresponding DSL model elements.
                            // We only add the DSL model elements if Escher model diagrams are added (InheritanceConnector etc.)
                            if (!(change.Changed is Model.Entity.Association
                                  || change.Changed is ConceptualEntityType
                                  || change.Changed is EntityTypeBaseType))
                            {
                                OnEFObjectUpdated(change, xref);
                            }
                            break;

                        case EfiChange.EfiChangeType.Delete:
                            OnEFObjectDeleted(change.Changed, xref);
                            break;

                        default:
                            Debug.Assert(false, "Unknown change type.");
                            break;
                    }

                    // If Entity-type's base type has changed, invalidate the shape so the base type name in the shape can be updated.
                    var baseType = change.Changed as EntityTypeBaseType;
                    if (baseType != null)
                    {
                        Debug.Assert(baseType.Parent != null, "EntityTypeBaseType's Parent should not be null.");
                        if (baseType.Parent != null)
                        {
                            var entityType = ModelXRef.GetExisting(baseType.Parent) as EntityType;
                            // the entity type is null if it is not in the diagram.
                            if (entityType != null)
                            {
                                var ets = PresentationViewsSubject.GetPresentation(entityType).FirstOrDefault() as EntityTypeShape;
                                Debug.Assert(ets != null, "The shape for entity-type : " + entityType.Name + " is not available.");
                                if (ets != null)
                                {
                                    ets.Invalidate();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail("Exception caught while processing changes to the designer", e.Message);
                    ClearAndReloadDiagram();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Project the Diagram Model changes to DSL Model Elements.
        ///     Note that this is only to ensure that DSL Model Elements are in sync with the diagram model.
        /// </summary>
        private void ProcessSingleDiagramModelChange(EfiChange change, HashSet<EFObject> extraElementsToProcess)
        {
            var diagramObject = change.Changed as ModelDiagram.BaseDiagramObject;
            var xref = ModelXRef;

            // Ignore if the change is not model diagram object or diagram object does not below to this view model diagram.
            if (diagramObject == null
                || diagramObject.Diagram == null
                || diagramObject.Diagram.Id != DiagramId)
            {
                return;
            }

            if (change.Type == EfiChange.EfiChangeType.Create)
            {
                EFObject efObject = null;

                var entityTypeShape = change.Changed as ModelDesigner.EntityTypeShape;
                var inheritanceConnector = change.Changed as ModelDesigner.InheritanceConnector;
                var associationConnector = change.Changed as ModelDesigner.AssociationConnector;

                if (entityTypeShape != null)
                {
                    // There is a code above that will Debug.Fail if the object in extraElementsToProcess is not ConceptualEntityType
                    Model.Entity.EntityType et = entityTypeShape.EntityType.Target as ConceptualEntityType;
                    if (et != null)
                    {
                        extraElementsToProcess.Add(et);
                        efObject = et;
                    }
                }
                else if (inheritanceConnector != null)
                {
                    var et = inheritanceConnector.EntityType.Target as ConceptualEntityType;
                    if (et != null)
                    {
                        efObject = et.BaseType;
                    }
                }
                else if (associationConnector != null)
                {
                    efObject = associationConnector.Association.Target;
                }

                if (efObject != null)
                {
                    OnEFObjectCreatedOrUpdated(efObject, xref);
                }
            }
            else if (change.Type == EfiChange.EfiChangeType.Delete)
            {
                var entityTypeShape = change.Changed as ModelDesigner.EntityTypeShape;
                var inheritanceConnector = change.Changed as ModelDesigner.InheritanceConnector;
                var associationConnector = change.Changed as ModelDesigner.AssociationConnector;

                EFObject efObject = null;
                if (entityTypeShape != null)
                {
                    efObject = entityTypeShape.EntityType.Target;
                }
                else if (inheritanceConnector != null)
                {
                    var et = inheritanceConnector.EntityType.Target as ConceptualEntityType;
                    if (et != null)
                    {
                        efObject = et.BaseType;
                    }
                }
                else if (associationConnector != null)
                {
                    efObject = associationConnector.Association.Target;
                }

                if (efObject != null)
                {
                    OnEFObjectDeleted(efObject, xref);
                }
            }
        }

        private class ChangeComparer : EfiChangeComparer
        {
            protected override int GetVal(EfiChange change)
            {
                // we always order deletes before creates before updates.
                if (change.Type == EfiChange.EfiChangeType.Update)
                {
                    return 100;
                }
                else if (change.Type == EfiChange.EfiChangeType.Delete)
                {
                    return -1;
                }
                else
                {
                    Debug.Assert(change.Type == EfiChange.EfiChangeType.Create);

                    var entityType = change.Changed as Model.Entity.EntityType;
                    if (entityType != null)
                    {
                        return 1;
                    }

                    var association = change.Changed as Model.Entity.Association;
                    if (association != null)
                    {
                        return 2;
                    }

                    var prop = change.Changed as Model.Entity.Property;
                    if (prop != null)
                    {
                        return 3;
                    }

                    var propertyRef = change.Changed as PropertyRef;
                    if (propertyRef != null)
                    {
                        return 4;
                    }

                    var key = change.Changed as Key;
                    if (key != null)
                    {
                        return 5;
                    }

                    var ets = change.Changed as ModelDesigner.EntityTypeShape;
                    if (ets != null)
                    {
                        return 6;
                    }

                    var ac = change.Changed as ModelDesigner.AssociationConnector;
                    if (ac != null)
                    {
                        return 7;
                    }

                    var ic = change.Changed as ModelDesigner.InheritanceConnector;
                    if (ic != null)
                    {
                        return 8;
                    }

                    return 9;
                }
            }
        }

        /// <summary>
        ///     This is called whenever there is a change that originates from the the DSL surface.  The
        ///     changes are packaged up and the ModelController is used to make the changes to the model.
        /// </summary>
        /// <param name="e"></param>
        internal void OnTransactionCommited(TransactionCommitEventArgs e)
        {
            if (_reloading)
            {
                return;
            }

            var changeContext = ViewModelChangeContext.GetExistingContext(e.Transaction);
            if (changeContext != null
                && changeContext.ViewModelChanges.Count > 0)
            {
                var context = EditingContext;
                var service = context.GetEFArtifactService();
                var viewModelChanges = changeContext.ViewModelChanges.OfType<ViewModelChange>().ToList();

                Debug.Assert(
                    changeContext.ViewModelChanges.Count == viewModelChanges.Count,
                    "Not all changes from the view model were of type ViewModleChange");

                try
                {
                    SortAndOptimizeViewModelChanges(viewModelChanges);

                    // these changes will now be invoked, which will create the appropriate EFObjects
                    // and push changes down to the XLinq tree
                    ProcessViewModelChanges(e.Transaction.Name, viewModelChanges);
                }
                catch (Exception ex)
                {
                    ClearAndReloadDiagram();
                    if (ex is FileNotEditableException)
                    {
                        service.Artifact.IsDirty = false;
                    }
                    throw;
                }
            }
        }

        private static void SortAndOptimizeViewModelChanges(List<ViewModelChange> viewModelChangeList)
        {
            // Sort the list first
            viewModelChangeList.Sort(new ViewModelChangeComparer());

            // This dictionary is used to quickly find changes based on type. This is important since there could be lots of view model
            // changes in one batch (i.e. select all, delete)
            var changeType2ChangeMap = new Dictionary<Type, List<ViewModelChange>>();

            var changesToRemove = new List<ViewModelChange>();

            foreach (var change in viewModelChangeList)
            {
                // Insert the change into the hashset, keyed by type
                List<ViewModelChange> listOfChangesForChangeType;
                if (false == changeType2ChangeMap.TryGetValue(change.GetType(), out listOfChangesForChangeType))
                {
                    listOfChangesForChangeType = new List<ViewModelChange>();
                    changeType2ChangeMap.Add(change.GetType(), listOfChangesForChangeType);
                }
                listOfChangesForChangeType.Add(change);

                // Rule 1: If we have a delete of an EntityType, find all property deletes belonging to the
                // EntityType earlier and remove them
                var entityTypeDeleteChange = change as EntityTypeDelete;
                if (entityTypeDeleteChange != null)
                {
                    List<ViewModelChange> propertyDeletes;
                    if (changeType2ChangeMap.TryGetValue(typeof(PropertyDelete), out propertyDeletes))
                    {
                        foreach (PropertyDelete propertyDelete in propertyDeletes)
                        {
                            if (propertyDelete.Property != null
                                && propertyDelete.EntityType != null
                                && propertyDelete.EntityType == entityTypeDeleteChange.EntityType)
                            {
                                changesToRemove.Add(propertyDelete);
                            }
                        }
                    }
                }
            }

            foreach (var change in changesToRemove)
            {
                viewModelChangeList.Remove(change);
            }
        }

        /// <summary>
        ///     Invokes the ModelChanges within the ModelChangeContext that is typically wrapped within a
        ///     DSL transaction. This will mutate our model composed of EFObjects and will also push changes
        ///     down to the XLinq tree in the XML Model.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void ProcessViewModelChanges(string name, IList<ViewModelChange> viewModelChanges)
        {
            if (viewModelChanges != null
                && viewModelChanges.Count > 0)
            {
                var context = new EfiTransactionContext();
                var containsDiagramChanges = viewModelChanges.Any(vmc => vmc.IsDiagramChange);
                context.Add(
                    EfiTransactionOriginator.TransactionOriginatorDiagramId,
                    new ModelDesigner.DiagramContextItem(DiagramId, containsDiagramChanges));

                var cpc = new CommandProcessorContext(
                    EditingContext, EfiTransactionOriginator.EntityDesignerOriginatorId, name, null, context);
                var cp = new CommandProcessor(cpc);

                // check if the DeleteUnmappedStorageEntitySetsProperty property is set
                if (Store.PropertyBag.ContainsKey(DeleteUnmappedStorageEntitySetsProperty))
                {
                    // if the property is set then unset it and add a
                    // command to remove these for each item in the master list
                    var unmappedMasterList =
                        Store.PropertyBag[DeleteUnmappedStorageEntitySetsProperty] as List<ICollection<StorageEntitySet>>;
                    Store.PropertyBag.Remove(DeleteUnmappedStorageEntitySetsProperty);

                    if (unmappedMasterList != null)
                    {
                        foreach (var unmappedEntitySets in unmappedMasterList)
                        {
                            var duse = new DeleteUnmappedStorageEntitySetsCommand(unmappedEntitySets);
                            cp.EnqueueCommand(duse);
                        }
                    }
                }

                var setParentUndoUnit = false;
                try
                {
                    if (cpc.EditingContext.ParentUndoUnitStarted == false)
                    {
                        cpc.EditingContext.ParentUndoUnitStarted = true;
                        cpc.Artifact.XmlModelProvider.BeginUndoScope(name);
                        setParentUndoUnit = true;
                    }

                    var cmd = new DelegateCommand(
                        () =>
                            {
                                foreach (var change in viewModelChanges)
                                {
                                    change.Invoke(cpc);
                                }
                            });

                    cp.EnqueueCommand(cmd);
                    cp.Invoke();
                }
                catch (Exception e)
                {
                    if (!(e is CommandValidationFailedException || e is FileNotEditableException))
                    {
                        Debug.Fail("Exception thrown while committing change to Model. " + e.Message);
                    }
                    throw;
                }
                finally
                {
                    if (setParentUndoUnit)
                    {
                        cpc.EditingContext.ParentUndoUnitStarted = false;
                        cpc.Artifact.XmlModelProvider.EndUndoScope();
                    }
                }
            }
        }

        /// <summary>
        ///     Updates corresponding view model item.
        ///     For simplicity each change updates all item values.
        /// </summary>
        private void OnEFObjectUpdated(EfiChange change, ModelToDesignerModelXRefItem xref)
        {
            Debug.Assert(change.Changed != null);
            var efObject = change.Changed;

            // Updating a base type is a special case, because it really means creating or deleting an inheritance connector
            var baseType = efObject as EntityTypeBaseType;
            if (baseType != null)
            {
                if (xref.ContainsKey(baseType.Parent))
                {
                    // always recreate inheritance
                    OnEFObjectDeleted(baseType, xref);
                    if (xref.ContainsKey(baseType.Parent))
                    {
                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext).SynchronizeSingleDslModelElement(this, efObject);
                    }
                }
                return;
            }

            // for AssociationEnd we want to update parent Association
            var associationEnd = efObject as AssociationEnd;
            if (associationEnd != null)
            {
                efObject = associationEnd.Parent;
            }

            // regardless the change always update whole element (it shouldn't be too big overhead)
            OnEFObjectCreatedOrUpdated(efObject, xref);
        }

        /// <summary>
        ///     In general: the method update items if they are already created in the designer.
        ///     For Property element: creates if the entity-type exists.
        ///     For Association element: creates if both types exists.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void OnEFObjectCreatedOrUpdated(EFObject efObject, ModelToDesignerModelXRefItem xref)
        {
            Debug.Assert(efObject != null);

            if (false == ModelHelper.IsInConceptualModel(efObject))
            {
                return; // ignore events from storage model
            }

            // special case for creating base type
            var baseType = efObject as EntityTypeBaseType;
            if (baseType != null)
            {
                if (xref.ContainsKey(baseType.Parent))
                {
                    ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext).SynchronizeSingleDslModelElement(this, efObject);
                }
                return;
            }

            var efElement = efObject as EFElement;
            if (efElement == null)
            {
                efElement = efObject.Parent as EFElement;
            }

            if (efElement != null)
            {
                var modelRoot = efElement as ConceptualEntityModel;
                if (modelRoot != null)
                {
                    Namespace = modelRoot.Namespace.Value;
                    return;
                }

                var modelEntityType = efElement as ConceptualEntityType;
                if (modelEntityType != null)
                {
                    var viewEntityType = xref.GetExisting(modelEntityType) as EntityType;
                    if (viewEntityType != null)
                    {
                        //update only
                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                            .SynchronizeSingleDslModelElement(this, modelEntityType);
                    }
                        // Check to see if EFObject is type of ConceptualEntityType.
                        // This is to prevent that the scenario updating entity-type name causes a new entity-type to be created in different diagram.
                    else if (efObject is ConceptualEntityType)
                    {
                        //create EntityType and add it to the view model
                        viewEntityType =
                            ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                .SynchronizeSingleDslModelElement(this, modelEntityType) as EntityType;
                        EntityTypes.Add(viewEntityType);
                    }

                    return;
                }

                var modelProperty = efElement as Model.Entity.Property;
                if (modelProperty != null)
                {
                    var entityType = modelProperty.Parent as Model.Entity.EntityType;
                    if (entityType != null)
                    {
                        var viewEntityType = xref.GetExisting(entityType) as EntityType;
                        // If the view Entity Type does not exist skip, continue since nothing to update.
                        if (viewEntityType != null)
                        {
                            var viewProperty = xref.GetExisting(modelProperty) as Property;
                            if (viewProperty != null)
                            {
                                //update only
                                ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                    .SynchronizeSingleDslModelElement(viewEntityType, modelProperty);
                            }
                            else
                            {
                                //create property and add it to view's EntityType
                                viewProperty =
                                    ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                        .SynchronizeSingleDslModelElement(viewEntityType, modelProperty) as Property;
                                Debug.Assert(viewProperty != null, "Why DSL property is null?");
                                if (viewProperty != null)
                                {
                                    // Determine where we should insert the new property by getting the previous sibling.
                                    var previousSibling = FindPreviousProperty(viewProperty) as Property;
                                    if (previousSibling != null)
                                    {
                                        // Get the index of the previous sibling.
                                        var propertyIndex = viewEntityType.Properties.IndexOf(previousSibling);
                                        Debug.Assert(
                                            propertyIndex >= 0,
                                            "Unable to find index of DSL property: " + previousSibling.Name + " in entity:"
                                            + viewEntityType.Name);
                                        if (propertyIndex >= 0)
                                        {
                                            viewEntityType.Properties.Insert(propertyIndex + 1, viewProperty);
                                        }
                                    }
                                    else
                                    {
                                        // if previous sibling is null, that means we need to insert the property as the first property of the entity-type.
                                        viewEntityType.Properties.Insert(0, viewProperty);
                                    }
                                }
                            }
                        }
                    }
                    return;
                }

                var modelAssociationEnd = efElement as AssociationEnd;
                if (modelAssociationEnd != null)
                {
                    // change focus to the association and it will be processed below
                    efElement = modelAssociationEnd.Parent as EFElement;
                }

                var modelAssociation = efElement as Model.Entity.Association;
                if (modelAssociation != null)
                {
                    // this will create association if necessary (if not it's update only)
                    ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                        .SynchronizeSingleDslModelElement(this, modelAssociation);
                    return;
                }

                var modelNavigationProperty = efElement as Model.Entity.NavigationProperty;
                if (modelNavigationProperty != null)
                {
                    var entityType = modelNavigationProperty.Parent as Model.Entity.EntityType;
                    Debug.Assert(entityType != null);

                    var viewEntityType = xref.GetExisting(entityType) as EntityType;

                    if (viewEntityType != null)
                    {
                        var viewNavigationProperty = xref.GetExisting(modelNavigationProperty) as NavigationProperty;
                        if (viewNavigationProperty != null)
                        {
                            //update only
                            ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                .SynchronizeSingleDslModelElement(viewEntityType, modelNavigationProperty);
                        }
                        else
                        {
                            //create navigation property and add it to view's EntityType
                            viewNavigationProperty =
                                ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                    .SynchronizeSingleDslModelElement(viewEntityType, modelNavigationProperty) as NavigationProperty;
                            Debug.Assert(viewNavigationProperty != null, "Why DSL navigation property is null?");
                            if (viewNavigationProperty != null)
                            {
                                var previousSibling = FindPreviousProperty(viewNavigationProperty) as NavigationProperty;
                                if (previousSibling != null)
                                {
                                    var propertyIndex = viewEntityType.NavigationProperties.IndexOf(previousSibling);
                                    Debug.Assert(
                                        propertyIndex >= 0,
                                        "Unable to find index of DSL navigation property: " + previousSibling.Name + " in entity:"
                                        + viewEntityType.Name);
                                    if (propertyIndex >= 0)
                                    {
                                        viewEntityType.NavigationProperties.Insert(propertyIndex + 1, viewNavigationProperty);
                                    }
                                }
                                else
                                {
                                    // if previous sibling is null, that means we need to insert the property as the first navigation property of the entity-type.
                                    viewEntityType.NavigationProperties.Insert(0, viewNavigationProperty);
                                }
                            }
                        }
                    }
                    return;
                }

                // see if we are creating the Key element or just a PropertyRef
                var key = efElement as Key;
                var propertyRefs = new List<PropertyRef>();

                if (key != null)
                {
                    // if this is true, we are not necessarily creating the Key element with only one property ref element - 
                    // we may be handling an undo operation, and there may be more than one property ref element.
                    foreach (var propRef in key.PropertyRefs)
                    {
                        propertyRefs.Add(propRef);
                    }
                }
                else
                {
                    var propertyRef = efElement as PropertyRef;
                    if (propertyRef != null)
                    {
                        propertyRefs.Add(propertyRef);
                    }
                }

                if (propertyRefs.Count > 0)
                {
                    foreach (var propRef in propertyRefs)
                    {
                        if (propRef != null
                            && propRef.Parent is Key)
                        {
                            var property = propRef.Name.Target;
                            if (property != null)
                            {
                                var entityType = property.Parent as Model.Entity.EntityType;
                                Debug.Assert(entityType != null);

                                var viewEntityType = xref.GetExisting(entityType) as EntityType;

                                if (viewEntityType != null)
                                {
                                    ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                        .SynchronizeSingleDslModelElement(viewEntityType, property);
                                }
                            }
                        }
                    }
                    return;
                }
            }
        }

        /// <summary>
        ///     Given a DSL Property return the previous sibling.
        ///     The method will do the following:
        ///     - Given the DSL Property get its model property from Model XRef.
        ///     - Loop model property's previous siblings.
        ///     - For each model property's previous sibling, get the corresponding DSL property from Model XRef.
        ///     - If the DSL property is not null, return immediately.
        ///     - If the DSL property is null, check its previous sibling.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private PropertyBase FindPreviousProperty(PropertyBase property)
        {
            var modelProperty = ModelXRef.GetExisting(property) as Model.Entity.PropertyBase;
            Debug.Assert(modelProperty != null, "Unable to get Entity Designer Model for :" + property.Name + " from model xref.");
            if (modelProperty != null)
            {
                var previousModelProperty = modelProperty.PreviousSiblingInPropertyXElementOrder;

                while (previousModelProperty != null)
                {
                    var previousProperty = ModelXRef.GetExisting(previousModelProperty) as PropertyBase;
                    if (previousProperty != null)
                    {
                        return previousProperty;
                    }
                    previousModelProperty = previousModelProperty.PreviousSiblingInPropertyXElementOrder;
                }
            }
            return null;
        }

        /// <summary>
        ///     Deletes corresponding ModelItem (if exists in the view model)
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void OnEFObjectDeleted(EFObject efObject, ModelToDesignerModelXRefItem xref)
        {
            Debug.Assert(efObject != null);

            // deleting key means that we will have to re-translate the properties of the
            // EntityType because the propertyRefs were deleted
            var key = efObject as Key;
            if (key != null
                && key.Parent != null)
            {
                var entityType = key.Parent as Model.Entity.EntityType;
                if (entityType != null)
                {
                    var viewEntityType = xref.GetExisting(entityType) as EntityType;
                    if (viewEntityType != null)
                    {
                        // Only translate properties that are in the view model
                        foreach (var property in entityType.Properties().Where(p => xref.GetExisting(p) != null))
                        {
                            ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                .SynchronizeSingleDslModelElement(viewEntityType, property);
                        }
                    }
                }
                return;
            }

            // deleting key's propertyref means removing property from entity's key
            var propertyRef = efObject as PropertyRef;
            if (propertyRef != null
                && propertyRef.Parent is Key)
            {
                var property = propertyRef.Name.Target;
                if (property != null)
                {
                    var entityType = property.Parent as Model.Entity.EntityType;
                    Debug.Assert(entityType != null);

                    var viewEntityType = xref.GetExisting(entityType) as EntityType;
                    var viewProperty = xref.GetExisting(property) as Property;
                    // if we are deleting whole EntityType this will be null
                    if (viewEntityType != null
                        && viewProperty != null)
                    {
                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                            .SynchronizeSingleDslModelElement(viewEntityType, property);
                    }
                }
                return;
            }

            var modelProperty = efObject.Parent as ComplexConceptualProperty;
            if (modelProperty != null)
            {
                var entityType = modelProperty.Parent as Model.Entity.EntityType;
                if (entityType != null)
                {
                    var viewEntityType = xref.GetExisting(entityType) as EntityType;
                    if (viewEntityType != null)
                    {
                        Debug.Assert(xref.ContainsKey(modelProperty), "view for modelProperty not found");
                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                            .SynchronizeSingleDslModelElement(viewEntityType, modelProperty);
                    }
                }
                return;
            }

            ModelElement modelElement = null;

            var et = efObject as ConceptualEntityType;
            if (et != null)
            {
                foreach (var prop in et.Properties())
                {
                    modelElement = xref.GetExisting(prop);
                    if (modelElement != null)
                    {
                        ModelXRef.Remove(prop, modelElement);
                        modelElement.Delete();
                    }
                }
                foreach (var prop in et.NavigationProperties())
                {
                    modelElement = xref.GetExisting(prop);
                    if (modelElement != null)
                    {
                        ModelXRef.Remove(prop, modelElement);
                        modelElement.Delete();
                    }
                }
            }

            // If a diagram is deleted and the diagram is the only diagram that is opened in VS, we need to open the first diagram.
            // The assumption is that there will be another diagram exists in the model because the user can't delete a diagram if the diagram is the only diagram in the model.
            // We need to do this to ensure Model-Browser window is not closed and there at least 1 active designer for the EDMX in VS.
            var modelDiagram = efObject as ModelDesigner.Diagram;
            var currentDiagram = GetDiagram();
            if (null != currentDiagram
                && null != modelDiagram
                && currentDiagram.DiagramId == modelDiagram.Id.Value)
            {
                var foundAnotherActiveDiagram = false;
                // Check if there is any active diagram in VS.
                foreach (var diagram in Store.ElementDirectory.FindElements<EntityDesignerDiagram>())
                {
                    if (currentDiagram != diagram
                        && diagram.ActiveDiagramView != null
                        && diagram.IsDeleted == false
                        && diagram.IsDeleting == false)
                    {
                        foundAnotherActiveDiagram = true;
                        break;
                    }
                }

                if (!foundAnotherActiveDiagram)
                {
                    var service = EditingContext.GetEFArtifactService();
                    var entityDesignArtifact = service.Artifact as EntityDesignArtifact;
                    Debug.Assert(entityDesignArtifact != null, "EFArtifactService's artifact is null.");
                    if (entityDesignArtifact != null)
                    {
                        var firstModelDiagram = entityDesignArtifact.DesignerInfo.Diagrams.FirstDiagram;

                        // firstModelDiagram should never be equal to the diagram that was deleted.
                        Debug.Assert(
                            firstModelDiagram != null && firstModelDiagram != modelDiagram, "There is no valid diagram in the model.");

                        if (firstModelDiagram != null
                            && firstModelDiagram != modelDiagram)
                        {
                            // Change the DiagramID and reload.
                            currentDiagram.DiagramId = firstModelDiagram.Id.Value;
                            // Unfortunately we could not call ClearAndReloadDiagram here because a DSL transaction has been opened before this method is called; 
                            // Need to make wait until the transaction is committed.
                            _shouldClearAndReloadDiagram = true;
                            return;
                        }
                    }
                }
            }

            modelElement = xref.GetExisting(efObject);
            if (modelElement != null)
            {
                ModelXRef.Remove(efObject, modelElement);
                modelElement.Delete();
            }
        }

        #region IDisposable Members

        /// <summary>
        ///     DSL calls this (if we've implemented it) when removing the store(s) on a document reload
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EntityDesignerViewModel()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                // TODO:
                // Clean up the ModelXRef in EditingContext.

                UnregisterEventDelegates();
                _editingContext = null;
                _isDisposed = true;
            }
        }

        #endregion
    }
}
