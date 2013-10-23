// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class ExplorerNavigationHelper
    {
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Show")]
        public static void NavigateTo(EFObject efobject)
        {
            if (efobject == null)
            {
                return;
            }

            // if this is a mapping element, just return, since we can't navigate to a mapping element in the explorer
            if (efobject.RuntimeModelRoot() != null && efobject.RuntimeModelRoot() is MappingModel
                || efobject is MappingModel)
            {
                return;
            }

            // now set the focus on this node
            var editingContext =
                PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(
                    efobject.Artifact.ArtifactSet.GetEntityDesignArtifact().Uri);
            Debug.Assert(editingContext != null, "Unable get EditingContext");
            if (editingContext != null)
            {
                // make sure that ExplorerWindow has the right context (this is necessary when navigating to efObject in file that's not currently opened)
                var explorerWindow = PackageManager.Package.ExplorerWindow;
                if (explorerWindow.Context != editingContext)
                {
                    explorerWindow.Context = editingContext;
                }

                var explorerInfo = editingContext.Items.GetValue<ExplorerWindow.ExplorerInfo>();
                Debug.Assert(explorerInfo != null, "Unable get ExplorerInfo from EditingContext");
                if (explorerInfo != null)
                {
                    var explorerFrame = explorerInfo._explorerFrame;
                    Debug.Assert(explorerFrame != null, "Unable to get ExplorerFrame from ExplorerInfo");
                    if (explorerFrame != null)
                    {
                        // note - if we don't show the frame before calling GetTreeViewItem, then 
                        // we don't find the correct tree view item the first time GetTreeViewItem is called.
                        // we find a parent. Rather, we find a parent close to the root. 
                        var frame = PackageManager.Package.ExplorerWindow.Frame as IVsWindowFrame;
                        if (frame != null)
                        {
                            frame.Show();
                        }

                        // Ensure that the root node is expanded so the selected node is shown.
                        // Only need to expand the root node, the rest of the nodes will be expanded when the code traverse to the selected node.
                        explorerFrame.ExplorerTreeRoot.IsExpanded = true;

                        var treeViewItem = GetTreeViewItemForEFObject(efobject, explorerFrame);
                        Debug.Assert(treeViewItem != null, "Unable to get TreeViewItem from explorerFrame");

                        if (treeViewItem != null)
                        {
                            treeViewItem.IsSelected = true;
                            treeViewItem.Focus();
                        }
                    }
                }
            }
        }

        private static ExplorerTreeViewItem GetTreeViewItemForEFObject(EFObject efobject, ExplorerFrame explorerFrame)
        {
            var treeViewItem = explorerFrame.ExplorerTreeRoot;
            if (null != efobject
                && !(efobject is EFArtifact))
            {
                var entityDesignArtifact = efobject.Artifact.ArtifactSet.GetEntityDesignArtifact();
                var editingContext =
                    PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(entityDesignArtifact.Uri);
                var element = explorerFrame.ExplorerViewModelHelper.GetExplorerEFElementForEFObject(editingContext, efobject);
                Debug.Assert(element != null, "Unable to find ExplorerEFElement for efobject of type " + efobject.GetType());
                if (element != null)
                {
                    treeViewItem = explorerFrame.GetTreeViewItem(element, true);
                }
            }
            return treeViewItem;
        }
    }
}
