// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
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
    using System.Xml.Linq;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.DbContextPackage.Extensions;
    using Microsoft.DbContextPackage.Handlers;
    using Microsoft.DbContextPackage.Resources;
    using Microsoft.DbContextPackage.Utilities;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;
    using Configuration = System.Configuration.Configuration;
    using ConfigurationManager = System.Configuration.ConfigurationManager;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "0.9.2", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidDbContextPackagePkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    public sealed class DbContextPackage : Package
    {
        private readonly AddCustomTemplatesHandler _addCustomTemplatesHandler;
        private readonly OptimizeContextHandler _optimizeContextHandler;
        private readonly ReverseEngineerCodeFirstHandler _reverseEngineerCodeFirstHandler;
        private readonly ViewContextHandler _viewContextHandler;
        private readonly ViewDdlHandler _viewDdlHandler;

        private DTE2 _dte2;

        public DbContextPackage()
        {
            _addCustomTemplatesHandler = new AddCustomTemplatesHandler(this);
            _optimizeContextHandler = new OptimizeContextHandler(this);
            _reverseEngineerCodeFirstHandler = new ReverseEngineerCodeFirstHandler(this);
            _viewContextHandler = new ViewContextHandler(this);
            _viewDdlHandler = new ViewDdlHandler(this);
        }

        internal DTE2 DTE2
        {
            get { return _dte2; }
        }

        protected override void Initialize()
        {
            base.Initialize();

            _dte2 = GetService(typeof(DTE)) as DTE2;

            if (_dte2 == null)
            {
                return;
            }

            var oleMenuCommandService
                = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

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

                var menuCommandID5 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidReverseEngineerCodeFirst);
                var menuItem5 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null, OnProjectMenuBeforeQueryStatus, menuCommandID5);

                oleMenuCommandService.AddCommand(menuItem5);

                var menuCommandID6 = new CommandID(GuidList.guidDbContextPackageCmdSet, (int)PkgCmdIDList.cmdidCustomizeReverseEngineerTemplates);
                var menuItem6 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null, OnProjectMenuBeforeQueryStatus, menuCommandID6);

                oleMenuCommandService.AddCommand(menuItem6);
            }
        }

        private void OnProjectMenuBeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;

            if (menuCommand == null)
            {
                return;
            }

            if (_dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var project = _dte2.SelectedItems.Item(1).Project;

            if (project == null)
            {
                return;
            }

            menuCommand.Visible =
                project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"; // csproj
        }

        private void OnItemMenuBeforeQueryStatus(object sender, EventArgs e)
        {
            OnItemMenuBeforeQueryStatus(
                sender,
                new[] { FileExtensions.CSharp, FileExtensions.VisualBasic });
        }

        private void OnOptimizeContextBeforeQueryStatus(object sender, EventArgs e)
        {
            OnItemMenuBeforeQueryStatus(
                sender,
                new[] { FileExtensions.CSharp, FileExtensions.VisualBasic, FileExtensions.EntityDataModel });
        }

        private void OnItemMenuBeforeQueryStatus(object sender, IEnumerable<string> supportedExtensions)
        {
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

        private void OnProjectContextMenuInvokeHandler(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;
            if (menuCommand == null || _dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var project = _dte2.SelectedItems.Item(1).Project;
            if (project == null)
            {
                return;
            }

            if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidReverseEngineerCodeFirst)
            {
                _reverseEngineerCodeFirstHandler.ReverseEngineerCodeFirst(project);
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidCustomizeReverseEngineerTemplates)
            {
                _addCustomTemplatesHandler.AddCustomTemplates(project);
            }
        }

        private void OnItemContextMenuInvokeHandler(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;

            if (menuCommand == null)
            {
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
        }

        private void OnOptimizeContextInvokeHandler(object sender, EventArgs e)
        {
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
            DebugCheck.NotEmpty(statusMessage);
            DebugCheck.NotNull(exception);

            var edmSchemaErrorException = exception as EdmSchemaErrorException;
            var compilerErrorException = exception as CompilerErrorException;

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
            else if (compilerErrorException != null)
            {
                buildOutputWindow.OutputString(compilerErrorException.Message + Environment.NewLine);

                foreach (var error in compilerErrorException.Errors)
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
            systemContextType = null;
            var project = _dte2.SelectedItems.Item(1).ProjectItem.ContainingProject;

            if (!project.TryBuild())
            {
                _dte2.StatusBar.Text = Strings.BuildFailed;

                return null;
            }

            DynamicTypeService typeService;
            IVsSolution solution;
            using (var serviceProvider = new ServiceProvider((VisualStudio.OLE.Interop.IServiceProvider)_dte2.DTE))
            {
                typeService = (DynamicTypeService)serviceProvider.GetService(typeof(DynamicTypeService));
                solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            }

            IVsHierarchy vsHierarchy;
            var hr = solution.GetProjectOfUniqueName(_dte2.SelectedItems.Item(1).ProjectItem.ContainingProject.UniqueName, out vsHierarchy);

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

        private static Configuration GetUserConfig(Project project, string assemblyFullName)
        {
            DebugCheck.NotNull(project);

            var userConfigFilename
                = Path.Combine(
                    (string)project.Properties.Item("FullPath").Value,
                    project.IsWebProject()
                        ? "Web.config"
                        : "App.config");

            var document = XDocument.Load(userConfigFilename);
            FixUpConfig(document, assemblyFullName);

            var tempFile = Path.GetTempFileName();
            document.Save(tempFile);

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

        private static void FixUpConfig(XDocument document, string assemblyFullName)
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