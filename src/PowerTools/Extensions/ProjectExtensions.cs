// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    internal static class ProjectExtensions
    {
        public const int S_OK = 0;
        public const string WebApplicationProjectTypeGuid = "{349C5851-65DF-11DA-9384-00065B846F21}";
        public const string WebSiteProjectTypeGuid = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

        public static ProjectItem AddNewFile(this Project project, string path, string contents)
        {
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(path));

            if (string.IsNullOrWhiteSpace(contents))
            {
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            project.DTE.SourceControl.CheckOutItemIfNeeded(path);
            File.WriteAllText(path, contents);

            return project.ProjectItems.AddFromFile(path);
        }

        public static string GetProjectDir(this Project project)
        {
            Contract.Requires(project != null);

            return project.GetPropertyValue<string>("FullPath");
        }

        public static string GetTargetDir(this Project project)
        {
            Contract.Requires(project != null);

            var fullPath = project.GetProjectDir();
            string outputPath;

            outputPath = project.GetConfigurationPropertyValue<string>("OutputPath");

            return Path.Combine(fullPath, outputPath);
        }

        public static void InstallPackage(this Project project, string packageId)
        {
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(packageId));

            var typeNuGetConstants = Type.GetType("NuGet.NuGetConstants, NuGet.VisualStudio", true);
            var typeIVsPackageInstaller = Type.GetType("NuGet.VisualStudio.IVsPackageInstaller, NuGet.VisualStudio", true);
            var typeSemanticVersion = Type.GetType("NuGet.SemanticVersion, NuGet.Core", true);

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var packageInstaller = componentModel.GetService(typeIVsPackageInstaller);
            var source = (string)typeNuGetConstants.GetField("DefaultFeedUrl").GetValue(null);

            typeIVsPackageInstaller.GetMethod(
                "InstallPackage",
                new[] { typeof(string), typeof(Project), typeof(string), typeSemanticVersion, typeof(bool) })
                .Invoke(packageInstaller, new object[] { source, project, packageId, null, false });
        }

        public static bool IsWebProject(this Project project)
        {
            Contract.Requires(project != null);

            return project.GetProjectTypes().Any(
                    g => g.EqualsIgnoreCase(WebApplicationProjectTypeGuid)
                        || g.EqualsIgnoreCase(WebSiteProjectTypeGuid));
        }

        public static bool TryBuild(this Project project)
        {
            Contract.Requires(project != null);

            var dte = project.DTE;
            var configuration = dte.Solution.SolutionBuild.ActiveConfiguration.Name;

            dte.Solution.SolutionBuild.BuildProject(configuration, project.UniqueName, true);

            return dte.Solution.SolutionBuild.LastBuildInfo == 0;
        }

        private static T GetPropertyValue<T>(this Project project, string propertyName)
        {
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            var property = project.Properties.Item(propertyName);

            if (property == null)
            {
                return default(T);
            }

            return (T)property.Value;
        }

        private static T GetConfigurationPropertyValue<T>(this Project project, string propertyName)
        {
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            var property = project.ConfigurationManager.ActiveConfiguration.Properties.Item(propertyName);

            if (property == null)
            {
                return default(T);
            }

            return (T)property.Value;
        }

        private static IEnumerable<string> GetProjectTypes(this Project project)
        {
            Contract.Requires(project != null);

            IVsSolution solution;
            using (var serviceProvider = new ServiceProvider((IServiceProvider)project.DTE))
            {
                solution = (IVsSolution)serviceProvider.GetService(typeof(IVsSolution));
            }

            IVsHierarchy hierarchy;
            var hr = solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            if (hr != S_OK)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            string projectTypeGuidsString;

            var aggregatableProject = (IVsAggregatableProject)hierarchy;
            hr = aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuidsString);

            if (hr != S_OK)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return projectTypeGuidsString.Split(';');
        }
    }
}