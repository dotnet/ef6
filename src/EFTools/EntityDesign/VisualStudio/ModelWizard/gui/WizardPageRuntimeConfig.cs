// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.ViewModels;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.WizardFramework;

    internal partial class WizardPageRuntimeConfig : WizardPageBase
    {
        private RuntimeConfigState _state;

        internal WizardPageRuntimeConfig(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            Logo = Resources.PageIcon;
            Headline = Resources.RuntimeConfigPage_Title;
            Id = "WizardPageRuntimeConfig";
            HelpKeyword = null;

            promptLabel.Font = LabelFont;
            promptLabel.Text = Resources.RuntimeConfig_Prompt;

            VSHelpers.AssignLinkLabelColor(notificationLinkLabel);
            notificationLabel.Text = Resources.RuntimeConfig_LearnMore;
        }

        public override bool OnActivate()
        {
            if (!Visited)
            {
                Initialize();
            }

            if (_state == RuntimeConfigState.Skip)
            {
                if (Wizard.MovingNext)
                {
                    // Schedule a call to Wizard.OnNext()
                    Wizard.BeginInvoke((MethodInvoker)Wizard.OnNext);
                }
                else
                {
                    Debug.Assert(Wizard.MovingPrevious);

                    // Schedule a call to Wizard.OnPrevious()
                    Wizard.BeginInvoke((MethodInvoker)Wizard.OnPrevious);
                }
            }

            return base.OnActivate();
        }

        public override void OnActivated()
        {
            Wizard.EnableButton(ButtonType.Next, _state != RuntimeConfigState.Error);

            base.OnActivated();
        }

        public override bool OnDeactivate()
        {
            if (Wizard.MovingNext)
            {
                var selectedVersion = GetSelectedVersion();
                var useLegacyProvider = selectedVersion != null
                                            ? RuntimeVersion.RequiresLegacyProvider(selectedVersion)
                                            : OptionsDesignerInfo.UseLegacyProviderDefault;

                if (Wizard.ModelBuilderSettings.UseLegacyProvider != useLegacyProvider)
                {
                    Wizard.InvalidateFollowingPages();
                }

                Wizard.ModelBuilderSettings.UseLegacyProvider = useLegacyProvider;
                Wizard.ModelBuilderSettings.TargetSchemaVersion =
                    RuntimeVersion.GetTargetSchemaVersion(
                        selectedVersion,
                        NetFrameworkVersioningHelper.TargetNetFrameworkVersion(Wizard.Project, ServiceProvider));

                VsUtils.EnsureProvider(
                    Wizard.ModelBuilderSettings.RuntimeProviderInvariantName,
                    Wizard.ModelBuilderSettings.UseLegacyProvider,
                    Wizard.Project,
                    ServiceProvider);

                Wizard.ModelBuilderSettings.ProviderManifestToken =
                    VsUtils.GetProviderManifestTokenConnected(
                        DependencyResolver.Instance,
                        Wizard.ModelBuilderSettings.RuntimeProviderInvariantName,
                        Wizard.ModelBuilderSettings.DesignTimeConnectionString);
            }

            return base.OnDeactivate();
        }

        private void HandleLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Debug.Assert(sender != null, "sender is null.");
            Debug.Assert(e != null, "e is null.");

            var url = e.Link.LinkData as string;

            if (!string.IsNullOrWhiteSpace(url))
            {
                Process.Start(url);
            }
            else
            {
                Debug.Fail("url is null or empty.");
            }
        }

        private void Initialize()
        {
            var targetFrameworkVersion = NetFrameworkVersioningHelper.TargetNetFrameworkVersion(Wizard.Project, ServiceProvider);
            var installedEntityFrameworkVersion = VsUtils.GetInstalledEntityFrameworkAssemblyVersion(Wizard.Project);

            // NOTE: Despite the fact that this provider is for design-time, we use
            //       RuntimeProviderInvariantName since this is how modern providers are
            //       registered
            var isModernProviderAvailable = VsUtils.IsModernProviderAvailable(
                Wizard.ModelBuilderSettings.RuntimeProviderInvariantName,
                Wizard.Project,
                ServiceProvider);

            var viewModel = new RuntimeConfigViewModel(
                targetFrameworkVersion,
                installedEntityFrameworkVersion,
                isModernProviderAvailable, 
                Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase);

            versionsPanel.Controls.Clear();
            versionsPanel.Controls.AddRange(viewModel.EntityFrameworkVersions.Select(CreateRadioButton).ToArray());

            if (!string.IsNullOrWhiteSpace(viewModel.Message))
            {
                notificationPictureBox.Image = GetNotificationBitmap(viewModel.State);
                notificationLabel.Text = viewModel.Message;

                if (!string.IsNullOrWhiteSpace(viewModel.HelpUrl))
                {
                    notificationLinkLabel.Links[0].LinkData = viewModel.HelpUrl;
                    notificationLinkLabel.Visible = true;
                }
                else
                {
                    notificationLinkLabel.Visible = false;
                }

                notificationPanel.Visible = true;
            }
            else
            {
                notificationPanel.Visible = false;
            }

            _state = viewModel.State;
            if (isModernProviderAvailable && _state == RuntimeConfigState.Skip)
            {
                // we are skipping this page but need to set UseLegacyProvider to false
                // on ModelBuilderSettings for later pages (and the engine) to use
                Wizard.ModelBuilderSettings.UseLegacyProvider = false;
            }
        }

        private Version GetSelectedVersion()
        {
            var selectedVersion = versionsPanel.Controls.OfType<RadioButton>().Where(b => b.Checked)
                .Select(b => b.Tag as Version).FirstOrDefault();

            Debug.Assert(selectedVersion != null, "selectedVersion is null.");

            return selectedVersion;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static RadioButton CreateRadioButton(EntityFrameworkVersionOption option)
        {
            Debug.Assert(option != null, "option is null.");

            return new RadioButton
                {
                    AutoSize = true,
                    Checked = option.IsDefault,
                    Enabled = !option.Disabled,
                    Tag = option.Version,
                    Text = option.Name
                };
        }

        private Bitmap GetNotificationBitmap(RuntimeConfigState state)
        {
            Bitmap bitmap;

            switch (state)
            {
                case RuntimeConfigState.Normal:
                    bitmap = Resources.Information;
                    break;

                case RuntimeConfigState.Error:
                    bitmap = Resources.Error;
                    break;

                default:
                    return null;
            }
            return ThemeUtils.GetThemedButtonImage(bitmap, BackColor);
        }
    }
}
