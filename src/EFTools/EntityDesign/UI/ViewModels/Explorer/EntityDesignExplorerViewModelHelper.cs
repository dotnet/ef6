// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.UI.Util;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     This class provides a wrapper around ExplorerViewModel so as to provide
    ///     a layer of indirection which can be utilized for e.g. testing the ViewModel
    ///     without the necessity of creating the View objects
    /// </summary>
    internal class EntityDesignExplorerViewModelHelper : ExplorerViewModelHelper
    {
        // constructor expects ViewModel to be set later (which will then 
        // cause ExplorerViewModelChanged event to be fired)
        internal EntityDesignExplorerViewModelHelper()
        {
            // ensure the ModelToExplorerModelXRef cache is initialized
            ModelToExplorerModelXRef.AddModelManager2XRefType(
                typeof(EntityDesignModelManager), typeof(EntityDesignModelToExplorerModelXRef));
        }

        public new ExplorerViewModel ViewModel
        {
            get { return (ExplorerViewModel)base.ViewModel; }
        }

        public override void CreateViewModel(EditingContext ctx)
        {
            var service = ctx.GetEFArtifactService();
            Debug.Assert(service != null, "Null service in ExplorerViewModelHelper.CreateViewModel()");
            var artifact = service.Artifact;
            Debug.Assert(artifact != null, "Null artifact in ExplorerViewModelHelper.CreateViewModel()");

            var xref = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(ctx);
            xref.Clear();

            var edmRootNode = new ExplorerRootNode(ctx, null, artifact.Uri);

            var designerInfo = artifact.DesignerInfo();
            if (designerInfo != null
                && designerInfo.Diagrams != null)
            {
                var explorerDiagrams = (ExplorerDiagrams)
                                       ModelToExplorerModelXRef.GetNewOrExisting(
                                           ctx, designerInfo.Diagrams, edmRootNode, typeof(ExplorerDiagrams));
                edmRootNode.Diagrams = explorerDiagrams;
            }

            if (artifact.ConceptualModel() != null)
            {
                var browserCsdlEntityModel = (ExplorerConceptualEntityModel)
                                             ModelToExplorerModelXRef.GetNewOrExisting(
                                                 ctx, artifact.ConceptualModel(), edmRootNode, typeof(ExplorerConceptualEntityModel));
                edmRootNode.ConceptualModel = browserCsdlEntityModel;
            }

            if (artifact.StorageModel() != null)
            {
                var browserSsdlEntityModel = (ExplorerStorageEntityModel)
                                             ModelToExplorerModelXRef.GetNewOrExisting(
                                                 ctx, artifact.StorageModel(), edmRootNode, typeof(ExplorerStorageEntityModel));
                edmRootNode.StorageModel = browserSsdlEntityModel;
            }

            // expand the tree view so that the Conceptual, Storage Models, and Diagram nodes are visible
            if (edmRootNode.Diagrams != null)
            {
                edmRootNode.Diagrams.Types.ExpandTreeViewToMe();
            }

            if (edmRootNode.ConceptualModel != null)
            {
                edmRootNode.ConceptualModel.Types.ExpandTreeViewToMe();
            }

            if (edmRootNode.StorageModel != null)
            {
                edmRootNode.StorageModel.Types.ExpandTreeViewToMe();
            }

            base.ViewModel = new ExplorerViewModel(ctx, edmRootNode);
        }

        protected override HashSet<EFArtifact> GetCurrentArtifactsInView(EditingContext context)
        {
            var artifacts = new HashSet<EFArtifact>();
            var service = context.GetEFArtifactService();
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

        protected override bool ProcessCreateOrDeleteChange(EditingContext ctx, ModelToExplorerModelXRef xref, EfiChange change)
        {
            var artifact = change.Changed as EFArtifact;

            var conceptualModel = change.Changed as ConceptualEntityModel;
            var storageModel = change.Changed as StorageEntityModel;
            var mappingModel = change.Changed as MappingModel;
            if (null != artifact
                ||
                null != conceptualModel
                ||
                null != storageModel
                ||
                null != mappingModel)
            {
                // reset the search results - they will no longer be valid
                // once the view model is recalculated below
                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(ctx);
                explorerSearchResults.Reset();

                // reload the UI - the ExplorerViewModelChanged will be fired
                // allowing the frame to rebind
                CreateViewModel(ctx);

                // don't process any more
                return false;
            }

            var efElement = change.Changed as EFElement;
            // ExplorerViewModelHelper only needs to process EFElement changes
            // (others are DefaultableValue and SingleItemBinding - but
            // ExplorerViewModel does not currently need to map these latter)
            if (null != efElement)
            {
                var parent = efElement.Parent as EFElement;
                Debug.Assert(
                    null != parent,
                    "received changed element of type " + change.Changed.GetType().FullName + " with non-EFElement parent of type "
                    + (efElement.Parent == null ? "NULL" : efElement.Parent.GetType().FullName));
                if (null != parent)
                {
                    // special case changing of an entity's key
                    // If we are creating/deleting a Key PropertyRef then we need
                    // to update the underlying ExplorerProperty
                    var propRef = efElement as PropertyRef;
                    var keyElement = efElement as Key;
                    if (propRef != null)
                    {
                        keyElement = propRef.GetParentOfType(typeof(Key)) as Key;
                        if (keyElement != null
                            && null != propRef.Name
                            && null != propRef.Name.Target)
                        {
                            ExplorerEFElement explorerProp =
                                xref.GetExisting(propRef.Name.Target) as ExplorerProperty;
                            if (null != explorerProp)
                            {
                                explorerProp.OnModelPropertyChanged(ExplorerEFElement.IsKeyPropertyID);
                            }
                        }
                    }
                    else if (keyElement != null)
                    {
                        // key must be a child of an entity
                        var et = parent as EntityType;
                        if (et != null)
                        {
                            // the key is being created or completely removed, so sync up every property
                            foreach (var prop in et.Properties())
                            {
                                ExplorerEFElement explorerProp = xref.GetExisting(prop) as ExplorerProperty;
                                if (null != explorerProp)
                                {
                                    explorerProp.OnModelPropertyChanged(ExplorerEFElement.IsKeyPropertyID);
                                }
                            }
                        }
                    }
                    else
                    {
                        // find Explorer node which maps to the model's parent
                        // this can be null is the ViewModel does not map the
                        // parent object
                        if (typeof(FunctionImport) == efElement.GetType())
                        {
                            // the FunctionImport Explorer Parent node has been decided to be the ConceptualEntityModel ExplorerEFElement
                            // rather than the ConceptualEntityContainer one which we would more naturally use to match the model setup
                            parent = parent.Parent as EFElement;
                        }
                        var explorerParentItem = xref.GetExisting(parent);
                        if (null != explorerParentItem)
                        {
                            // now find the Explorer node which should be the new/deleted ViewModel element's parent
                            // (may not be the same as above due to Explorer ghost nodes)
                            explorerParentItem = explorerParentItem.GetParentNodeForElement(efElement);

                            if (EfiChange.EfiChangeType.Create == change.Type)
                            {
                                // It's possible that between the Create of a parent and its child
                                // the Children property is called on the parent which loads the
                                // child into the parent, even though the child is being added with 
                                // change.Type = Create. For safety, we should remove the existing
                                // child (if it exists) so as to ensure any changes in the child change 
                                // are reflected.
                                var explorerEFElement = xref.GetExisting(efElement);
                                if (explorerEFElement != null)
                                {
                                    explorerParentItem.RemoveChildIfLoaded(efElement);
                                }
                                explorerParentItem.InsertChildIfLoaded(efElement);
                            }
                            else
                            {
                                explorerParentItem.RemoveChildIfLoaded(efElement);
                            }
                        }
                    }
                }
            }

            return true;
        }

        // implementation of the SearchVisitor.EFElementTextToSearch delegate
        // returning the EFElement's DisplayName
        private static string SearchOnDisplayName(EFElement efElement)
        {
            return efElement.DisplayName;
        }

        internal override ModelSearchResults SearchModelByDisplayName(string searchCriteria)
        {
            var searchResults = new ModelSearchResults();
            searchResults.Action = Resources.SearchResultItemsMatching;
            searchResults.SearchCriteria = String.Format(
                CultureInfo.CurrentCulture,
                Resources.SearchResultSearchCriteria, searchCriteria);
            var visitor = new SearchVisitor(searchCriteria, SearchOnDisplayName);
            searchResults.TargetString = searchCriteria;
            searchResults.ElementTextToSearch = SearchOnDisplayName;
            if (null != ViewModel.EDMRootNode()
                && null != ViewModel.EDMRootNode().ConceptualModel
                && null != ViewModel.EDMRootNode().ConceptualModel.ModelItem)
            {
                visitor.Traverse(ViewModel.EDMRootNode().ConceptualModel.ModelItem);
            }
            if (null != ViewModel.EDMRootNode()
                && null != ViewModel.EDMRootNode().StorageModel
                && null != ViewModel.EDMRootNode().StorageModel.ModelItem)
            {
                visitor.Traverse(ViewModel.EDMRootNode().StorageModel.ModelItem);
            }
            if (null != ViewModel.EDMRootNode()
                && null != ViewModel.EDMRootNode().Diagrams
                && null != ViewModel.EDMRootNode().Diagrams.ModelItem)
            {
                visitor.Traverse(ViewModel.EDMRootNode().Diagrams.ModelItem);
            }

            searchResults.Results = visitor.SearchResults;

            return searchResults;
        }

        protected override EFElement GetNavigationTarget(EFObject efObject)
        {
            return GetNavigationTargetForEFObject(efObject);
        }

        // Given an EFObject, this will transform it into something that should be referenceable in the explorer tree.
        internal static EFElement GetNavigationTargetForEFObject(EFObject efObject)
        {
            // find the first parent of EFObject that is an EFElement.  This is what we want to navigate to.
            var efElement = efObject as EFElement;
            if (efElement == null)
            {
                efElement = efObject.GetParentOfType(typeof(EFElement)) as EFElement;
            }
            Debug.Assert(efElement != null, "Unable to find parent EFElement of EFObject");

            var propertyRef = efElement as PropertyRef;
            var propertyRefContainer = efElement as PropertyRefContainer;
            if (propertyRef != null
                || propertyRefContainer != null)
            {
                Property target = null;

                if (propertyRef != null)
                {
                    if (propertyRef.Name.Target != null)
                    {
                        target = propertyRef.Name.Target;
                        efElement = target;
                    }
                }
                else if (propertyRefContainer != null)
                {
                    // use first referenced Property
                    foreach (var pr in propertyRefContainer.PropertyRefs)
                    {
                        if (pr.Name.Target != null)
                        {
                            target = pr.Name.Target;
                            efElement = target;
                            break;
                        }
                    }
                }

                if (target == null)
                {
                    // Couldn't find a property to reference, so try for a parent EntityType or Association. 
                    EFElement rtrn = efElement.GetParentOfType(typeof(EntityType)) as EntityType;
                    if (rtrn == null)
                    {
                        rtrn = efElement.GetParentOfType(typeof(Association)) as Association;
                    }
                    Debug.Assert(
                        rtrn != null, "Couldn't find EntityType or Association when swizzling EFObject of type " + efElement.GetType());
                    efElement = rtrn;
                }
            }
            else if (efElement is AssociationEnd)
            {
                Debug.Assert(
                    efElement.Parent is Association, "Found unexpected parent type of AssociationEnd:  " + efElement.Parent.GetType());
                efElement = efElement.Parent as EFElement;
            }
            else if (efElement is AssociationSetEnd)
            {
                Debug.Assert(
                    efElement.Parent is AssociationSet, "Found unexpected parent type of AssociationSetEnd:  " + efElement.Parent.GetType());
                efElement = efElement.Parent as EFElement;
            }
            else if (efElement is ReferentialConstraint)
            {
                Debug.Assert(
                    efElement.Parent is Association, "Found unexpected parent type of ReferentialConstraint:  " + efElement.Parent.GetType());
                efElement = efElement.Parent as EFElement;
            }

            // some elements are not displayed in the Explorer - if so search for parent that is
            while (null != efElement
                   && !EntityDesignModelToExplorerModelXRef.IsDisplayedInExplorer(efElement))
            {
                efElement = efElement.Parent as EFElement;
            }
            return efElement;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void CreateFunctionImport(Function sproc)
        {
            var root = ViewModel.EDMRootNode();
            if (null == root)
            {
                Debug.Fail("There is no root node in the EDM Explorer");
                return;
            }
            if (null == root.ConceptualModel)
            {
                Debug.Fail("There is no conceptual entity model representation in the EDM Explorer");
                return;
            }
            var cModel = root.ConceptualModel.ModelItem as ConceptualEntityModel;
            if (null == cModel)
            {
                Debug.Fail("There is no corresponding model item for the EDM Explorer's conceptual entity model representation");
                return;
            }
            var cContainer = cModel.FirstEntityContainer as ConceptualEntityContainer;
            if (null == cContainer)
            {
                Debug.Fail("There is no conceptual entity container in the conceptual entity model");
                return;
            }

            if (null == root.StorageModel)
            {
                Debug.Fail("The EDM Explorer must have a storage entity model");
                return;
            }
            var sModel = root.StorageModel.ModelItem as StorageEntityModel;
            if (null == sModel)
            {
                Debug.Fail("There is no corresponding model item for the EDM Explorer's storage entity model representation");
                return;
            }

            var artifact = ViewModel.EditingContext.GetEFArtifactService().Artifact;
            if (null == artifact)
            {
                Debug.Fail("There is no artifact for the EDM Explorer's view model representation");
                return;
            }

            EntityDesignViewModelHelper.CreateFunctionImport(
                ViewModel.EditingContext,
                artifact,
                sproc,
                sModel,
                cModel,
                cContainer,
                null,
                EfiTransactionOriginator.ExplorerWindowOriginatorId);
        }

        /// <summary>
        ///     Launch edit function import dialog box.
        /// </summary>
        public void EditFunctionImport(FunctionImport functionImport)
        {
            Debug.Assert(functionImport != null, "The functionImport passed in to EditFunctionImport is null.");
            if (null == functionImport)
            {
                return;
            }

            var artifact = functionImport.Artifact;
            Debug.Assert(artifact != null, "There is no artifact in the passed in functionImport.");
            if (null == artifact)
            {
                return;
            }

            var cModel = artifact.ConceptualModel();
            Debug.Assert(
                cModel != null, "There is no corresponding model item for the EDM Explorer's conceptual entity model representation.");
            if (null == cModel)
            {
                return;
            }

            var sModel = artifact.StorageModel();
            Debug.Assert(sModel != null, "There is no corresponding model item for the EDM Explorer's storage entity model representation");
            if (null == sModel)
            {
                return;
            }

            var cContainer = cModel.FirstEntityContainer as ConceptualEntityContainer;
            Debug.Assert(cContainer != null, "There is no conceptual entity container in the conceptual entity model");
            if (null == cContainer)
            {
                return;
            }

            EntityDesignViewModelHelper.EditFunctionImport(
                ViewModel.EditingContext,
                functionImport,
                sModel,
                cModel,
                cContainer,
                GetFunctionImportReturnType(cModel, functionImport),
                EfiTransactionOriginator.ExplorerWindowOriginatorId);
        }

        // Method is moved from EntityDesignViewModelHelper. 
        internal static Object GetFunctionImportReturnType(ConceptualEntityModel cModel, FunctionImport functionImport)
        {
            object value = null;
            if (functionImport.IsReturnTypeEntityType)
            {
                if (functionImport.ReturnTypeAsEntityType.Status == BindingStatus.Known)
                {
                    value = functionImport.ReturnTypeAsEntityType.Target.NormalizedNameExternal;
                }
                else
                {
                    value = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(functionImport.ReturnTypeAsEntityType.RefName, true);
                }
            }
            else if (functionImport.IsReturnTypeComplexType)
            {
                if (functionImport.ReturnTypeAsComplexType.Status == BindingStatus.Known)
                {
                    value = functionImport.ReturnTypeAsComplexType.Target.NormalizedNameExternal;
                }
                else
                {
                    value = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(functionImport.ReturnTypeAsComplexType.RefName, true);
                }
            }
            else
            {
                value = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(functionImport.ReturnTypeAsPrimitiveType.Value);
            }

            var selectedElement = ModelHelper.FindComplexTypeEntityTypeOrPrimitiveTypeForFunctionImportReturnType(cModel, value as string);
            return selectedElement;
        }
    }
}
