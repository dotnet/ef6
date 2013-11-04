// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;

    internal static class ConfigFileUtils
    {
        /// <summary>
        ///     Updates app. or web.config to include connection strings, registers the build provider
        ///     for WebSite projects and the assembly for WebApp projects
        /// </summary>
        internal static void UpdateConfig(ModelBuilderSettings settings)
        {
            var metadataFileNames = 
                ConnectionManager.GetMetadataFileNamesFromArtifactFileName(settings.Project, settings.ModelPath, PackageManager.Package);

            if (settings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                || settings.GenerationOption == ModelGenerationOption.GenerateDatabaseScript)
            {
                if (settings.SaveConnectionStringInAppConfig
                    && !settings.SaveToWebConfig)
                {
                    // save connection string in App Config
                    UpdateAppConfig(metadataFileNames, settings);
                }
                else if (settings.SaveConnectionStringInAppConfig
                         && settings.SaveToWebConfig)
                {
                    // save connection string in Web Config
                    UpdateWebConfig(metadataFileNames, settings);
                }
            }

            var containingProject = settings.Project;

            // regardless of GenerationOption we always need to register the build
            // provider for web site projects and the assembly for web app projects
            if (settings.VSApplicationType == VisualStudioProjectSystem.Website)
            {
                RegisterBuildProvidersInWebConfig(containingProject);
            }

            // Ensure that System.Data.Entity.Design reference assemblies are added in the web.config.
            if (settings.VSApplicationType == VisualStudioProjectSystem.WebApplication
                || settings.VSApplicationType == VisualStudioProjectSystem.Website)
            {
                // Get the correct assembly name based on target framework
                var projectHierarchy = VsUtils.GetVsHierarchy(containingProject, Services.ServiceProvider);
                Debug.Assert(projectHierarchy != null, "Could not get the IVsHierarchy from the EnvDTE.Project");
                if (projectHierarchy != null)
                {
                    var targetInfo = PackageManager.Package.GetService(typeof(SVsFrameworkMultiTargeting)) as IVsFrameworkMultiTargeting;
                    var openScope = PackageManager.Package.GetService(typeof(SVsSmartOpenScope)) as IVsSmartOpenScope;
                    Debug.Assert(targetInfo != null, "Unable to get IVsFrameworkMultiTargeting from service provider");
                    var targetFrameworkMoniker = VsUtils.GetTargetFrameworkMonikerForProject(containingProject, PackageManager.Package);
                    if ((targetInfo != null)
                        && (openScope != null)
                        && settings.VSApplicationType == VisualStudioProjectSystem.Website)
                    {
                        var provider = new VsTargetFrameworkProvider(targetInfo, targetFrameworkMoniker, openScope);
                        var dataEntityDesignAssembly = provider.GetReflectionAssembly(new AssemblyName("System.Data.Entity.Design"));
                        if (dataEntityDesignAssembly != null)
                        {
                            RegisterAssemblyInWebConfig(containingProject, dataEntityDesignAssembly.FullName);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Update App.Config with connection string if specified in ModelBuilderSettings
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void UpdateAppConfig(ICollection<string> metadataFiles, ModelBuilderSettings settings)
        {
            if (settings.SaveConnectionStringInAppConfig)
            {
                var statusMessage = string.Empty;

                try
                {
                    var manager = PackageManager.Package.ConnectionManager;
                    manager.AddConnectionString(
                        settings.Project,
                        metadataFiles,
                        settings.AppConfigConnectionPropertyName,
                        settings.AppConfigConnectionString,
                        settings.DesignTimeConnectionString,
                        settings.RuntimeProviderInvariantName,
                        settings.IsSql9OrNewer);
                    statusMessage = String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Engine_AppConfigSuccess,
                        VsUtils.AppConfigFileName);
                }
                catch (Exception e)
                {
                    statusMessage = String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Engine_AppConfigException,
                        e.Message);
                }
                VsUtils.LogOutputWindowPaneMessage(settings.Project, statusMessage);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void UpdateWebConfig(ICollection<string> metadataFiles, ModelBuilderSettings settings)
        {
            if (settings.SaveConnectionStringInAppConfig)
            {
                var statusMessage = string.Empty;

                try
                {
                    var manager = PackageManager.Package.ConnectionManager;
                    manager.AddConnectionString(
                        settings.Project,
                        metadataFiles,
                        settings.AppConfigConnectionPropertyName,
                        settings.AppConfigConnectionString,
                        settings.DesignTimeConnectionString,
                        settings.RuntimeProviderInvariantName,
                        settings.IsSql9OrNewer);
                    statusMessage = String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Engine_WebConfigSuccess,
                        VsUtils.WebConfigFileName);
                }
                catch (Exception e)
                {
                    statusMessage = String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Engine_WebConfigException,
                        e.Message);
                }
                VsUtils.LogOutputWindowPaneMessage(settings.Project, statusMessage);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void RegisterBuildProvidersInWebConfig(Project project)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            var statusMessage = string.Empty;

            try
            {
                VsUtils.RegisterBuildProviders(project);
                statusMessage = String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Engine_WebConfigBPSuccess,
                    VsUtils.WebConfigFileName);
            }
            catch (Exception e)
            {
                statusMessage = String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Engine_WebConfigBPException,
                    e.Message);
            }
            VsUtils.LogOutputWindowPaneMessage(project, statusMessage);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void RegisterAssemblyInWebConfig(Project project, string assemblyFullName)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            var statusMessage = string.Empty;

            try
            {
                VsUtils.RegisterAssembly(project, assemblyFullName);
                statusMessage = String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Engine_WebConfigAssemblySuccess,
                    assemblyFullName);
            }
            catch (Exception e)
            {
                statusMessage = String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Engine_WebConfigAssemblyException,
                    assemblyFullName, e.Message);
            }
            VsUtils.LogOutputWindowPaneMessage(project, statusMessage);
        }
    }
}
