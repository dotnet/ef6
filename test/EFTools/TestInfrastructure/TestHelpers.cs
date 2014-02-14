// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

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

        public static void TakeScreenShot(string outputDirectory, string screenshotName)
        {
            try
            {
                var now = DateTime.Now;
                outputDirectory = Path.Combine(outputDirectory, now.ToString("yyyyMMdd"));
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                var filename = Path.Combine(outputDirectory, screenshotName + "_" + now.ToString("yyyyMMdd-HHmmss") + ".jpeg");
                var screenSize = Screen.PrimaryScreen.Bounds.Size;
                var btm = new Bitmap(screenSize.Width, screenSize.Height);
                using (var g = Graphics.FromImage(btm))
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), screenSize);
                }
                btm.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                btm.Dispose();
            }
            catch (Exception ex)
            {
                // Do not rethrow exceptions as we do not want failure of this method to affect the test run
                Console.WriteLine("TakeScreenShot(): Exception of type " + ex.GetType() + ". Details: " + ex.ToString());
            }
        }
    }
}
