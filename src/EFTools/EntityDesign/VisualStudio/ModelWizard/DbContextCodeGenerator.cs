// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;

    // <summary>
    //     Handles adding the DbContext item templates for code generation automatically if
    //     the template VSIX are installed.
    // </summary>
    internal class DbContextCodeGenerator
    {
        private readonly string _templatePattern = "DbCtx{0}{1}EF{2}.zip";

        // <summary>
        //     Constructor to use with the default template pattern.
        // </summary>
        public DbContextCodeGenerator()
        {
        }

        // <summary>
        //     Constructor that allows injection of a different item template zip file pattern
        //     for use when testing.
        // </summary>
        // <param name="templatePattern">The pattern</param>
        public DbContextCodeGenerator(string templatePattern)
        {
            _templatePattern = templatePattern;
        }

        // <summary>
        //     Adds the DbContext templates for the given EDMX project item if the templates are
        //     installed and can be used, otherwise does nothing.
        // </summary>
        // <param name="edmxItem">The project item representing the EDMX model.</param>
        // <param name="useLegacyTemplate">A value indicating whether to use the legacy version of the template.</param>
        public void AddDbContextTemplates(ProjectItem edmxItem, bool useLegacyTemplate = true)
        {
            Debug.Assert(edmxItem != null, "edmxItem is null.");

            var templatePath = FindDbContextTemplate(edmxItem.ContainingProject, useLegacyTemplate);

            if (templatePath != null)
            {
                try
                {
                    var edmxFilePath = edmxItem.get_FileNames(1);
                    var edmxFileName = Path.GetFileNameWithoutExtension(edmxFilePath);
                    AddArtifactGeneratorWizard.EdmxUri = Utils.FileName2Uri(edmxFilePath);

                    if (!(edmxItem.ProjectItems ?? edmxItem.Collection).OfType<ProjectItem>().Any(
                        i => string.Equals(i.Name, edmxFileName + ".tt", StringComparison.OrdinalIgnoreCase)))
                    {
                        AddAndNestCodeGenTemplates(
                            edmxItem,
                            () => edmxItem.Collection.AddFromTemplate(templatePath, edmxFileName));
                    }
                }
                finally
                {
                    AddArtifactGeneratorWizard.EdmxUri = null;
                }
            }
        }

        public static void AddAndNestCodeGenTemplates(ProjectItem edmxItem, Action performAdd)
        {
            // Using AddFromTemplate to nest project items doesn't work directly. Instead you need
            // to add them at the type level and then move them to the nested location. But to do that
            // we need to find the T4 items added, which we do by creating a diff of the T4 items before
            // and the T4 items after calling AddFromTemplate.
            var before = GetT4Items(edmxItem);

            performAdd();

            // ProjectItems is null when other items cannot be nested, such as in web site projects.
            if (edmxItem != null
                && edmxItem.ProjectItems != null)
            {
                // The AddFromFileCopy called on the ProjectItems of the EDMX item moves the generated file
                // to be nested under the EDMX.
                GetT4Items(edmxItem).Except(before).ToList().ForEach(
                    i => edmxItem.ProjectItems.AddFromFileCopy(i.FileNames[1]));
            }
        }

        // <summary>
        //     Gets all the items co-located with the given edmxItem with names ending in ".tt".
        // </summary>
        private static IEnumerable<ProjectItem> GetT4Items(ProjectItem edmxItem)
        {
            return edmxItem == null
                       ? Enumerable.Empty<ProjectItem>()
                       : edmxItem.Collection
                             .OfType<ProjectItem>()
                             .Where(i => i.Name.EndsWith(".tt", StringComparison.OrdinalIgnoreCase))
                             .ToList();
        }

        // <summary>
        //     Finds the DbContext item template path and filename for the given project type.
        //     Different paths are returned for C# and VB projects and for normal and Web Site
        //     projects. Null is returned if the templates are not installed or the project targets
        //     a Framework version without support for T4 code generation.
        // </summary>
        // <param name="project">The project to which templates will be added.</param>
        // <param name="useLegacyTemplate">A value indicating whether to use the legacy version of the template.</param>
        // <returns>The template path and file name or null if templates should not be used.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string FindDbContextTemplate(Project project, bool useLegacyTemplate = true)
        {
            if (!TemplateSupported(project, Services.ServiceProvider))
            {
                // DbContext is not supported on .NET 3.5, so just use built-in ObjectContext code gen.
                return null;
            }

            var webTag = VsUtils.IsWebSiteProject(project) ? "WS" : "";
            var languageTag = VsUtils.GetLanguageForProject(project) == LangEnum.VisualBasic ? "VB" : "CS";
            var version = useLegacyTemplate ? "5" : "6";

            var templateName = string.Format(CultureInfo.InvariantCulture, _templatePattern, languageTag, webTag, version);

            try
            {
                return ((Solution2)project.DTE.Solution).GetProjectItemTemplate(templateName, project.Kind);
            }
            catch (SystemException)
            {
                // If the item template is not installed, then silently fall back to build-in
                // ObjectContext generation. This makes the DbContext generation a light-up feature
                // that works as soon as the templates are installed.
                return null;
            }
        }

        internal static bool TemplateSupported(Project project, IServiceProvider serviceProvider)
        {
            return
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(project, serviceProvider) >=
                NetFrameworkVersioningHelper.NetFrameworkVersion4;
        }
    }
}
