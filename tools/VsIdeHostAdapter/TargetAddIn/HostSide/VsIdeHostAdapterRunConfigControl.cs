// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.VisualStudio.TestTools.Vsip;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// UI control for my host adapter configuration. Hosted inside test run config editor.
    /// It contains a data grid view where you could define environment variables.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs", Justification = "Public class, cannot rename")]
    public sealed partial class VsIdeHostAdapterRunConfigControl : UserControl, IRunConfigurationCustomHostEditor
    {
        #region Private
        private VsIdeHostRunConfigData m_data;
        #endregion

        #region Constructor
        public VsIdeHostAdapterRunConfigControl()
        {
            InitializeComponent();
        }
        #endregion

        #region IRunConfigurationEditor

        /// <summary>
        /// Initialize the editor to a default state based on given test run.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="run">Obsolete. Always null.</param>
        void IRunConfigurationEditor.Initialize(System.IServiceProvider serviceProvider, TestRun run)
        {
            // Initialize to something like: 7.0, 7.1, 8.0
            foreach (string version in VSRegistry.GetVersions())
            {
                m_hiveCombo.Items.Add(version);
            }
        }

        /// <summary>
        /// Fire this event when data is modified in this editor.
        /// </summary>
        public event EventHandler DataGetDirty;

        /// <summary>
        /// Handle the event that core (non-host and not-test-specific) run config data are modified outside this editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dirtyEventArgs">contains run config object that is changed outside</param>
        void IRunConfigurationEditor.OnCommonDataDirty(object sender, CommonRunConfigurationDirtyEventArgs dirtyEventArgs)
        {
            // Our test config does not depend on other data contained in the run config
            // but for the case when nobody modifies our config we still want to have our default section in RC,
            // that's why when the user switches hosts to VS IDE and we did not exist we say we are dirty, and get data will return our data.
            if (m_data == null)
            {
                SetDirty();

                // Select 1st item
                if (m_hiveCombo.SelectedIndex < 0)
                {
                    m_hiveCombo.SelectedItem = VSRegistry.GetDefaultVersion();
                }
            }
        }

        /// <summary>
        /// Description about this editor is displayed in the help panel of main run config editor.
        /// </summary>
        string IRunConfigurationEditor.Description
        {
            get
            {
                return Resources.HostAdapterDescription;
            }
        }

        /// <summary>
        /// The keyword that is hooked up with the help topic.
        /// </summary>
        string IRunConfigurationEditor.HelpKeyword
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Verify the data in the editor. Prompt the user when necessary.
        /// </summary>
        /// <returns>true if the data are correct and don't need correction; otherwise, false.</returns>
        bool IRunConfigurationEditor.VerifyData()
        {
            return true;
        }
        #endregion

        #region IRunConfigurationCustomHostEditor
        /// <summary>
        /// The host adapter type that this editor is used for.
        /// </summary>
        string IRunConfigurationCustomHostEditor.HostType
        {
            get { return VsIdeHostAdapter.Name; }
        }

        /// <summary>
        /// Called by the main editor to load the data into UI.
        /// </summary>
        /// <param name="data">host specific data</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")] // TODO: should we care about rightAlign? -- no unless we ship this HA.
        void IRunConfigurationCustomHostEditor.SetData(IHostSpecificRunConfigurationData data)
        {
            string currentVersion = VSRegistry.GetDefaultVersion();  // Throws if VS is not installed.
            
            VsIdeHostRunConfigData vsIdeHostData = data as VsIdeHostRunConfigData;
            if (vsIdeHostData == null)
            {
                vsIdeHostData = new VsIdeHostRunConfigData(currentVersion);

                // TODO: SetDirty() that set run config to dirty is disabled in parent during Initialize/SetData, so this makes no effect.
                SetDirty();
            }
            else if (!m_hiveCombo.Items.Contains(vsIdeHostData.RegistryHive))
            {
                // If .testrunconfig file has VS version that we do not have in combobox,
                // show message box and use default version.
                MessageBox.Show(
                    this, 
                    Resources.WrongVSVersionPassedToRunConfigControl(vsIdeHostData.RegistryHive, currentVersion),
                    Resources.MicrosoftVisualStudio);

                vsIdeHostData.RegistryHive = currentVersion;
                // TODO: SetDirty() that set run config to dirty is disabled in parent during Initialize/SetData, so this makes no effect.
                SetDirty();
            }

            // Set the data.
            m_data = vsIdeHostData;

            int selectedIndex = m_hiveCombo.Items.IndexOf(vsIdeHostData.RegistryHive);
            if (selectedIndex < 0)
            {
                selectedIndex = m_hiveCombo.Items.IndexOf(currentVersion);
                Debug.Assert(selectedIndex >= 0);
            }
            if (selectedIndex >= 0)
            {
                m_hiveCombo.SelectedIndex = selectedIndex;
            }

            m_additionalCommandLineArgumentsEdit.Text = vsIdeHostData.AdditionalCommandLineArguments;
            m_additionalTestDataEdit.Text = vsIdeHostData.AdditionalTestData;
        }

        /// <summary>
        /// Main editor is asking for the current host specific data.
        /// </summary>
        /// <returns></returns>
        IHostSpecificRunConfigurationData IRunConfigurationCustomHostEditor.GetData()
        {
            if (m_data == null)
            {
                m_data = new VsIdeHostRunConfigData(VSRegistry.GetDefaultVersion());
            }
            m_data.AdditionalCommandLineArguments = m_additionalCommandLineArgumentsEdit.Text;
            m_data.AdditionalTestData = m_additionalTestDataEdit.Text;
            return m_data;
        }
        #endregion

        #region Private
        private void SetDirty()
        {
            if (DataGetDirty != null)
            {
                DataGetDirty(this, EventArgs.Empty);
            }
        }

        private void HiveCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(m_data != null);

            // Set internal data from combo box.
            if (m_hiveCombo.SelectedItem != null && m_data != null)
            {
                string selectedHive = m_hiveCombo.SelectedItem.ToString();
                if (!string.Equals(m_data.RegistryHive, selectedHive, StringComparison.OrdinalIgnoreCase))
                {
                    m_data.RegistryHive = selectedHive;
                    SetDirty();
                }
            }
        }

        private void AdditionalCommandLineArgumentsEdit_TextChanged(object sender, EventArgs e)
        {
            SetDirty();
            // Note: we set the data the text when we get data from the control, there's no need to that here.
        }

        private void m_additionalTestDataEdit_TextChanged(object sender, EventArgs e)
        {
            SetDirty();
            // Note: we set the data the text when we get data from the control, there's no need to that here.
        }
        #endregion
    }
}
