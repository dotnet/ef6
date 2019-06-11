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
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            DebugCheck.NotNull(project);

            try
            {
                if (project.IsCSharpProject())
                {
                    AddTemplate(project, Templates.CsharpContextTemplate);
                    AddTemplate(project, Templates.CsharpEntityTemplate);
                }
                if (project.IsVBProject())
                {
                    AddTemplate(project, Templates.VBContextTemplate);
                    AddTemplate(project, Templates.VBEntityTemplate);
                }
            }
            catch (Exception ex)
            {
                _package.LogError(Strings.AddTemplatesError, ex);
            }
        }

        private static void AddTemplate(Project project, string templatePath)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

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
