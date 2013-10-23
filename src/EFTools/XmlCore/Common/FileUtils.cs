// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Text;

    /// <summary>
    ///     Used to create temporary files.
    /// </summary>
    internal static class FileUtils
    {
        private const string MSSqlUrlScheme = "MSSQL::";
        private const string MSSqlClrUrlScheme = "MSSQLCLR::";

        /// <summary>
        ///     Creates a unique filename based on the users
        ///     document directory and the input extension
        /// </summary>
        public static void CreateUniqueFilename(
            string fileNamePrefix,
            string fileExtension,
            out string tempFileName)
        {
            CreateUniqueFilename(fileNamePrefix, fileExtension, string.Empty, false, out tempFileName);
        }

        /// <summary>
        ///     Creates a unique filename based on the users
        ///     document directory and the input extension
        /// </summary>
        /// <param name="fileNamePrefix">
        ///     The file prefix
        /// </param>
        /// <param name="fileExtension">
        ///     The file extension.  Should start with a dot.
        /// </param>
        /// <param name="indexSeparator">
        ///     If this routine must supply an index then this is the separator.
        ///     For example '_' will produce myfile_1.txt, myfile_2.txt.  If the
        ///     separator is string.Empty then myfile1.txt and myfile2.txt will
        ///     be produced.
        /// </param>
        /// <param name="alwaysUseSeparator">
        ///     Flag indicating that separator should always be used even for the
        ///     first unique name.
        /// </param>
        /// <param name="tempFileName">
        ///     the returned temporary filename
        /// </param>
        public static void CreateUniqueFilename(
            string fileNamePrefix,
            string fileExtension,
            string indexSeparator,
            bool alwaysUseSeparator,
            out string tempFileName)
        {
            tempFileName = string.Empty;

            if (string.IsNullOrEmpty(fileNamePrefix) == false
                &&
                string.IsNullOrEmpty(fileExtension) == false)
            {
                // There's no routine in C# to return a temp file with a
                // specified extension and directory.  So we build it up here.
                var filePathAndPrefix = Path.Combine(Path.GetTempPath(), fileNamePrefix);

                // 0 means don't use separator if possible.
                var index = alwaysUseSeparator ? 1 : 0;

                while (true)
                {
                    var builder = new StringBuilder(filePathAndPrefix);
                    if (index > 0)
                    {
                        if (string.IsNullOrEmpty(indexSeparator) == false)
                        {
                            builder.Append(indexSeparator);
                        }
                        builder.Append(index.ToString(CultureInfo.InvariantCulture));
                    }
                    builder.Append(fileExtension);
                    tempFileName = builder.ToString();
                    if (File.Exists(tempFileName))
                    {
                        index++;
                    }
                    else
                    {
                        try
                        {
                            using (File.Create(tempFileName))
                            {
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            // This may happen if the file got created by something else between the time File.Exists and File.Create.
                        }
                    }

                    if (index <= 0)
                    {
                        // The integer wrapped around.  We need to return an empty
                        // string because we were not able to create the file.
                        tempFileName = null;
                        break;
                    }
                }
                if (tempFileName == null)
                {
                    // fallback to the standard method that disregards the template
                    tempFileName = Path.GetTempFileName();
                }
            }
        }

        /// <summary>
        ///     Make sure the full path does not end in a backslash
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string EnsureNoBackslash(string fullPath)
        {
            var retString = fullPath;
            if (string.IsNullOrEmpty(fullPath) == false
                &&
                fullPath.Length > 1)
            {
                if (fullPath[fullPath.Length - 1] == '\\'
                    ||
                    fullPath[fullPath.Length - 1] == '/')
                {
                    retString = fullPath.Substring(0, fullPath.Length - 1);
                }
            }
            return retString;
        }

        /// <devdoc>
        ///     This is the approved method coming from mpf, copied here since we can't take
        ///     that dependency.
        /// </devdoc>
        public static bool IsSamePath(string file1, string file2)
        {
            if (file1 == null
                || file1.Length == 0)
            {
                return (file2 == null || file2.Length == 0);
            }

            Uri uri1;
            Uri uri2;

            try
            {
                if (!Uri.TryCreate(file1, UriKind.Absolute, out uri1)
                    || !Uri.TryCreate(file2, UriKind.Absolute, out uri2))
                {
                    return false;
                }

                if (uri1 != null
                    && uri1.IsFile
                    && uri2 != null
                    && uri2.IsFile)
                {
                    try
                    {
                        // Canonicalize as a directory so they both have slashes on the end.
                        var canonicalPath1 = CanonicalizeDirectoryName(uri1.LocalPath);
                        var canonicalPath2 = CanonicalizeDirectoryName(uri2.LocalPath);
                        return 0 == string.Compare(canonicalPath1, canonicalPath2, StringComparison.OrdinalIgnoreCase);
                    }
                    catch (PathTooLongException)
                    {
                        // Path too long, so at least one of the string is not a valid path
                        return false;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                    catch (SecurityException)
                    {
                        return false;
                    }
                    catch (NotSupportedException)
                    {
                        return false;
                    }
                }

                return file1 == file2;
            }
            catch (UriFormatException)
            {
            }

            return false;
        }

        /// <summary>
        ///     Canonicalize folders so they can be used in dictionaries as keys.  Note
        ///     that this canonicalization assumes you have a case-insensitive filesystem
        ///     because part of the canonicalization is to upcase.
        /// </summary>
        public static string CanonicalizeDirectoryName(string fullPathDirName)
        {
            if (string.IsNullOrEmpty(fullPathDirName))
            {
                throw new ArgumentNullException("fullPathDirName");
            }

            return CanonicalizeFileNameOrDirectoryImpl(fullPathDirName, true);
        }

        /// <summary>
        ///     Internal implementation of the Canonicalization routine.  Note
        ///     that this canonicalization assumes you have a case-insensitive filesystem
        ///     because part of the canonicalization is to upcase.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathIsDir"></param>
        /// <returns></returns>
        private static string CanonicalizeFileNameOrDirectoryImpl(string path, bool pathIsDir)
        {
            if (path.StartsWith(MSSqlUrlScheme, StringComparison.OrdinalIgnoreCase)
                ||
                path.StartsWith(MSSqlClrUrlScheme, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            // Remove ".."
            path = Path.GetFullPath(path);

            // Upcase
            path = path.ToUpperInvariant();

            if (pathIsDir)
            {
                return EnsureNoBackslash(path);
            }
            return path;
        }
    }
}
