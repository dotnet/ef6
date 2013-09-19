// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Handlers
{
    using System;
    using System.IO;
    using EnvDTE;
    using Microsoft.DbContextPackage.Extensions;
    using Microsoft.DbContextPackage.Resources;
    using Microsoft.DbContextPackage.Utilities;

    internal class AddCustomTemplatesHandler
    {
        private readonly DbContextPackage _package;

        public AddCustomTemplatesHandler(DbContextPackage package)
        {
            DebugCheck.NotNull(package);

            _package = package;
        }

        public void AddCustomTemplates(Project project)
        {
            DebugCheck.NotNull(project);

            try
            {
                AddTemplate(project, Templates.ContextTemplate);
                AddTemplate(project, Templates.EntityTemplate);
                AddTemplate(project, Templates.MappingTemplate);
            }
            catch (Exception ex)
            {
                _package.LogError(Strings.AddTemplatesError, ex);
            }
        }

        private static void AddTemplate(Project project, string templatePath)
        {
            DebugCheck.NotNull(project);
            DebugCheck.NotEmpty(templatePath);

            var projectDir = project.GetProjectDir();

            var filePath = Path.Combine(projectDir, templatePath);
            var contents = Templates.GetDefaultTemplate(templatePath);
            var item = project.AddNewFile(filePath, contents);
            item.Properties.Item("CustomTool").Value = null;
        }
    }
}
