// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Microsoft.Win32;

    public class ResourcesHelper
    {
#if VS14
        private static readonly string _vsInstallDir = (string)Registry.GetValue(
            Registry.LocalMachine + "\\SOFTWARE\\Microsoft\\VisualStudio\\14.0",
            "InstallDir",
            null);
#elif VS12
        private static readonly string _vsInstallDir = (string)Registry.GetValue(
            Registry.LocalMachine + "\\SOFTWARE\\Microsoft\\VisualStudio\\12.0",
            "InstallDir",
            null);
#else
        private static readonly string _vsInstallDir = (string)Registry.GetValue(
            Registry.LocalMachine + "\\SOFTWARE\\Microsoft\\VisualStudio\\11.0",
            "InstallDir",
            null);
#endif

        private readonly AssemblyResourceLookup _dataConnectionDialogResourceLookup;
        private readonly AssemblyResourceLookup _sqlConnectionUIControlResourceLookup;
        private readonly AssemblyResourceLookup _wizardFrameworkResourceLookup;
        private readonly AssemblyResourceLookup _modelWizardResourceLookup;
        private readonly AssemblyResourceLookup _designPackageResourceLookup;
        private readonly AssemblyResourceLookup _viewsDialogsResourceLookup;
        private readonly AssemblyResourceLookup _entityDesignResourceLookup;

        public ResourcesHelper()
        {
            var filepath = Path.Combine(_vsInstallDir, "Microsoft.Data.ConnectionUI.Dialog.dll");
            _dataConnectionDialogResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.ConnectionUI.DataConnectionDialog");
            _sqlConnectionUIControlResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.ConnectionUI.SqlConnectionUIControl");

            filepath = Path.Combine(_vsInstallDir, "Microsoft.WizardFramework.dll");
            _wizardFrameworkResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.WizardFramework.Properties.Resources");

            filepath = Path.Combine(_vsInstallDir, "Microsoft.Data.Entity.Design.Package.dll");
            _designPackageResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.Entity.Design.Package.Resources");

            filepath = Path.Combine(_vsInstallDir, "Microsoft.Data.Entity.Design.dll");
            _modelWizardResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources");
            _viewsDialogsResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.Entity.Design.UI.Views.Dialogs.DialogsResource");
            _entityDesignResourceLookup = new AssemblyResourceLookup(
                Assembly.LoadFile(filepath), "Microsoft.Data.Entity.Design.Resources");
        }

        /// <summary>
        ///     Look up resource string in all relevant resource tables for the
        ///     Microsoft.Data.ConnectionUI.Dialog.dll reference file.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <returns>Real display string</returns>
        public string GetConnectionUIDialogResourceString(string key)
        {
            foreach (var resourceLookup in new[] { _sqlConnectionUIControlResourceLookup, _dataConnectionDialogResourceLookup })
            {
                try
                {
                    return GetResourceString(resourceLookup, key);
                }
                catch (ArgumentException)
                {
                    // In order to check multiple tables with a single function, we have to handle
                    // this exception (meaning the key wasn't found in the table) between checks
                }
            }

            return "";
        }

        /// <summary>
        ///     Look up resource string in all relevant resource tables for the
        ///     Microsoft.Data.Entity.Design.dll reference file.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <returns>Real display string</returns>
        public string GetEntityDesignResourceString(string key)
        {
            foreach (
                var resourceLookup in
                    new[]
                    { _modelWizardResourceLookup, _designPackageResourceLookup, _entityDesignResourceLookup, _viewsDialogsResourceLookup })
            {
                try
                {
                    return GetResourceString(resourceLookup, key);
                }
                catch (ArgumentException)
                {
                    // In order to check multiple tables with a single function, we have to handle
                    // this exception (meaning the key wasn't found in the table) between checks
                }
            }

            return "";
        }

        /// <summary>
        ///     Look up resource string in all relevant resource tables for the
        ///     Microsoft.WizardFramework.dll reference file.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <returns>Real display string</returns>
        public string GetWizardFrameworkResourceString(string key)
        {
            try
            {
                return GetResourceString(_wizardFrameworkResourceLookup, key);
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        private string GetResourceString(AssemblyResourceLookup resourceLookup, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var value = resourceLookup.LookupString(key);
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

            var ch = '\u0001'.ToString(CultureInfo.InvariantCulture);
            return value.Replace("&&", ch).Replace("&", "").Replace(ch, "&");
        }
    }
}
