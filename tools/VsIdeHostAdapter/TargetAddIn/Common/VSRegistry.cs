// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Helper class for VS registry.
    /// Note: used by both Host Adapter and UI side.
    /// </summary>
    internal static class VSRegistry
    {
        private const string DefaultProcessName = "devenv.exe";

        internal static List<string> GetVersions()
        {
            List<string> versions;
            GetVersionsHelper(out versions);
            return versions;
        }

        /// <summary>
        /// Returns max version without suffix.
        /// </summary>
        /// <returns></returns>
        internal static string GetDefaultVersion()
        {
            List<string> versions;
            return GetVersionsHelper(out versions);
        }

        internal static string GetVSLocation(string registryHive)
        {
            Debug.Assert(!string.IsNullOrEmpty(registryHive));

            string versionKeyName = string.Format(CultureInfo.InvariantCulture,
                @"SOFTWARE\Microsoft\VisualStudio\{0}", registryHive);
            string installDir = RegistryHelper<string>.GetValueIgnoringExceptions(
                Registry.LocalMachine, versionKeyName, "InstallDir", null);

            if (string.IsNullOrEmpty(installDir))
            {
                throw new VsIdeTestHostException(Resources.CannotFindVSInstallation(registryHive));
            }

            // If the registry has a value specifying which DevEnv executable to use, then use it. This can be useful where
            // another tool needs to run a UI test using the VSIdeTestHost, but with a different .config file for the DevEnv
            // process (for example, LKG test tools for UI testing). If the environment variable does not exist, use the actual
            // executable name.
            string processName = RegistrySettings.VisualStudioProcessName;
            if (string.IsNullOrEmpty(processName))
            {
                processName = DefaultProcessName;
            }

            string location = Path.Combine(installDir, processName);
            return location;
        }

        /// <summary>
        /// Returns default version = max version without suffix.
        /// </summary>
        /// <param name="versions">If null, this is ignored.</param>
        /// <returns></returns>
        private static string GetVersionsHelper(out List<string> versions)
        {
            versions = new List<string>();

            // Default is the current version without suffix, like 8.0.
            string defaultVersion = null;
            Regex versionNoSuffixRegex = VsVersionComparer.VsVersionRegex;
            VsVersionComparer versionComparer = new VsVersionComparer();

            // Note that the version does not have to be numeric only: can be 8.0Exp.
            using (RegistryKey vsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio"))
            {
                foreach (string versionKeyName in vsKey.GetSubKeyNames())
                {
                    // If there's no InstallDir subkey we skip this key.
                    using (RegistryKey versionKey = vsKey.OpenSubKey(versionKeyName))
                    {
                        if (versionKey.GetValue("InstallDir") == null)
                        {
                            continue;
                        }

                        versions.Add(versionKeyName);
                    }

                    if (RegistrySettings.VsRegistryRoot.EndsWith(versionKeyName))
                    {
                        defaultVersion = versionKeyName;
                    }
                }
            }

            return defaultVersion;
        }
    }
}
