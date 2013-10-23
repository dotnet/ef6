// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;

    internal class SearchTickAdorner : Adorner
    {
        private readonly List<ExplorerEFElement> _explorerElements = new List<ExplorerEFElement>();
        private bool _explorerElementsAreSorted;
        private Point _center;
        //private bool _isSelected = false;
        private readonly double _tickSize;
        private readonly Brush _brush;

        public SearchTickAdorner(FrameworkElement adornedElement, ExplorerEFElement explorerElement, ExplorerFrame explorerFrame)
            : this(SearchAdornerDecorator.GetAdornerY(adornedElement, explorerElement, explorerFrame), adornedElement)
        {
            AddExplorerElement(explorerElement);
        }

        public SearchTickAdorner(double adornerY, FrameworkElement adornedElement)
            : base(adornedElement)
        {
            _brush = adornedElement.FindResource(new ComponentResourceKey(typeof(ExplorerContent), "SearchResultBrush")) as Brush;
            var tooltip = new ToolTip();
            tooltip.Content = null;
            ToolTip = tooltip;

            _tickSize = GetTickSize(adornedElement);

            // The center point of of the adorner shape that will be rendered
            _center = new Point(adornedElement.ActualWidth / 2, adornerY);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Cursor = Cursors.Hand;
            MouseLeftButtonUp += SearchTickAdorner_MouseLeftButtonUp;
        }

        internal static double GetTickSize(FrameworkElement adornedElement)
        {
            return (adornedElement.ActualWidth * 5) / 9;
        }

        internal void AddExplorerElement(ExplorerEFElement explorerElement)
        {
            _explorerElements.Add(explorerElement);
            _explorerElementsAreSorted = false;
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            base.OnToolTipOpening(e);

            EnsureToolTipContent();
        }

        private void EnsureToolTipContent()
        {
            var toolTip = ToolTip as ToolTip;

            if (toolTip != null)
            {
                var toolTipText = new StringBuilder();
                var first = true;
                const int maxItemsInToolTip = 5;
                var countItems = 0;
                foreach (var seXsdInfo in ExplorerElements)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        toolTipText.Append(Environment.NewLine);
                    }
                    toolTipText.Append(seXsdInfo.Name);
                    countItems++;
                    if (countItems == maxItemsInToolTip)
                    {
                        toolTipText.Append(Environment.NewLine);
                        toolTipText.Append("...");
                        break;
                    }
                }
                toolTip.Content = toolTipText.ToString();
            }
        }

        internal string GetToolTipText()
        {
            EnsureToolTipContent();
            return ((ToolTip)ToolTip).Content as string;
        }

        internal IEnumerable<ExplorerEFElement> ExplorerElements
        {
            get
            {
                if (!_explorerElementsAreSorted)
                {
                    _explorerElements.Sort(ExplorerHierarchyComparer.Instance);
                    _explorerElementsAreSorted = true;
                }
                return _explorerElements;
            }
        }

        private static ExplorerFrame GetExplorerFrame(FrameworkElement element)
        {
            return ExplorerUtility.FindVisualAncestorOfType<ExplorerFrame>(element);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AdornerAutomationPeer(this);
        }

        internal static Size GetSize(double tickSize)
        {
            return new Size(tickSize, 6);
        }

        internal Rect Bounds
        {
            get
            {
                var size = GetSize(_tickSize);
                return new Rect(_center.X - size.Width / 2, _center.Y - size.Height / 2, size.Width, size.Height);
            }
        }

        internal ExplorerEFElement ExplorerElement
        {
            get { return ExplorerElements.FirstOrDefault(); }
        }

        internal ExplorerTreeViewItem TreeViewItem
        {
            get { return GetExplorerFrame(this).GetTreeViewItem(ExplorerElement, false); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = GetRectangle(_center, _tickSize);
            drawingContext.DrawRectangle(_brush, null, rect);
        }

        private void SearchTickAdorner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GetExplorerFrame((FrameworkElement)sender).SelectTreeViewItem(ExplorerElement);
        }

        internal static Size GetRectangleSize(double tickSize)
        {
            var size = GetSize(tickSize);
            return new Size(size.Width, size.Height / 2);
        }

        private static Rect GetRectangle(Point center, double tickSize)
        {
            var size = GetRectangleSize(tickSize);
            return new Rect(center.X - size.Width / 2, center.Y - (size.Height + 1) / 2, size.Width, size.Height);
        }

        //    private static Geometry GetTriangleGeometry(Point center, double tickSize, bool pointRight) {
        //        Size size = GetSize(tickSize);
        //        StreamGeometry geometry = new StreamGeometry();
        //        geometry.FillRule = FillRule.Nonzero;

        //        using (StreamGeometryContext ctx = geometry.Open()) {
        //            // Begin the triangle. Notice that the shape is set to 
        //            // be closed so only two lines need to be specified below to make the triangle.
        //            ctx.BeginFigure(new Point(-size.Height, -size.Height * 0.75), true /* is filled */, true /* is closed */);

        //            // Draw a line to the next specified point.
        //            ctx.LineTo(new Point(0, 0), true /* is stroked */, false /* is smooth join */);

        //            // Draw another line to the next specified point.
        //            ctx.LineTo(new Point(-size.Height, size.Height * 0.75), true /* is stroked */, false /* is smooth join */);
        //        }

        //        TransformGroup transform = new TransformGroup();
        //        transform.Children.Add(new TranslateTransform(center.X, center.Y));
        //        if (!pointRight)
        //            transform.Children.Add(new RotateTransform(180, center.X, center.Y));
        //        geometry.Transform = transform;
        //        geometry.Freeze();

        //        return geometry;                        
        //    }
    }

    internal class AdornerAutomationPeer : FrameworkElementAutomationPeer
    {
        public AdornerAutomationPeer(SearchTickAdorner owner)
            : base(owner)
        {
            // do nothing
        }

        protected override string GetClassNameCore()
        {
            return "SearchTickAdorner";
        }

        protected override Point GetClickablePointCore()
        {
            var adornerBounds = GetBoundingRectangleCore();
            return new Point(adornerBounds.Left + adornerBounds.Width / 2, adornerBounds.Top + adornerBounds.Height / 2);
        }

        protected override Rect GetBoundingRectangleCore()
        {
            var baseRect = base.GetBoundingRectangleCore();
            var adornerBounds = ((SearchTickAdorner)Owner).Bounds;
            return new Rect(baseRect.X + adornerBounds.X, baseRect.Y + adornerBounds.Y, adornerBounds.Width, adornerBounds.Height);
        }

        protected override string GetItemStatusCore()
        {
            var itemStatus = new StringBuilder();
            var searchTickAdorner = (SearchTickAdorner)Owner;

            foreach (var seXsdInfo in searchTickAdorner.ExplorerElements)
            {
                itemStatus.Append("[Name]");
                itemStatus.Append(seXsdInfo.Name);
            }

            return itemStatus.ToString();
        }
    }
}
