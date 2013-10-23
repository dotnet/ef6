// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DesignerModel = Microsoft.Data.Entity.Design.Model.Designer;
using DslModeling = Microsoft.VisualStudio.Modeling;
using Model = Microsoft.Data.Entity.Design.Model.Entity;
using ModelDiagram = Microsoft.Data.Tools.Model.Diagram;
using ViewModelDiagram = Microsoft.VisualStudio.Modeling.Diagrams;

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Tools.Dsl.ModelTranslator;
    using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;
    using EntityDesignerResources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;
    using ModelAssociation = Microsoft.Data.Entity.Design.Model.Entity.Association;
    using ModelEntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using ModelNavigationProperty = Microsoft.Data.Entity.Design.Model.Entity.NavigationProperty;
    using ModelProperty = Microsoft.Data.Entity.Design.Model.Entity.Property;
    using ViewModelAssociation = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Association;
    using ViewModelEntityType = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityType;
    using ViewModelNavigationProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.NavigationProperty;
    using ViewModelProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Property;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EntityModelToDslModelTranslatorStrategy : BaseTranslatorStrategy
    {
        internal EntityModelToDslModelTranslatorStrategy(EditingContext context)
            : base(context)
        {
        }

        #region Override methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override DslModeling.ModelElement TranslateModelToDslModel(EFObject modelElement, DslModeling.Partition partition)
        {
            DesignerModel.Diagram diagram = null;

            if (modelElement != null)
            {
                diagram = modelElement as DesignerModel.Diagram;
                if (diagram == null)
                {
                    throw new ArgumentException("modelElement should be a diagram");
                }
            }

            // get the service so that we can access the root of the entity model
            var service = _editingContext.GetEFArtifactService();
            if (service == null)
            {
                throw new InvalidOperationException(EntityDesignerResources.Error_NoArtifactService);
            }

            EntityDesignerViewModel entityViewModel = null;

            var entityDesignArtifact = service.Artifact as EntityDesignArtifact;
            Debug.Assert(entityDesignArtifact != null, "Artifact is not type of EntityDesignArtifact");

            if (entityDesignArtifact != null)
            {
                // Only translate the Escher Model to Dsl Model if the artifact is designer safe.
                if (entityDesignArtifact.IsDesignerSafe)
                {
                    // now get the root of the model.
                    var model = entityDesignArtifact.ConceptualModel;
                    Debug.Assert(model != null, "Could not get ConceptualModel from the artifact.");

                    if (model != null)
                    {
                        entityViewModel =
                            ModelToDesignerModelXRef.GetNewOrExisting(_editingContext, model, partition) as EntityDesignerViewModel;
                        entityViewModel.Namespace = model.Namespace.Value;

                        // If the passed-in diagram is null, retrieve the first diagram if available.
                        if (diagram == null
                            && entityDesignArtifact.DesignerInfo() != null
                            && entityDesignArtifact.DesignerInfo().Diagrams != null
                            && entityDesignArtifact.DesignerInfo().Diagrams.FirstDiagram != null)
                        {
                            diagram = entityDesignArtifact.DesignerInfo().Diagrams.FirstDiagram;
                        }

                        IList<ModelEntityType> entities;
                        IList<ModelAssociation> associations;

                        if (diagram != null)
                        {
                            RetrieveModelElementsFromDiagram(diagram, out entities, out associations);
                        }
                        else
                        {
                            entities = model.EntityTypes().ToList();
                            associations = model.Associations().ToList();
                        }
                        TranslateEntityModel(entities, associations, entityViewModel);
                    }
                }
                else
                {
                    // return empty view model if the artifact is not designer safe so the Diagram can show safe-mode watermark
                    entityViewModel = new EntityDesignerViewModel(partition);
                    entityViewModel.EditingContext = _editingContext;
                }
            }

            return entityViewModel;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal override DslModeling.ModelElement SynchronizeSingleDslModelElement(
            DslModeling.ModelElement parentViewModel, EFObject modelElement)
        {
            var t = modelElement.GetType();
            if (t == typeof(ConceptualEntityType))
            {
                return TranslateEntityType(parentViewModel as EntityDesignerViewModel, modelElement as ConceptualEntityType);
            }
            else if (t == typeof(EntityTypeBaseType))
            {
                return TranslateBaseType(parentViewModel as EntityDesignerViewModel, (modelElement).Parent as ConceptualEntityType);
            }
            else if (t == typeof(ModelNavigationProperty))
            {
                return TranslateNavigationProperty(parentViewModel as ViewModelEntityType, modelElement as ModelNavigationProperty);
            }
            else if (t == typeof(ComplexConceptualProperty)
                     || t == typeof(ConceptualProperty))
            {
                return TranslateProperty(parentViewModel as ViewModelEntityType, modelElement as ModelProperty);
            }
            else if (t == typeof(ModelAssociation))
            {
                return TranslateAssociation(parentViewModel as EntityDesignerViewModel, modelElement as ModelAssociation);
            }
            else if (t == typeof(DesignerModel.Diagram))
            {
                return TranslateDiagramValues(parentViewModel as EntityDesignerViewModel, modelElement as DesignerModel.Diagram);
            }

            Debug.Assert(false, "modelElement with type= " + t.Name + " is not supported");

            return null;
        }

        #endregion

        #region Translator Methods

        private void TranslateEntityModel(
            IEnumerable<ModelEntityType> entityTypes,
            IEnumerable<ModelAssociation> associations,
            EntityDesignerViewModel entityViewModel)
        {
            // create each entity type and add its properties
            foreach (var et in entityTypes)
            {
                var cet = et as ConceptualEntityType;
                Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                var viewET = TranslateEntityType(entityViewModel, cet);
                entityViewModel.EntityTypes.Add(viewET);
                TranslatePropertiesOfEntityType(et, viewET);
            }

            // create any inheritance relationships
            foreach (var et in entityTypes)
            {
                var cet = et as ConceptualEntityType;
                Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                TranslateBaseType(entityViewModel, cet);
            }

            // create the associations
            foreach (var assoc in associations)
            {
                TranslateAssociation(entityViewModel, assoc);
            }

            // add navigation properties to the entities
            foreach (var et in entityTypes)
            {
                var viewET =
                    ModelToDesignerModelXRef.GetNewOrExisting(entityViewModel.EditingContext, et, entityViewModel.Partition) as
                    ViewModelEntityType;
                Debug.Assert(viewET != null, "Why wasn't the entity shape added already?");
                TranslateNavigationPropertiesOfEntityType(et, viewET);
            }
        }

        /// <summary>
        ///     Translate model EntityType into view EntityType (creates a view EntityType if not yet created)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="entityType"></param>
        /// <param name="processChildren"></param>
        /// <returns></returns>
        private static ViewModelEntityType TranslateEntityType(EntityDesignerViewModel viewModel, ConceptualEntityType entityType)
        {
            var viewET =
                ModelToDesignerModelXRef.GetNewOrExisting(viewModel.EditingContext, entityType, viewModel.Partition) as ViewModelEntityType;
            viewET.Name = entityType.LocalName.Value;
            return viewET;
        }

        private void TranslatePropertiesOfEntityType(
            ModelEntityType entityType, ViewModelEntityType viewET)
        {
            foreach (var property in entityType.Properties())
            {
                var viewProperty = TranslateProperty(viewET, property);
                Debug.Assert(viewProperty != null);
                if (viewProperty != null)
                {
                    viewET.Properties.Add(viewProperty);
                }
            }
        }

        private void TranslateNavigationPropertiesOfEntityType(
            ModelEntityType entityType, ViewModelEntityType viewET)
        {
            var cet = entityType as ConceptualEntityType;

            if (cet != null)
            {
                foreach (var navProp in cet.NavigationProperties())
                {
                    var viewNavProp = TranslateNavigationProperty(viewET, navProp);
                    if (viewNavProp != null)
                    {
                        viewET.NavigationProperties.Add(viewNavProp);
                    }
                }
            }
        }

        /// <summary>
        ///     Translate model Property into view Property (creates a view Property if not yet created)
        /// </summary>
        /// <param name="viewEntityType"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private ViewModelProperty TranslateProperty(ViewModelEntityType viewEntityType, ModelProperty property)
        {
            var viewProperty =
                ModelToDesignerModelXRef.GetNewOrExisting(EditingContext, property, viewEntityType.Partition) as ViewModelProperty;
            var scalarProperty = viewProperty as ScalarProperty;
            if (scalarProperty != null)
            {
                // flag if we are part of the key
                scalarProperty.EntityKey = property.IsKeyProperty;
            }

            // set the other properties if they aren't null
            if (property.LocalName.Value != null)
            {
                viewProperty.Name = property.LocalName.Value;
            }

            viewProperty.Type = property.TypeName;

            return viewProperty;
        }

        /// <summary>
        ///     Translate base type of a model EntityType into view Inheritance (creates an Inheritance if not yet created)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private static Inheritance TranslateBaseType(EntityDesignerViewModel viewModel, ConceptualEntityType entityType)
        {
            if (entityType.BaseType.Status == BindingStatus.Known)
            {
                var baseType =
                    ModelToDesignerModelXRef.GetExisting(viewModel.EditingContext, entityType.BaseType.Target, viewModel.Partition) as
                    ViewModelEntityType;
                var derivedType =
                    ModelToDesignerModelXRef.GetExisting(viewModel.EditingContext, entityType, viewModel.Partition) as ViewModelEntityType;

                // in Multiple diagram scenario, baseType and derivedType might not exist in the diagram.
                if (baseType != null
                    && derivedType != null)
                {
                    return
                        ModelToDesignerModelXRef.GetNewOrExisting(viewModel.EditingContext, entityType.BaseType, baseType, derivedType) as
                        Inheritance;
                }
            }

            return null;
        }

        /// <summary>
        ///     Translate model Association into view Association (creates a view Association if not yet created)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="association"></param>
        /// <returns></returns>
        private static ViewModelAssociation TranslateAssociation(
            EntityDesignerViewModel viewModel,
            ModelAssociation association)
        {
            var ends = association.AssociationEnds();

            var end1 = ends[0];
            var end2 = ends[1];

            if (end1.Type.Status == BindingStatus.Known
                && end2.Type.Status == BindingStatus.Known)
            {
                var viewEnd1 =
                    ModelToDesignerModelXRef.GetExisting(viewModel.EditingContext, end1.Type.Target, viewModel.Partition) as
                    ViewModelEntityType;
                var viewEnd2 =
                    ModelToDesignerModelXRef.GetExisting(viewModel.EditingContext, end2.Type.Target, viewModel.Partition) as
                    ViewModelEntityType;

                // Only create association if both entityType exist.
                if (viewEnd1 != null
                    && viewEnd2 != null)
                {
                    var viewAssoc =
                        ModelToDesignerModelXRef.GetNewOrExisting(viewModel.EditingContext, association, viewEnd1, viewEnd2) as
                        ViewModelAssociation;
                    viewAssoc.Name = association.LocalName.Value;

                    viewAssoc.SourceMultiplicity = end1.Multiplicity.Value;

                    viewAssoc.TargetMultiplicity = end2.Multiplicity.Value;

                    // There could be a situation where association is created after navigation property (for example: the user add an entity-type and then add related types ).
                    // In that case we need to make sure that view's association and navigation property are linked.
                    Debug.Assert(
                        end1.Type.Target != null, "Association End: " + end1.DisplayName + " does not reference a valid entity-type.");
                    if (end1.Type.Target != null)
                    {
                        var modelSourceNavigationProperty =
                            ModelHelper.FindNavigationPropertyForAssociationEnd(end1.Type.Target as ConceptualEntityType, end1);
                        if (modelSourceNavigationProperty != null)
                        {
                            var viewSourceNavigationProperty =
                                ModelToDesignerModelXRef.GetExisting(
                                    viewModel.EditingContext, modelSourceNavigationProperty, viewModel.Partition) as
                                ViewModelNavigationProperty;
                            if (viewSourceNavigationProperty != null)
                            {
                                viewAssoc.SourceNavigationProperty = viewSourceNavigationProperty;
                                viewSourceNavigationProperty.Association = viewAssoc;
                            }
                        }
                    }

                    Debug.Assert(
                        end2.Type.Target != null, "Association End: " + end2.DisplayName + " does not reference a valid entity-type.");
                    if (end2.Type.Target != null)
                    {
                        var modelTargetNavigatioNProperty =
                            ModelHelper.FindNavigationPropertyForAssociationEnd(end2.Type.Target as ConceptualEntityType, end2);
                        if (modelTargetNavigatioNProperty != null)
                        {
                            var viewTargetNavigationProperty =
                                ModelToDesignerModelXRef.GetExisting(
                                    viewModel.EditingContext, modelTargetNavigatioNProperty, viewModel.Partition) as
                                ViewModelNavigationProperty;
                            if (viewTargetNavigationProperty != null)
                            {
                                viewAssoc.TargetNavigationProperty = viewTargetNavigationProperty;
                                viewTargetNavigationProperty.Association = viewAssoc;
                            }
                        }
                    }

                    return viewAssoc;
                }
            }
            return null;
        }

        /// <summary>
        ///     Translate model NavigationProperty into view NavigationProperty (creates a view NavigationProperty if not yet created)
        /// </summary>
        private ViewModelNavigationProperty TranslateNavigationProperty(ViewModelEntityType viewEntityType, ModelNavigationProperty navProp)
        {
            var viewNavProp =
                ModelToDesignerModelXRef.GetNewOrExisting(EditingContext, navProp, viewEntityType.Partition) as ViewModelNavigationProperty;

            Debug.Assert(viewNavProp != null, "Expected non-null navigation property");
            viewNavProp.Name = navProp.LocalName.Value;

            if (navProp.Relationship.Status == BindingStatus.Known)
            {
                var association =
                    ModelToDesignerModelXRef.GetExisting(EditingContext, navProp.Relationship.Target, viewEntityType.Partition) as
                    ViewModelAssociation;
                // Association might be null here if the related entity does not exist in the current diagram.

                if (association != null)
                {
                    viewNavProp.Association = association;

                    // On a self-relationship case, we ensure that the Source and Target NavProps are not set with the same value.
                    // The source is set first only if SourceEntity == TargetEntityType and the Source navprop has not been set yet. The other case would be
                    // if this is not a self-relationship.
                    if (viewEntityType == association.SourceEntityType
                        && ((association.SourceEntityType == association.TargetEntityType && association.SourceNavigationProperty == null)
                            || (association.SourceEntityType != association.TargetEntityType)))
                    {
                        association.SourceNavigationProperty = viewNavProp;
                    }

                    // SourceEntityType might be the same as TargetEntityType, so we need to check this as well
                    if (viewEntityType == association.TargetEntityType)
                    {
                        association.TargetNavigationProperty = viewNavProp;
                    }
                }
            }
            return viewNavProp;
        }

        private static EntityDesignerDiagram TranslateDiagramValues(EntityDesignerViewModel viewModel, DesignerModel.Diagram modelDiagram)
        {
            var diagram = viewModel.GetDiagram();

            Debug.Assert(diagram != null, "Why diagram is null?");
            if (diagram != null)
            {
                if (viewModel.ModelXRef.ContainsKey(modelDiagram) == false)
                {
                    viewModel.ModelXRef.Add(modelDiagram, diagram, viewModel.EditingContext);
                }

                using (var t = diagram.Store.TransactionManager.BeginTransaction("Translate diagram values", true))
                {
                    // set zoom level, grid and scalar property options
                    diagram.ZoomLevel = modelDiagram.ZoomLevel.Value;
                    diagram.ShowGrid = modelDiagram.ShowGrid.Value;
                    diagram.SnapToGrid = modelDiagram.SnapToGrid.Value;
                    diagram.DisplayNameAndType = modelDiagram.DisplayType.Value;
                    diagram.Title = modelDiagram.Name.Value;
                    diagram.DiagramId = modelDiagram.Id.Value;
                    t.Commit();
                }
            }
            return diagram;
        }

        /// <summary>
        ///     This method assumes that the ShapeElement as well as the Designer EFObject already exist but they haven't been pushed
        ///     into the view model's XRef yet.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static void TranslateDiagramObject(
            EntityDesignerViewModel viewModel, ModelDiagram.BaseDiagramObject modelDiagramObject,
            bool updateShapeElements, IList<ViewModelDiagram.ShapeElement> shapesToAutoLayout)
        {
            Debug.Assert(modelDiagramObject is EFObject, "Why did you define a DiagramEFObject that is not an EFObject?");

            var modelEntityTypeShape = modelDiagramObject as DesignerModel.EntityTypeShape;

            // the view model could have gotten deleted as a result of OnEFObjectDeleted() so don't attempt to translate the diagram EFObject.
            if (modelEntityTypeShape != null
                && modelEntityTypeShape.IsDisposed != true
                && modelEntityTypeShape.EntityType.Target != null
                && modelEntityTypeShape.EntityType.Target.IsDisposed != true)
            {
                TranslateDiagramObjectHelper(
                    viewModel, modelDiagramObject, modelEntityTypeShape.EntityType.Target, updateShapeElements,
                    shapeElement =>
                        {
                            var viewEntityTypeShape = shapeElement as EntityTypeShape;
                            var rectangle = new ViewModelDiagram.RectangleD(
                                modelEntityTypeShape.PointX.Value
                                , modelEntityTypeShape.PointY.Value, modelEntityTypeShape.Width.Value, 0.0);
                            viewEntityTypeShape.AbsoluteBounds = rectangle;
                            viewEntityTypeShape.IsExpanded = modelEntityTypeShape.IsExpanded.Value;
                            if (!shapesToAutoLayout.Contains(viewEntityTypeShape))
                            {
                                shapesToAutoLayout.Add(viewEntityTypeShape);
                            }

                            // Loop through all the shape's connectors and add the connector to list to be autolayout if the connector is not manually routed.
                            foreach (var linkShape in viewEntityTypeShape.Link)
                            {
                                if (linkShape.ManuallyRouted == false
                                    && shapesToAutoLayout.Contains(linkShape) == false)
                                {
                                    shapesToAutoLayout.Add(linkShape);
                                }
                            }
                        });

                var dslEntityTypeShape = viewModel.ModelXRef.GetExisting(modelEntityTypeShape) as EntityTypeShape;
                // dslEntityTypeShape is null if the entity-type is deleted, in that case skip sync FillColor property.
                if (dslEntityTypeShape != null)
                {
                    dslEntityTypeShape.FillColor = modelEntityTypeShape.FillColor.Value;
                }
            }
            // the view model could have gotten deleted as a result of OnEFObjectDeleted() so don't attempt to translate the diagram EFObject.
            var modelAssociationConnectorShape = modelDiagramObject as DesignerModel.AssociationConnector;
            if (modelAssociationConnectorShape != null
                && modelAssociationConnectorShape.IsDisposed != true
                && modelAssociationConnectorShape.Association.Target != null
                && modelAssociationConnectorShape.Association.Target.IsDisposed != true)
            {
                TranslateDiagramObjectHelper(
                    viewModel, modelDiagramObject, modelAssociationConnectorShape.Association.Target, true,
                    shapeElement => TranslateAssociationConnectors(
                        shapeElement as AssociationConnector, modelAssociationConnectorShape, shapesToAutoLayout));
            }

            // the view model could have gotten deleted as a result of OnEFObjectDeleted() so don't attempt to translate the diagram EFObject.
            var modelInheritanceConnectorShape = modelDiagramObject as DesignerModel.InheritanceConnector;
            if (modelInheritanceConnectorShape != null
                && modelInheritanceConnectorShape.IsDisposed != true
                && modelInheritanceConnectorShape.EntityType.Target != null
                && modelInheritanceConnectorShape.EntityType.Target.IsDisposed != true)
            {
                var cet = modelInheritanceConnectorShape.EntityType.Target as ConceptualEntityType;
                if (cet != null
                    && cet.BaseType != null
                    && cet.BaseType.RefName != null)
                {
                    TranslateDiagramObjectHelper(
                        viewModel, modelDiagramObject, cet.BaseType, true,
                        shapeElement => TranslateInheritanceConnectors(
                            shapeElement as InheritanceConnector, modelInheritanceConnectorShape, shapesToAutoLayout));
                }
            }
        }

        private static void TranslateDiagramObjectHelper(
            EntityDesignerViewModel viewModel, ModelDiagram.BaseDiagramObject modelDiagramEFObject,
            EFObject modelObjectToFindViewModel, bool updateShapeElements, UpdateShapeInfoCallback updateShapeInfoCallback)
        {
            var diagram = viewModel.GetDiagram();
            EFObject diagramEFObject = modelDiagramEFObject;
            Debug.Assert(diagram != null, "Where is the DSL diagram?");
            Debug.Assert(diagramEFObject != null, "Where is the EFObject corresponding to the diagram?");

            if (diagram != null
                && diagramEFObject != null)
            {
                var shapeElement = viewModel.ModelXRef.GetExisting(diagramEFObject) as ViewModelDiagram.ShapeElement;
                if (shapeElement == null)
                {
                    // find the view model associated with the model EFObject
                    var viewModelElement = viewModel.ModelXRef.GetExisting(modelObjectToFindViewModel);
                    Debug.Assert(viewModelElement != null, "Where is the view model for the model object?");

                    if (viewModelElement != null)
                    {
                        // get the shape element fromm the view model
                        shapeElement = diagram.FindShape(viewModelElement);
                        Debug.Assert(shapeElement != null, "Where is the DSL ShapeElement for the view model?");

                        if (shapeElement != null)
                        {
                            // associate the designer model EFObject with the shape element
                            if ((viewModel.ModelXRef.GetExisting(diagramEFObject) == null)
                                && (viewModel.ModelXRef.GetExisting(shapeElement) == null))
                            {
                                viewModel.ModelXRef.Add(diagramEFObject, shapeElement, viewModel.EditingContext);
                            }
                        }
                    }
                }

                // update the shape information for the element
                if (updateShapeElements && shapeElement != null)
                {
                    updateShapeInfoCallback(shapeElement);
                }
            }
        }

        private delegate void UpdateShapeInfoCallback(ViewModelDiagram.ShapeElement shapeElement);

        /// <summary>
        ///     Apply layout information from modelDiagram to DSL Diagram.
        ///     We loop through all shape element in DSL Diagram, get its model diagram element and apply the layout information.
        ///     Note that we don't assert if we didn't find the corresponding model diagram element.
        ///     In this case, we let DSL to auto layout the shape.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static void TranslateDiagram(EntityDesignerDiagram diagram, DesignerModel.Diagram modelDiagram)
        {
            var viewModel = diagram.ModelElement;
            viewModel.ModelXRef.Add(modelDiagram, diagram, viewModel.EditingContext);

            using (var t = diagram.Store.TransactionManager.BeginTransaction("Translate diagram", true))
            {
                // list of shapes that don't have corresponding element in model and require auto-layout
                var shapesToAutoLayout = new List<ViewModelDiagram.ShapeElement>();

                // try to find object in model for each shape on a diagram
                foreach (var shapeElement in diagram.NestedChildShapes)
                {
                    var entityShape = shapeElement as EntityTypeShape;
                    if (entityShape != null
                        && entityShape.ModelElement != null)
                    {
                        var modelEntity = viewModel.ModelXRef.GetExisting(entityShape.ModelElement) as ModelEntityType;
                        if (modelEntity != null)
                        {
                            var modelEntityTypeShape =
                                modelDiagram.EntityTypeShapes.FirstOrDefault(ets => ets.EntityType.Target == modelEntity);
                            if (modelEntityTypeShape != null)
                            {
                                viewModel.ModelXRef.Add(modelEntityTypeShape, entityShape, viewModel.EditingContext);
                                var rectangle = new ViewModelDiagram.RectangleD(
                                    modelEntityTypeShape.PointX.Value, modelEntityTypeShape.PointY.Value
                                    , modelEntityTypeShape.Width.Value, 0.0);
                                entityShape.AbsoluteBounds = rectangle;
                                entityShape.IsExpanded = modelEntityTypeShape.IsExpanded.Value;
                                entityShape.FillColor = modelEntityTypeShape.FillColor.Value;
                            }
                        }
                        if (viewModel.ModelXRef.GetExisting(entityShape) == null)
                        {
                            shapesToAutoLayout.Add(entityShape);
                        }
                        continue;
                    }

                    var associationConnector = shapeElement as AssociationConnector;
                    if (associationConnector != null
                        && associationConnector.ModelElement != null)
                    {
                        var modelAssociation = viewModel.ModelXRef.GetExisting(associationConnector.ModelElement) as ModelAssociation;
                        if (modelAssociation != null)
                        {
                            var modelAssociationConnector =
                                modelDiagram.AssociationConnectors.FirstOrDefault(ac => ac.Association.Target == modelAssociation);
                            if (modelAssociationConnector != null)
                            {
                                viewModel.ModelXRef.Add(modelAssociationConnector, associationConnector, viewModel.EditingContext);
                                TranslateAssociationConnectors(associationConnector, modelAssociationConnector, shapesToAutoLayout);
                            }
                        }
                        continue;
                    }

                    var inheritanceConnector = shapeElement as InheritanceConnector;
                    if (inheritanceConnector != null
                        && inheritanceConnector.ModelElement != null)
                    {
                        var entityTypeBase = viewModel.ModelXRef.GetExisting(inheritanceConnector.ModelElement) as EntityTypeBaseType;
                        var modelEntity = entityTypeBase.Parent as ModelEntityType;
                        if (modelEntity != null)
                        {
                            var modelInheritanceConnector =
                                modelDiagram.InheritanceConnectors.FirstOrDefault(ic => ic.EntityType.Target == modelEntity);
                            if (modelInheritanceConnector != null)
                            {
                                viewModel.ModelXRef.Add(modelInheritanceConnector, inheritanceConnector, viewModel.EditingContext);
                                TranslateInheritanceConnectors(inheritanceConnector, modelInheritanceConnector, shapesToAutoLayout);
                            }
                        }
                        continue;
                    }
                }

                diagram.AutoLayoutDiagram(shapesToAutoLayout);

                // initiate zoom level, grid and scalar property options
                diagram.ZoomLevel = modelDiagram.ZoomLevel.Value;
                diagram.ShowGrid = modelDiagram.ShowGrid.Value;
                diagram.SnapToGrid = modelDiagram.SnapToGrid.Value;
                diagram.DisplayNameAndType = modelDiagram.DisplayType.Value;
                diagram.DiagramId = modelDiagram.Id.Value;
                diagram.Title = modelDiagram.Name.Value;

                t.Commit();
            }
        }

        internal static void CreateDefaultDiagram(EditingContext context, EntityDesignerDiagram diagram)
        {
            var service = context.GetEFArtifactService();
            var artifact = service.Artifact;
            Debug.Assert(artifact != null, "Artifact is null");

            var cpc = new CommandProcessorContext(
                context, EfiTransactionOriginator.EntityDesignerOriginatorId, EntityDesignerResources.Tx_CreateDiagram);
            var cmd = new DelegateCommand(
                () =>
                    {
                        EntityDesignerDiagramAdd.StaticInvoke(cpc, diagram);

                        foreach (var shapeElement in diagram.NestedChildShapes)
                        {
                            var entityShape = shapeElement as EntityTypeShape;
                            if (entityShape != null)
                            {
                                EntityTypeShapeAdd.StaticInvoke(cpc, entityShape);
                                EntityTypeShapeChange.StaticInvoke(
                                    cpc, entityShape, ViewModelDiagram.NodeShape.AbsoluteBoundsDomainPropertyId);
                                EntityTypeShapeChange.StaticInvoke(cpc, entityShape, ViewModelDiagram.NodeShape.IsExpandedDomainPropertyId);
                                continue;
                            }

                            var associationConnector = shapeElement as AssociationConnector;
                            if (associationConnector != null)
                            {
                                AssociationConnectorAdd.StaticInvoke(cpc, associationConnector);
                                AssociationConnectorChange.StaticInvoke(
                                    cpc, associationConnector, ViewModelDiagram.LinkShape.EdgePointsDomainPropertyId);
                                AssociationConnectorChange.StaticInvoke(
                                    cpc, associationConnector, ViewModelDiagram.LinkShape.ManuallyRoutedDomainPropertyId);
                                continue;
                            }

                            var inheritanceConnector = shapeElement as InheritanceConnector;
                            if (inheritanceConnector != null)
                            {
                                InheritanceConnectorAdd.StaticInvoke(cpc, inheritanceConnector);
                                InheritanceConnectorChange.StaticInvoke(
                                    cpc, inheritanceConnector, ViewModelDiagram.LinkShape.EdgePointsDomainPropertyId);
                                InheritanceConnectorChange.StaticInvoke(
                                    cpc, inheritanceConnector, ViewModelDiagram.LinkShape.ManuallyRoutedDomainPropertyId);
                                continue;
                            }
                        }
                    });
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
        }

        private static void RetrieveModelElementsFromDiagram(
            DesignerModel.Diagram diagram,
            out IList<ModelEntityType> entityTypes,
            out IList<ModelAssociation> associations)
        {
            entityTypes = new List<ModelEntityType>();
            associations = new List<ModelAssociation>();
            var elementsToDelete = new List<EFElement>();

            foreach (var entityTypeShape in diagram.EntityTypeShapes)
            {
                if (entityTypeShape.EntityType != null
                    && entityTypeShape.EntityType.Status == BindingStatus.Known)
                {
                    if (entityTypes.Contains(entityTypeShape.EntityType.Target))
                    {
                        elementsToDelete.Add(entityTypeShape);
                    }
                    else
                    {
                        entityTypes.Add(entityTypeShape.EntityType.Target);
                    }
                }
            }

            foreach (var associationConnector in diagram.AssociationConnectors)
            {
                if (associationConnector.Association != null
                    && associationConnector.Association.Status == BindingStatus.Known)
                {
                    if (associations.Contains(associationConnector.Association.Target))
                    {
                        elementsToDelete.Add(associationConnector);
                    }
                    else
                    {
                        associations.Add(associationConnector.Association.Target);
                    }
                }
            }

            if (elementsToDelete.Any())
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, "Found {0} duplicate items in diagram, which will be deleted as part of loading: {1}",
                        elementsToDelete.Count, string.Join(",", elementsToDelete.Select(e => e.DisplayName))));

                if (diagram.Artifact.CanEditArtifact())
                {
                    // Don't use an EfiTransaction here, since we just want to clean up the XML and avoid triggering diagram ui deletes
                    using (var tx = diagram.Artifact.XmlModelProvider.BeginTransaction("LoadDiagram", "Remove duplicate diagram elements"))
                    {
                        foreach (var element in elementsToDelete)
                        {
                            element.Delete();
                        }

                        tx.Commit();
                    }
                }
                else
                {
                    Debug.Fail(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Could not delete duplicate diagram items, proceeding with load and ignoring duplicates"));
                }
            }
        }

        private static void TranslateInheritanceConnectors(
            InheritanceConnector dslInheritanceConnector, DesignerModel.InheritanceConnector modelInheritanceConnector,
            IList<ViewModelDiagram.ShapeElement> shapesToAutoLayout)
        {
            dslInheritanceConnector.ManuallyRouted = modelInheritanceConnector.ManuallyRouted.Value;
            // if the EdgePoint values is an empty collection or connectors are not manually routed, add the shape element to the collections that will be autolayout.
            if (modelInheritanceConnector.ManuallyRouted.Value == false
                || modelInheritanceConnector.ConnectorPoints.Count == 0)
            {
                if (shapesToAutoLayout.Contains(dslInheritanceConnector) == false)
                {
                    shapesToAutoLayout.Add(dslInheritanceConnector);
                }
            }
            else
            {
                var collection = new ViewModelDiagram.EdgePointCollection();
                foreach (var connectorPoint in modelInheritanceConnector.ConnectorPoints)
                {
                    collection.Add(
                        new ViewModelDiagram.EdgePoint(connectorPoint.PointX.Value, connectorPoint.PointY.Value, VGPointType.Normal));
                }
                if (AreEdgePointsIdentical(dslInheritanceConnector.EdgePoints, collection) == false)
                {
                    dslInheritanceConnector.EdgePoints = collection;
                }
            }
        }

        private static void TranslateAssociationConnectors(
            AssociationConnector dslAssociationConnector, DesignerModel.AssociationConnector modelAssociationConnector,
            IList<ViewModelDiagram.ShapeElement> shapesToAutoLayout)
        {
            dslAssociationConnector.ManuallyRouted = modelAssociationConnector.ManuallyRouted.Value;
            // if the EdgePoint values is an empty collection or connectors are not manually routed, add the shape element to the collections that will be autolayout.
            if (modelAssociationConnector.ManuallyRouted.Value == false
                || modelAssociationConnector.ConnectorPoints.Count == 0)
            {
                if (shapesToAutoLayout.Contains(dslAssociationConnector) == false)
                {
                    shapesToAutoLayout.Add(dslAssociationConnector);
                }
            }
            else
            {
                var collection = new ViewModelDiagram.EdgePointCollection();
                foreach (var connectorPoint in modelAssociationConnector.ConnectorPoints)
                {
                    collection.Add(
                        new ViewModelDiagram.EdgePoint(connectorPoint.PointX.Value, connectorPoint.PointY.Value, VGPointType.Normal));
                }

                if (AreEdgePointsIdentical(dslAssociationConnector.EdgePoints, collection) == false)
                {
                    dslAssociationConnector.EdgePoints = collection;
                }
            }
        }

        private static bool AreEdgePointsIdentical(
            ViewModelDiagram.EdgePointCollection collection1, ViewModelDiagram.EdgePointCollection collection2)
        {
            Debug.Assert(collection1 != null, "EdgeCollection1 is null");
            Debug.Assert(collection2 != null, "EdgeCollection2 is null");

            if (collection1 == null
                || collection2 == null)
            {
                throw new ArgumentException("One of the passed parameter is null");
            }

            if (collection1.Count != collection2.Count)
            {
                return false;
            }

            for (var i = 0; i < collection1.Count; i++)
            {
                if (collection1[i].Point != collection2[i].Point)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
