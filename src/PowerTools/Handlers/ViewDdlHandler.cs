// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Handlers
{
    using System;
    using System.IO;
    using Resources;
    using Utilities;

    internal class ViewDdlHandler
    {
        private readonly DbContextPackage _package;

        public ViewDdlHandler(DbContextPackage package)
        {
            DebugCheck.NotNull(package);

            _package = package;
        }

        public void ViewDdl(dynamic context)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Type contextType = context.GetType();

            try
            {
                var filePath = Path.Combine(
                    Path.GetTempPath(),
                    contextType.Name + FileExtensions.Sql);

                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                var objectContext = DbContextPackage.GetObjectContext(context);

                File.WriteAllText(filePath, objectContext.CreateDatabaseScript());
                File.SetAttributes(filePath, FileAttributes.ReadOnly);

                _package.DTE2.ItemOperations.OpenFile(filePath);
            }
            catch (Exception exception)
            {
                _package.LogError(Strings.ViewDdlError(contextType.Name), exception);
            }
        }
    }
}