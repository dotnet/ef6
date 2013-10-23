// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Shell;

    /// <summary>
    ///     This class will set the focus on the "most-appropriate" DSL node for the give EFObject in the diagrams.
    ///     The diagram selection works as follow:
    ///     1. Find the match dsl node match in the current active designer view.
    ///     2. If no match is found, find it in any opened designer doc views.
    /// </summary>
    internal static class DSLDesignerNavigationHelper
    {
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Show")]
        internal static void NavigateTo(EFObject efobject)
        {
            if (efobject.RuntimeModelRoot() == null)
            {
                // nothing to navigate to, so just return;
                return;
            }

            if (efobject.RuntimeModelRoot() is StorageEntityModel)
            {
                // s-space object, so just return;
                return;
            }

            var selectionService = Services.DslMonitorSelectionService;
            Debug.Assert(selectionService != null, "Could not retrieve IMonitorSelectionService from Escher package.");
            var foundDSLElementMatchInDiagram = false;
            if (selectionService != null)
            {
                var singleDiagramDocView = selectionService.CurrentDocumentView as SingleDiagramDocView;

                if (singleDiagramDocView != null)
                {
                    foundDSLElementMatchInDiagram = NavigateToDSLNodeInDiagram(
                        singleDiagramDocView.Diagram as EntityDesignerDiagram, efobject);
                    if (foundDSLElementMatchInDiagram)
                    {
                        // The code below is added to ensure that the right doc-view is shown and activated.
                        singleDiagramDocView.Frame.Show();
                        return;
                    }
                }
            }

            // Retrieves the doc data for the efobject.
            var docdata = VSHelpers.GetDocData(Services.ServiceProvider, efobject.Uri.LocalPath) as ModelingDocData;
            Debug.Assert(docdata != null, "Could not find get doc data for artifact with URI:" + efobject.Uri.LocalPath);
            if (docdata != null)
            {
                foreach (var docView in docdata.DocViews)
                {
                    var singleDiagramDocView = docView as SingleDiagramDocView;
                    Debug.Assert(
                        singleDiagramDocView != null,
                        "Why the doc view is not type of SingleDiagramDocView? Actual type:" + docView.GetType().Name);
                    if (docView != null)
                    {
                        foundDSLElementMatchInDiagram = NavigateToDSLNodeInDiagram(
                            singleDiagramDocView.Diagram as EntityDesignerDiagram, efobject);
                        if (foundDSLElementMatchInDiagram)
                        {
                            // The code below is added to ensure that the right doc-view is shown and activated.
                            singleDiagramDocView.Frame.Show();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This class will set the focus on the "most-appropriate" DSL node for the given EFObject and DSL Diagram.  It is assumed that the
        ///     EFObject is either a C-Space node, or an M-space node.
        /// </summary>
        internal static bool NavigateToDSLNodeInDiagram(EntityDesignerDiagram diagram, EFObject efobject)
        {
            var foundDSLElementMatchInDiagram = false;
            var context = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(efobject.Artifact.Uri);

            // find the model parent (if this is a c-space object)
            var cModel = efobject.GetParentOfType(typeof(ConceptualEntityModel)) as ConceptualEntityModel;

            // by default, we assume that this our c-space object
            var cspaceEFObject = efobject;
            EFObject mspaceEFObject = null;
            if (cModel == null)
            {
                var mModel = efobject.GetParentOfType(typeof(MappingModel)) as MappingModel;
                Debug.Assert(mModel != null, "efobject is neither in c-space or s-space");

                // if this is a mapping node, then we want to find the closest corresponding c-space node
                // to which this mapping node is mapped, and set the focus on that.
                cspaceEFObject = GetCSpaceEFObjectForMSpaceEFObject(efobject);
                mspaceEFObject = efobject;
            }

            // navigate to the shape in the DSL designer
            var diagramItemCollection = new DiagramItemCollection();
            RetrieveDiagramItemCollectionForEFObject(diagram, cspaceEFObject, diagramItemCollection);
            if (diagram != null
                && diagramItemCollection.Count > 0)
            {
                diagram.Show();

                if (diagram.ActiveDiagramView != null)
                {
                    diagram.ActiveDiagramView.Focus();
                    diagram.ActiveDiagramView.Selection.Set(diagramItemCollection);
                    diagram.EnsureSelectionVisible();
                }
                else
                {
                    // If no active view exists, do the following:
                    // - Set the selection on the first associated views (if any).
                    // - Set InitialSelectionDIagramItemSelectionProperty to prevent the first EntityTypeShape to be selected (default behavior)
                    //   This case can happen when the diagram is not initialized or is not fully rendered.
                    diagram.InitialDiagramItemSelection = diagramItemCollection;
                    if (diagram.ClientViews != null
                        && diagram.ClientViews.Count > 0)
                    {
                        foreach (DiagramClientView clientView in diagram.ClientViews)
                        {
                            clientView.Selection.Set(diagramItemCollection);
                            clientView.Selection.EnsureVisible(DiagramClientView.EnsureVisiblePreferences.ScrollIntoViewCenter);
                            break;
                        }
                    }
                }
                foundDSLElementMatchInDiagram = true;
            }

            if (mspaceEFObject != null) // navigate to the item in the mapping screen (if we are doing MSL items)
            {
                var mappingDetailsInfo = context.Items.GetValue<MappingDetailsInfo>();
                if (mappingDetailsInfo.MappingDetailsWindow != null)
                {
                    mappingDetailsInfo.MappingDetailsWindow.NavigateTo(mspaceEFObject);
                }
            }

            return foundDSLElementMatchInDiagram;
        }

        /// <summary>
        ///     Given a efobject in m-space (ie, it is defined in the mapping section of the edmx file), find the
        ///     closest object in the c-space (ie, defined in the conceptual schema).  We do this by looking
        ///     for binding objects of the current node bound to something in c-space. If there is no such object, we
        ///     recursively call this on efobject's parent.
        /// </summary>
        /// <param name="efobject"></param>
        /// <returns></returns>
        private static EFObject GetCSpaceEFObjectForMSpaceEFObject(EFObject mspaceEFObject)
        {
            EFObject cspaceEFObject = null;
            var o = mspaceEFObject;

            // see if this m-space object has a parent of an association set mapping.
            var asm = mspaceEFObject.GetParentOfType(typeof(AssociationSetMapping)) as AssociationSetMapping;
            if (asm != null)
            {
                var associationSet = asm.Name.Target;
                if (associationSet != null)
                {
                    var association = associationSet.Association.Target;
                    if (association != null)
                    {
                        Debug.Assert(
                            association.RuntimeModelRoot() is ConceptualEntityModel, "Expected association to be in C-space, but it is not!");
                        return association;
                    }
                }
            }

            // see if this is a node that requires the function view in the mapping pane
            var mfm = mspaceEFObject.GetParentOfType(typeof(ModificationFunctionMapping)) as ModificationFunctionMapping;
            var context = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(mspaceEFObject.Artifact.Uri);
            var mappingDetailsInfo = context.Items.GetValue<MappingDetailsInfo>();
            if (mfm != null)
            {
                mappingDetailsInfo.EntityMappingMode = EntityMappingModes.Functions;
            }
            else
            {
                mappingDetailsInfo.EntityMappingMode = EntityMappingModes.Tables;
            }

            // default case, walk up the model looking for node that has a binding bound to something in c-space.  
            while (cspaceEFObject == null
                   && o != null)
            {
                var binding = o as ItemBinding;
                var container = o as EFContainer;

                if (binding != null)
                {
                    // see if this binding is bound to something in c-space
                    cspaceEFObject = GetCSpaceObjectFromBinding(binding);
                }
                else if (container != null)
                {
                    // see if any direct children are bindings bound to something in c-space
                    foreach (var child in container.Children)
                    {
                        // check every binding to see if it is bound to seomthing in cspace
                        var b = child as ItemBinding;
                        if (b != null)
                        {
                            cspaceEFObject = GetCSpaceObjectFromBinding(b);
                            if (cspaceEFObject != null)
                            {
                                // break out of the for loop
                                break;
                            }
                        }
                    }
                }

                if (cspaceEFObject == null)
                {
                    o = o.Parent;
                }
            }
            return cspaceEFObject;
        }

        /// <summary>
        ///     Given an item binding, return the target of the binding if it is mapped to something in c-space
        /// </summary>
        /// <param name="itemBinding"></param>
        /// <returns></returns>
        private static EFObject GetCSpaceObjectFromBinding(ItemBinding itemBinding)
        {
            foreach (EFObject dep in itemBinding.ResolvedTargets)
            {
                var cModel = dep.RuntimeModelRoot() as ConceptualEntityModel;
                if (cModel != null)
                {
                    return dep;
                }
            }
            return null;
        }

        /// <summary>
        ///     Given an EFObject, return collection of DiagramItems for it.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void RetrieveDiagramItemCollectionForEFObject(
            EntityDesignerDiagram diagram, EFObject efobject, DiagramItemCollection diagramItemCollection)
        {
            if (efobject == null)
            {
                return;
            }

            var cModel = efobject.RuntimeModelRoot() as ConceptualEntityModel;

            if (cModel == null)
            {
                // this either isn't a c-space object, or it is the ConceptualEntityModel node, so just return null
                return;
            }

            // if this is a child element of the association, return the diagram item for the association
            if (!(efobject is Association))
            {
                var association = efobject.GetParentOfType(typeof(Association)) as Association;
                if (association != null)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, association, diagramItemCollection);
                    return;
                }
            }

            if (efobject is Association)
            {
                var shapeElement = GetDesignerShapeElementForEFObject(diagram, efobject);
                if (shapeElement != null)
                {
                    diagramItemCollection.Add(new DiagramItem(shapeElement));
                    return;
                }
            }
            else if (efobject is NavigationProperty)
            {
                var np = efobject as NavigationProperty;
                var shapeElement = GetDesignerShapeElementForEFObject(diagram, np.Parent);
                var entityTypeShape = shapeElement as EntityTypeShape;

                if (entityTypeShape != null)
                {
                    // get the view model navigation property
                    var vmNavProp = diagram.ModelElement.ModelXRef.GetExisting(np) as ViewModel.NavigationProperty;

                    // try to create the DiagramItem from this
                    if (vmNavProp != null)
                    {
                        var index = entityTypeShape.NavigationCompartment.Items.IndexOf(vmNavProp);
                        if (index >= 0)
                        {
                            diagramItemCollection.Add(
                                new DiagramItem(
                                    entityTypeShape.NavigationCompartment, entityTypeShape.NavigationCompartment.ListField,
                                    new ListItemSubField(index)));
                            return;
                        }
                    }
                }
            }
            else if (efobject is Property)
            {
                var prop = efobject as Property;
                if (prop.IsComplexTypeProperty)
                {
                    // complex type properties are not supported in the designer
                    return;
                }
                var shapeElement = GetDesignerShapeElementForEFObject(diagram, prop.Parent);
                var entityTypeShape = shapeElement as EntityTypeShape;
                if (entityTypeShape != null)
                {
                    // get the view model  property
                    var vmProp = diagram.ModelElement.ModelXRef.GetExisting(prop) as ViewModel.Property;

                    if (vmProp != null)
                    {
                        var index = entityTypeShape.PropertiesCompartment.Items.IndexOf(vmProp);
                        if (index >= 0)
                        {
                            diagramItemCollection.Add(
                                new DiagramItem(
                                    entityTypeShape.PropertiesCompartment, entityTypeShape.PropertiesCompartment.ListField,
                                    new ListItemSubField(index)));
                            return;
                        }
                    }
                }
            }
            else if (efobject is EntityType)
            {
                var shapeElement = GetDesignerShapeElementForEFObject(diagram, efobject);
                if (shapeElement != null)
                {
                    diagramItemCollection.Add(new DiagramItem(shapeElement));
                    return;
                }
            }
            else if (efobject is EntitySet)
            {
                var es = efobject as EntitySet;
                foreach (var entityType in es.GetEntityTypesInTheSet())
                {
                    if (entityType != null)
                    {
                        RetrieveDiagramItemCollectionForEFObject(diagram, entityType, diagramItemCollection);
                    }
                }
                return;
            }
            else if (efobject is AssociationSet)
            {
                // return a diagram item for the association
                var associationSet = efobject as AssociationSet;
                var association = associationSet.Association.Target;
                if (association != null)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, association, diagramItemCollection);
                    return;
                }
            }
            else if (efobject is AssociationSetEnd)
            {
                var associationSetEnd = efobject as AssociationSetEnd;
                var end = associationSetEnd.Role.Target;
                if (end != null)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, end, diagramItemCollection);
                    return;
                }
                else
                {
                    var es = associationSetEnd.EntitySet.Target;
                    if (es != null)
                    {
                        RetrieveDiagramItemCollectionForEFObject(diagram, es, diagramItemCollection);
                        return;
                    }
                }
            }
            else if (efobject is PropertyRef)
            {
                var pref = efobject as PropertyRef;
                if (pref.Name.Target != null)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, pref.Name.Target, diagramItemCollection);
                    return;
                }
            }
            else if (efobject is PropertyRefContainer)
            {
                var prefContainer = efobject as PropertyRefContainer;

                // just use the first entry in the list.
                foreach (var pref in prefContainer.PropertyRefs)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, pref, diagramItemCollection);
                    return;
                }
            }
            else if (efobject is EFAttribute)
            {
                // this is an EFAttribute node, so get the DiagramItem for the parent
                RetrieveDiagramItemCollectionForEFObject(diagram, efobject.Parent, diagramItemCollection);
                return;
            }
            else if (efobject is ConceptualEntityModel)
            {
                // nothing in the DSL surface to map to, so return null
                return;
            }
            else if (efobject is ConceptualEntityContainer)
            {
                // nothing in the DSL surface to map to, so return null
                return;
            }
            else if (efobject is FunctionImport)
            {
                // nothing in the DSL surface to map to, so return null
                return;
            }
            else
            {
                Debug.Fail("unexpected type of efobject.  type = " + efobject.GetType());
                if (efobject.Parent != null)
                {
                    RetrieveDiagramItemCollectionForEFObject(diagram, efobject.Parent, diagramItemCollection);
                }
            }
        }

        /// <summary>
        ///     This method will return a DSL ShapeElement for the given efobject.  If the given efobject doesn't map to a designer shape,
        ///     then this will look for a designer shape for the object's parent.
        ///     If no designer shape can be found, this will return null.
        /// </summary>
        /// <param name="efobject"></param>
        /// <returns></returns>
        private static ShapeElement GetDesignerShapeElementForEFObject(EntityDesignerDiagram diagram, EFObject efobject)
        {
            ShapeElement shapeElement = null;
            while (shapeElement == null
                   && efobject != null
                   && ((efobject is ConceptualEntityModel) == false))
            {
                var dslElement = diagram.ModelElement.ModelXRef.GetExisting(efobject);
                shapeElement = dslElement as ShapeElement;

                if (shapeElement == null
                    && dslElement != null)
                {
                    var shapes = PresentationViewsSubject.GetPresentation(dslElement);

                    // just select the first shape for this item
                    if (shapes != null
                        && shapes.Count > 0)
                    {
                        shapeElement = shapes[0] as ShapeElement;
                    }
                }

                // walk up the EFObject tree until we find a node that has a ShapeElement.
                if (shapeElement == null)
                {
                    efobject = efobject.Parent;
                }
            }
            return shapeElement;
        }
    }
}
