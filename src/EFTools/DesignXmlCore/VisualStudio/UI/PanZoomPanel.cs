// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Reflection;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.PlatformUI;

    /// <summary>
    ///     Diagram thumbnail control to be used in pan/zoom window and thumbnail view features.
    /// </summary>
    internal sealed class PanZoomPanel : Control
    {
        #region Private fields, types

        private DiagramClientView _diagramClientView;
        private Bitmap _diagramImage;
        private double _imageScale;

        private enum MouseMode
        {
            None,
            Move,
        }

        private MouseMode _mouseMode = MouseMode.None;

        #endregion

        #region Properties

        private DiagramClientView DiagramClientView
        {
            [DebuggerStepThrough] get { return _diagramClientView; }
        }

        private Bitmap DiagramImage
        {
            [DebuggerStepThrough] get { return _diagramImage; }
        }

        private new bool Enabled
        {
            [DebuggerStepThrough] get { return _diagramClientView != null && _diagramImage != null; }
        }

        private Diagram Diagram
        {
            [DebuggerStepThrough] get { return _diagramClientView != null ? _diagramClientView.Diagram : null; }
        }

        private Size MaximumImageSize
        {
            [DebuggerStepThrough]
            get
            {
                var size = Size;
                var location = MinimalImageOffset;
                size.Width -= location.X * 2;
                size.Height -= location.Y * 2;
                return size;
            }
        }

        private static Point MinimalImageOffset
        {
            [DebuggerStepThrough] get { return new Point(5, 5); }
        }

        private Size ImageSize
        {
            [DebuggerStepThrough]
            get
            {
                Debug.Assert(_diagramImage != null);
                return _diagramImage != null ? _diagramImage.Size : Size.Empty;
            }
            set
            {
                if (_diagramImage != null)
                {
                    if (_diagramImage.Size != value)
                    {
                        _diagramImage.Dispose();
                        _diagramImage = null;
                    }
                    else
                    {
                        return;
                    }
                }
                if (_diagramImage == null)
                {
                    _diagramImage = new Bitmap(value.Width, value.Height);
                }
            }
        }

        private Point ImageLocation
        {
            [DebuggerStepThrough]
            get
            {
                var location = MinimalImageOffset;
                var maxSize = MaximumImageSize;
                var realSize = ImageSize;
                location.Offset((maxSize.Width - realSize.Width) / 2, (maxSize.Height - realSize.Height) / 2);
                return location;
            }
        }

        private Rectangle ImageViewBounds
        {
            [DebuggerStepThrough]
            get
            {
                Debug.Assert(Enabled);
                if (Enabled)
                {
                    var viewBounds = DiagramClientView.ViewBounds;
                    var imageViewBounds = new Rectangle(DiagramToImage(viewBounds.Location), DiagramToImage(viewBounds.Size));
                    imageViewBounds.Offset(ImageLocation);
                    return imageViewBounds;
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        #endregion

        #region Coordinates translation

        private Point DiagramToImage(PointD worldPoint)
        {
            Debug.Assert(Enabled);
            if (Enabled)
            {
                var ds = DiagramClientView.WorldToDevice(new SizeD(worldPoint.X, worldPoint.Y));
                return new Point((int)(ds.Width * _imageScale), (int)(ds.Height * _imageScale));
            }
            else
            {
                return Point.Empty;
            }
        }

        private Size DiagramToImage(SizeD worldSize)
        {
            Debug.Assert(Enabled);
            if (Enabled)
            {
                Debug.Assert(Enabled);
                var ds = DiagramClientView.WorldToDevice(worldSize);
                return new Size((int)(ds.Width * _imageScale), (int)(ds.Height * _imageScale));
            }
            else
            {
                return Size.Empty;
            }
        }

        private PointD ImageToDiagram(Point imagePoint)
        {
            Debug.Assert(Enabled);
            if (Enabled)
            {
                var s = DiagramClientView.DeviceToWorld(
                    new Size(
                        (int)(imagePoint.X / _imageScale),
                        (int)(imagePoint.Y / _imageScale)));
                return new PointD(s.Width, s.Height);
            }
            else
            {
                return PointD.Empty;
            }
        }

        #endregion

        #region Diagram view control

        private void SetViewLocation(PointD viewLocation)
        {
            Debug.Assert(Enabled);
            if (Enabled)
            {
                Invalidate(Rectangle.Inflate(ImageViewBounds, 2, 2));

                var scrollUnitLength =
                    (double)
                    typeof(DiagramClientView).GetProperty("ScrollUnitLength", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DiagramClientView, new object[0]);

                DiagramClientView.HorizontalScrollPosition = (int)(viewLocation.X / scrollUnitLength);
                DiagramClientView.VerticalScrollPosition = (int)(viewLocation.Y / scrollUnitLength);

                DiagramClientView.Invalidate();

                Invalidate(Rectangle.Inflate(ImageViewBounds, 2, 2));
            }
        }

        #endregion

        #region Thumbnail image

        internal void InvalidateImage()
        {
            InvalidateImage(DiagramClientView);
        }

        internal void InvalidateImage(DiagramClientView diagramClientView)
        {
            _diagramClientView = diagramClientView;
            if (_diagramClientView != null)
            {
                var diagramSize = Diagram.Size;
                var deviceDiagramSize = DiagramClientView.WorldToDevice(diagramSize);
                var maxImageSize = MaximumImageSize;

                _imageScale = Math.Min(
                    (double)maxImageSize.Width / deviceDiagramSize.Width,
                    (double)maxImageSize.Height / deviceDiagramSize.Height);

                ImageSize = new Size(
                    (int)(deviceDiagramSize.Width * _imageScale),
                    (int)(deviceDiagramSize.Height * _imageScale));

                using (var g = Graphics.FromImage(DiagramImage))
                {
                    // Need to use background color from theme.
                    g.Clear(VSColorTheme.GetThemedColor(EnvironmentColors.DesignerBackgroundColorKey));

                    var drawMethod = typeof(Diagram).GetMethod("DrawDiagram", BindingFlags.NonPublic | BindingFlags.Instance);
                    drawMethod.Invoke(
                        Diagram, new object[]
                            {
                                g,
                                new Rectangle(0, 0, ImageSize.Width, ImageSize.Height), // fit the image
                                new PointD(0, 0), // from origin
                                (float)(_imageScale * DiagramClientView.ZoomFactor), // fit the whole diagram
                                null // don't need selection etc
                            });
                }
            }
            Invalidate();
        }

        #endregion

        #region Event overrides

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_diagramImage != null)
            {
                _diagramImage.Dispose();
                _diagramImage = null;
            }
            _diagramClientView = null;

            base.OnHandleCreated(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Enabled)
            {
                InvalidateImage();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (Enabled)
            {
                var graphics = pevent.Graphics;
                var clientRect = ClientRectangle;

                var imageLocation = ImageLocation;
                var imageSize = ImageSize;

                graphics.SetClip(new Rectangle(imageLocation, imageSize), CombineMode.Exclude);
                var diagramBackgroundBrush = Diagram.StyleSet.GetBrush(DiagramBrushes.DiagramBackground);
                Debug.Assert(diagramBackgroundBrush != null);
                graphics.FillRectangle(diagramBackgroundBrush, clientRect);
                graphics.ResetClip();

                graphics.DrawImage(DiagramImage, imageLocation.X, imageLocation.Y, imageSize.Width, imageSize.Height);

                var zoomLassoPen = Diagram.StyleSet.GetPen(DiagramPens.ZoomLasso);
                Debug.Assert(zoomLassoPen != null);
                graphics.DrawRectangle(zoomLassoPen, ImageViewBounds);
            }
            else
            {
                pevent.Graphics.Clear(SystemColors.Control);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            switch (_mouseMode)
            {
                case MouseMode.None:
                    {
                        Cursor = Cursors.SizeAll;
                        break;
                    }
                case MouseMode.Move:
                    {
                        var p = e.Location;
                        var imageLocation = ImageLocation;
                        p.Offset(-imageLocation.X, -imageLocation.Y);
                        var imageBounds = ImageViewBounds;
                        p.Offset(-imageBounds.Width / 2, -imageBounds.Height / 2);
                        SetViewLocation(ImageToDiagram(p));
                        break;
                    }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            if (_mouseMode == MouseMode.None)
            {
                _mouseMode = MouseMode.Move;
                Capture = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            Capture = false;
            _mouseMode = MouseMode.None;

            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            var p = e.Location;
            p.Offset(-ImageLocation.X, -ImageLocation.Y);
            p.Offset(-ImageViewBounds.Width / 2, -ImageViewBounds.Height / 2);
            SetViewLocation(ImageToDiagram(p));
        }

        internal void StartMove()
        {
            Debug.Assert(Enabled);
            if (!Enabled)
            {
                return;
            }

            Debug.Assert(_mouseMode == MouseMode.None);
            if (_mouseMode == MouseMode.None)
            {
                Capture = true;
                var viewRect = ImageViewBounds;
                viewRect.Offset(Location);
                Cursor.Position = PointToScreen(new Point(viewRect.Left + viewRect.Width / 2, viewRect.Top + viewRect.Height / 2));
                _mouseMode = MouseMode.Move;
            }
        }

        #endregion
    }
}
