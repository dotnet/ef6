// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Utilities;
    using Microsoft.WizardFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
////#if VS12ORNEWER
////    using Microsoft.VisualStudio.PlatformUI;
////#endif

    // <summary>
    //     This is the first page in the ModelBuilder VS wizard and lets the user select whether to:
    //     - start with an empty model or
    //     - generate the model from a database
    // </summary>
    // <remarks>
    //     To view this class in the forms designer, make it temporarily derive from
    //     Microsoft.WizardFramework.WizardPage
    // </remarks>
    internal partial class WizardPageStart : WizardPageBase
    {
        internal static readonly int GenerateFromDatabaseIndex = 0;
        internal static readonly int GenerateEmptyModelIndex = 1;
        internal static readonly int GenerateEmptyModelCodeFirstIndex = 2;
        internal static readonly int GenerateCodeFirstFromDatabaseIndex = 3;

        private static readonly IDictionary<string, string> _templateContent = new Dictionary<string, string>();
        private readonly bool _codeFirstAllowed;
        private readonly ConfigFileUtils _configFileUtils;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public WizardPageStart(ModelBuilderWizardForm wizard, ConfigFileUtils configFileUtils = null)
            : base(wizard)
        {
            InitializeComponent();

            _codeFirstAllowed = CodeFirstAllowed(Wizard.ModelBuilderSettings);
            _configFileUtils = configFileUtils
                               ?? new ConfigFileUtils(wizard.Project, wizard.ServiceProvider, wizard.ModelBuilderSettings.VSApplicationType);

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

            var dpi = (int)DpiAwareness.GetDpi(listViewModelContents);
            imageList.Images.Add("database.png",
                ImageManifestUtils.Instance.GetBitmap(ImageManifestUtils.DatabaseImageMoniker, dpi, 32));
            imageList.Images.Add("EmptyModel.png",
                ImageManifestUtils.Instance.GetBitmap(ImageManifestUtils.EmptyModelImageMoniker, dpi, 32));
            imageList.Images.Add("EmptyModelCodeFirst.png",
                ImageManifestUtils.Instance.GetBitmap(ImageManifestUtils.EmptyModelCodeFirstImageMoniker, dpi, 32));
            imageList.Images.Add("CodeFirstFromDatabase.png",
                ImageManifestUtils.Instance.GetBitmap(ImageManifestUtils.CodeFirstFromDatabaseImageMoniker, dpi, 32));

            // Re-create ListView and add the list items so we are sure to use our string resources)
            listViewModelContents.Clear();
            listViewModelContents.ShowItemToolTips = true;
            listViewModelContents.LargeImageList = imageList;

            listViewModelContents.Items.AddRange(
                new[]
                {
                    new ListViewItem(Resources.GenerateFromDatabaseOption, "database.png"),
                    new ListViewItem(Resources.EmptyModelOption, "EmptyModel.png"),
                });

            if (NetFrameworkVersioningHelper.TargetNetFrameworkVersion(wizard.ModelBuilderSettings.Project, Wizard.ServiceProvider) >=
                NetFrameworkVersioningHelper.NetFrameworkVersion4)
            {
                listViewModelContents.Items.Add(new ListViewItem(Resources.EmptyModelCodeFirstOption, "EmptyModelCodeFirst.png"));
                listViewModelContents.Items.Add(new ListViewItem(Resources.CodeFirstFromDatabaseOption, "CodeFirstFromDatabase.png"));
            }

            // Always select the first item
            listViewModelContents.MultiSelect = false;
            listViewModelContents.Items[0].Selected = true;

            listViewModelContents.Focus();
        }

        public override bool IsDataValid
        {
            get { return listViewModelContents.SelectedIndices.Count == 1; }
        }

        public override bool OnActivate()
        {
            // Prevents flickering if the user provides a name of the model that
            // conflicts with an existing file. If this happens we block activation
            // of the next page so the wizard will want to re-activate this page. 
            // Beacuse we close the wizard form anyways we can block activating 
            // this page which will prevent flickering.
            return base.OnActivate() && !Wizard.FileAlreadyExistsError;
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is entered.
        //     Updates GUI from ModelBuilderSettings
        // </summary>
        public override void OnActivated()
        {
            base.OnActivated();
            UpdateGuiFromSettings();
            Wizard.EnableButton(ButtonType.Finish, false);
        }

        // used for testing/mocking
        protected virtual int GetSelectedOptionIndex()
        {
            return listViewModelContents.SelectedIndices[0];
        }

        // used for testing/mocking
        protected virtual bool AnyItemSelected
        {
            get { return listViewModelContents.SelectedIndices.Count > 0; }
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        //     Updates ModelBuilderSettings from the GUI
        // </summary>
        public override bool OnDeactivate()
        {
            var modelPath = 
                CreateModelFileInfo(Wizard.ModelBuilderSettings, GetNewModelFileExtension(Wizard.ModelBuilderSettings));

            // if we threw the exception here it would be swallowed and then the "Add New Item" dialog
            // would be closed. Therefore we set the flag so that the exception is thrown from the 
            // ModelObjectItemWizard which will make the "Add New Item" dialog re-appear which allows
            // the user to enter a different name.
            Wizard.FileAlreadyExistsError = !VerifyModelFilePath(modelPath);

            if (!Wizard.FileAlreadyExistsError)
            {
                UpdateSettingsFromGui(GetSelectedOptionIndex(), modelPath);

                return base.OnDeactivate();
            }
            else
            {
                var deactivateResult = base.OnDeactivate();
                Wizard.Close();
                return deactivateResult;
            }
        }

        private static string GetNewModelFileExtension(ModelBuilderSettings settings)
        {
            switch (settings.GenerationOption)
            {
                case ModelGenerationOption.GenerateFromDatabase:
                case ModelGenerationOption.EmptyModel:
                    return "edmx";

                default :
                    Debug.Assert(
                        settings.GenerationOption == ModelGenerationOption.EmptyModelCodeFirst || 
                        settings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase , 
                        "unexpected generation option");

                    return
                        VsUtils.GetLanguageForProject(settings.Project) == LangEnum.VisualBasic
                            ? FileExtensions.VbExt
                            : FileExtensions.CsExt;
            }
        }

        // <summary>
        //     Helper to update ModelBuilderSettings from listbox selection
        // </summary>
        private void UpdateSettingsFromGui(int selectedOptionIndex, string modelPath)
        {
            Wizard.ModelBuilderSettings.ModelPath = modelPath;

            if (selectedOptionIndex == GenerateEmptyModelIndex)
            {
                Debug.Assert(
                    Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.EmptyModel, 
                    "Generation option not updated correctly");

                Wizard.ModelBuilderSettings.ModelBuilderEngine = null;

                LazyInitialModelContentsFactory.AddSchemaSpecificReplacements(
                    Wizard.ModelBuilderSettings.ReplacementDictionary,
                    Wizard.ModelBuilderSettings.TargetSchemaVersion);
            }
            else if (selectedOptionIndex == GenerateFromDatabaseIndex)
            {
                Debug.Assert(
                    Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase,
                    "Generation option not updated correctly");
                Debug.Assert(Wizard.ModelBuilderSettings.VsTemplatePath != null, "Invalid vstemplate path.");

                Wizard.ModelBuilderSettings.ModelBuilderEngine =
                    new EdmxModelBuilderEngine(
                        new LazyInitialModelContentsFactory(
                            GetEdmxTemplateContent(Wizard.ModelBuilderSettings.VsTemplatePath),
                            Wizard.ModelBuilderSettings.ReplacementDictionary));
            }
            else if (selectedOptionIndex == GenerateCodeFirstFromDatabaseIndex)
            {
                Debug.Assert(
                    Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase,
                    "Generation option not updated correctly");

                Wizard.ModelBuilderSettings.ModelBuilderEngine = new CodeFirstModelBuilderEngine();
            }
            else
            {
                Debug.Assert(selectedOptionIndex == GenerateEmptyModelCodeFirstIndex, "Unexpected index.");

                Debug.Assert(
                    (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.EmptyModelCodeFirst
                     && selectedOptionIndex == GenerateEmptyModelCodeFirstIndex),
                    "Generation option not updated correctly");

                Wizard.ModelBuilderSettings.ModelBuilderEngine = null;
                Wizard.ModelBuilderSettings.AppConfigConnectionPropertyName =
                    ConnectionManager.GetUniqueConnectionStringName(
                        _configFileUtils, Wizard.ModelBuilderSettings.ModelName);

                // for CodeFirst empty model we always add a localdb connection
                var initialCatalog = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0}.{1}", 
                    Wizard.ModelBuilderSettings.ReplacementDictionary["$rootnamespace$"], 
                    Wizard.ModelBuilderSettings.ModelName);

                Wizard.ModelBuilderSettings.SaveConnectionStringInAppConfig = true;
                var defaultConnectionString =
                    ConnectionManager.CreateDefaultLocalDbConnectionString(initialCatalog);

                Wizard.ModelBuilderSettings.SetInvariantNamesAndConnectionStrings(
                    ServiceProvider, Wizard.Project, ConnectionManager.SqlClientProviderName,
                    defaultConnectionString, defaultConnectionString, isDesignTime: false);
            }
        }

        private static string CreateModelFileInfo(ModelBuilderSettings settings, string extension)
        {
            return Path.ChangeExtension(Path.Combine(settings.NewItemFolder, settings.ModelName), extension);
        }

        // protected virtual for mocking/testing
        protected virtual bool VerifyModelFilePath(string modelFilePath)
        {
            var modelFileInfo = new FileInfo(modelFilePath);

            if (modelFileInfo.Exists)
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Design.Resources.ModelObjectItemWizard_FileAlreadyExists,
                        Path.GetFileName(modelFileInfo.FullName)));

                return false;
            }

            return true;
        }

        // <summary>
        //     Helper to update listbox selection from ModelBuilderSettings
        // </summary>
        private void UpdateGuiFromSettings()
        {
            switch (Wizard.ModelBuilderSettings.GenerationOption)
            {
                case ModelGenerationOption.EmptyModel:
                    listViewModelContents.SelectedIndices.Add(GenerateEmptyModelIndex);
                    break;
                case ModelGenerationOption.GenerateFromDatabase:
                    listViewModelContents.SelectedIndices.Add(GenerateFromDatabaseIndex);
                    break;
                case ModelGenerationOption.EmptyModelCodeFirst:
                    listViewModelContents.SelectedIndices.Add(GenerateEmptyModelCodeFirstIndex);
                    break;
                case ModelGenerationOption.CodeFirstFromDatabase:
                    listViewModelContents.SelectedIndices.Add(GenerateCodeFirstFromDatabaseIndex);
                    break;
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

        // <summary>
        //     SelectedIndexChanged event: fired by ListBox when the selection changes
        // </summary>
        private void listViewModelContents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AnyItemSelected)
            {
                RemoveAllExceptFirstPage();

                var nSelectedItemIndex = GetSelectedOptionIndex();
                if (nSelectedItemIndex == GenerateEmptyModelIndex)
                {
                    textboxListViewSelectionInfo.Text = Resources.StartPage_EmptyModelText;
                    Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.EmptyModel;

                    Wizard.OnValidationStateChanged(this);
                }
                else if (nSelectedItemIndex == GenerateFromDatabaseIndex)
                {
                    textboxListViewSelectionInfo.Text = Resources.StartPage_GenerateFromDBText;
                    Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateFromDatabase;

                    AddPagesForReverseEngineerDb();

                    Wizard.OnValidationStateChanged(this);
                }
                else if (nSelectedItemIndex == GenerateEmptyModelCodeFirstIndex)
                {
                    Debug.Assert(
                        NetFrameworkVersioningHelper.TargetNetFrameworkVersion(Wizard.ModelBuilderSettings.Project, Wizard.ServiceProvider)
                        > NetFrameworkVersioningHelper.NetFrameworkVersion3_5, "Option should be disabled for .NET Framework 3.5");

                    Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.EmptyModelCodeFirst;

                    textboxListViewSelectionInfo.Text =
                        _codeFirstAllowed
                            ? Resources.StartPage_EmptyModelCodeFirstText
                            : string.Format(
                                CultureInfo.InvariantCulture, "{0}\r\n{1}",
                                Resources.StartPage_CodeFirstSupportedOnlyForEF6, Resources.StartPage_EmptyModelCodeFirstText);

                    Wizard.OnValidationStateChanged(this);
                    Wizard.EnableButton(ButtonType.Finish, _codeFirstAllowed);
                }
                else
                {
                    Debug.Assert(nSelectedItemIndex == GenerateCodeFirstFromDatabaseIndex, "Unexpected Index");

                    Wizard.ModelBuilderSettings.GenerationOption = ModelGenerationOption.CodeFirstFromDatabase;

                    textboxListViewSelectionInfo.Text = 
                        _codeFirstAllowed ? 
                            Resources.StartPage_CodeFirstFromDatabaseText :
                            string.Format(
                                CultureInfo.InvariantCulture, "{0}\r\n{1}",
                                Resources.StartPage_CodeFirstSupportedOnlyForEF6, Resources.StartPage_CodeFirstFromDatabaseText);

                    AddPagesForReverseEngineerDb();

                    Wizard.OnValidationStateChanged(this);

                    Wizard.EnableButton(ButtonType.Next, _codeFirstAllowed);
                }
            }
        }

        private void AddPagesForReverseEngineerDb()
        {
            Debug.Assert(
                GetSelectedOptionIndex() == GenerateFromDatabaseIndex || 
                GetSelectedOptionIndex() == GenerateCodeFirstFromDatabaseIndex,
                "Should be called only for reverse engineer database workflows");

            // add in the WizardPageDbConfig and WizardPageSelectTables pages:
            // skip first wizard page, since it is still in the collection
            // add in the rest
            foreach (var page in Wizard.RegisteredPages.Skip(1))
            {
                Wizard.AddPage(page);
            }
        }

        // internal for testing
        internal void listViewModelContents_DoubleClick(object sender, EventArgs e)
        {
            if (AnyItemSelected)
            {
                var nSelectedItemIndex = GetSelectedOptionIndex();
                if (nSelectedItemIndex == GenerateEmptyModelIndex)
                {
                    // "Empty Model" - act as if user had clicked "Finish"
                    Wizard.OnFinish();
                }
                else if (nSelectedItemIndex == GenerateFromDatabaseIndex)
                {
                    // "Generate from DB" - act as if user had clicked "Next"
                    Wizard.OnNext();
                }
                else if(nSelectedItemIndex == GenerateEmptyModelCodeFirstIndex)
                {
                    if (CodeFirstAllowed(Wizard.ModelBuilderSettings))
                    {
                        Wizard.OnFinish();
                    }
                }
                else
                {
                    Debug.Assert(nSelectedItemIndex == GenerateCodeFirstFromDatabaseIndex, "Unexpected index.");

                    if (CodeFirstAllowed(Wizard.ModelBuilderSettings))
                    {
                        Wizard.OnNext();
                    }
                }
            }
        }

        // <summary>
        //     Return EDMX template content.
        //     The method will return the template cache value if available.
        // </summary>
        protected virtual string GetEdmxTemplateContent(string vstemplatePath)
        {
            string edmxTemplate;

            if (!_templateContent.TryGetValue(vstemplatePath, out edmxTemplate))
            {
                var fileInfo = new FileInfo(vstemplatePath);
                Debug.Assert(fileInfo.Exists, "vstemplate file does not exist");
                edmxTemplate = File.ReadAllText(Path.Combine(fileInfo.Directory.FullName, "ProjectItem.edmx"));
                _templateContent.Add(vstemplatePath, edmxTemplate);
            }
            return edmxTemplate;
        }

        private static bool CodeFirstAllowed(ModelBuilderSettings settings)
        {
            Debug.Assert(settings != null, "settings must not be null");

            // OneEF supported only for EF6 or if the project does not have any references to EF
            var entityFrameworkAssemblyVersion = VsUtils.GetInstalledEntityFrameworkAssemblyVersion(settings.Project);
            return entityFrameworkAssemblyVersion == null
                   || entityFrameworkAssemblyVersion >= RuntimeVersion.Version6;
        }

        private void listViewModelContents_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.DrawDefault = true;
                return;
            }
            // custom drawing performed to avoid truncated text on non-selected items
            // see http://entityframework.codeplex.com/workitem/2046 for more details
            e.DrawBackground();
            var image = e.Item.ImageList.Images[e.Item.ImageKey];
            Debug.Assert(image!=null, "Images shouldn't be null");
            e.Graphics.DrawImage(
                image, 
                e.Bounds.X + (e.Bounds.Width - image.Width) / 2, 
                e.Bounds.Y + 2);
            var textBounds = e.Bounds;
            textBounds.Y = e.Bounds.Y + image.Height + 5;
            TextRenderer.DrawText(
                e.Graphics, e.Item.Text, e.Item.Font, textBounds, e.Item.ForeColor,
                TextFormatFlags.WordBreak |
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.NoPadding);
        }
    }
}
