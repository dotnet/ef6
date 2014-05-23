// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// KeyNames/settings for VsIdeHostAdapter and VsIde.
    /// </summary>
    internal static class RegistrySettings
    {
#if VS14
        internal const string VsRegistryRoot = @"SOFTWARE\Microsoft\VisualStudio\14.0";
#elif VS12
        internal const string VsRegistryRoot = @"SOFTWARE\Microsoft\VisualStudio\12.0";
#else
        internal const string VsRegistryRoot = @"SOFTWARE\Microsoft\VisualStudio\11.0";
#endif
    
        internal const string HostAdapterRegistryKeyName =   // VS IDE HA registry (HKCU). Note this is for HA itself and is ALWAYS HKCU/<version number>.
            VsRegistryRoot + @"\EnterpriseTools\QualityTools\HostAdapters\" + VsIdeHostAdapter.Name;

        private const string RestartVsBetweenRunsValueName = "RestartVsCounter";         // DWORD (0/N): VS is restarted AFTER Run method, then the key value is decremented.
        private const string RegistryHiveOverrideValueName = "RegistryHiveOverride"; // string: if specified, Run config and env var are ignored.
        private const string EnableVerboseAssertionsValueName = "EnableVerboseAssertions"; // Some places where we do Debug.Fail.
        private const string VisualStudioProcessNameValueName = "VisualStudioProcessName"; // Name of the DevEnv executable file to use
        private const string BaseTimeoutValueName = "BaseTimeout";                   // int/millisecs: base of all timeouts.
        private const string BaseSleepDurationValueName = "BaseSleepDuration";       // int/millisecs: base of all sleep durations.

        private static int s_baseTimeout = 1000;        // Milliseconds.
        private static int s_baseSleepDuration = 250;   // Milliseconds.

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static RegistrySettings()
        {
            int baseTimeoutFromRegistry = LookupRegistrySetting<int>(BaseTimeoutValueName, -1);
            if (baseTimeoutFromRegistry >= 0)
            {
                s_baseTimeout = baseTimeoutFromRegistry;
            }

            int baseSleepDurationFromRegistry = LookupRegistrySetting<int>(BaseSleepDurationValueName, -1);
            if (baseSleepDurationFromRegistry >= 0)
            {
                s_baseSleepDuration = baseSleepDurationFromRegistry;
            }
        }

        internal static T LookupRegistrySetting<T>(string settingName, T defaultValue)
        {
            T value = defaultValue;

            // Try looking in HKCU first
            if (RegistryHelper<T>.TryGetValueIgnoringExceptions(Registry.CurrentUser, HostAdapterRegistryKeyName,
                settingName, out value))
            {
                return value;
            }
            
            // Else fall back to HKLM
            if (RegistryHelper<T>.TryGetValueIgnoringExceptions(Registry.LocalMachine, HostAdapterRegistryKeyName,
                settingName, out value))
            {
                return value;
            }

            // Default
            return defaultValue;
        }

        internal static int BaseTimeout
        {
            get 
            {
                return s_baseTimeout;
            }
        }

        internal static int BaseSleepDuration
        {
            get
            {
                return s_baseSleepDuration;
            }
        }

        /// <summary>
        /// Returns # of times left to restart VS.
        /// </summary>
        internal static uint RestartVsBetweenTests
        {
            get
            {
                // Registry cannot unbox as uint, so unbox as int and then cast to uint.
                return (uint)LookupRegistrySetting<int>(RegistrySettings.RestartVsBetweenRunsValueName, 0);
            }
            set
            {
                RegistryHelper<int>.SetValueIgnoringExceptions(
                    Registry.CurrentUser,
                    RegistrySettings.HostAdapterRegistryKeyName,
                    RegistrySettings.RestartVsBetweenRunsValueName,
                    (int)value);
            }
        }

        /// <summary>
        /// Returns Registry Hive override value. I.e. when override is set, this returns registry hive to use.
        /// Returns null if the override is not set.
        /// Each time check registry as the value can change in time.
        /// </summary>
        internal static string RegistryHiveOverride
        {
            get
            {
                return LookupRegistrySetting<string>(RegistrySettings.RegistryHiveOverrideValueName, null);
            }
        }

        /// <summary>
        /// Returns whether verbose assertions are enabled.
        /// Each time check registry as the value can change in time.
        /// </summary>
        internal static bool VerboseAssertionsEnabled
        {
            get
            {
                return LookupRegistrySetting<bool>(EnableVerboseAssertionsValueName, false);
            }
        }

        /// <summary>
        /// Name of the DevEnv executable file to use.
        /// Each time check registry as the value can change in time.
        /// </summary>
        internal static string VisualStudioProcessName
        {
            get
            {
                return LookupRegistrySetting<string>(VisualStudioProcessNameValueName, null);
            }
        }
    }
}
