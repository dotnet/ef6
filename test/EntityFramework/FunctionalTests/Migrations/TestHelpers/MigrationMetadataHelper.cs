// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.IO.Compression;
    using System.Xml;

    internal static class MigrationMetadataHelper
    {
        public static string GetModel<TContext>()
            where TContext : DbContext
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    using (var xmlWriter = XmlWriter.Create(gzipStream, new XmlWriterSettings { Indent = true }))
                    {
                        using (var context = new DbContextInfo(typeof(TContext)).CreateInstance())
                        {
                            EdmxWriter.WriteEdmx(context, xmlWriter);
                        }
                    }
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
