// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    ///     This class provides utility methods for common I/O tasks not directly supported by .NET Framework BCL,
    ///     as well as I/O tasks that require elevated privileges.
    /// </summary>
    public static class IOHelpers
    {
        private const int DefaultCopyBufferSize = 65536;

#if !SILVERLIGHT
        /// <summary>
        ///     Safely determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path"> The path to test. </param>
        /// <returns> True if path refers to an existing directory; otherwise, false. </returns>
        [SecuritySafeCritical]
        // Calling Directory.Exists demands FileIOPermission (Read flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        ///     Creates the specified directory if it doesn't exist or removes all contents of an existing directory.
        /// </summary>
        /// <param name="path"> Path to directory to create. </param>
        [SecuritySafeCritical]
        // Calling Directory.Exists demands FileIOPermission (Read flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void EnsureDirectoryEmpty(string path)
        {
            if (Directory.Exists(path))
            {
                SafeDeleteDirectory(path);
            }

            EnsureDirectoryExists(path);
        }

        /// <summary>
        ///     Creates the specified directory if it doesn't exist.
        /// </summary>
        /// <param name="path"> Path to directory to create. </param>
        [SecuritySafeCritical]
        // Calling Directory.Exists and Directory.CreateDirectory demands FileIOPermission (Read | Write) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        ///     Safely determines whether the specified file exists.
        /// </summary>
        /// <param name="path"> The file to check. </param>
        /// <returns> True if the caller has the required permissions and path contains the name of an existing file; otherwise, false. This method also returns false if path is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of path. </returns>
        [SecuritySafeCritical]
        // Calling File.Exists demands FileIOPermission (Read flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        ///     Safely returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path"> The file or directory for which to obtain absolute path information. </param>
        /// <returns> A string containing the fully qualified location of path, such as "C:\MyFile.txt". </returns>
        [SecuritySafeCritical]
        // Calling Path.GetFullPath demands FileIOPermission (PathDiscovery flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        ///     Safely deletes the file and ignores any access violation exceptions.
        /// </summary>
        /// <param name="path"> The directory to delete. </param>
        [SecuritySafeCritical]
        // Calling File.Delete demands FileIOPermission (Write flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to catch everything here.")]
        public static void SafeDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }

        /// <summary>
        ///     Safely deletes the directory and ignores any access violation exceptions.
        /// </summary>
        /// <param name="path"> The directory to delete. </param>
        [SecuritySafeCritical]
        // Calling Directory.Delete demands FileIOPermission (Write flag) for the specified path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to catch everything here.")]
        public static void SafeDeleteDirectory(string path)
        {
            // try different ways of contents of directory, fail after 3 attempts
            for (var i = 0; i < 3; ++i)
            {
                try
                {
                    Directory.Delete(path, true);

                    return;
                }
                catch (Exception)
                {
                }

                Thread.Sleep(500);

                try
                {
                    foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        SafeDeleteFile(file);
                    }

                    return;
                }
                catch (Exception)
                {
                }

                Thread.Sleep(500);
            }
        }

        /// <summary>
        ///     Creates a uniquely named, empty temporary directory on disk and returns the
        ///     full path of that directory.
        /// </summary>
        /// <returns> A <see cref="String" /> containing the full path of the temporary directory. </returns>
        public static string GetTempDirName()
        {
            var tempDir = Path.GetTempFileName();
            File.Delete(tempDir);
            Directory.CreateDirectory(tempDir);

            return tempDir;
        }

        /// <summary>
        ///     Copies the specified source files to a given directory.
        /// </summary>
        /// <param name="destinationDirectory"> The destination directory. </param>
        /// <param name="sourceFiles"> The source files. </param>
        [SecuritySafeCritical]
        // Calling File.Copy demands FileIOPermission (Write flag) for the destination file path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void CopyToDirectory(string destinationDirectory, params string[] sourceFiles)
        {
            foreach (var sourceFile in sourceFiles)
            {
                var baseName = Path.GetFileName(sourceFile);
                var destinationFile = Path.Combine(destinationDirectory, baseName);

                if (new FileInfo(sourceFile).FullName
                    != new FileInfo(destinationFile).FullName)
                {
                    File.Copy(sourceFile, destinationFile, true);
                }
            }
        }
#endif

        public static void CopyToDir(string sourceFileName, string destDirName)
        {
            File.Copy(
                sourceFileName,
                Path.Combine(
                    destDirName,
                    Path.GetFileName(sourceFileName)));
        }

        /// <summary>
        ///     Copies contents of one stream into another.
        /// </summary>
        /// <param name="source"> Stream to read from. </param>
        /// <param name="destination"> Stream to write to. </param>
        /// <returns> The number of bytes copied from the source. </returns>
        public static int CopyStream(Stream source, Stream destination)
        {
            return CopyStream(source, destination, new byte[DefaultCopyBufferSize]);
        }

        /// <summary>
        ///     Copies contents of one stream into another.
        /// </summary>
        /// <param name="source"> Stream to read from. </param>
        /// <param name="destination"> Stream to write to. </param>
        /// <param name="copyBuffer"> The copy buffer. </param>
        /// <returns> The number of bytes copied from the source. </returns>
        public static int CopyStream(Stream source, Stream destination, byte[] copyBuffer)
        {
            ExceptionHelpers.CheckArgumentNotNull(source, "source");
            ExceptionHelpers.CheckArgumentNotNull(destination, "destination");
            ExceptionHelpers.CheckArgumentNotNull(copyBuffer, "copyBuffer");

            var bytesCopied = 0;
            int bytesRead;

            do
            {
                bytesRead = source.Read(copyBuffer, 0, copyBuffer.Length);
                destination.Write(copyBuffer, 0, bytesRead);
                bytesCopied += bytesRead;
            }
            while (bytesRead != 0);

            return bytesCopied;
        }

        /// <summary>
        ///     Write an embedded resource to a local file
        /// </summary>
        /// <param name="resourceName"> Resource to be written </param>
        /// <param name="fileName"> File to write resource to </param>
        /// <param name="assembly"> Assembly to extract resource from </param>
#if !SILVERLIGHT
        [SecuritySafeCritical]
        // Calling File.Open demands FileIOPermission (Append flag) for the destination file path.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
#endif
        public static void WriteResourceToFile(string resourceName, string fileName, Assembly assembly)
        {
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new IOException("Resource '" + resourceName + "' not found in '" + assembly.FullName + "'");
                }

                using (var fileStream = File.Open(fileName, FileMode.Append))
                {
                    CopyStream(resourceStream, fileStream);
                }
            }
        }

#if !SILVERLIGHT
        /// <summary>
        ///     Adds the given set of file attributes to the file at the given path
        /// </summary>
        /// <param name="fileName"> The name/path of the file </param>
        /// <param name="toAdd"> The bit-field of attributes to add </param>
        public static void AddFileAttributes(string fileName, FileAttributes toAdd)
        {
            File.SetAttributes(fileName, File.GetAttributes(fileName) | toAdd);
        }

        /// <summary>
        ///     Removes the given set of file attributes from the file at the given path
        /// </summary>
        /// <param name="fileName"> The name/path of the file </param>
        /// <param name="toRemove"> The bit-field of attributes to remove </param>
        public static void RemoveFileAttributes(string fileName, FileAttributes toRemove)
        {
            File.SetAttributes(fileName, File.GetAttributes(fileName) & ~toRemove);
        }
#endif
    }
}
