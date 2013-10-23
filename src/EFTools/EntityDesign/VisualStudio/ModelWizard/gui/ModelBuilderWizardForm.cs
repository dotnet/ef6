// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.WizardFramework;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    /// <summary>
    ///     ModelBuilder Wizard form that contains the wizard pages
    /// </summary>
    internal partial class ModelBuilderWizardForm : WizardForm
    {
        internal enum WizardMode
        {
            PerformAllFunctionality,
            PerformDatabaseConfigAndSelectTables,
            PerformSelectTablesOnly,
            PerformDBGenSummaryOnly,
            PerformDatabaseConfigAndDBGenSummary
        }

        /// <summary>
        ///     Constructor to start the wizard in the specified mode
        /// </summary>
        public ModelBuilderWizardForm(
            IServiceProvider serviceProvider, Project project, ICollection<string> metadataFileNames,
            ModelBuilderSettings modelBuilderSettings, WizardMode wizardMode)
        {
            _wizardMode = wizardMode;
            _serviceProvider = serviceProvider;
            _project = project;
            _metadataFileNames = metadataFileNames;

            // use ModelBuilderSettings passed in as parameter
            _modelBuilderSettings = modelBuilderSettings;

            Initialize();
        }

        // Keeping the lists of pages as member vars ensures
        // you get the same page back even if you use "Previous"/"Next" buttons
        private readonly List<WizardPage> _standardPages = new List<WizardPage>();

// WizardExtensionPage feature not shipping for Dev10
#if WIZARD_EXTENSION_PAGE
        private readonly List<WizardPage> _preModelGenerationPages = new List<WizardPage>();
        private readonly List<WizardPage> _postModelGenerationPages = new List<WizardPage>();
#endif

        private readonly ModelBuilderSettings _modelBuilderSettings;
        private readonly Project _project;
        private readonly ICollection<string> _metadataFileNames;
        private readonly IServiceProvider _serviceProvider;
        private readonly WizardMode _wizardMode;

        private bool _wizardCancelled;
        private bool _wizardFinishing;
        private bool _wizardFinished;

        public ModelBuilderSettings ModelBuilderSettings
        {
            get { return _modelBuilderSettings; }
        }

        internal IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public Project Project
        {
            get { return _project; }
        }

        public ICollection<string> MetadataFileNames
        {
            get { return _metadataFileNames; }
        }

        public bool WizardCancelled
        {
            get { return _wizardCancelled; }
        }

        public bool WizardFinishing
        {
            get { return _wizardFinishing; }
        }

        public bool WizardFinished
        {
            get { return _wizardFinished; }
        }

        internal WizardMode Mode
        {
            get { return _wizardMode; }
        }

        public IEnumerable<WizardPage> RegisteredPages
        {
            get
            {
                foreach (var p in _standardPages)
                {
                    yield return p;
                }
// WizardExtensionPage feature not shipping for Dev10
#if WIZARD_EXTENSION_PAGE
                foreach (var p in _preModelGenerationPages)
                {
                    yield return p;
                }
                foreach (var p in _postModelGenerationPages)
                {
                    yield return p;
                }
#endif
            }
        }

        private void Initialize()
        {
            SetFont();
            InitializeComponent();
            InitializeWizardForm();
            InitializeWizardPages();
        }

        /// <summary>
        ///     Helper to initialize GUI elements of the form
        /// </summary>
        protected void InitializeWizardForm()
        {
            ShowOrientationPanel = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            HelpKeyword = null;

            switch (_wizardMode)
            {
                case WizardMode.PerformSelectTablesOnly:
                case WizardMode.PerformDatabaseConfigAndSelectTables:
                    Title = Resources.UpdateFromDatabaseWizard_Title;
                    break;
                case WizardMode.PerformDBGenSummaryOnly:
                case WizardMode.PerformDatabaseConfigAndDBGenSummary:
                    Title = Resources.DbGenWizard_Title;
                    break;
                default:
                    Title = Resources.WizardFormDialog_Title;
                    break;
            }
        }

        /// <summary>
        ///     Helper to create &amp; initialize wizard pages depending on the mode
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected void InitializeWizardPages()
        {
            switch (_wizardMode)
            {
                case WizardMode.PerformDatabaseConfigAndSelectTables:
                    ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
                    RegisterStandardPage(new WizardPageDbConfig(this));
                    RegisterStandardPage(new WizardPageRuntimeConfig(this));
                    RegisterStandardPage(new WizardPageUpdateFromDatabase(this));
                    break;

                case WizardMode.PerformSelectTablesOnly:
                    ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateFromDatabase;
                    RegisterStandardPage(new WizardPageUpdateFromDatabase(this));
                    break;

                case WizardMode.PerformDatabaseConfigAndDBGenSummary:
                    ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateDatabaseScript;
                    RegisterStandardPage(new WizardPageDbConfig(this));
                    RegisterStandardPage(new WizardPageRuntimeConfig(this));
                    RegisterStandardPage(new WizardPageDbGenSummary(this));
                    break;

                case WizardMode.PerformDBGenSummaryOnly:
                    ModelBuilderSettings.GenerationOption = ModelGenerationOption.GenerateDatabaseScript;
                    RegisterStandardPage(new WizardPageDbGenSummary(this));
                    break;

                case WizardMode.PerformAllFunctionality:
                default:
                    // Add the Start Page
                    // rest of pages will be added by the start page (if needed)
                    RegisterStandardPage(new WizardPageStart(this));
                    RegisterStandardPage(new WizardPageDbConfig(this));
                    RegisterStandardPage(new WizardPageRuntimeConfig(this));
                    RegisterStandardPage(new WizardPageSelectTables(this));
                    break;
            }

// WizardExtensionPage feature not shipping for Dev10
#if WIZARD_EXTENSION_PAGE
            var wizardExtensionPages = EscherExtensionPointManager.LoadWizardPageExtensions();
            foreach (var importInfo in wizardExtensionPages)
            {
                try
                {
                    var metadata = importInfo.Metadata;
                    if (_wizardMode == metadata.WizardMode)
                    {
                        var wpfactory = importInfo.Value;
                        WizardPage wp = wpfactory.CreatePage(this, ModelBuilderSettings.ExtensionData);

                        if (wp != null)
                        {
                            if (metadata.WizardStage == WizardStage.PostModelGeneration)
                            {
                                RegisterPostModelGenerationPage(wp);
                            }
                            else if (metadata.WizardStage == WizardStage.PreModelGeneration)
                            {
                                RegisterPreModelGenerationPage(wp);
                            }
                            else
                            {
                                Debug.Fail("Unexpected wizard stage found");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail("Exception caught initializing wizard pages: " + e);
                }
            }

#endif

            foreach (var page in RegisteredPages)
            {
                AddPage(page);
            }
        }

        protected void RegisterStandardPage(WizardPage page)
        {
            _standardPages.Add(page);
        }

// WizardExtensionPage feature not shipping for Dev10
#if WIZARD_EXTENSION_PAGE
        protected void RegisterPreModelGenerationPage(WizardPage page)
        {
            _preModelGenerationPages.Add(page);
        }

        protected void RegisterPostModelGenerationPage(WizardPage page)
        {
            _postModelGenerationPages.Add(page);
        }
#endif

        public override void OnCancel()
        {
            _wizardCancelled = true;

            // walk through all remaining pages and tell them we are cancelling
            var page = ActivePage;
            while (null != page)
            {
                var basePage = page as WizardPageBase;
                Debug.Assert(null != basePage, "All wizard pages should inherit from WizardPageBase");
                if (null != basePage)
                {
                    basePage.OnWizardCancel();
                }
                page = NextPageFromPage(page);
            }

            base.OnCancel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // check whether wizard is closing because of clicking Finish button
            _wizardFinished = _wizardFinishing && !e.Cancel;
        }

        public override void OnFinish()
        {
            _wizardFinishing = true;

            // walk through all remaining pages to check if all data is valid
            var page = ActivePage;
            while (null != page)
            {
                var basePage = page as WizardPageBase;
                Debug.Assert(null != basePage, "All wizard pages should inherit from WizardPageBase");
                if (null != basePage)
                {
                    if (!basePage.OnWizardFinish())
                    {
                        _wizardFinishing = false;
                        GotoPage(page);
                        return;
                    }
                }
                page = NextPageFromPage(page);
            }

            base.OnFinish();
            _wizardFinishing = false;
        }

        // computes a unique connection string name based on the input base name
        internal string GetUniqueConnectionStringName(string baseConnectionStringName)
        {
            if (String.IsNullOrEmpty(baseConnectionStringName))
            {
                baseConnectionStringName = ModelConstants.DefaultEntityContainerName;
            }
            else
            {
                // replace spaces with underscores
                baseConnectionStringName = baseConnectionStringName.Replace(" ", "_");
                baseConnectionStringName = baseConnectionStringName.Replace("\t", "_");
                baseConnectionStringName = baseConnectionStringName + ModelConstants.DefaultEntityContainerName;
            }

            if (!VsUtils.IsValidIdentifier(baseConnectionStringName, Project, _modelBuilderSettings.VSApplicationType))
            {
                baseConnectionStringName = ModelConstants.DefaultEntityContainerName;
            }

            var connectionStringNames = PackageManager.Package.ConnectionManager.GetExistingConnectionStringNames(Project);

            var i = 1;
            var uniqueConnectionStringName = baseConnectionStringName;
            while (connectionStringNames.Contains(uniqueConnectionStringName))
            {
                uniqueConnectionStringName = baseConnectionStringName + i++;
            }

            return uniqueConnectionStringName;
        }

        // mark all following pages as not yet visited
        // this should be invoked if some page data has changed and following pages state is no more valid
        public void InvalidateFollowingPages()
        {
            var page = NextPage;
            while (null != page)
            {
                page.Visited = false;
                page = NextPageFromPage(page);
            }
        }

        // method to compare Model Namespace and Entity Container names - centralized for maintenance
        internal static bool ModelNamespaceAndEntityContainerNameSame(ModelBuilderSettings modelBuilderSettings)
        {
            var entityContainerName = modelBuilderSettings.AppConfigConnectionPropertyName;
            var modelNamespaceName = modelBuilderSettings.ModelNamespace;

            return (!string.IsNullOrEmpty(entityContainerName) &&
                    !string.IsNullOrEmpty(modelNamespaceName) &&
                    entityContainerName.ToUpper(CultureInfo.CurrentCulture) == modelNamespaceName.ToUpper(CultureInfo.CurrentCulture));
        }

        /// <summary>
        ///     Helper method to raise a dialog to display the encountered database connection errors
        /// </summary>
        /// <returns>the error message</returns>
        internal static string ShowDatabaseConnectionErrorDialog(Exception e)
        {
            if (null == e)
            {
                Debug.Fail("exception should not be null");
                return null;
            }

            // show the error dialog
            var errMsgWithInnerExceptions = VsUtils.ConstructInnerExceptionErrorMessage(e);
            var errMsg = string.Format(
                CultureInfo.CurrentCulture, Resources.DbConnectionErrorText, e.GetType().FullName, errMsgWithInnerExceptions);
            VsUtils.ShowErrorDialog(errMsg);

            return errMsg;
        }

        private void SetFont()
        {
            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }
        }

        internal bool IsLastPreModelGenerationPageActive()
        {
// WizardExtensionPage feature not shipping for Dev10
#if WIZARD_EXTENSION_PAGE
            if (_preModelGenerationPages.Count > 0)
            {
                return (ActivePage == _preModelGenerationPages[_preModelGenerationPages.Count - 1]);
            }
#endif

            return (ActivePageIndex == PageCount - 1);
        }
    }
}
