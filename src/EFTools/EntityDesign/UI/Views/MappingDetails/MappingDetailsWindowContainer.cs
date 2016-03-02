// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.EntityDesigner;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class MappingDetailsWindowContainer : UserControl, ITreeGridDesignerToolWindowContainer
    {
        private readonly MappingDetailsWindow _toolWindow;
        private readonly Control _mainControl;
        private const int Border = 1;
        private EditingContext _hostContext;

        public MappingDetailsWindowContainer(MappingDetailsWindow toolWindow, Control mainControl)
        {
            Debug.Assert(toolWindow != null, "toolWindow is null.");
            Debug.Assert(mainControl != null, "mainControl is null.");

            _toolWindow = toolWindow;

            InitializeComponent();

            // ensure the colors for the watermark LinkLabel are correct by VS UX
            SetWatermarkThemedColors();

            // hide the watermark
            watermarkLabel.Visible = false;
            watermarkLabel.LinkClicked += watermarkLabel_LinkClicked;

            // add main control hosted in the tool window
            _mainControl = mainControl;
            mainControl.TabIndex = 0;
            mainControl.Bounds = new Rectangle(1, 1, 50, 50);
            mainControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            mainControl.Dock = DockStyle.Fill;
            contentsPanel.Controls.Add(mainControl);

            // adjust control sizes
            var colorHintStripWidth = 3;
#if VS12ORNEWER
            toolbar.ImageScalingSize = toolbar.ImageScalingSize.LogicalToDeviceUnits();
            foreach (var button in toolbar.Items.OfType<ToolStripButton>())
            {
                button.Size = button.Size.LogicalToDeviceUnits();
            }
            colorHintStripWidth = DpiHelper.LogicalToDeviceUnitsX(colorHintStripWidth);
#endif

            contentsPanel.Padding = new Padding(colorHintStripWidth, 0, 0, 0);

            // By default set mainControl as a top control
            contentsPanel.Controls.SetChildIndex(mainControl, 0);

            // protect against unhandled exceptions in message loop.
            WindowTarget = new SafeWindowTarget(_toolWindow, WindowTarget);
            SafeWindowTarget.ReplaceWindowTargetRecursive(_toolWindow, Controls, false);

            // set color table for toolbar to use system colors
            toolbar.Renderer = GetToolbarRenderer();
            SetToolbarThemedColors();

            UpdateToolbar();
        }

        public void SetWatermarkThemedColors()
        {
            VSHelpers.AssignLinkLabelColor(watermarkLabel);
            BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
        }

        public void SetToolbarThemedColors()
        {
            toolbar.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.CommandBarOptionsBackgroundColorKey);
            MappingDetailsImages.InvalidateCache();
            SetToolbarImages();
        }

        private static ToolStripRenderer GetToolbarRenderer()
        {
            var uiService = Package.GetGlobalService(typeof(IUIService)) as IUIService;
            if ((uiService != null))
            {
                var renderer = uiService.Styles["VsRenderer"] as ToolStripRenderer;
                if ((renderer != null))
                {
                    var toolStripProfessionalRenderer = renderer as ToolStripProfessionalRenderer;
                    if (toolStripProfessionalRenderer != null)
                    {
                        toolStripProfessionalRenderer.RoundedEdges = false;
                    }
                    return renderer;
                }
            }
            return new ToolStripProfessionalRenderer(new ProfessionalColorTable { UseSystemColors = true });
        }

        private void SetToolbarImages()
        {
            var toolbarImageList = MappingDetailsImages.GetToolbarImageList();
            tablesButton.Image = toolbarImageList.Images[MappingDetailsImages.TOOLBAR_TABLE];
            sprocsButton.Image = toolbarImageList.Images[MappingDetailsImages.TOOLBAR_SPROCS];
        }

        public void SetHintColor(Color color)
        {
            contentsPanel.BackColor = color;
        }

        public IWin32Window Window
        {
            get { return this; }
        }

        public object HostContext
        {
            get { return _hostContext; }
            set
            {
                _hostContext = value as EditingContext;
                UpdateToolbar();
            }
        }

        // <summary>
        //     Gets/sets the current display mode based on the selection in the drawing surface.
        // </summary>
        public bool WatermarkVisible
        {
            get { return watermarkLabel.Visible; }
            set
            {
                if (watermarkLabel.Visible == value)
                {
                    return;
                }
                watermarkLabel.Visible = value;
                _mainControl.Visible = !value;

                // Set top child when watermark visibility change to favorite accessbility tool
                if (value)
                {
                    contentsPanel.Controls.SetChildIndex(watermarkLabel, 0);
                    SetHintColor(Color.Transparent);
                }
                else
                {
                    contentsPanel.Controls.SetChildIndex(_mainControl, 0);
                }

                UpdateToolbar();
            }
        }

        public void SetWatermarkInfo(TreeGridDesignerWatermarkInfo watermarkInfo)
        {
            if (watermarkInfo == null)
            {
                Debug.Fail("Unexpected null value for watermarkInfo");
                return;
            }

            SetHintColor(Color.Transparent);

            watermarkLabel.Links.Clear();

            if (watermarkInfo.WatermarkText == null)
            {
                Debug.Fail("Unexpected null value for watermarkText");
                return;
            }
            watermarkLabel.Text = watermarkInfo.WatermarkText;
            if (watermarkInfo.WatermarkLinkData == null)
            {
                Debug.Fail("Unexpected null value watermarkInfo.WatermarkLinkData");
                return;
            }
            foreach (var ld in watermarkInfo.WatermarkLinkData.Where(ld => ld.LinkStart > 0))
            {
                watermarkLabel.Links.Add(new LinkLabel.Link(ld.LinkStart, ld.LinkLength, ld.LinkClickedHandler));
            }
        }

        private void watermarkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Link data could be an event handler
            var handler = e.Link.LinkData as LinkLabelLinkClickedEventHandler;
            if (handler == null)
            {
                Debug.Fail("didn't find link-clicked handler as link data!");
                return;
            }
            handler.Invoke(sender, e);
        }

        internal void watermarkLabel_LinkClickedShowExplorer(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PackageManager.Package.ExplorerWindow.Show();
        }

        internal void watermarkLabel_LinkClickedDeleteAssociation(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var association = _toolWindow.GetAssociationFromLastPrimarySelection();
            if (association == null)
            {
                Debug.Fail("association is null");
                return;
            }
            var associationSetMappings = association.AssociationSet.GetAntiDependenciesOfType<AssociationSetMapping>();
            Debug.Assert(_hostContext == _toolWindow.Context, "this.HostContext != to window.Context!");
            if (HostContext == null)
            {
                Debug.Fail("Host context is null");
            }
            else
            {
                var cpc = new CommandProcessorContext(
                    _hostContext,
                    EfiTransactionOriginator.MappingDetailsOriginatorId,
                    Resources.Tx_DeleteAssociationSetMapping);
                var cp = new CommandProcessor(cpc);
                foreach (var associationSetMapping in associationSetMappings)
                {
                    cp.EnqueueCommand(new DeleteEFElementCommand(associationSetMapping));
                }
                if (cp.CommandCount > 0)
                {
                    cp.Invoke();
                }
            }

            // reset watermark text to account for the deleted ASM.
            if (_toolWindow.CanEditMappingsForAssociation(association, false))
            {
                _toolWindow.SetUpAssociationDisplay();
            }
        }

        internal void watermarkLabel_LinkClickedDisplayAssociation(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var association = _toolWindow.GetAssociationFromLastPrimarySelection();
            if (association == null)
            {
                Debug.Fail("association is null");
                return;
            }
            if (_toolWindow.CanEditMappingsForAssociation(association, true))
            {
                _toolWindow.SetUpAssociationDisplay();
            }
        }

        // <summary>
        //     Event handler when window is being repainted.
        // </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw a bounding rect to make it look better...
            var rectangle = new Rectangle(Border + 1, Border, Width - Border * 2 - 1, Height - Border * 2);
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rectangle);

            base.OnPaint(e);
        }

        // <summary>
        //     Moves initial focus to the concents panel.
        // </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            contentsPanel.Controls[0].Focus();
            base.OnGotFocus(e);
        }

        // <summary>
        //     Wraps tab navigation around since this control is the top-level one.
        // </summary>
        protected override bool ProcessTabKey(bool forward)
        {
            if (ActiveControl == _mainControl)
            {
                toolbar.Select();
                if (tablesButton.Checked)
                {
                    tablesButton.Select();
                }
                else
                {
                    sprocsButton.Select();
                }
            }
            else
            {
                _mainControl.Select();
            }

            return true;
        }

        // <summary>
        //     Propagates font to invisible controls.
        // </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            watermarkLabel.Font = Font;
        }

        internal void UpdateToolbar()
        {
            tablesButton.Enabled = false;
            tablesButton.Checked = false;
            sprocsButton.Enabled = false;
            sprocsButton.Checked = false;

            // TODO: do we really need to do this?
            // Ensure that the sproc button text is reset
            sprocsButton.Text = Resources.MappingDetails_SProcsButtonText;

            if (_hostContext == null)
            {
                return;
            }

            // Check the EnableEntityDesignerCommandsBehavior to see if the sproc button is allowed.
            var mappingDetailsInfo = _hostContext.Items.GetValue<MappingDetailsInfo>();
            if (mappingDetailsInfo.ViewModel == null)
            {
                return;
            }

            var entity = mappingDetailsInfo.ViewModel.RootNode.ModelItem as ConceptualEntityType;
            Debug.Assert(
                !(mappingDetailsInfo.ViewModel.RootNode.ModelItem is EntityType) || entity != null, "EntityType is not ConceptualEntityType");

            if (entity != null
                && entity.Abstract.Value)
            {
                // leave sprocs not enabled for abstract entity types
                tablesButton.Enabled = true;
                tablesButton.Checked = true;
            }
            else
            {
                if (mappingDetailsInfo.EntityMappingMode == EntityMappingModes.Tables)
                {
                    tablesButton.Enabled = true;
                    sprocsButton.Enabled = true;
                    tablesButton.Checked = true;
                }
                else if (mappingDetailsInfo.EntityMappingMode == EntityMappingModes.Functions)
                {
                    tablesButton.Enabled = true;
                    sprocsButton.Enabled = true;
                    sprocsButton.Checked = true;
                }
            }

            SetHintColor(GetFillColorFromSelection(_hostContext));
        }

        // Detects if an entity shape was selected an use its fill color as hint color
        private static Color GetFillColorFromSelection(EditingContext editingContext)
        {
            Debug.Assert(editingContext != null, "_hostContext != null");
            var selection = editingContext.Items.GetValue<EntityDesignerSelection>();
            if (selection != null)
            {
                var entityShape = selection.PrimarySelection as EntityTypeShape;
                if (entityShape != null)
                {
                    return entityShape.FillColor.Value;
                }
                //TODO: how to cross-reference to an EntityTypeShape when PrimarySelection is a property?
            }
            return Color.Transparent;
        }

        private void sprocsButton_Click(object sender, EventArgs e)
        {
            tablesButton.Checked = !sprocsButton.Checked;

            if (_hostContext != null)
            {
                var mappingDetailsInfo = _hostContext.Items.GetValue<MappingDetailsInfo>();
                mappingDetailsInfo.EntityMappingMode = EntityMappingModes.Functions;
                mappingDetailsInfo.MappingDetailsWindow.RefreshCurrentSelection();
                _mainControl.Focus();
            }
        }

        private void tablesButton_Click(object sender, EventArgs e)
        {
            sprocsButton.Checked = !tablesButton.Checked;

            if (_hostContext != null)
            {
                var mappingDetailsInfo = _hostContext.Items.GetValue<MappingDetailsInfo>();
                mappingDetailsInfo.EntityMappingMode = EntityMappingModes.Tables;
                mappingDetailsInfo.MappingDetailsWindow.RefreshCurrentSelection();
                _mainControl.Focus();
            }
        }
    }
}
