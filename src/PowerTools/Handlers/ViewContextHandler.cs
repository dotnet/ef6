// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Handlers
{
    using System;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using Microsoft.DbContextPackage.Resources;
    using Microsoft.DbContextPackage.Utilities;

    internal class ViewContextHandler
    {
        private readonly DbContextPackage _package;

        public ViewContextHandler(DbContextPackage package)
        {
            DebugCheck.NotNull(package);

            _package = package;
        }

        public void ViewContext(MenuCommand menuCommand, dynamic context, Type systemContextType)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotNull(menuCommand);
            DebugCheck.NotNull(systemContextType);

            Type contextType = context.GetType();

            try
            {
                var filePath = Path.Combine(
                    Path.GetTempPath(),
                    contextType.Name
                        + (menuCommand.CommandID.ID == PkgCmdIDList.cmdidViewEntityDataModel
                            ? FileExtensions.EntityDataModel
                            : FileExtensions.Xml));

                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                using (var fileStream = File.Create(filePath))
                {
                    using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true }))
                    {
                        var edmxWriterType = systemContextType.Assembly.GetType("System.Data.Entity.Infrastructure.EdmxWriter");

                        if (edmxWriterType != null)
                        {
                            edmxWriterType.InvokeMember(
                                "WriteEdmx",
                                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                                null,
                                null,
                                new object[] { context, xmlWriter });
                        }
                    }
                }

                _package.DTE2.ItemOperations.OpenFile(filePath);

                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            }
            catch (Exception exception)
            {
                _package.LogError(Strings.ViewContextError(contextType.Name), exception);
            }
        }
    }
}