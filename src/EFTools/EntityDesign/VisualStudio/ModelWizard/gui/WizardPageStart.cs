// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if VS12
using Microsoft.VisualStudio.PlatformUI;
#endif

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.WizardFramework;

    /// <summary>
    ///     This is the first page in the ModelBuilder VS wizard and lets the user select whether to:
    ///     - start with an empty model or
    ///     - generate the model from a database
    /// </summary>
    /// <remarks>
    ///     To view this class in the forms designer, make it temporarily derive from
    ///     Microsoft.WizardFramework.WizardPage
    /// </remarks>
    internal partial class WizardPageStart : WizardPageBase
    {
        private const int GenerateFromDatabaseIndex = 0;
        private const int GenerateEmptyModelIndex = 1;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public WizardPageStart(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            components = new Container();

            Logo = Resources.PageIcon;
            Headline = Resources.StartPage_Title;
            Id = "WizardPageStartId";
            ShowInfoPanel = false;

            labelPrompt.Text = Resources.StartPage_PromptLabelText;
            labelPrompt.Font = LabelFont;

            // Load new ImageList with glyphs from resources
            var imageList = new ImageList(components)
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = new Size(32, 32),
                    TransparentColor = Color.Magenta
                };

            imageList.Images.Add("database.bmp", Resources.Database);
            imageList.Images.Add("EmptyModel.bmp", Resources.EmptyModel);

#if VS12
    // scale images as appropriate for screen resolution
            DpiHelper.LogicalToDeviceUnits(ref imageList);
#endif

            // Re-create ListView and add the list items so we are sure to use our string resources)
            listViewModelContents.Clear();
            listViewModelContents.ShowItemToolTips = true;
            listViewModelContents.LargeImageList = imageList;

            listViewModelContents.Items.AddRange(
                new[]
                    {
                        new ListViewItem(Resources.GenerateFromDatabaseOption, "database.bmp"),
                        new ListViewItem(Resources.EmptyModelOption, "EmptyModel.bmp")
                    });

            // Always select the first item
            listViewModelContents.MultiSelect = false;
            listViewModelContents.Items[0].Selected = true;

            listViewModelContents.Focus();
        }

        public override bool IsDataValid
        {
            get { return listViewModelContents.SelectedIndices.Count == 1; }
        }

        /// <summary>
        ///     Invoked by the VS Wizard framework when this page is entered.
        ///     Updates GUI from ModelBuilderSettings
        /// </summary>
        public override void OnActivated()
        {
            base.OnActivated();
            UpdateGuiFromSettings();
            Wizard.EnableButton(ButtonType.Finish, false);
        }

        /// <summary>
        ///     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        ///     Updates ModelBuilderSettings from the GUI
        /// </summary>
        public override bool OnDeactivate()
        {
            if (Wizard.MovingNext
                && !Wizard.WizardFinishing)
            {
                if (!OnWizardFinish())
                {
                    return false;
                }
            }

            UpdateSettingsFromGui();

            return base.OnDeactivate();
        }

        internal override bool OnWizardFinish()
        {
            UpdateSettingsFromGui();
            return true;
        }

        /// <summary>
        ///     Helper to update listbox selection from ModelBuilderSettings
        /// </summary>
        private void UpdateGuiFromSettings()
        {
            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.EmptyModel)
            {
                listViewModelContents.SelectedIndices.Add(GenerateEmptyModelIndex);
            }
            else if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
            {
                listViewModelContents.SelectedIndices.Add(GenerateFromDatabaseIndex);
            }
        }

        /// <summary>
        ///     Helper to update ModelBuilderSettings from listbox selection
        /// </summary>
        private void UpdateSettingsFromGui()
        {
            var nSelectedItemIndex = listViewModelContents.SelectedIndices[0];
            if (nSelectedItemIndex == GenerateEmptyModelIndex)
            {
                Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.EmptyModel;
            }
            else
            {
                Debug.Assert(nSelectedItemIndex == GenerateFromDatabaseIndex, "Unexpected index.");
                Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
            }
        }

        private void RemoveAllExceptFirstPage()
        {
            // remove all pages after first (must do in reverse order otherwise
            // index of later ones changes after first RemovePage())
            for (var i = (Wizard.PageCount - 1); i > 0; i--)
            {
                Wizard.RemovePage(i);
            }
        }

        /// <summary>
        ///     SelectedIndexChanged event: fired by ListBox when the selection changes
        /// </summary>
        private void listViewModelContents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewModelContents.SelectedIndices.Count > 0)
            {
                var nSelectedItemIndex = listViewModelContents.SelectedIndices[0];
                if (nSelectedItemIndex == GenerateEmptyModelIndex)
                {
                    // User selection = "Empty Model"
                    // - update hint textbox
                    textboxListViewSelectionInfo.Text = Resources.StartPage_EmptyModelText;

                    // remove all future pages
                    RemoveAllExceptFirstPage();
                }
                else if (nSelectedItemIndex == GenerateFromDatabaseIndex)
                {
                    // User selection = "Generate from database"
                    // - update hint textbox
                    textboxListViewSelectionInfo.Text = Resources.StartPage_GenerateFromDBText;

                    // remove all future pages
                    RemoveAllExceptFirstPage();

                    // add in the WizardPageDbConfig and WizardPageSelectTables pages:
                    // skip first wizard page, since it is still in the collection
                    // add in the rest
                    foreach (var page in Wizard.RegisteredPages.Skip(1))
                    {
                        Wizard.AddPage(page);
                    }
                }
            }
            Wizard.OnValidationStateChanged(this);
        }

        private void listViewModelContents_DoubleClick(object sender, EventArgs e)
        {
            if (listViewModelContents.SelectedIndices.Count > 0)
            {
                var nSelectedItemIndex = listViewModelContents.SelectedIndices[0];
                if (nSelectedItemIndex == GenerateEmptyModelIndex)
                {
                    // "Empty Model" - act as if user had clicked "Finish"
                    Wizard.OnFinish();
                }
                else
                {
                    Debug.Assert(nSelectedItemIndex == GenerateFromDatabaseIndex, "Unexpected index.");
                    // "Generate from DB" - act as if user had clicked "Next"
                    Wizard.OnNext();
                }
            }
        }
    }
}
