// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Security;
    using System.Security.Permissions;

#if !SILVERLIGHT
#endif

    /// <summary>
    ///     Various utilities for dealing with embedded resources
    /// </summary>
    public static class ResourceUtilities
    {
#if !SILVERLIGHT
        /// <summary>
        ///     Extracts the specified compressed resource to a given directory
        /// </summary>
        /// <param name="outputDirectory"> Output directory </param>
        /// <param name="assembly"> Assembly that contains the resource </param>
        /// <param name="resourceName"> Partial resource name </param>
        /// <remarks>
        ///     The method prefixes the actual resource with unqualified assembly name + '.Resources.' (
        ///     so you should put all your resources under 'Resources' directory within a project)
        ///     and appends '.gz' to it. You should use gzip to produce compressed files.
        /// </remarks>
        [SecuritySafeCritical]
        // Calling File.Create demands FileIOPermission (Write flag) for the file path to which the resource is extracted.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "gz",
            Justification = ".gz is GZIP file extension")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times",
            Justification = "Bogus warning, object will be disposed once.")]
        public static void ExtractCompressedResourceToDirectory(string outputDirectory, Assembly assembly, string resourceName)
        {
            ValidateCommonArguments(outputDirectory, assembly, resourceName);

            var fullResourceName = new AssemblyName(assembly.FullName).Name + ".Resources." + resourceName + ".gz";
            using (var compressedInputStream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (compressedInputStream == null)
                {
                    throw new IOException("Embedded resource " + fullResourceName + " not found in " + assembly.FullName);
                }

                using (var decompressedInputStream = new GZipStream(compressedInputStream, CompressionMode.Decompress))
                {
                    using (Stream outputStream = File.Create(Path.Combine(outputDirectory, resourceName)))
                    {
                        IOHelpers.CopyStream(decompressedInputStream, outputStream);
                    }
                }
            }
        }
#endif

        /// <summary>
        ///     Copies the named embedded resources from given assembly into the current directory.
        /// </summary>
        /// <param name="assembly"> The assembly. </param>
        /// <param name="prefix"> The prefix to use for each name. </param>
        /// <param name="overwrite"> if set to <c>true</c> then an existing file of the same name name will be overwritten. </param>
        /// <param name="names"> The resource names, which will become the file names. </param>
        public static void CopyEmbeddedResourcesToCurrentDir(
            Assembly assembly, string prefix, bool overwrite,
            params string[] names)
        {
            foreach (var name in names)
            {
                using (var sourceStream = assembly.GetManifestResourceStream(prefix + name))
                {
                    Debug.Assert(sourceStream != null, "Could not create stream for embedded resource " + prefix + name);

                    var destinationPath = Path.Combine(@".\", name);
                    if (!File.Exists(destinationPath) || overwrite)
                    {
                        using (
                            var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                        {
                            var sourceBuffer = new byte[sourceStream.Length];
                            sourceStream.Read(sourceBuffer, 0, sourceBuffer.Length);
                            destinationStream.Write(sourceBuffer, 0, sourceBuffer.Length);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Builds a resource manager for the given assembly.
        /// </summary>
        /// <param name="assembly"> The assembly to build the resource manager for. </param>
        /// <returns> The resource manager. </returns>
        public static ResourceManager BuildResourceManager(Assembly assembly)
        {
            ExceptionHelpers.CheckArgumentNotNull(assembly, "assembly");
            return new ResourceManager(FindSingleResourceTable(assembly), assembly);
        }

        private static void ValidateCommonArguments(string outputDirectory, Assembly assembly, string resourceName)
        {
            ExceptionHelpers.CheckArgumentNotNull(outputDirectory, "outputDirectory");
            ExceptionHelpers.CheckArgumentNotNull(assembly, "assembly");
            ExceptionHelpers.CheckArgumentNotNull(resourceName, "resourceName");

            if (!Directory.Exists(outputDirectory))
            {
                throw new IOException("Output directory '" + outputDirectory + "' does not exist.");
            }
        }

        private static string FindSingleResourceTable(Assembly assembly)
        {
            var resources = assembly.GetManifestResourceNames().Where(r => r.EndsWith(".resources", StringComparison.Ordinal));
            if (resources.Count() != 1)
            {
                throw new NotSupportedException(
                    "The supplied assembly does not contain exactly one resource table, if the assembly contains multiple tables call the overload that specifies which table to use.");
            }

            var resource = resources.Single();

            // Need to trim the ".resources" off the end
            return resource.Substring(0, resource.Length - 10);
        }
    }
}
