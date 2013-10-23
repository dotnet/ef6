// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using Microsoft.Win32;

    public class ResourcesHelper
    {
#if VS12
        private static string vsInstallDir = (string)Microsoft.Win32.Registry.GetValue(
            Microsoft.Win32.Registry.LocalMachine + "\\SOFTWARE\\Microsoft\\VisualStudio\\12.0", 
            "InstallDir", 
            null);
#else
        private static readonly string vsInstallDir = (string)Registry.GetValue(
            Registry.LocalMachine + "\\SOFTWARE\\Microsoft\\VisualStudio\\11.0",
            "InstallDir",
            null);
#endif

        private readonly ResourceManager _dataConnectionDialogResources;
        private readonly ResourceManager _sqlConnectionUIControlResources;
        private readonly ResourceManager _modelWizardResources;
        private readonly ResourceManager _wizardResources;
        private static CultureInfo _currentCulture;

        public ResourcesHelper()
        {
            var filepath = Path.Combine(vsInstallDir, "Microsoft.Data.ConnectionUI.Dialog.dll");
            var assembly = Assembly.LoadFile(filepath);
            _dataConnectionDialogResources = new ResourceManager("Microsoft.Data.ConnectionUI.DataConnectionDialog", assembly);
            _sqlConnectionUIControlResources = new ResourceManager("Microsoft.Data.ConnectionUI.SqlConnectionUIControl", assembly);

            filepath = Path.Combine(vsInstallDir, "Microsoft.WizardFramework.dll");
            assembly = Assembly.LoadFile(filepath);
            _wizardResources = new ResourceManager("Microsoft.WizardFramework.Properties.Resources", assembly);

            assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(vsInstallDir), "Microsoft.Data.Entity.Design.dll"));

            _modelWizardResources = new ResourceManager(
                "Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources",
                assembly);

            _currentCulture = CultureInfo.CurrentCulture;
        }

        public string GetConnectionDialogResourceString(string key)
        {
            var value = GetResourceString(_sqlConnectionUIControlResources, key);
            if (value == null)
            {
                return GetResourceString(_dataConnectionDialogResources, key);
            }

            return value;
        }

        public string GetModelWizardResourceString(string key)
        {
            return GetResourceString(_modelWizardResources, key);
        }

        public string GetWizardResourceString(string key)
        {
            return GetResourceString(_wizardResources, key);
        }

        private string GetResourceString(ResourceManager resourceManager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var value = resourceManager.GetString(key, _currentCulture);
            return RemoveAmpersands(value);
        }

        /// <summary>
        ///     The strings from the resources file have & in them to denote Hot key. For ex. &New Connection.
        ///     If you assign this text to a button the button text appears to be "New Connection" with N as keyboard hot key.
        ///     Here we need to know the string without all the &s. There is not standard method to get the real string
        ///     from resource string (at least none was found). So created this one.
        /// </summary>
        /// <param name="value">Resource string</param>
        /// <returns>Real display string</returns>
        public string RemoveAmpersands(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var ch = '\u0001'.ToString();
            return value.Replace("&&", ch).Replace("&", "").Replace(ch, "&");
        }
    }
}
