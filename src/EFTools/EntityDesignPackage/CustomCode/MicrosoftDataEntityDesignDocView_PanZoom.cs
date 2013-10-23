// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.PlatformUI;

    /// <summary>
    ///     This partial class adds the PanZoom controls to the main canvas.
    /// </summary>
    internal partial class MicrosoftDataEntityDesignDocView
    {
        private readonly List<Action> _themeChangedActions = new List<Action>();

        private readonly ToolTip _toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500
            };

        /// <summary>
        ///     Override the base class method so that we can
        ///     add controls to pan, zoom in, zoom out,
        ///     and zoom to 100% underneath the vertical scrollbar.
        /// </summary>
        public override VSDiagramView CreateDiagramView()
        {
            // Let the base class create and initialise the standard view
            var view = base.CreateDiagramView();
            Debug.Assert(view.DiagramClientView != null, "DiagramClientView was null");

            // Add handler for ZoomChanged event so we can persist 
            // zoom level regardless where the change came form 
            // (context menu, mouse scroll, pan/zoom panel)
            view.DiagramClientView.ZoomChanged += DiagramClientView_ZoomChanged;

            // Standard view contains sometimes a panel that occludes the pan button
            var fantomPanel = view.Controls.OfType<Panel>().FirstOrDefault();
            if (fantomPanel != null)
            {
                view.Controls.Remove(fantomPanel);
            }

            var vscroll = view.Controls.OfType<VScrollBar>().FirstOrDefault();
            Debug.Assert(vscroll != null, "couldn't find the vertical scroll bar");
            var hscroll = view.Controls.OfType<HScrollBar>().FirstOrDefault();
            Debug.Assert(hscroll != null, "couldn't find the horizontal scroll bar");

            var panelHeight = hscroll.Height;
            var panelWidth = vscroll.Width;
            var panelLeft = view.Width - panelWidth;

            // Need to resize the vertical scrollbar
            // every time the view is resized to account for the
            // pan/zoom panel.
            // NB the Resize and SizeChanged events fire too 
            // early i.e. before the VSDiagramView has resized
            // the vertical scroll bar.
            view.ClientSizeChanged += (sender, e) => vscroll.Height -= panelHeight * 3;

            // Create the panels from bottom to top 
            // and set their event handlers:

            // Pan panel
            view.Controls.Add(
                CreatePanel(
                    panelLeft,
                    view.Height - panelHeight,
                    panelWidth,
                    panelHeight,
                    "Resources.ThumbnailView.bmp",
                    Resources.AccName_ThumbnailView,
                    Resources.AccDesc_ThumbnailView,
                    mouseDownHandler: PanPanelMouseDownHandler));

            // Zoom out panel
            view.Controls.Add(
                CreatePanel(
                    panelLeft,
                    view.Height - panelHeight * 2,
                    panelWidth,
                    panelHeight,
                    "Resources.ZoomOutButton.bmp",
                    Resources.AccName_ZoomOutButton,
                    Resources.AccDesc_ZoomOutButton,
                    mouseClickHandler: (sender, e) => view.ZoomOut()));

            // Zoom to 100% panel
            view.Controls.Add(
                CreatePanel(
                    panelLeft,
                    view.Height - panelHeight * 3,
                    panelWidth,
                    panelHeight,
                    "Resources.Zoom100Button.bmp",
                    Resources.AccName_Zoom100Button,
                    Resources.AccDesc_Zoom100Button,
                    mouseClickHandler: (sender, e) => view.ZoomAtViewCenter(1)));

            // Zoom in panel
            view.Controls.Add(
                CreatePanel(
                    panelLeft,
                    view.Height - panelHeight * 4,
                    panelWidth,
                    panelHeight,
                    "Resources.ZoomInButton.bmp",
                    Resources.AccName_ZoomInButton,
                    Resources.AccDesc_ZoomInButton,
                    mouseClickHandler: (sender, e) => view.ZoomIn()));

            // Set theme colors before returning new view.
            UpdateTheme(view);

            // Hookup event handler so that we can keep colors updated if user changes theme.
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            return view;
        }

        /// <summary>
        ///     Set colors, e.g. background and watermark,
        ///     and colorize icons according to the theme
        /// </summary>
        private void UpdateTheme(VSDiagramView view)
        {
            if (view.HasWatermark)
            {
                VSHelpers.AssignLinkLabelColor(view.Watermark);
            }

            view.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ScrollBarBackgroundColorKey);

            foreach (var action in _themeChangedActions)
            {
                action();
            }

            view.Invalidate();
        }

        /// <summary>
        ///     Handle updates to VS theme
        /// </summary>
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            UpdateTheme(CurrentDesigner);
        }

        /// <summary>
        ///     Handler for ZoomChanged event that will persist current zoom level when changed
        /// </summary>
        private void DiagramClientView_ZoomChanged(object sender, DiagramEventArgs e)
        {
            var diagram = CurrentDiagram as EntityDesignerDiagram;
            var docData = DocData as MicrosoftDataEntityDesignDocData;
            // make sure that the Model Diagram has already been created or translated before persisting ZoomLevel
            if (diagram == null
                || docData == null
                || !docData.IsModelDiagramLoaded)
            {
                return;
            }
            try
            {
                diagram.PersistZoomLevel();
            }
            catch (FileNotEditableException fileNotEditableException)
            {
                VsUtils.ShowErrorDialog(fileNotEditableException.Message);
            }
        }

        /// <summary>
        ///     Display the pan window on the mouse down.
        /// </summary>
        private void PanPanelMouseDownHandler(object sender, MouseEventArgs e)
        {
            var diagram = CurrentDiagram as EntityDesignerDiagram;
            if (diagram == null)
            {
                return;
            }

            // store off whether we are showing the grid
            var showingGrid = diagram.ShowGrid;

            // turn off the grid before we build our thumbnail 
            // (without this, VS hangs while generating the thumbnail)
            diagram.ShowGrid = false;

            // we pass the parent Panel control so that the thumbnail view centers on it
            using (var thumbnailViewForm = 
                new ThumbnailViewForm(((Control)sender).Parent, CurrentDesigner.DiagramClientView))
            {
                thumbnailViewForm.ShowDialog();
            }

            // restore the original setting
            diagram.ShowGrid = showingGrid;
        }

        /// <summary>
        ///     Create and return a new panel displaying the
        ///     image in the specified resource as a background
        ///     image.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private Panel CreatePanel(
            int left,
            int top,
            int width,
            int height,
            string imageResourceName,
            string accessibleName,
            string accessibleDescription,
            EventHandler mouseClickHandler = null,
            MouseEventHandler mouseDownHandler = null)
        {
            // Create a new panel to draw the image on
            var panel = new Panel
                {
                    Left = left,
                    Top = top,
                    Height = height,
                    Width = width,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

            Debug.Assert(imageResourceName != null, "imageResourceName != null");
            var bitmap = new Bitmap(GetType(), imageResourceName);
            var pictureBox = new PictureBox
                {
                    AccessibleName = accessibleName,
                    AccessibleDescription = accessibleDescription,
                    AccessibleRole = AccessibleRole.PushButton
                };

            _themeChangedActions.Add(
                ()
                => pictureBox.Image
                   = ThemeUtils.GetThemedButtonImage(
                       bitmap,
                       EnvironmentColors.ScrollBarBackgroundColorKey));

            if (mouseClickHandler != null)
            {
                pictureBox.Click += mouseClickHandler;
            }

            if (mouseDownHandler != null)
            {
                pictureBox.MouseDown += mouseDownHandler;
            }

            _toolTip.SetToolTip(pictureBox, accessibleDescription);

            panel.Controls.Add(pictureBox);
            panel.BringToFront();
            return panel;
        }
    }
}
