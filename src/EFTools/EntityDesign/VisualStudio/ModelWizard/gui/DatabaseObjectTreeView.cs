// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if VS12
using Microsoft.VisualStudio.PlatformUI;
#endif

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.VisualStudio.Shell;

    internal partial class DatabaseObjectTreeView : UserControl
    {
        public TreeView TreeViewControl
        {
            get { return treeView; }
        }

        internal enum TreeViewImage
        {
            DbTablesImage = 0,
            TableImage = 1,
            DbViewsImage = 2,
            ViewImage = 3,
            DbStoreProcsImage = 4,
            StoreProcImage = 5,
            DbDeletedItemsImage = 6,
            DeletedItemImage = 7,
            DbAddedItemsImage = 8,
            DbUpdatedItemsImage = 9,
            DbDatabaseSchemaImage = 10,
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DatabaseObjectTreeView()
        {
            InitializeComponent();

            components = new Container();

            // Load new ImageList with glyphs from resources
            var imageList = new ImageList(components)
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = new Size(16, 16),
                    TransparentColor = Color.Magenta
                };

            imageList.Images.Add("DbTables.bmp", Resources.DbTables);
            imageList.Images.Add("Table.bmp", Resources.Table);
            imageList.Images.Add("DbViews.bmp", Resources.DbViews);
            imageList.Images.Add("View.bmp", Resources.View);
            imageList.Images.Add("DBStoredProcs.bmp", Resources.DBStoredProcs);
            imageList.Images.Add("StoredProc.bmp", Resources.StoredProc);
            imageList.Images.Add("DbDeletedItems.bmp", Resources.DbDeletedItems);
            imageList.Images.Add("DeletedItem.bmp", Resources.DeletedItem);
            imageList.Images.Add("DbAddedItems.bmp", Resources.DbAddedItems);
            imageList.Images.Add("DbUpdatedItems.bmp", Resources.DbUpdatedItems);
            imageList.Images.Add("database_schema.bmp", Resources.database_schema);

#if VS12
    // scale images as appropriate for screen resolution
            DpiHelper.LogicalToDeviceUnits(ref imageList);
#endif
            treeView.ImageList = imageList;

            VsShellUtilities.ApplyTreeViewThemeStyles(treeView);
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView.DrawNode += TreeViewControl_DrawNode;
            treeView.AfterCheck += TreeViewControl_AfterCheck;
        }

        private Label _statusLabel;

        // <summary>
        //     Helper to show a status message Label control on top of the client area of the TreeView control
        // </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void ShowStatus(string message)
        {
            if (_statusLabel == null)
            {
                _statusLabel = new Label
                    {
                        BackColor = TreeViewControl.BackColor,
                        Size = ClientSize,
                        Location = new Point(
                            Left + SystemInformation.Border3DSize.Width,
                            Top + SystemInformation.Border3DSize.Height),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Anchor = Anchor
                    };
            }

            _statusLabel.Text = message;

            Controls.Add(_statusLabel);
            Controls.SetChildIndex(_statusLabel, 0);
        }

        // <summary>
        //     Helper to hide the status message Label control
        // </summary>
        public void HideStatus()
        {
            if (_statusLabel != null)
            {
                Controls.Remove(_statusLabel);
            }
        }

        public void FocusAndSetFirstNodeSelected()
        {
            if (treeView.Nodes.Count > 0)
            {
                treeView.SelectedNode = treeView.Nodes[0];
            }
            treeView.Focus();
        }

        public static TreeNode CreateRootNodeAndDescendents(
            ICollection<EntityStoreSchemaFilterEntry> entries,
            string label,
            TreeViewImage rootImage,
            TreeViewImage leafNodeImage)
        {
            var rootNode = CreateTreeNode(label, false, null, label, rootImage, null);

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    EnsureSchemaAndLeafNode(rootNode, entry, leafNodeImage);
                }
            }

            return rootNode;
        }

        private static void EnsureSchemaAndLeafNode(
            TreeNode rootNode,
            EntityStoreSchemaFilterEntry entry,
            TreeViewImage leafNodeImage)
        {
            Debug.Assert(rootNode != null, "rootNode must be non-null");
            Debug.Assert(entry != null, "entry must be non-null");

            var dbObj = DatabaseObject.CreateFromEntityStoreSchemaFilterEntry(entry, null);
            EnsureSchemaAndLeafNode(rootNode, dbObj, leafNodeImage, entry);
        }

        public static void EnsureSchemaAndLeafNode(
            TreeNode rootNode,
            DatabaseObject dbObj,
            TreeViewImage leafNodeImage,
            EntityStoreSchemaFilterEntry leafNodeTagObject)
        {
            Debug.Assert(rootNode != null, "rootNode must be non-null");
            if (rootNode == null)
            {
                return;
            }
            // find or create schema node
            var schemaName = (dbObj.Schema ?? Resources.SelectTablesPage_NullSchemaDisplayName);
            var schemaNode = FindOrCreateTreeSchemaNode(rootNode, schemaName, TreeViewImage.DbDatabaseSchemaImage);

            Debug.Assert(schemaNode != null, "null schemaNode for rootNode with label " + rootNode.Name + ", schemaName " + schemaName);
            if (schemaNode != null)
            {
                // now add child node to schema node
                var detailNode = CreateTreeNode(dbObj.Name, false, leafNodeTagObject, dbObj.Name, leafNodeImage, null);
                schemaNode.Nodes.Add(detailNode);
            }
        }

        private static TreeNode FindOrCreateTreeSchemaNode(
            TreeNode parentNode,
            string schemaName,
            TreeViewImage schemaNodeImage)
        {
            var schemaNodes = parentNode.Nodes.Find(schemaName, false);
            Debug.Assert(
                schemaNodes.Length <= 1,
                "was expecting 0 or 1 nodes with name " + schemaName + ", found " + schemaNodes.Length);
            if (schemaNodes.Length != 0)
            {
                return schemaNodes[0];
            }
            // no previous node - so create one
            var schemaNode = CreateTreeNode(schemaName, false, null, schemaName, schemaNodeImage, null);
            parentNode.Nodes.Add(schemaNode);
            return schemaNode;
        }

        public static TreeNode CreateTreeNode(
            string treeNodeText,
            bool treeNodeIsChecked,
            object treeNodeTag,
            string treeNodeName,
            TreeViewImage treeNodeImage,
            string treeNodeToolTip)
        {
            Debug.Assert(treeNodeName != null, "treeNodeName cannot be null");
            Debug.Assert(treeNodeText != null, "treeNodeText cannot be null");

            return new TreeNode(treeNodeText, (int)treeNodeImage, (int)treeNodeImage)
                {
                    Checked = treeNodeIsChecked,
                    Name = treeNodeName,
                    ToolTipText = treeNodeToolTip ?? String.Empty,
                    Tag = treeNodeTag
                };
        }

        private bool _processingTreeViewCheckStateChange;

        private void TreeViewControl_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // avoiding re-entry 
            if (_processingTreeViewCheckStateChange)
            {
                return;
            }
            try
            {
                _processingTreeViewCheckStateChange = true;
                treeView.BeginUpdate();
                UpdateChildrenNodesCheckedState(e.Node);
            }
            finally
            {
                treeView.EndUpdate();
                _processingTreeViewCheckStateChange = false;
            }
        }

        // <summary>
        //     Once a given node has been checked this method updates the Checked
        //     state of various other nodes to match.
        // </summary>
        private static void UpdateChildrenNodesCheckedState(TreeNode parentNode)
        {
            var nodeChecked = parentNode.Checked;

            // nodeDepth value: 0 => rootNode, 1 => schemaNode, 2 => leafNode
            var nodeDepth = GetNodeDepth(parentNode);

            if (nodeDepth < 2)
            {
                // Root or schema node check changed: this is one of the Tables, 
                // Views or Stored Procedures root nodes or one of their child nodes
                // which represent individual schemas.
                // Iterate over the node's children (and grandchildren if it has any) 
                // and update their checked state.

                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    if (childNode.Checked != nodeChecked)
                    {
                        childNode.Checked = nodeChecked;
                    }
                    foreach (TreeNode grandchildNode in childNode.Nodes)
                    {
                        if (grandchildNode.Checked != nodeChecked)
                        {
                            grandchildNode.Checked = nodeChecked;
                        }
                    }
                }
            }

            if (nodeDepth > 0)
            {
                // a schema or leaf node's Checked state has changed - update the parent 
                // node's Checked state recursively up the tree
                UpdateParentNodeCheckedState(parentNode);
            }
        }

        private static int GetNodeDepth(TreeNode node)
        {
            var nodeDepth = 0;
            while (node.Parent != null)
            {
                node = node.Parent;
                nodeDepth++;
            }
            return nodeDepth;
        }

        // <summary>
        //     Updates a parent node's Checked state recursively up the tree
        //     according to the child's new Checked state.
        //     Note: childNode could be any kind of node.
        // </summary>
        private static void UpdateParentNodeCheckedState(TreeNode childNode)
        {
            // no parent to update
            if (null == childNode
                || null == childNode.Parent)
            {
                return;
            }

            if (childNode.Checked
                && !childNode.Parent.Checked)
            {
                // child has been checked, ensure parent is checked
                childNode.Parent.Checked = true;
                // call recursively on the parent node's parent
                UpdateParentNodeCheckedState(childNode.Parent);
            }
            else if (!childNode.Checked
                     && childNode.Parent.Checked
                     &&
                     !childNode.Parent.Nodes.Cast<TreeNode>().Any(n => n.Checked))
            {
                // child has been unchecked, scan through parent's children 
                // and see if any one is checked - if now none are then
                // uncheck parent
                childNode.Parent.Checked = false;
                // call recursively on the parent node's parent
                UpdateParentNodeCheckedState(childNode.Parent);
            }
        }

        // constants used to manipulate a checkbox 
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        // struct used to set node properties 
        internal struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;

            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpszText;

            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        private void TreeViewControl_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Disable the checkbox for top level nodes such as Views and Sprocs if they have no children.
            if (e.Node.Level == 0
                && e.Node.Nodes.Count == 0)
            {
                var tvi = new TVITEM
                    {
                        hItem = e.Node.Handle,
                        mask = TVIF_STATE,
                        stateMask = TVIS_STATEIMAGEMASK,
                        state = 1 << 12
                    };
                var lparam = IntPtr.Zero;
                try
                {
                    lparam = Marshal.AllocHGlobal(Marshal.SizeOf(tvi));
                    Marshal.StructureToPtr(tvi, lparam, false);
                    NativeMethods.SendMessage(treeView.Handle, TVM_SETITEM, IntPtr.Zero, lparam);
                }
                finally
                {
                    if (lparam != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lparam);
                    }
                }
            }
            e.DrawDefault = true;
        }
    }
}
