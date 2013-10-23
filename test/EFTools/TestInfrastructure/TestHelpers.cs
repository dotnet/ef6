// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public static class TestUtils
    {
        public static Uri FileName2Uri(string fileName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "!string.IsNullOrWhiteSpace(fileName)");

            return new Uri(new FileInfo(fileName).FullName, UriKind.Absolute);
        }

        public static string LoadEmbeddedResource(string resourceName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(resourceName), "!string.IsNullOrWhiteSpace(resourceName)");

            return LoadEmbeddedResource(Assembly.GetCallingAssembly(), resourceName);
        }

        public static string LoadEmbeddedResource(Assembly assembly, string resourceName)
        {
            Debug.Assert(assembly != null, "assembly != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(resourceName), "!string.IsNullOrWhiteSpace(resourceName)");

            using (var stream = GetEmbeddedResourceStream(assembly, resourceName))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(resourceName), "!string.IsNullOrWhiteSpace(resourceName)");

            return GetEmbeddedResourceStream(Assembly.GetCallingAssembly(), resourceName);
        }

        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceName)
        {
            Debug.Assert(assembly != null, "assembly != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(resourceName), "!string.IsNullOrWhiteSpace(resourceName)");

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
