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
    internal static class RegistryHelper<T>
    {
        /// <summary>
        /// Get value of the specified key in registry, returning defaultValue if it doesn't exist or cannot be read
        /// </summary>
        /// <param name="hive">Registry hive, like: Registry.LocalMachine.</param>
        /// <param name="subkeyName">The name of the subkey under hive specified by the hive parameter.</param>
        /// <param name="valueName">The name of the value. To get default key value, specify null.</param>
        /// <param name="defaultValue">The value to return when key does not exist or not enough permissions or wrong key type.</param>
        internal static T GetValueIgnoringExceptions(RegistryKey hive, string subkeyName, string valueName, T defaultValue)
        {
            T value = defaultValue;

            if (!TryGetValueIgnoringExceptions(hive, subkeyName, valueName, out value))
                return defaultValue;

            return value;
        }

        /// <summary>
        /// Safely attempt to read the value of the specified key in registry.
        /// If the key does not exist or not enough security permissions returns false
        /// 'ignoring exception' means it should not throw except really bad exceptions like outofmery, clrexecution, etc.
        /// </summary>
        /// <param name="hive">Registry hive, like: Registry.LocalMachine.</param>
        /// <param name="subkeyName">The name of the subkey under hive specified by the hive parameter.</param>
        /// <param name="valueName">The name of the value. To get default key value, specify null.</param>
        /// <param name="value">The output variable that will contain the read value if successful.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static bool TryGetValueIgnoringExceptions(RegistryKey hive, string subkeyName, string valueName, out T value)
        {
            Debug.Assert(hive != null, "GetValueIgnoringExceptions: hive is null.");
            Debug.Assert(!string.IsNullOrEmpty(subkeyName), "GetValueIgnoringExceptions: subkeyName is null.");
            // valueName can be null or empty - this is to get default key value.

            try
            {
                using (RegistryKey key = hive.OpenSubKey(subkeyName))
                {
                    if (key != null)
                    {
                        object tmpValue = key.GetValue(valueName);
                        if (tmpValue != null)
                        {
                            value = (T)tmpValue;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Too many: ArgumentException, ObjectDisposedException, SecurityException, InvalidCastException, UnauthorizedAccessException
                Debug.Fail(string.Format(CultureInfo.InvariantCulture,
                    "RegistryHelper.GetValueIgnoringExceptions: {0}", ex));
                // Ignore the exception.
            }

            value = default(T);
            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void SetValueIgnoringExceptions(RegistryKey hive, string subkeyName, string valueName, T newValue)
        {
            Debug.Assert(hive != null, "GetValueIgnoringExceptions: hive is null.");
            Debug.Assert(!string.IsNullOrEmpty(subkeyName), "GetValueIgnoringExceptions: subkeyName is null.");
            // valueName can be null or empty - this is to get default key value.
            // Can't check newValue for null - it can be value type.

            try
            {
                using (RegistryKey key = hive.OpenSubKey(subkeyName, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                // Too many: ArgumentException, ObjectDisposedException, SecurityException, InvalidCastException, UnauthorizedAccessException
                Debug.Fail(string.Format(CultureInfo.InvariantCulture, 
                    "RegistryHelper.GetValueIgnoringExceptions: ", ex));
                // Ignore the exception.
            }
        }
    }
}
