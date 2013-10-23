// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using Microsoft.Data.Entity.Design.VisualStudio;

    /// <summary>
    ///     The extensibility of certain features such as Model First may be driven by files in the user's file system.
    ///     This converter allows displaying lists of files in three separate scopes:
    ///     1. per-user. i.e. the display will show '(User) MySSDLToSQL10.tt'
    ///     2. across all users. i.e. the display will show '(VS) SSDLToSQL10.tt'
    ///     3. within the project: 'ProjectSSDLToSQL10.tt'
    ///     This converter also allows direct editing of the path in the property window as well.
    /// </summary>
    internal abstract class ExtensibleFileListConverter : StringConverter
    {
        protected abstract string SubDirPath { get; }

        private string UserPathWithMacro
        {
            get { return Path.Combine(ExtensibleFileManager.UserEFToolsMacro, SubDirPath); }
        }

        private string VSPathWithMacro
        {
            get { return Path.Combine(ExtensibleFileManager.VSEFToolsMacro, SubDirPath); }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var extFilePath = value as string;
            if (extFilePath != null
                && destinationType == typeof(string))
            {
                // Format the display. Note that we use InvariantCulture to create the display value since the order of the type '(User)', etc. and the name is significant.
                // In some languages this could be reversed.
                extFilePath = extFilePath.Trim();
                var indexOfUserMacro = extFilePath.IndexOf(UserPathWithMacro, StringComparison.OrdinalIgnoreCase);
                var indexOfVSMacro = extFilePath.IndexOf(VSPathWithMacro, StringComparison.OrdinalIgnoreCase);
                if (indexOfUserMacro != -1)
                {
                    return String.Format(
                        CultureInfo.InvariantCulture, "{0} {1}",
                        extFilePath.Substring(indexOfUserMacro + UserPathWithMacro.Length).TrimStart('\\'),
                        Resources.DbGenExtensibileListConverter_UserDir);
                }
                else if (indexOfVSMacro != -1)
                {
                    return String.Format(
                        CultureInfo.InvariantCulture, "{0} {1}",
                        extFilePath.Substring(indexOfVSMacro + VSPathWithMacro.Length).TrimStart('\\'),
                        Resources.DbGenExtensibleListConverter_VSDir);
                }
                return extFilePath;
            }
            return null;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var v = value as string;
            if (!String.IsNullOrEmpty(v))
            {
                // given the display, we need to return back the actual value that will be injected into the XML
                v = v.Trim();
                var lastIndexOfSpace = v.LastIndexOf(' ');
                if (lastIndexOfSpace != -1)
                {
                    var filename = v.Substring(0, lastIndexOfSpace);
                    if (v.Substring(lastIndexOfSpace + 1).Equals(Resources.DbGenExtensibileListConverter_UserDir))
                    {
                        return Path.Combine(UserPathWithMacro, filename);
                    }
                    else if (v.Substring(lastIndexOfSpace + 1).Equals(Resources.DbGenExtensibleListConverter_VSDir))
                    {
                        return Path.Combine(VSPathWithMacro, filename);
                    }
                }
                return v;
            }
            return null;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        ///     We take the full path and create macros out of the user and VS dirs. This allows us to understand where the file is semantically without
        ///     being bound to a full path that is specific to one user's box.
        /// </summary>
        protected static string MacroizeFilePath(string fullPath)
        {
            var userEFToolsIndex = fullPath.IndexOf(ExtensibleFileManager.UserEFToolsDir.FullName, StringComparison.OrdinalIgnoreCase);
            var vsEFToolsIndex = fullPath.IndexOf(ExtensibleFileManager.VSEFToolsDir.FullName, StringComparison.OrdinalIgnoreCase);
            if (userEFToolsIndex != -1)
            {
                var restOfPath = fullPath.Substring(userEFToolsIndex + ExtensibleFileManager.UserEFToolsDir.FullName.Length).TrimStart('\\');
                return Path.Combine(
                    String.Format(CultureInfo.InvariantCulture, "$({0})", ExtensibleFileManager.EFTOOLS_USER_MACRONAME), restOfPath);
            }
            else if (vsEFToolsIndex != -1)
            {
                var restOfPath = fullPath.Substring(vsEFToolsIndex + ExtensibleFileManager.VSEFToolsDir.FullName.Length).TrimStart('\\');
                return Path.Combine(
                    String.Format(CultureInfo.InvariantCulture, "$({0})", ExtensibleFileManager.EFTOOLS_VS_MACRONAME), restOfPath);
            }
            return fullPath;
        }
    }
}
