// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    internal static class ExplorerUtility
    {
        internal static ScrollBar FindFirstVerticalScrollBar(Visual element)
        {
            var childScrollBars = GetTypeDescendents(element, typeof(ScrollBar));
            foreach (var childScrollBar in childScrollBars)
            {
                var scrollBar = childScrollBar as ScrollBar;
                if (scrollBar != null
                    && scrollBar.Orientation == Orientation.Vertical)
                {
                    return scrollBar;
                }
            }
            return null;
        }

        internal static IEnumerable<Visual> GetTypeDescendents(Visual element, Type type)
        {
            foreach (var child in GetChildren(element))
            {
                if (child.GetType() == type)
                {
                    yield return child;
                }
            }
            foreach (var child in GetChildren(element))
            {
                foreach (var descendentOfType in GetTypeDescendents(child, type))
                {
                    yield return descendentOfType;
                }
            }
        }

        internal static IEnumerable<Visual> GetChildren(Visual element)
        {
            var count = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as Visual;
                if (child != null)
                {
                    yield return child;
                }
            }
        }

        internal static T FindLogicalAncestorOfType<T>(FrameworkElement element) where T : FrameworkElement
        {
            DependencyObject e = element;
            while (e != null)
            {
                var returnValue = e as T;
                if (returnValue != null)
                {
                    return (T)e;
                }
                e = LogicalTreeHelper.GetParent(e);
            }
            return null;
        }

        internal static T FindVisualAncestorOfType<T>(FrameworkElement element) where T : FrameworkElement
        {
            DependencyObject e = element;
            while (e != null)
            {
                var returnValue = e as T;
                if (returnValue != null)
                {
                    return (T)e;
                }
                e = VisualTreeHelper.GetParent(e);
            }
            return null;
        }

        internal static FrameworkElement GetTreeViewItemPartHeader(TreeViewItem treeViewItem)
        {
            if (treeViewItem != null)
            {
                var grid = VisualTreeHelper.GetChild(treeViewItem, 0) as Grid;
                if (grid != null)
                {
                    var border = VisualTreeHelper.GetChild(grid, 0) as Border;
                    if (border != null
                        && border.Name == "PART_Header")
                    {
                        var panel = VisualTreeHelper.GetChild(border, 0) as StackPanel;
                        return panel;
                    }
                }
            }

            Debug.Assert(false, "Should have returned PART_Header's child panel");
            return null;
        }
    }
}
