// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Eventing;

    /// <summary>
    ///     Represents the ViewModel that will be exposed in the Explorer Window.
    /// </summary>
    internal interface IExplorerViewModel
    {
        EditingContext EditingContext { get; }
        ExplorerEFElement RootNode { get; }
    }

    /// <summary>
    ///     This class provides a wrapper around ExplorerViewModel so as to provide
    ///     a layer of indirection which can be utilized for e.g. testing the ViewModel
    ///     without the necessity of creating the View objects
    /// </summary>
    internal abstract class ExplorerViewModelHelper
    {
        internal class ExplorerViewModelChangedEventArgs : EventArgs
        {
            private readonly IExplorerViewModel _newViewModel;

            internal ExplorerViewModelChangedEventArgs(IExplorerViewModel newViewModel)
            {
                _newViewModel = newViewModel;
            }

            internal IExplorerViewModel NewViewModel
            {
                get { return _newViewModel; }
            }
        }

        // views should listen to this event to be notified when the view model changes
        public EventHandler<ExplorerViewModelChangedEventArgs> ExplorerViewModelChanged;

        private IExplorerViewModel _viewModel;

        // constructor expects ViewModel to be set later (which will then 
        // cause ExplorerViewModelChanged event to be fired)

        public IExplorerViewModel ViewModel
        {
            get { return _viewModel; }
            protected set
            {
                _viewModel = value;

                // now let everyone know we've changed
                var args = new ExplorerViewModelChangedEventArgs(_viewModel);
                if (ExplorerViewModelChanged != null)
                {
                    ExplorerViewModelChanged(this, args);
                }
            }
        }

        /// <summary>
        ///     Creates a new IExplorerViewModel and sets it on this ExplorerViewModelHelper.
        /// </summary>
        public abstract void CreateViewModel(EditingContext ctx);

        /// <summary>
        ///     Processes a Create or Delete change.
        /// </summary>
        /// <returns>
        ///     true if more model changes should be processed; otherwise, false;
        /// </returns>
        protected abstract bool ProcessCreateOrDeleteChange(EditingContext ctx, ModelToExplorerModelXRef xref, EfiChange change);

        /// <summary>
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        internal abstract ModelSearchResults SearchModelByDisplayName(string searchCriteria);

        /// <summary>
        /// </summary>
        /// <param name="efObject"></param>
        /// <returns></returns>
        protected abstract EFElement GetNavigationTarget(EFObject efObject);

        /// <summary>
        ///     Returns the current active artifacts.
        ///     Assumption in here is that a view can span multiple artifacts..
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual HashSet<EFArtifact> GetCurrentArtifactsInView(EditingContext context)
        {
            var serviceFromContext = context.GetEFArtifactService();
            Debug.Assert(
                serviceFromContext != null, "Null service in EditingContext for ExplorerViewModelHelper.ProcessModelChangesCommitted()");

            var artifacts = new HashSet<EFArtifact>();
            artifacts.Add(serviceFromContext.Artifact);

            return artifacts;
        }

        /// <summary>
        ///     this is called by the handler in ExplorerFrame so that we can catch
        ///     exceptions and reload the UI if needed - don't call this from anywhere else
        /// </summary>
        internal void ProcessModelChangesCommitted(EditingContext ctx, EfiChangedEventArgs e)
        {
            if (ctx == null)
            {
                // some other view has caused the context and/or artifact to close & reload
                // and we are still in the middle of firing the ModelChangesCommitted event
                // so just exit
                return;
            }

            var xref = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(ctx);
            var artifacts = GetCurrentArtifactsInView(ctx);

            foreach (var change in e.ChangeGroup.Changes)
            {
                // only process changes for the artifact this view model is associated with
                if (artifacts.Contains(change.Changed.Artifact) == false)
                {
                    continue;
                }

                switch (change.Type)
                {
                        // Create and Delete have the same action at the moment
                    case EfiChange.EfiChangeType.Create:
                    case EfiChange.EfiChangeType.Delete:
                        if (!ProcessCreateOrDeleteChange(ctx, xref, change))
                        {
                            // don't process any more
                            return;
                        }
                        break;

                    case EfiChange.EfiChangeType.Update:
                        var updatedElement = change.Changed as EFElement;
                        if (updatedElement != null)
                        {
                            var explorerItem = xref.GetExisting(updatedElement);
                            if (explorerItem != null)
                            {
                                foreach (var propName in change.Properties.Keys)
                                {
                                    explorerItem.OnModelPropertyChanged(propName);
                                }
                            }
                        }
                        else
                        {
                            var defaultableValue = change.Changed as DefaultableValue;
                            if (defaultableValue != null)
                            {
                                updatedElement = defaultableValue.Parent as EFElement;
                                if (updatedElement != null)
                                {
                                    var explorerItem = xref.GetExisting(updatedElement);
                                    if (explorerItem != null)
                                    {
                                        explorerItem.OnModelPropertyChanged(defaultableValue.PropertyName);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        internal ExplorerSearchResults ExpandViewModelToDisplaySearchResults(ModelSearchResults modelSearchResults)
        {
            // Loop through each Model item in the search results list and
            // use NavigationHelper to navigate to the search result - this 
            // will automatically expand the tree view as necessary
            foreach (var result in modelSearchResults.Results)
            {
                GetExplorerEFElementForEFObject(ViewModel.EditingContext, result);
            }

            // must recalculate results _after_ navigation above, otherwise some ViewModel elements
            // may not have been loaded
            var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(ViewModel.EditingContext);
            explorerSearchResults.RecalculateResults(ViewModel.EditingContext, modelSearchResults);
            return explorerSearchResults;
        }

        internal ExplorerEFElement GetExplorerEFElementForEFObject(EditingContext editingContext, EFObject targetEFObject)
        {
            if (targetEFObject == null)
            {
                return null;
            }

            // 
            //  Find the EFObject we want to navigate to for the given EFObject
            //
            var targetEFElement = GetNavigationTarget(targetEFObject);

            //
            // get all EFElement nodes on the path from our target node to the root.
            //
            var elementStack = new LinkedList<EFElement>();
            var e = targetEFElement;
            while (e != null)
            {
                elementStack.AddLast(e);
                e = e.Parent as EFElement;
            }

            //
            // unwind the stack, making sure that each element's explorer model node exists. 
            //
            var explorerElement = ViewModel.RootNode;
            while (elementStack.Count > 0)
            {
                var e2 = elementStack.Last.Value;
                elementStack.RemoveLast();
                var viewModelType = ModelToExplorerModelXRef.GetViewModelTypeForEFlement(editingContext, e2);
                // if viewModelType is null then that kind of EFElement is not displayed
                // in the Explorer (e.g. S-side StorageEntityContainer and children)
                if (null != viewModelType)
                {
                    var nextElement = ModelToExplorerModelXRef.GetNewOrExisting(editingContext, e2, explorerElement, viewModelType);

                    // There are "dummy-nodes" in the view-model that don't correspond to a node in the model.  
                    // We need to skip over them here. 
                    if (nextElement != null
                        && elementStack.Count > 0)
                    {
                        nextElement = nextElement.GetParentNodeForElement(elementStack.Last.Value);
                        Debug.Assert(nextElement != null, "GetParentNodeForElement returned null");
                    }

                    if (nextElement != null)
                    {
                        explorerElement = nextElement;
                    }
                }
            }
            Debug.Assert(explorerElement != null, "no explorer element found for targetEFObject");

            if (explorerElement != null)
            {
                //
                // now make sure that our target node in the explorer tree is visible.
                //
                explorerElement.ExpandTreeViewToMe();
            }

            return explorerElement;
        }
    }
}
