// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Microsoft.Data.Entity.Design.UI.Commands;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class ExplorerTreeViewItem : TreeViewItem
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty IndentProperty = DependencyProperty.Register(
            "Indent", typeof(double), typeof(ExplorerTreeViewItem));

        [SuppressMessage("Microsoft.Performance",
            "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "The static constructor is required to register the mouse and key gestures")]
        static ExplorerTreeViewItem()
        {
            CommandManager.RegisterClassInputBinding(
                typeof(ExplorerTreeViewItem),
                new MouseBinding(WorkspaceCommands.Activate, new MouseGesture(MouseAction.LeftDoubleClick)));
            CommandManager.RegisterClassInputBinding(
                typeof(ExplorerTreeViewItem),
                new KeyBinding(WorkspaceCommands.Activate, new KeyGesture(Key.Return)));

            // register left-click binding to allow rename mode for "slow-double-click"
            CommandManager.RegisterClassInputBinding(
                typeof(ExplorerTreeViewItem),
                new MouseBinding(WorkspaceCommands.PutInRenameMode, new MouseGesture(MouseAction.LeftClick)));
        }

        private static readonly Dictionary<string, object> _iconCache = new Dictionary<string, object>();

        // Automation (and accessibility) clients call this method
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ExplorerTreeViewItemAutomationPeer(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public double Indent
        {
            get { return (double)GetValue(IndentProperty); }
            set { SetValue(IndentProperty, value); }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public object Icon
        {
            get
            {
                object result = null;
                var viewModelElement = GetViewModelElement();
                if (null != viewModelElement)
                {
                    var resourceKey = viewModelElement.ExplorerImageResourceKeyName;
                    if (string.IsNullOrEmpty(resourceKey))
                    {
                        Debug.Assert(false, "Resource key is null or empty for view model element named " + viewModelElement.Name);
                    }
                    else
                    {
                        if (!_iconCache.TryGetValue(resourceKey, out result))
                        {
                            try
                            {
                                result = FindResource(resourceKey);
                                if (result != null)
                                {
                                    _iconCache.Add(resourceKey, result);
                                }
                            }
                            catch (ResourceReferenceKeyNotFoundException)
                            {
                                // do nothing - just Assert
                                Debug.Assert(false, "Could not find resource with key " + resourceKey);
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            var treeViewItem = new ExplorerTreeViewItem();
            treeViewItem.Indent = Indent + 19;
            return treeViewItem;
        }

        internal ExplorerEFElement GetViewModelElement()
        {
            return DataContext as ExplorerEFElement;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            // Code below handles the logic to determine which item that will get selected and focus if one of the child is deleted.
            // Skip if the treeviewitem is not selected.
            if (e.Action == NotifyCollectionChangedAction.Remove && IsSelected)
            {
                var setKeyboardFocus = IsKeyboardFocusWithin;
                TreeViewItem itemToBeSetFocusOn = this;

                // If a child item is deleted, select its previous sibling. If there is no previous sibling, then select the parent.
                if (e.OldStartingIndex > 0)
                {
                    // There is no guarantee that the Child UI Elements have been created yet; so we check the item container generator status.
                    if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        var item = ItemContainerGenerator.ContainerFromIndex(e.OldStartingIndex - 1) as TreeViewItem;
                        Debug.Assert(item != null, "Could not get previous sibling of the deleted item");
                        if (item != null)
                        {
                            item.IsSelected = true;
                            itemToBeSetFocusOn = item;
                        }
                    }
                }

                if (setKeyboardFocus)
                {
                    Keyboard.Focus(itemToBeSetFocusOn);
                }
            }
        }
    }

    /// <summary>
    ///     TreeViewItemAutomationPeer is a standard WPF Automation Peer; we are overriding it for the model browser
    /// </summary>
    internal class ExplorerTreeViewItemAutomationPeer : TreeViewItemAutomationPeer
    {
        public ExplorerTreeViewItemAutomationPeer(ExplorerTreeViewItem owner)
            : base(owner)
        {
        }

        /// <summary>
        ///     This is so test framework will be able to query model browser nodes
        /// </summary>
        /// <returns></returns>
        protected override string GetAutomationIdCore()
        {
            var owner = Owner as ExplorerTreeViewItem;
            Debug.Assert(owner != null, "Where is the ExplorerTreeViewItem for the automation peer?");

            if (owner != null)
            {
                // sometimes the label is stored as a string "Model2.edmx" or as an ExplorerEFElement
                var element = owner.Header as ExplorerEFElement;
                var elementstr = owner.Header as string;
                if (element != null)
                {
                    return element.Name;
                }
                else if (!String.IsNullOrEmpty(elementstr))
                {
                    return elementstr;
                }
            }

            // we will use the name for the automation id
            return base.GetNameCore();
        }

        /// <summary>
        ///     Returns back the name of the selected TreeViewItem that accessibility narration will read aloud.
        /// </summary>
        /// <returns></returns>
        protected override string GetNameCore()
        {
            var owner = Owner as ExplorerTreeViewItem;
            Debug.Assert(owner != null, "Where is the ExplorerTreeViewItem for the automation peer?");

            if (owner != null)
            {
                // sometimes the label is stored as a string "Model2.edmx" or as an ExplorerEFElement
                var element = owner.Header as ExplorerEFElement;
                var elementstr = owner.Header as string;
                if (element != null)
                {
                    var sb = new StringBuilder();

                    // if the currently selected node is not a 'ghost node' (e.g. Entity Sets), we'll also tack on the name
                    if (element.ModelItem != null)
                    {
                        sb.Append(element.ModelItem.GetType().Name);
                        sb.Append(" ");
                    }

                    sb.Append(element.Name);
                    return sb.ToString();
                }
                else if (!String.IsNullOrEmpty(elementstr))
                {
                    return elementstr;
                }
            }

            return base.GetNameCore();
        }

        /// <summary>
        ///     This is so test framework will be able to determine the IsInSearchResults property of browser nodes
        /// </summary>
        /// <returns></returns>
        protected override string GetItemStatusCore()
        {
            var owner = Owner as ExplorerTreeViewItem;
            Debug.Assert(owner != null, "Where is the ExplorerTreeViewItem for the automation peer?");

            if (owner != null)
            {
                var element = owner.Header as ExplorerEFElement;
                if (element != null)
                {
                    return element.ItemStatus;
                }
            }

            return base.GetItemStatusCore();
        }

        /// <summary>
        ///     Used in a tree context as the narrator traverses up the tree, reading each parent of the currently
        ///     focused node.
        /// </summary>
        /// <returns></returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            var owner = Owner as ExplorerTreeViewItem;
            Debug.Assert(owner != null, "Where is the ExplorerTreeViewItem for the automation peer?");
            if (owner != null)
            {
                return GetAutomationChildren(owner);
            }
            return base.GetChildrenCore();
        }

        /// <summary>
        ///     Get the immediate children of the current ExplorerTreeViewItem, create automation peers for them, and return them for
        ///     accessibility/automation
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static List<AutomationPeer> GetAutomationChildren(ExplorerTreeViewItem parent)
        {
            var automationChildren = new List<AutomationPeer>();
            foreach (ExplorerEFElement element in parent.Items)
            {
                // get the TreeViewItem from the explorer element.
                var treeViewItem = parent.ItemContainerGenerator.ContainerFromItem(element) as ExplorerTreeViewItem;
                if (treeViewItem != null)
                {
                    // create an AutomationPeer for this TreeViewItem (will call OnCreateAutomationPeer) and add that to the list to return
                    var automationChild = CreatePeerForElement(treeViewItem);
                    Debug.Assert(automationChild != null, "Every ExplorerTreeViewItem should have an automation peer");
                    if (automationChild != null)
                    {
                        automationChildren.Add(automationChild);
                    }
                }
            }
            return automationChildren;
        }
    }
}
