// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System.Configuration;

namespace Microsoft.DbContextPackage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml.Linq;
    using EnvDTE;
    using EnvDTE80;
    using Extensions;
    using Handlers;
    using Resources;
    using Utilities;
    using VisualStudio;
    using VisualStudio.Shell;
    using VisualStudio.Shell.Design;
    using VisualStudio.Shell.Interop;
    using Configuration = Configuration;
    using ConfigurationManager = ConfigurationManager;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "0.9.2", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidDbContextPackagePkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class DbContextPackage : AsyncPackage
    {
        private readonly OptimizeContextHandler _optimizeContextHandler;
        private readonly ViewContextHandler _viewContextHandler;
        private readonly ViewDdlHandler _viewDdlHandler;
        private readonly AboutHandler _aboutHandler;
        private HashSet<string> _tempFileNames;

        private DTE2 _dte2;

        public DbContextPackage()
        {
            _optimizeContextHandler = new OptimizeContextHandler(this);
            _viewContextHandler = new ViewContextHandler(this);
            _viewDdlHandler = new ViewDdlHandler(this);
            _aboutHandler = new AboutHandler(this);
            _tempFileNames = new HashSet<string>();
        }

        internal DTE2 DTE2
        {
            get { return _dte2; }
        }

        internal Guid ProjectGuid { get; private set; }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            _dte2 = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte2);
            if (_dte2 == null)
            {
                return;
            }

            var oleMenuCommandService
                = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (oleMenuCommandService != null)
            {
                var menuCommandID1 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidViewEntityDataModel);
                var menuItem1 = new OleMenuCommand(OnItemContextMenuInvokeHandler, null, OnItemMenuBeforeQueryStatus, menuCommandID1);

                oleMenuCommandService.AddCommand(menuItem1);

                var menuCommandID2 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidViewEntityDataModelXml);
                var menuItem2 = new OleMenuCommand(OnItemContextMenuInvokeHandler, null, OnItemMenuBeforeQueryStatus, menuCommandID2);

                oleMenuCommandService.AddCommand(menuItem2);

                var menuCommandID3 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidPrecompileEntityDataModelViews);
                var menuItem3 = new OleMenuCommand(OnOptimizeContextInvokeHandler, null, OnOptimizeContextBeforeQueryStatus, menuCommandID3);

                oleMenuCommandService.AddCommand(menuItem3);

                var menuCommandID4 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidViewEntityModelDdl);
                var menuItem4 = new OleMenuCommand(OnItemContextMenuInvokeHandler, null, OnItemMenuBeforeQueryStatus, menuCommandID4);

                oleMenuCommandService.AddCommand(menuItem4);

                var menuCommandID5 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidAbout);
                var menuItem5 = new OleMenuCommand(OnItemContextMenuInvokeHandler, null, OnItemMenuBeforeQueryStatus, menuCommandID5);

                oleMenuCommandService.AddCommand(menuItem5);
            }
        }

        private void OnItemMenuBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OnItemMenuBeforeQueryStatus(
                sender,
                new[] { FileExtensions.CSharp, FileExtensions.VisualBasic });
        }

        private void OnOptimizeContextBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OnItemMenuBeforeQueryStatus(
                sender,
                new[] { FileExtensions.CSharp, FileExtensions.VisualBasic, FileExtensions.EntityDataModel });
        }

        private void OnItemMenuBeforeQueryStatus(object sender, IEnumerable<string> supportedExtensions)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotNull(supportedExtensions);

            var menuCommand = sender as MenuCommand;

            if (menuCommand == null)
            {
                return;
            }

            if (_dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var extensionValue = GetSelectedItemExtension();
            menuCommand.Visible = supportedExtensions.Contains(extensionValue);
        }

        private string GetSelectedItemExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selectedItem = _dte2.SelectedItems.Item(1);

            if ((selectedItem.ProjectItem == null)
                || (selectedItem.ProjectItem.Properties == null))
            {
                return null;
            }

            var extension = selectedItem.ProjectItem.Properties.Item("Extension");

            if (extension == null)
            {
                return null;
            }

            return (string)extension.Value;
        }

        private void OnItemContextMenuInvokeHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var menuCommand = sender as MenuCommand;

            if (menuCommand == null)
            {
                return;
            }

            if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidAbout)
            {
                _aboutHandler.ShowDialog();
                return;
            }

            if (_dte2.SelectedItems.Count != 1)
            {
                return;
            }

            try
            {
                Type systemContextType;
                var context = DiscoverUserContextType(out systemContextType);

                if (context != null)
                {
                    if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidPrecompileEntityDataModelViews)
                    {
                        _optimizeContextHandler.OptimizeContext(context);
                    }
                    else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidViewEntityModelDdl)
                    {
                        _viewDdlHandler.ViewDdl(context);
                    }
                    else
                    {
                        _viewContextHandler.ViewContext(menuCommand, context, systemContextType);
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException;

                var remoteStackTraceString =
                    typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? typeof(Exception).GetField("remote_stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);
                remoteStackTraceString.SetValue(innerException, innerException.StackTrace + "$$RethrowMarker$$");

                throw innerException;
            }
            finally
            {
                foreach (var file in _tempFileNames)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                    catch
                    {
                        //Ignored
                    }
                }
                _tempFileNames.Clear();
            }
        }

        private void OnOptimizeContextInvokeHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var extensionValue = GetSelectedItemExtension();

            if (extensionValue != FileExtensions.EntityDataModel)
            {
                OnItemContextMenuInvokeHandler(sender, e);

                return;
            }

            _optimizeContextHandler.OptimizeEdmx(
                (string)_dte2.SelectedItems.Item(1).ProjectItem.Properties.Item("FullPath").Value);
        }

        internal static dynamic GetObjectContext(dynamic context)
        {
            var objectContextAdapterType
                = context.GetType().GetInterface("System.Data.Entity.Infrastructure.IObjectContextAdapter");

            return objectContextAdapterType.InvokeMember("ObjectContext", BindingFlags.GetProperty, null, context, null);
        }

        internal void LogError(string statusMessage, Exception exception)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotEmpty(statusMessage);
            DebugCheck.NotNull(exception);

            var edmSchemaErrorException = exception as EdmSchemaErrorException;

            _dte2.StatusBar.Text = statusMessage;

            var buildOutputWindow = _dte2.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");

            buildOutputWindow.OutputString(Environment.NewLine);

            if (edmSchemaErrorException != null)
            {
                buildOutputWindow.OutputString(edmSchemaErrorException.Message + Environment.NewLine);

                foreach (var error in edmSchemaErrorException.Errors)
                {
                    buildOutputWindow.OutputString(error + Environment.NewLine);
                }
            }
            else
            {
                buildOutputWindow.OutputString(exception + Environment.NewLine);
            }

            buildOutputWindow.Activate();
        }

        private dynamic DiscoverUserContextType(out Type systemContextType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            systemContextType = null;
            var project = _dte2.SelectedItems.Item(1).ProjectItem.ContainingProject;

            if (!project.TryBuild())
            {
                _dte2.StatusBar.Text = Strings.BuildFailed;

                return null;
            }

            var outputFolder = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var projectPath = Path.GetDirectoryName(project.FileName);
            
            // Set current folder so that resolution/construction of the derived DbContext succeeds when there are referenced assemblies
            Directory.SetCurrentDirectory(Path.Combine(projectPath, outputFolder));

            DynamicTypeService typeService;
            IVsSolution solution;
            using (var serviceProvider = new ServiceProvider((VisualStudio.OLE.Interop.IServiceProvider)_dte2.DTE))
            {
                typeService = (DynamicTypeService)serviceProvider.GetService(typeof(DynamicTypeService));
                Assumes.Present(typeService);
                solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
                Assumes.Present(solution);
            }

            IVsHierarchy vsHierarchy;
            var hr = solution.GetProjectOfUniqueName(_dte2.SelectedItems.Item(1).ProjectItem.ContainingProject.UniqueName, out vsHierarchy);

            solution.GetGuidOfProject(vsHierarchy, out Guid projectGuid);
            ProjectGuid = projectGuid;

            if (hr != ProjectExtensions.S_OK)
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            var resolver = typeService.GetTypeResolutionService(vsHierarchy);

            var codeElements = FindClassesInCodeModel(_dte2.SelectedItems.Item(1).ProjectItem.FileCodeModel.CodeElements);

            if (codeElements.Any())
            {
                foreach (var codeElement in codeElements)
                {
                    var userContextType = resolver.GetType(codeElement.FullName);

                    if (userContextType != null && IsContextType(userContextType, out systemContextType))
                    {
                        dynamic contextInfo;

                        var contextInfoType = systemContextType.Assembly.GetType("System.Data.Entity.Infrastructure.DbContextInfo");
                        if (contextInfoType != null)
                        {
                            var startUpProject = GetStartUpProject() ?? project;
                            Configuration userConfig;

                            try
                            {
                                userConfig = GetUserConfig(startUpProject, systemContextType.Assembly.FullName);
                            }
                            catch (Exception ex)
                            {
                                LogError(Strings.LoadConfigFailed, ex);

                                return null;
                            }

                            SetDataDirectory(startUpProject);

                            var constructor = contextInfoType.GetConstructor(new[] { typeof(Type), typeof(Configuration) });

                            if (constructor != null)
                            {
                                // Versions 4.3.0 and higher
                                contextInfo = constructor.Invoke(new object[] { userContextType, userConfig });
                            }
                            else
                            {
                                constructor = contextInfoType.GetConstructor(new[] { typeof(Type), typeof(ConnectionStringSettingsCollection) });
                                Debug.Assert(constructor != null);

                                // Versions 4.1.10715 through 4.2.0.0
                                contextInfo = constructor.Invoke(new object[] { userContextType, userConfig.ConnectionStrings.ConnectionStrings });
                            }
                        }
                        else
                        {
                            // Versions 4.1.10331.0 and lower
                            throw Error.UnsupportedVersion();
                        }

                        if (contextInfo.IsConstructible)
                        {
                            DisableDatabaseInitializer(userContextType, systemContextType);

                            try
                            {
                                return contextInfo.CreateInstance();
                            }
                            catch (Exception exception)
                            {
                                LogError(Strings.CreateContextFailed(userContextType.Name), exception);

                                return null;
                            }
                        }
                    }
                }
            }

            _dte2.StatusBar.Text = Strings.NoContext;

            return null;
        }

        private Project GetStartUpProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var startupProjectPaths = (object[])_dte2.Solution.SolutionBuild.StartupProjects;

            if (startupProjectPaths.Length == 1)
            {
                var startupProjectPath = (string)startupProjectPaths[0];

                if (!Path.IsPathRooted(startupProjectPath))
                {
                    var solutionPath = Path.GetDirectoryName((string)_dte2.Solution.Properties.Item("Path").Value);
                    startupProjectPath = Path.Combine(
                        solutionPath,
                        startupProjectPath);
                }

                return GetSolutionProjects().Single(
                    p =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        string fullName;
                        try
                        {
                            fullName = p.FullName;
                        }
                        catch (NotImplementedException)
                        {
                            return false;
                        }

                        return fullName == startupProjectPath;
                    });
            }

            return null;
        }

        private IEnumerable<Project> GetSolutionProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = new Stack<Project>();

            foreach (var project in _dte2.Solution.Projects.Cast<Project>())
            {
                projects.Push(project);
            }

            while (projects.Count != 0)
            {
                var project = projects.Pop();

                yield return project;

                if (project.ProjectItems != null)
                {
                    foreach (var projectItem in project.ProjectItems.Cast<ProjectItem>())
                    {
                        if (projectItem.SubProject != null)
                        {
                            projects.Push(projectItem.SubProject);
                        }
                    }
                }
            }
        }

        private Configuration GetUserConfig(Project project, string assemblyFullName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotNull(project);

            var userConfigDirectory
                = (string)project.Properties.Item("FullPath").Value;

            var userConfigFilename
                = Path.Combine(
                    userConfigDirectory,
                    project.IsWebProject()
                        ? "Web.config"
                        : "App.config");

            string tempConfigDirectory;
            do
            {
                tempConfigDirectory
                    = Path.Combine(
                        Path.GetTempPath(),
                        Path.GetRandomFileName());
            } while (Directory.Exists(tempConfigDirectory) || File.Exists(tempConfigDirectory));

            Directory.CreateDirectory(tempConfigDirectory);

            var document = XDocument.Load(userConfigFilename);
            FixUpConfig(document, assemblyFullName, userConfigDirectory, tempConfigDirectory);
            var tempFile
                = Path.Combine(tempConfigDirectory,
                               "temp.config");

            document.Save(tempFile);
            _tempFileNames.Add(tempFile);

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = tempFile },
                ConfigurationUserLevel.None);
        }

        private static void SetDataDirectory(Project project)
        {
            DebugCheck.NotNull(project);

            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                Path.GetFullPath(
                    project.IsWebProject()
                        ? Path.Combine(project.GetProjectDir(), "App_Data")
                        : project.GetTargetDir()));
        }

        private static IEnumerable<CodeElement> FindClassesInCodeModel(CodeElements codeElements)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotNull(codeElements);

            foreach (CodeElement codeElement in codeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementClass)
                {
                    yield return codeElement;
                }

                foreach (var element in FindClassesInCodeModel(codeElement.Children))
                {
                    yield return element;
                }
            }
        }

        private static void DisableDatabaseInitializer(Type userContextType, Type systemContextType)
        {
            DebugCheck.NotNull(userContextType);
            DebugCheck.NotNull(systemContextType);

            var databaseType = systemContextType.Assembly.GetType("System.Data.Entity.Database");

            if (databaseType != null)
            {
                var setInitializerMethodInfo
                    = databaseType.GetMethod("SetInitializerInternal", BindingFlags.NonPublic | BindingFlags.Static);

                if (setInitializerMethodInfo != null)
                {
                    var boundSetInitializerMethodInfo
                        = setInitializerMethodInfo.MakeGenericMethod(userContextType);

                    boundSetInitializerMethodInfo.Invoke(null, new object[] { null, true });
                }
            }
        }

        private static bool IsContextType(Type userContextType, out Type systemContextType)
        {
            systemContextType = GetBaseTypes(userContextType).FirstOrDefault(
                t => t.FullName == "System.Data.Entity.DbContext" && t.Assembly.GetName().Name == "EntityFramework");

            return systemContextType != null;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            while (type != typeof(object))
            {
                yield return type.BaseType;

                type = type.BaseType;
            }
        }

        private void FixUpConfig(XDocument document, string assemblyFullName, string userConfigDirectory, string tempConfigDirectory)
        {
            var entityFramework = document.Descendants("entityFramework").FirstOrDefault();
            if (entityFramework == null)
            {
                return;
            }

            var defaultConnectionFactory = entityFramework.Descendants("defaultConnectionFactory").FirstOrDefault();
            if (defaultConnectionFactory != null)
            {
                var type = defaultConnectionFactory.Attribute("type");
                if (type != null)
                {
                    type.SetValue(QualifyAssembly(type.Value, assemblyFullName));
                }
            }

            var appSettings = document.Descendants("appSettings").FirstOrDefault();
            if (appSettings != null)
            {
                var file = appSettings.Attribute("file");
                CopyRelatedConfigFile(userConfigDirectory, tempConfigDirectory, file);
            }

            foreach (var configSection in document.Descendants().Where((section)=>section.Attribute("configSource") != null))
            {
                var configSource = configSection.Attribute("configSource");
                CopyRelatedConfigFile(userConfigDirectory, tempConfigDirectory, configSource);
            }
        }

        private void CopyRelatedConfigFile(string userConfigDirectory, string tempConfigDirectory, XAttribute attr)
        {
            if (attr != null)
            {
                string tempFileName;
                string tempQualifiedFileName;
                do
                {
                    tempFileName = Path.GetRandomFileName();
                    tempQualifiedFileName = Path.Combine(tempConfigDirectory, tempFileName);
                } while (File.Exists(tempQualifiedFileName));

                File.Copy(
                    Path.Combine(userConfigDirectory, attr.Value),
                    tempQualifiedFileName);
                _tempFileNames.Add(tempQualifiedFileName);

                attr.SetValue(tempFileName);
            }
        }

        private static string QualifyAssembly(string typeName, string assemblyFullName)
        {
            var parts = typeName.Split(new[] { ',' }, 2);
            if (parts.Length == 2)
            {
                if (parts[1].Trim().EqualsIgnoreCase("EntityFramework"))
                {
                    return parts[0] + ", " + assemblyFullName;
                }
            }

            return typeName;
        }

        internal T GetService<T>()
            where T : class
        {
            return (T)GetService(typeof(T));
        }

        internal TResult GetService<TService, TResult>()
            where TService : class
            where TResult : class
        {
            return (TResult)GetService(typeof(TService));
        }
    }
}