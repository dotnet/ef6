// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    // <summary>
    //     Given a type of file (via the extension) and a subdirectory name, this proffers out the list of files in:
    //     1. A 'User' directory
    //     2. A 'VS' directory
    // </summary>
    internal class ExtensibleFileManager
    {
        internal enum TypeOfFile
        {
            User,
            VS
        }

        private readonly string _subdirectoryName;
        private readonly string _extension;

        // used to cache the user extensibility part
        private static string _userExtDirPart;

        // TODO: Find a 'more standard' way to obtain the MEF Extensions dirs
        private const string VsExtDirPart = @"Extensions\Microsoft\Entity Framework Tools";
        private const string UserExtDirPartFormat = @"Microsoft\{0}\10.0\Extensions\Microsoft\Entity Framework Tools";
        internal static readonly string EFTOOLS_USER_MACRONAME = "UserEFTools";
        internal static readonly string EFTOOLS_VS_MACRONAME = "VSEFTools";

        // used to cache the extensibility dirs
        private static DirectoryInfo _userEFToolsDir;
        private static DirectoryInfo _vsEFToolsDir;

        internal ExtensibleFileManager(string subdirectoryName, string extension)
        {
            _subdirectoryName = subdirectoryName;
            _extension = extension;
        }

        private static string UserExtDirPart
        {
            get
            {
                if (String.IsNullOrEmpty(_userExtDirPart))
                {
                    _userExtDirPart = String.Format(
                        CultureInfo.InvariantCulture, UserExtDirPartFormat, VsUtils.GetVisualStudioApplicationID());
                }
                return _userExtDirPart;
            }
        }

        private static string VSExtDirPart
        {
            get { return VsExtDirPart; }
        }

        internal IEnumerable<FileInfo> UserFiles
        {
            get
            {
                foreach (var fileInfo in GetSortedFilesByType(TypeOfFile.User))
                {
                    yield return fileInfo;
                }
            }
        }

        internal IEnumerable<FileInfo> VSFiles
        {
            get
            {
                foreach (var fileInfo in GetSortedFilesByType(TypeOfFile.VS))
                {
                    yield return fileInfo;
                }
            }
        }

        internal IEnumerable<FileInfo> AllFiles
        {
            get
            {
                foreach (var fileinfo in UserFiles)
                {
                    yield return fileinfo;
                }
                foreach (var fileInfo in VSFiles)
                {
                    yield return fileInfo;
                }
            }
        }

        private IEnumerable<FileInfo> GetSortedFilesByType(TypeOfFile typeOfFile)
        {
            var dirPath = String.Empty;
            if (typeOfFile == TypeOfFile.User)
            {
                dirPath = Path.Combine(UserEFToolsDir.FullName, _subdirectoryName);
            }
            else if (typeOfFile == TypeOfFile.VS)
            {
                dirPath = Path.Combine(VSEFToolsDir.FullName, _subdirectoryName);
            }

            Debug.Assert(!String.IsNullOrEmpty(dirPath), "We should have determined the dirPath for extensible files");
            if (!String.IsNullOrEmpty(dirPath))
            {
                var dirInfo = new DirectoryInfo(dirPath);
                if (dirInfo.Exists)
                {
                    foreach (var fileInfo in GetSortedFilesByExtension(dirInfo.GetFiles(), _extension))
                    {
                        yield return fileInfo;
                    }
                }
            }
        }

        private static IEnumerable<FileInfo> GetSortedFilesByExtension(IEnumerable<FileInfo> allFiles, string extension)
        {
            return from fi in allFiles
                   where Path.GetExtension(fi.FullName).Equals(extension, StringComparison.OrdinalIgnoreCase)
                   orderby Path.GetFileName(fi.FullName) descending
                   select fi;
        }

        internal static DirectoryInfo UserEFToolsDir
        {
            get
            {
                if (_userEFToolsDir == null)
                {
                    var appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var userExtDirPath = Path.Combine(appDataDirPath, UserExtDirPart);
                    _userEFToolsDir = new DirectoryInfo(userExtDirPath);
                }
                return _userEFToolsDir;
            }
        }

        internal static string UserEFToolsMacro
        {
            get { return String.Format(CultureInfo.InvariantCulture, "$({0})", EFTOOLS_USER_MACRONAME); }
        }

        internal static DirectoryInfo VSEFToolsDir
        {
            get
            {
                if (_vsEFToolsDir == null)
                {
                    var vsExtDirPath = Path.Combine(VsUtils.GetVisualStudioInstallDir(), VSExtDirPart);
                    _vsEFToolsDir = new DirectoryInfo(vsExtDirPath);
                }
                return _vsEFToolsDir;
            }
        }

        internal static string VSEFToolsMacro
        {
            get { return String.Format(CultureInfo.InvariantCulture, "$({0})", EFTOOLS_VS_MACRONAME); }
        }
    }
}
