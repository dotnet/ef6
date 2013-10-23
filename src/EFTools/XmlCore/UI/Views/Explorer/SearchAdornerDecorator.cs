// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class SearchAdornerDecorator : AdornerDecorator
    {
        private readonly Dictionary<double, SearchTickAdorner> _adorners = new Dictionary<double, SearchTickAdorner>(200);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FrameworkElementAutomationPeer(this);
        }

        internal void ResetAdorners()
        {
            _adorners.Clear();
        }

        internal void AddAdorner(
            AdornerLayer treeViewAdornerLayer, FrameworkElement adornedElement, ExplorerEFElement explorerElement,
            ExplorerFrame explorerFrame)
        {
            var adornerY = GetAdornerY(adornedElement, explorerElement, explorerFrame);

            if (adornerY >= 0)
            {
                SearchTickAdorner adorner;
                if (!_adorners.TryGetValue(adornerY, out adorner))
                {
                    adorner = new SearchTickAdorner(adornerY, adornedElement);
                    _adorners[adornerY] = adorner;
                    treeViewAdornerLayer.Add(adorner);

                    // adding adorners in batches of 100 - see bug: Windows OS Bugs 1750717 
                    if ((_adorners.Count % 100) == 0)
                    {
                        treeViewAdornerLayer.UpdateLayout();
                    }
                }

                adorner.AddExplorerElement(explorerElement);
            }
        }

        internal static double GetAdornerY(FrameworkElement adornedElement, ExplorerEFElement explorerElement, ExplorerFrame explorerFrame)
        {
            var treeViewItemY = explorerFrame.GetY(explorerFrame.GetTreeViewItem(explorerElement, true));

            // The adorner Y offset in the scrollbar adornedElement
            var size = SearchTickAdorner.GetRectangleSize(SearchTickAdorner.GetTickSize(adornedElement));
            var padding = (Thickness)adornedElement.GetValue(Border.PaddingProperty);
            var y = padding.Top +
                    ((adornedElement.ActualHeight - padding.Top - padding.Bottom) * treeViewItemY)
                    / (explorerFrame.ScrollViewer.ExtentHeight);
            return y - (y % size.Height) + size.Height;
        }
    }
}
