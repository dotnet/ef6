// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.VisualStudio;

    #region IndentBitmapData structure

    /// <summary>
    ///     A structure to enable derived controls to set custom colors
    ///     and bitmap images for the indent region of the tree control.
    /// </summary>
    internal struct IndentBitmapData
    {
        private GraphicsPath myPlusPath;
        private GraphicsPath myMinusPath;
        private GraphicsPath myBoxPath;
        private bool myDisposePaths;

        /// <summary>
        ///     The color used to draw the background. Defaults to
        ///     SystemColors.Window.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        ///     The color used to draw the lines. Defaults to
        ///     SystemColors.GrayText.
        /// </summary>
        public Color LineColor { get; set; }

        /// <summary>
        ///     The dash style used to draw the background. Defaults to
        ///     DashStyle.Dot.
        /// </summary>
        public DashStyle LineStyle { get; set; }

        /// <summary>
        ///     The color used to draw the lines. Defaults to
        ///     SystemColors.GrayText.
        /// </summary>
        public Color BoxColor { get; set; }

        /// <summary>
        ///     The color used to draw the lines. Defaults to
        ///     SystemColors.WindowText.
        /// </summary>
        public Color PlusMinusColor { get; set; }

        /// <summary>
        ///     The path used to draw the plus sign. This path is drawn with a brush
        ///     using Graphics.FillPath.
        /// </summary>
        public GraphicsPath PlusPath
        {
            get { return myPlusPath; }
        }

        /// <summary>
        ///     The path used to draw the minus sign. This path is drawn with a brush
        ///     using Graphics.FillPath.
        /// </summary>
        public GraphicsPath MinusPath
        {
            get { return myMinusPath; }
        }

        /// <summary>
        ///     The path used to draw the box around the plus and minus signs. This is draw
        ///     with a pen using DrawPath.
        /// </summary>
        public GraphicsPath BoxPath
        {
            get { return myBoxPath; }
        }

        /// <summary>
        ///     Points which represent the icon for tree expander in expanded state.
        /// </summary>
        public Point[] ExpandedIconPoints { get; set; }

        /// <summary>
        ///     Points which represent the icon for tree expander in un-expanded state.
        /// </summary>
        public Point[] UnexpandedIconPoints { get; set; }

        /// <summary>
        ///     Set the graphics paths used to draw the bitmap images. All paths should be centered
        ///     at (0, 0), not anchored there.
        /// </summary>
        /// <param name="plusPath">The path used to draw the plus sign</param>
        /// <param name="minusPath">The path used to draw the minus sign</param>
        /// <param name="boxPath">The path used to draw the box around the plus/minus sign</param>
        /// <param name="disposePaths">
        ///     Should the paths be disposed in the Dispose method? Set to true
        ///     if the path was created or cloned for this structure, false if the paths are cached.
        /// </param>
        public void SetPaths(GraphicsPath plusPath, GraphicsPath minusPath, GraphicsPath boxPath, bool disposePaths)
        {
            var oldDisposePaths = myDisposePaths;
            myDisposePaths = disposePaths;
            SetPath(ref myPlusPath, plusPath, oldDisposePaths);
            SetPath(ref myMinusPath, minusPath, oldDisposePaths);
            SetPath(ref myBoxPath, boxPath, oldDisposePaths);
        }

        private static void SetPath(ref GraphicsPath oldPath, GraphicsPath newPath, bool disposeOldPath)
        {
            if (disposeOldPath && (oldPath != null))
            {
                oldPath.Dispose();
            }
            oldPath = newPath;
        }

        /// <summary>
        ///     Dispose resources held by this structure.
        /// </summary>
        public void Dispose()
        {
            if (myDisposePaths)
            {
                myDisposePaths = false;
                if (myPlusPath != null)
                {
                    myPlusPath.Dispose();
                }
                if (myMinusPath != null)
                {
                    myMinusPath.Dispose();
                }
                if (myBoxPath != null)
                {
                    myBoxPath.Dispose();
                }
            }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare IndentBitmapData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(IndentBitmapData operand1, IndentBitmapData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two IndentBitmapData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare IndentBitmapData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(IndentBitmapData operand1, IndentBitmapData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare IndentBitmapData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(IndentBitmapData operand1, IndentBitmapData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }

    #endregion // IndentBitmapData structure

    /// <summary>
    ///     A control to display ITree and IMultiColumnTree implementations
    /// </summary>
    internal partial class VirtualTreeControl
    {
        #region Virtual Tree Control Indent Bmp Generation

        // UNDONE: Fill in the rest of the IndentBitmap description when completed.
        /// <summary>
        ///     The bitmap used to draw the indent region of the tree control. The
        ///     shape of the bitmap depends on the line and button settings.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        protected Bitmap IndentBitmap
        {
            get
            {
                Debug.Assert(IsHandleCreated); // Should only be called while painting
                if (0 == (myStyleFlags & VTCStyleFlags.MaskHasIndentBitmaps))
                {
                    return null;
                }
                if (myIndentBmp == null)
                {
                    var xMid = ((myImageWidth != 0) ? (myImageWidth - MAGIC_INDENT) : myIndentWidth) / 2;
                    myIndentBmp = CreateIndentBmp(BackColor, ForeColor, myIndentWidth, xMid, myItemHeight);
                    myIndentBackgroundColor = BackColor;
                }

                return myIndentBmp;
            }
            private set
            {
                Debug.Assert(value == null); // Delayed generation in getter
                myIndentBmp = null;
            }
        }

        /// <summary>
        ///     Gets the indent bitmap using the given backcolor and forecolor.
        /// </summary>
        /// <param name="backColor"></param>
        /// <param name="foreColor"></param>
        /// <returns></returns>
        private Bitmap GetIndentBitmap(Color backColor, Color foreColor)
        {
            if (myIndentBmp == null
                || !Equals(backColor, myIndentBackgroundColor))
            {
                var xMid = ((myImageWidth != 0) ? (myImageWidth - MAGIC_INDENT) : myIndentWidth) / 2;
                myIndentBmp = CreateIndentBmp(backColor, foreColor, myIndentWidth, xMid, myItemHeight);
                myIndentBackgroundColor = backColor;
            }

            return myIndentBmp;
        }

        /// <summary>
        ///     Explicitly recreate the indent bitmap. The bitmap will regenerate automatically
        ///     when styles and other settings are changed, so explicit recreation is required
        ///     only if a derived control requires a change not generally accounted for by the
        ///     normal control. For example, a derived control could surface an 'IndentationColor'
        ///     property.
        /// </summary>
        protected void RecreateIndentBitmap()
        {
            IndentBitmap = null;
        }

        /// <summary>
        ///     Get data used to draw the indent bitmaps
        /// </summary>
        /// <param name="backColor">Color to be used in background</param>
        /// <param name="requireButtonPaths">True if paths for button drawing are required.  Paths are only required if buttons will be drawn, and XP themes are not available.</param>
        /// <param name="buttonExtent">Half the size of a button. The returned paths should be centered at (0, 0).</param>
        /// <returns>Colors and paths used to draw the indent bitmap.</returns>
        protected virtual IndentBitmapData GetIndentBitmapData(Color backColor, bool requireButtonPaths, int buttonExtent)
        {
            var data = new IndentBitmapData();
            data.BackgroundColor = backColor;
            data.LineColor = SystemColors.GrayText;
            data.LineStyle = DashStyle.Dot;
            data.BoxColor = SystemColors.GrayText;
            data.PlusMinusColor = SystemColors.WindowText;

            // Update the expander points given the button extent.
            GenerateExpanderIconPoints(buttonExtent, ref data);

            if (requireButtonPaths)
            {
                GraphicsPath plusPath;
                GraphicsPath minusPath;
                GraphicsPath boxPath;
                GeneratePlusMinusPaths(buttonExtent, out plusPath, out minusPath, out boxPath);
                data.SetPaths(plusPath, minusPath, boxPath, true);
            }
            return data;
        }

        /// <summary>
        ///     Generates the points which represent the tree expander in expanded/unexpanded state, and updates the data parameter.
        /// </summary>
        /// <param name="buttonExtent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IndentBitmapData GenerateExpanderIconPoints(int buttonExtent, ref IndentBitmapData data)
        {
            var p = (buttonExtent * 7) / 10;
            data.UnexpandedIconPoints = new[]
                {
                    new Point(-p, -p * 2),
                    new Point(-p, p * 2),
                    new Point(p, 0)
                };
            data.ExpandedIconPoints = new[]
                {
                    new Point(p, -p * 2),
                    new Point(p, p),
                    new Point(-2 * p, p)
                };
            return data;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private Bitmap CreateIndentBmp(Color backColor, Color foreColor, int xExtent, int xMid, int yExtent)
        {
            var buttons = HasButtons;
            var lines = HasLines;
            var rootLines = HasRootLines;
            var rootButtons = HasRootButtons;
            var oddItemHeight = (yExtent & 1) == 1;
            int rows;
            int columns;

            if (lines)
            {
                // The bitmap is laid out with one row for no/plus/minus buttons.
                // There are three columns for normal lines, with a fourth and
                // fifth added if root lines are also wanted.
                columns = (rootLines || rootButtons) ? 5 : 3;
                rows = buttons ? 3 : 1;
            }
            else if (buttons)
            {
                // Layout in a column to facilitate the same calculations as
                // with the fuller bitmap.
                columns = 1;
                rows = 2;
            }
            else
            {
                return null;
            }

            if (lines && oddItemHeight)
            {
                // in the odd item height case, we double the number of columns.  This is because there are two copies of each bitmap,
                // identical except dot pattern in the dotted line is reversed.  We paint these on alternating rows in the tree, to 
                // keep the vertical lines looking good.
                columns *= 2;
            }

            var buttonExtent = 0; // 1/2 the size of a button
            // yMid must be even for the indent bitmaps to draw correctly.  Horizontal gridlines occupy a pixel a the
            // bottom of each item, so take this into account.
            var yMid = ((yExtent / 2) + (HasHorizontalGridLines ? 0 : 1)) & ~1;
            if (buttons)
            {
                buttonExtent = Math.Min(yMid, xMid) / 2;
            }

            Bitmap returnBmp = null;
            Bitmap bmp = null;
            Graphics graphics = null;
            Brush backgroundBrush = null;
            Pen linePen = null;
            Pen boxPen = null;
            Brush plusMinusBrush = null;
            var themeHandle = IntPtr.Zero;
            var data = new IndentBitmapData();

            try
            {
                // themeHandle is not used when the tree expander is derived from VS theme.
                if (!UseVSThemeForTreeExpander)
                {
                    // see if XP themes are available to draw the buttons.
                    themeHandle = OpenTheme(Handle, EnableExplorerTheme); // we've already asserted handle creation at this point
                }

                data = GetIndentBitmapData(backColor, buttons && themeHandle == IntPtr.Zero, buttonExtent);

                var plusPath = data.PlusPath;
                var minusPath = data.MinusPath;
                var boxPath = data.BoxPath;

                bmp = new Bitmap(columns * xExtent, rows * yExtent);
                graphics = Graphics.FromImage(bmp);

                // Fill using a transparent brush
                EnsureBrush(ref backgroundBrush, data.BackgroundColor);
                graphics.FillRectangle(backgroundBrush, 0, 0, bmp.Width, bmp.Height);
                graphics.CompositingMode = CompositingMode.SourceCopy;

                if (!lines)
                {
                    // Draw the two buttons and get out
                    graphics.TranslateTransform(xMid, yMid);
                    EnsureBrush(ref plusMinusBrush, data.PlusMinusColor);
                    EnsurePen(ref boxPen, data.BoxColor);
                    if (UseVSThemeForTreeExpander)
                    {
                        DrawTreeExpanderPolygon(graphics, foreColor, data, false /*expanded*/);
                    }
                    else if (themeHandle != IntPtr.Zero)
                    {
                        DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, false /* expanded */);
                    }
                    else
                    {
                        if (plusPath != null)
                        {
                            graphics.FillPath(plusMinusBrush, plusPath);
                        }
                        if (boxPath != null)
                        {
                            graphics.DrawPath(boxPen, boxPath);
                        }
                    }

                    graphics.TranslateTransform(0, yExtent);

                    if (UseVSThemeForTreeExpander)
                    {
                        DrawTreeExpanderPolygon(graphics, foreColor, data, true /*expanded*/);
                    }
                    else if (themeHandle != IntPtr.Zero)
                    {
                        DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, true /* expanded */);
                    }
                    else
                    {
                        if (minusPath != null)
                        {
                            graphics.FillPath(plusMinusBrush, minusPath);
                        }
                        if (boxPath != null)
                        {
                            graphics.DrawPath(boxPen, boxPath);
                        }
                    }
                    returnBmp = bmp;
                    bmp = null;
                    return returnBmp;
                }

                // We always create this pen to support the different dash styles
                linePen = new Pen(data.LineColor);
                linePen.DashStyle = data.LineStyle;

                var oddItemOffset = 0;
                var finaloddItemOffset = (oddItemHeight && lines) ? 1 : 0;

                // loop through twice if we have an odd item height, since we need two sets of bitmaps to draw the lines smoothly.
                while (oddItemOffset <= finaloddItemOffset)
                {
                    // don't use GraphicsContainers here because GDI drawing (used for themes) doesn't respect the transform matrix.
                    // BeginContainer resets the transform matrix, so there's no way to get the information we need to draw the themed
                    // buttons in the right place.
                    graphics.ResetTransform();
                    graphics.TranslateTransform(oddItemOffset * (bmp.Width / 2), 0); // offset for odd item heights

                    // Draw the first row (no buttons)
                    var beginTransform = graphics.Transform;
                    graphics.DrawLine(linePen, xMid, oddItemOffset, xMid, yExtent);
                    graphics.TranslateTransform(xExtent, 0);
                    graphics.DrawLine(linePen, xMid, oddItemOffset, xMid, yExtent);
                    // add 2*oddItemOffset below because this is an intersection of horizontal and vertical gridlines.
                    // In the odd height case, not adding an offset creates an ugly pixel cluster, and only adding 1
                    // causes the ends of the lines to be jagged.  Adding 2 creates a bit of extra whitespace, but I believe
                    // it is visually the best compromise.
                    graphics.DrawLine(linePen, xMid + (2 * oddItemOffset), yMid, xExtent, yMid);

                    graphics.TranslateTransform(xExtent, 0);
                    graphics.DrawLine(linePen, xMid, oddItemOffset, xMid, yMid);
                    graphics.DrawLine(linePen, xMid + (2 * oddItemOffset), yMid, xExtent, yMid); // see comment about 2*oddItemOffset above.

                    if (rootLines)
                    {
                        graphics.TranslateTransform(xExtent, 0);
                        graphics.DrawLine(linePen, xMid, yMid + oddItemOffset, xMid, yExtent);
                        graphics.DrawLine(linePen, xMid + (2 * oddItemOffset), yMid, xExtent, yMid);
                            // see comment about 2*oddItemOffset above.

                        graphics.TranslateTransform(xExtent, 0);
                        graphics.DrawLine(linePen, xMid, yMid, xExtent, yMid);
                    }
                    graphics.Transform = beginTransform; // Back to beginning of line

                    if (buttons)
                    {
                        // Draw the second and third rows (plus and minus signs)
                        // These are the same except for the sign in the button, so
                        // use the same code with a different path
                        EnsureBrush(ref plusMinusBrush, data.PlusMinusColor);
                        EnsurePen(ref boxPen, data.BoxColor);
                        for (var i = 0; i < 2; i++)
                        {
                            var curPath = (i == 0) ? plusPath : minusPath;
                            graphics.TranslateTransform(0, yExtent);
                            beginTransform = graphics.Transform;

                            // First column is the same in all three rows
                            graphics.DrawLine(linePen, xMid, oddItemOffset, xMid, yExtent);

                            graphics.TranslateTransform(xExtent + xMid, yMid);
                            graphics.DrawLine(linePen, 0, -yMid + oddItemOffset, 0, -buttonExtent);
                            graphics.DrawLine(linePen, 0, ((buttonExtent + 1) & ~1) + oddItemOffset, 0, yExtent - yMid);
                            graphics.DrawLine(linePen, buttonExtent, 0, xExtent - xMid, 0);

                            if (UseVSThemeForTreeExpander)
                            {
                                DrawTreeExpanderPolygon(graphics, foreColor, data, i == 1 /*expanded*/);
                            }
                            else if (themeHandle != IntPtr.Zero)
                            {
                                DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, i == 1 /* expanded */);
                            }
                            else
                            {
                                if (curPath != null)
                                {
                                    graphics.FillPath(plusMinusBrush, curPath);
                                }
                                if (boxPath != null)
                                {
                                    graphics.DrawPath(boxPen, boxPath);
                                }
                            }

                            graphics.TranslateTransform(xExtent, 0);

                            if (UseVSThemeForTreeExpander)
                            {
                                DrawTreeExpanderPolygon(graphics, foreColor, data, i == 1 /*expanded*/);
                            }
                            else if (themeHandle != IntPtr.Zero)
                            {
                                DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, i == 1 /* expanded */);
                            }
                            else
                            {
                                if (curPath != null)
                                {
                                    graphics.FillPath(plusMinusBrush, curPath);
                                }
                                if (boxPath != null)
                                {
                                    graphics.DrawPath(boxPen, boxPath);
                                }
                            }
                            graphics.DrawLine(linePen, 0, -yMid + oddItemOffset, 0, -buttonExtent);
                            graphics.DrawLine(linePen, buttonExtent, 0, xExtent - xMid, 0);

                            if (rootButtons)
                            {
                                graphics.TranslateTransform(xExtent, 0);
                                if (UseVSThemeForTreeExpander)
                                {
                                    DrawTreeExpanderPolygon(graphics, foreColor, data, i == 1 /*expanded*/);
                                }
                                else if (themeHandle != IntPtr.Zero)
                                {
                                    DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, i == 1 /* expanded */);
                                }
                                else
                                {
                                    if (curPath != null)
                                    {
                                        graphics.FillPath(plusMinusBrush, curPath);
                                    }
                                    if (boxPath != null)
                                    {
                                        graphics.DrawPath(boxPen, boxPath);
                                    }
                                }

                                graphics.DrawLine(linePen, 0, ((buttonExtent + 1) & ~1) + oddItemOffset, 0, yExtent - yMid);
                                graphics.DrawLine(linePen, buttonExtent, 0, xExtent - xMid, 0);

                                graphics.TranslateTransform(xExtent, 0);
                                graphics.DrawLine(linePen, buttonExtent, 0, xExtent - xMid, 0);

                                if (UseVSThemeForTreeExpander)
                                {
                                    DrawTreeExpanderPolygon(graphics, foreColor, data, i == 1 /*expanded*/);
                                }
                                else if (themeHandle != IntPtr.Zero)
                                {
                                    DrawThemedButtonGlyph(themeHandle, graphics, data.BackgroundColor, buttonExtent, i == 1 /* expanded */);
                                }
                                else
                                {
                                    if (curPath != null)
                                    {
                                        graphics.FillPath(plusMinusBrush, curPath);
                                    }
                                    if (boxPath != null)
                                    {
                                        graphics.DrawPath(boxPen, boxPath);
                                    }
                                }
                            }
                            graphics.Transform = beginTransform; // Back to beginning of line
                        }
                    }

                    oddItemOffset++;
                }
                returnBmp = bmp;
                bmp = null;
            }
            finally
            {
                if (themeHandle != IntPtr.Zero)
                {
                    CloseTheme(themeHandle);
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                }
                if (bmp != null)
                {
                    bmp.Dispose();
                }
                CleanBrush(ref backgroundBrush, data.BackgroundColor);
                CleanPen(ref boxPen, data.BoxColor);
                CleanBrush(ref plusMinusBrush, data.PlusMinusColor);
                if (linePen != null)
                {
                    linePen.Dispose();
                }
                data.Dispose();
            }
            return returnBmp;
        }

        private static void GeneratePlusMinusPaths(
            int extent, out GraphicsPath plusPath, out GraphicsPath minusPath, out GraphicsPath boxPath)
        {
            var p = (extent * 7) / 10;
            var n = p * 2 + 1;
            minusPath = new GraphicsPath();
            plusPath = new GraphicsPath(FillMode.Winding);
            boxPath = new GraphicsPath();
            var rect = new Rectangle(0, 0, 0, 0);
            if (p >= 5)
            {
                // Minus sign
                rect.X = -p;
                rect.Y = -1;
                rect.Width = n;
                rect.Height = 3;
                minusPath.AddRectangle(rect);
                plusPath.AddRectangle(rect);

                // Rest of plus sign
                plusPath.StartFigure();
                rect.X = -1;
                rect.Y = -p;
                rect.Width = 3;
                rect.Height = n;
                plusPath.AddRectangle(rect);
            }
            else
            {
                // Minus sign
                rect.X = -p;
                rect.Y = 0;
                rect.Width = n;
                rect.Height = 1;
                minusPath.AddRectangle(rect);
                plusPath.AddRectangle(rect);

                // Rest of plus sign
                plusPath.StartFigure();
                rect.X = 0;
                rect.Y = -p;
                rect.Width = 1;
                rect.Height = n;
                plusPath.AddRectangle(rect);
            }

            rect.X = rect.Y = -extent;
            rect.Width = rect.Height = 2 * extent;
            boxPath.AddRectangle(rect);
        }

        private static bool SupportsExplorerTheme()
        {
            return NativeMethods.WinVistaOrHigher;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetWindowTheme(System.IntPtr,System.String,System.String)")]
        private static IntPtr OpenTheme(IntPtr handle, bool enableExplorerTheme)
        {
            var themeHandle = IntPtr.Zero;
            if (NativeMethods.WinXPOrHigher)
            {
                try
                {
                    if (enableExplorerTheme)
                    {
                        NativeMethods.SetWindowTheme(handle, NativeMethods.WC_EXPLORER, null);
                    }
                    else
                    {
                        NativeMethods.SetWindowTheme(handle, null, null);
                    }
                    themeHandle = NativeMethods.OpenThemeData(handle, NativeMethods.WC_TREEVIEW);
                }
                catch (DllNotFoundException)
                {
                    themeHandle = IntPtr.Zero; // for some reason uxtheme.dll could not be loaded, fall back on non-themed UI
                }
            }

            return themeHandle;
        }

        private static void CloseTheme(IntPtr themeHandle)
        {
            var hr = NativeMethods.CloseThemeData(themeHandle);
            if (hr != NativeMethods.S_OK)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        /// <summary>
        ///     Draws the tree expander polygon.
        /// </summary>
        private static void DrawTreeExpanderPolygon(Graphics graphics, Color foreColor, IndentBitmapData data, bool expanded)
        {
            Brush brush = null;
            Pen pen = null;
            try
            {
                if (expanded)
                {
                    EnsureBrush(ref brush, foreColor);
                    graphics.FillPolygon(brush, data.ExpandedIconPoints);
                }
                else
                {
                    EnsurePen(ref pen, foreColor);
                    graphics.DrawPolygon(pen, data.UnexpandedIconPoints);
                }
            }
            finally
            {
                CleanPen(ref pen, foreColor);
                CleanBrush(ref brush, foreColor);
            }
        }

        private static void DrawThemedButtonGlyph(
            IntPtr themeHandle, Graphics graphics, Color backgroundColor, int buttonExtent, bool expanded)
        {
            // determine glyph
            var themePartId = NativeMethods.TVP_GLYPH;
            var themeStateId = expanded ? NativeMethods.GLPS_OPENED : NativeMethods.GLPS_CLOSED;

            // get color-keyed glyph bitmap
            using (var glyph = GetThemedButtonGlyph(themeHandle, themePartId, themeStateId, backgroundColor, buttonExtent))
            {
                // get attributes so we can draw color-keyed glyph
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorKey(backgroundColor, backgroundColor);

                    // draw glyph
                    var dest = new Rectangle(-glyph.Width / 2, -glyph.Height / 2, glyph.Width, glyph.Height);
                    graphics.DrawImage(glyph, dest, 0, 0, glyph.Width, glyph.Height, GraphicsUnit.Pixel, attributes);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.DeleteDC(System.IntPtr)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.ReleaseDC(System.IntPtr,System.IntPtr)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.DrawThemeBackground(System.IntPtr,System.IntPtr,System.Int32,System.Int32,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@,System.IntPtr)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.FillRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@,System.IntPtr)")]
        private static Bitmap GetThemedButtonGlyph(
            IntPtr themeHandle, int themePartId, int themeStateId, Color backgroundColor, int buttonExtent)
        {
            Bitmap bitmap = null;
            var hScreenDC = IntPtr.Zero;
            var hBmpDC = IntPtr.Zero;
            var hBmp = IntPtr.Zero;
            var hBrush = IntPtr.Zero;

            try
            {
                // get screen device context
                hScreenDC = NativeMethods.GetDC(IntPtr.Zero);

                // get the size of the glyph
                var size = new NativeMethods.SIZE();
                var hr = NativeMethods.GetThemePartSize(
                    themeHandle, hScreenDC, themePartId, themeStateId, IntPtr.Zero, NativeMethods.THEME_SIZE.TS_DRAW, ref size);
                if (hr != NativeMethods.S_OK)
                {
                    // use default size
                    size.cx = buttonExtent * 2;
                    size.cy = buttonExtent * 2;
                }

                // create bitmap compatible with screen
                hBmpDC = NativeMethods.CreateCompatibleDC(hScreenDC);
                hBmp = NativeMethods.CreateCompatibleBitmap(hScreenDC, size.cx, size.cy);
                NativeMethods.SelectObject(hBmpDC, hBmp);

                // get rect for entire bitmap
                var rect = NativeMethods.RECT.FromXYWH(0, 0, size.cx, size.cy);

                // fill bitmap with background color
                var color = NativeMethods.RGB(backgroundColor.R, backgroundColor.G, backgroundColor.B);
                hBrush = NativeMethods.CreateSolidBrush(color);
                NativeMethods.FillRect(hBmpDC, ref rect, hBrush);

                // draw glyph
                NativeMethods.DrawThemeBackground(themeHandle, hBmpDC, themePartId, themeStateId, ref rect, IntPtr.Zero);

                // get bitmap
                bitmap = Image.FromHbitmap(hBmp);
            }
            finally
            {
                if (hScreenDC != IntPtr.Zero)
                {
                    NativeMethods.ReleaseDC(IntPtr.Zero, hScreenDC);
                }
                if (hBmpDC != IntPtr.Zero)
                {
                    NativeMethods.DeleteDC(hBmpDC);
                }
                if (hBmp != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(hBmp);
                }
                if (hBrush != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(hBrush);
                }
            }

            return bitmap;
        }

        #endregion // Virtual Tree Control Indent Bmp Generation
    }
}
