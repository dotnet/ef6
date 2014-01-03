// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.CSharp;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualBasic;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.Win32;
    using VSLangProj;
    using VSLangProj80;
    using VsWebSite;
    using VsWebSite90;
    using Constants = EnvDTE.Constants;
    using PrjKind = VSLangProj.PrjKind;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal enum VisualStudioProjectSystem
    {
        WindowsApplication = 0,
        WebApplication = 1,
        Website = 2
    }

    /// <summary>
    ///     Helper class to work with the VS DTE
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class VsUtils
    {
        private const string EntityFrameworkAssemblyName = "EntityFramework";
        private const string SystemDataEntityAssemblyName = "System.Data.Entity";
        internal const string AppConfigFileName = "App.Config";
        internal const string WebConfigFileName = "Web.Config";
        private const string MacroMatch = "macroMatch";
        private static readonly Regex MacroRegex = new Regex(@"\$\((?<" + MacroMatch + @">\w+)\)");
        private static readonly string OutputWindowPaneTitle = Resources.EDMOutputWindowPaneTitle;
        internal static readonly string DevEnvDirMacroName = "DevEnvDir";

        /// <summary>
        ///     Finds the first ProjectItem with the specified name in the given ProjectItems.
        /// </summary>
        /// <param name="projectItems">The ProjectItems to search</param>
        /// <param name="nameToMatch">The name to find.</param>
        /// <returns>ProjectItem if found, null otherwise</returns>
        internal static ProjectItem FindFirstProjectItemWithName(ProjectItems projectItems, string nameToMatch)
        {
            if (null == projectItems)
            {
                throw new ArgumentNullException("projectItems");
            }

            return projectItems
                .Cast<ProjectItem>()
                .FirstOrDefault(
                    projectItem => String.Compare(
                        projectItem.Name.ToUpperInvariant(),
                        nameToMatch.ToUpperInvariant(),
                        StringComparison.Ordinal) == 0);
        }

        /// <summary>
        ///     get the currently active project
        /// </summary>
        internal static Project GetActiveProject(DTE2 dte)
        {
            Project project = null;
            try
            {
                var activeSolutionProjects = (Array)dte.ActiveSolutionProjects;
                if (activeSolutionProjects.Length > 0)
                {
                    project = (Project)activeSolutionProjects.GetValue(0);
                }
            }
            catch (COMException)
            {
                // Accessing ActiveSolutionProjects can throw in some edge cases (628215), so just get the first project instead.
            }

            if (project == null)
            {
                if (dte.ActiveDocument != null
                    && dte.ActiveDocument.ProjectItem != null
                    && dte.ActiveDocument.ProjectItem.ContainingProject != null)
                {
                    project = dte.ActiveDocument.ProjectItem.ContainingProject;
                }
                else if (dte.Solution.Projects.Count > 0)
                {
                    project = dte.Solution.Projects.Item(1);
                }
            }

            return project;
        }

        // load web.config as an XML document (also output name of file to be used for saving)
        private static XmlDocument LoadWebConfig(Project project, out string webConfigFilepath)
        {
            webConfigFilepath = null;

            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            var webItemConfig = ConnectionManager.FindOrCreateWebConfig(project);
            if (null == webItemConfig)
            {
                return null;
            }

            // pass 1 to get_FileNames because it asserts if we pass 0
            webConfigFilepath = webItemConfig.FileNames[1];

            return ConnectionManager.LoadConfigFile(webConfigFilepath);
        }

        /// <summary>
        ///     This will resolve a path with a certain macro, such as: "$(DevEnvDir)\devenv.exe" and can
        ///     also handle custom macros.
        /// </summary>
        /// <param name="project">Pass in null to not resolve any project macros</param>
        internal static string ResolvePathWithMacro(Project project, string path, Dictionary<string, string> customMacros)
        {
            var macroMatch = MacroRegex.Match(path);
            if (macroMatch.Success)
            {
                var resolvedPathBuilder = new StringBuilder();
                var previousMacroEndIndex = 0;

                // we only want to match the 'macroMatch'
                var foundMacrosGroup = macroMatch.Groups[MacroMatch];

                var captures = from singleCapture in foundMacrosGroup.Captures.OfType<Capture>()
                               orderby singleCapture.Index
                               select singleCapture;
                foreach (var singleMacroCapture in captures)
                {
                    var macroValue = ResolveMacro(project, singleMacroCapture.Value, customMacros);
                    resolvedPathBuilder.Append(path.Substring(previousMacroEndIndex, singleMacroCapture.Index - 2 - previousMacroEndIndex));
                    resolvedPathBuilder.Append(macroValue);
                    previousMacroEndIndex = singleMacroCapture.Index + singleMacroCapture.Length + 1;
                }

                if (previousMacroEndIndex < path.Length)
                {
                    resolvedPathBuilder.Append(path.Substring(previousMacroEndIndex));
                }

                return resolvedPathBuilder.ToString();
            }
            return path;
        }

        /// <summary>
        ///     Used to resolve a single macro, such as "DevEnvDir", "ProjectDir", or "TargetDir". A dictionary of custom macros can also be passed in
        ///     as a 'backup'.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsBuildPropertyStorage.GetPropertyValue(System.String,System.String,System.UInt32,System.String@)")]
        internal static string ResolveMacro(Project project, string macroName, Dictionary<string, string> customMacros)
        {
            var macroValue = String.Empty;

            if (project != null)
            {
                var hierarchy = GetVsHierarchy(project, Services.ServiceProvider);
                var vsProject = hierarchy as IVsProject;
                Debug.Assert(vsProject != null, "IVsHierarchy should be an IVsProject");
                if (vsProject != null)
                {
                    var buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
                    if (buildPropertyStorage != null)
                    {
                        buildPropertyStorage.GetPropertyValue(
                            macroName, String.Empty, (uint)_PersistStorageType.PST_PROJECT_FILE, out macroValue);
                    }
                }
            }

            // try the custom macros if we can't find a default one
            if (String.IsNullOrEmpty(macroValue)
                && customMacros != null)
            {
                customMacros.TryGetValue(macroName, out macroValue);
                if (macroValue == null)
                {
                    macroValue = String.Empty;
                }
            }

            if (String.IsNullOrEmpty(macroValue))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.VsUtils_ErrorResolvingMacro, macroName));
            }

            return macroValue;
        }

        internal static void RegisterAssembly(Project project, string assemblyFullName)
        {
            string webConfigFilepath;
            var webConfigDocument = LoadWebConfig(project, out webConfigFilepath);
            if (null == webConfigDocument)
            {
                return;
            }

            var compilation = FindOrCreateXmlElement(webConfigDocument, "configuration/system.web/compilation");
            if (null != compilation)
            {
                // <assemblies>
                var assembliesElement = FindOrCreateXmlElement(webConfigDocument, "configuration/system.web/compilation/assemblies");

                // <add> for System.Data.Entity assembly
                FindOrCreateChildXmlElementWithAttribute(assembliesElement, "add", "assembly", assemblyFullName, new AssemblyNameComparer());
                ConnectionManager.UpdateConfigFile(webConfigDocument, webConfigFilepath);
            }
        }

        internal static void RegisterBuildProviders(Project project)
        {
            string webConfigFilepath;
            var webConfigDocument = LoadWebConfig(project, out webConfigFilepath);
            if (null == webConfigDocument)
            {
                return;
            }

            var compilation = FindOrCreateXmlElement(webConfigDocument, "configuration/system.web/compilation");
            if (null != compilation)
            {
                // <buildProviders>
                var buildProviders = FindOrCreateXmlElement(webConfigDocument, "configuration/system.web/compilation/buildProviders");

                // <add> for edmx
                var add = FindOrCreateChildXmlElementWithAttribute(buildProviders, "add", "extension", ".edmx", null);

                // Hardcode the build provider full type name to avoid the dependency on System.Data.Entity.Design.
                add.SetAttribute("type", "System.Data.Entity.Design.AspNet.EntityDesignerBuildProvider");

                ConnectionManager.UpdateConfigFile(webConfigDocument, webConfigFilepath);
            }
        }

        /// <summary>
        ///     Helper to finds or create all nodes specified in elementPath
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="elementPath">full path to element separated by '/'</param>
        /// <param name="prependChild"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        private static XmlNode FindOrCreateXmlElement(XmlNode parentNode, string elementPath, bool prependChild = false)
        {
            Debug.Assert(parentNode != null, "parentNode != null");
            Debug.Assert(
                parentNode.NodeType == XmlNodeType.Document ||
                parentNode.OwnerDocument != null, "parentNode.OwnerDocument != null");

            XmlNode xmlNode = null;

            var elementNames = elementPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var elementName in elementNames)
            {
                xmlNode = parentNode.SelectSingleNode(elementName);
                if (null == xmlNode)
                {
                    xmlNode = parentNode.OwnerDocument.CreateElement(elementName);
                    if (prependChild)
                    {
                        parentNode.PrependChild(xmlNode);
                    }
                    else
                    {
                        parentNode.AppendChild(xmlNode);
                    }
                }

                parentNode = xmlNode;
            }

            return xmlNode;
        }

        /// <summary>
        ///     Helper to find first child element with a particular attribute with
        ///     a particular value (if any attribute value is good use null, but
        ///     note this is different from an empty string which will require the
        ///     attribute to have an empty value).
        ///     If such a child element doesn't exist, create it.
        /// </summary>
        private static XmlElement FindOrCreateChildXmlElementWithAttribute(
            XmlNode parentNode, string elementName, string attributeName, string attributeValue, IComparer<string> comparer)
        {
            var xmlNodeList = parentNode.SelectNodes(elementName);
            if (null != xmlNodeList)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var attribute = xmlNode.Attributes[attributeName];
                    if (null != attribute)
                    {
                        var elem = xmlNode as XmlElement;
                        if (null == attributeValue)
                        {
                            // if attribute value doesn't matter
                            return elem;
                        }
                        else
                        {
                            // if customComparer is passed, use it to compare the attribute value
                            if (comparer != null
                                && comparer.Compare(attributeValue, attribute.Value) == 0)
                            {
                                elem.SetAttribute(attributeName, attributeValue);
                                return elem;
                            }
                                // else just do normal string comparison
                            else if (attribute.Value == attributeValue)
                            {
                                return elem;
                            }
                        }
                    }
                }
            }

            // otherwise element doesn't exist, so create it
            var xmlElement = parentNode.OwnerDocument.CreateElement(elementName);
            xmlElement.SetAttribute(attributeName, attributeValue);
            parentNode.AppendChild(xmlElement);
            return xmlElement;
        }

        private class AssemblyNameComparer : IComparer<string>
        {
            int IComparer<string>.Compare(string assemblyName1, string assemblyName2)
            {
                // The expected full assembly name will look like "System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                // we only interested in comparing the assembly name

                var assemblyName1Parts = assemblyName1.Split(',');
                var assemblyName2Parts = assemblyName2.Split(',');

                Debug.Assert(assemblyName1Parts.Length > 0 && assemblyName2Parts.Length > 0, "Invalid assembly names");
                if (assemblyName1Parts.Length > 0
                    && assemblyName2Parts.Length > 0)
                {
                    return String.Compare(assemblyName1Parts[0], assemblyName2Parts[0], StringComparison.OrdinalIgnoreCase);
                }
                // just compare the strings as is.
                return String.Compare(assemblyName1, assemblyName2, StringComparison.OrdinalIgnoreCase);
            }
        }

        internal static void LogStandardError(string message, string filePath, int lineNumber, int columnNumber)
        {
            LogStandardTask(message, filePath, lineNumber, columnNumber, TaskErrorCategory.Error);
        }

        internal static void LogStandardWarning(string message, string filePath, int lineNumber, int columnNumber)
        {
            LogStandardTask(message, filePath, lineNumber, columnNumber, TaskErrorCategory.Warning);
        }

        private static void LogStandardTask(string message, string filePath, int lineNumber, int columnNumber, TaskErrorCategory category)
        {
            ErrorListHelper.MiscErrorList.AddItem(
                new ErrorTask
                    {
                        Document = filePath,
                        Text = message,
                        Line = lineNumber,
                        Column = columnNumber,
                        ErrorCategory = category
                    });
        }

        /// <summary>
        ///     Logs a message to the output window pane for ModelGen output
        /// </summary>
        internal static void LogOutputWindowPaneMessage(Project project, string message, bool activateWindow = true)
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }

            var outputWindowPane = GetOutputWindowPane(project, activateWindow);
            if (null != outputWindowPane)
            {
                outputWindowPane.OutputString(message + Environment.NewLine);
            }
        }

        /// <summary>
        ///     Returns an output window pane for ModelGen output (creates it if necessary)
        /// </summary>
        private static OutputWindowPane GetOutputWindowPane(Project project, bool activate)
        {
            Debug.Assert(null != project, "project should not be null");
            Debug.Assert(null != project.DTE, "project.DTE should not be null");

            if (null == project.DTE)
            {
                return null;
            }

            var window = project.DTE.Windows.Item(Constants.vsWindowKindOutput);
            if (null == window)
            {
                return null;
            }

            if (activate)
            {
                window.Activate();
                window.Visible = true;
            }

            var outputWindow = window.Object as OutputWindow;
            if (null == outputWindow)
            {
                return null;
            }

            OutputWindowPane outputWindowPane = null;

            try
            {
                // Get an existing pane
                outputWindowPane = outputWindow.OutputWindowPanes.Item(OutputWindowPaneTitle);
            }
                // Spec says that OutputWindowPanes.Item() can throw ArgumentException, 
                // but actually it throws NullReferenceException and NotImplementedException (WCF projects)
            catch (Exception ex)
            {
                if (ex is ArgumentException
                    || ex is NullReferenceException
                    || ex is NotImplementedException)
                {
                    // Doesn't exist, so create it
                    outputWindowPane = outputWindow.OutputWindowPanes.Add(OutputWindowPaneTitle);
                }
                else
                {
                    throw;
                }
            }

            Debug.Assert(null != outputWindowPane, "outputWindowPane should not be null");
            if (activate && outputWindowPane != null)
            {
                outputWindowPane.Activate();
            }

            return outputWindowPane;
        }

        /// <summary>
        ///     returns the DirectoryInfo for the root of the project
        /// </summary>
        internal static DirectoryInfo GetProjectRoot(Project project, IServiceProvider serviceProvider)
        {
            DirectoryInfo projectRootDirectoryInfo = null;
            if (IsMiscellaneousProject(project))
            {
                var solution = serviceProvider.GetService(typeof(IVsSolution)) as IVsSolution;
                if (solution != null)
                {
                    string solutionDirectory;
                    string solutionFile;
                    string userOptionsFile;
                    var hr = solution.GetSolutionInfo(out solutionDirectory, out solutionFile, out userOptionsFile);
                    if (NativeMethods.Succeeded(hr) && !String.IsNullOrEmpty(solutionDirectory))
                    {
                        projectRootDirectoryInfo = new DirectoryInfo(solutionDirectory);
                    }
                }
            }
            else
            {
                var projectFullPathProperty = project.Properties.Item("FullPath");
                var value = projectFullPathProperty.Value as string;
                if (!String.IsNullOrEmpty(value))
                {
                    // Websites do not have a project filename so the resulting FullPath will not
                    // have a terminating directory separator char. As a result, any downstream
                    // APIs will think that the website name is a filename.
                    if (IsWebSiteProject(project))
                    {
                        value += Path.DirectorySeparatorChar;
                    }
                    projectRootDirectoryInfo = new DirectoryInfo(value);
                }
            }

            if (projectRootDirectoryInfo == null)
            {
                // just use the current working direcotry. Not sure that this should ever happen.
                Debug.Fail("Didn't find a project root for project or solution. Did we miss something?");
                projectRootDirectoryInfo = new DirectoryInfo(".\\");
            }
            return projectRootDirectoryInfo;
        }

        internal static string GetProjectPathWithName(Project project, out bool hasFileName)
        {
            hasFileName = false;
            if (project != null)
            {
                var projectDirectoryInfo = GetProjectRoot(project, Services.ServiceProvider);
                if (projectDirectoryInfo != null)
                {
                    var directoryPath = projectDirectoryInfo.FullName;
                    if (!IsWebSiteProject(project)
                        && !IsMiscellaneousProject(project))
                    {
                        var fileNameProperty = project.Properties.Item("FileName");
                        if (fileNameProperty == null
                            || String.IsNullOrWhiteSpace(fileNameProperty.Value.ToString()))
                        {
                            Debug.Fail(
                                "We weren't able to determine the filename of this project even though it is not a website or miscellaneous project");
                        }
                        else
                        {
                            hasFileName = true;
                            return Path.Combine(directoryPath, fileNameProperty.Value.ToString());
                        }
                    }
                    return directoryPath;
                }
            }
            Debug.Fail("didn't find a project path for project or solution.  Did we miss something?");
            return String.Empty;
        }

        internal static void ShowErrorDialog(string message)
        {
            ShowError(PackageManager.Package, message);
        }

        // returns whether an identifier is valid in the given language
        internal static bool IsValidIdentifier(string id, Project project, VisualStudioProjectSystem appType)
        {
            // for WebSite projects we should check with both languages (C# and VB)
            // because they can use both C# and VB code
            if (appType == VisualStudioProjectSystem.Website
                || project == null
                || project.CodeModel == null
                || IsMiscellaneousProject(project))
            {
                using (var csCodeDomProvider = new CSharpCodeProvider())
                {
                    if (csCodeDomProvider.IsValidIdentifier(id))
                    {
                        using (var vbCodeDomProvider = new VBCodeProvider())
                        {
                            return vbCodeDomProvider.IsValidIdentifier(id);
                        }
                    }
                }
                return false;
            }
            else
            {
                var language = project.CodeModel.Language;
                if (CodeModelLanguageConstants.vsCMLanguageVB == language)
                {
                    using (var codeDomProvider = new VBCodeProvider())
                    {
                        return codeDomProvider.IsValidIdentifier(id);
                    }
                }
                else
                {
                    Debug.Assert(CodeModelLanguageConstants.vsCMLanguageCSharp == language, "Project language is not VB or C#");
                    using (var codeDomProvider = new CSharpCodeProvider())
                    {
                        return codeDomProvider.IsValidIdentifier(id);
                    }
                }
            }
        }

        /// <summary>
        ///     This function checks that this is a valid file name. This checks for:
        ///     1. This file name is not a reserved name, such as 'CON' or 'LPT'
        ///     2. This file name does not contain any invalid file name characters
        /// </summary>
        internal static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var filenameWithNoExtension = Path.GetFileNameWithoutExtension(fileName);

            // check that filename w/o extension is not a system reserved name, such as 'CON'
            const string reservedNamesRegex = "^((nul|con|aux|prn)(.)?$)|^((com|lpt)[0-9])$";
            var reservedNameMatches = Regex.Matches(
                filenameWithNoExtension, reservedNamesRegex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (reservedNameMatches.Count != 0)
            {
                return false;
            }

            // check InvalidFileNameChars and any of: '\', '/', ':', '*', '?', '"', '<', '>', '|'
            var invalidPathChars = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1
                || fileName.IndexOfAny(invalidPathChars) != -1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns application type for given project
        ///     everything that is not WebSite nor WebApplication is considered to be a WindowsApplication
        /// </summary>
        internal static VisualStudioProjectSystem GetApplicationType(IServiceProvider serviceProvider, Project project)
        {
            var applicationType = VisualStudioProjectSystem.WindowsApplication;
            var guidsString = GetAggregateProjectTypeGuids(serviceProvider, project);
            if (null != guidsString)
            {
                // WebSite?
                if (guidsString.IndexOf(ProjectKindWeb, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // make sure, that no application contains both WebSite and WebApplication guids
                    Debug.Assert(
                        guidsString.IndexOf(WebAppProjectGuid, StringComparison.OrdinalIgnoreCase) < 0,
                        string.Format(
                            CultureInfo.CurrentCulture, "guidsString should not contain both {0} and {1}", ProjectKindWeb, WebAppProjectGuid));

                    applicationType = VisualStudioProjectSystem.Website;
                }
                    // WebApplication?
                else if (guidsString.IndexOf(WebAppProjectGuid, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    applicationType = VisualStudioProjectSystem.WebApplication;
                }
            }
            else
            {
                if (String.Equals(project.Kind, ProjectKindWeb, StringComparison.OrdinalIgnoreCase))
                {
                    applicationType = VisualStudioProjectSystem.Website;
                }
                else if (String.Equals(project.Kind, WebAppProjectGuid, StringComparison.OrdinalIgnoreCase))
                {
                    applicationType = VisualStudioProjectSystem.WebApplication;
                }
            }

            return applicationType;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject.GetAggregateProjectTypeGuids(System.String@)")]
        internal static string GetAggregateProjectTypeGuids(IServiceProvider serviceProvider, Project project)
        {
            var guidsString = PackageManager.Package.AggregateProjectTypeGuidCache.GetGuids(project);

            if (guidsString == null)
            {
                var solution = serviceProvider.GetService(typeof(IVsSolution)) as IVsSolution;
                Debug.Assert(null != solution, "solution should not be null");

                IVsHierarchy hierarchy;
                var hr = solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);
                if (NativeMethods.Succeeded(hr))
                {
                    var aggregatableProject = hierarchy as IVsAggregatableProject;
                    if (null != aggregatableProject)
                    {
                        // The project guids string looks like "{Guid 1};{Guid 2};...{Guid n}" with Guid n the inner most
                        aggregatableProject.GetAggregateProjectTypeGuids(out guidsString);
                    }
                }

                if (!string.IsNullOrEmpty(guidsString))
                {
                    PackageManager.Package.AggregateProjectTypeGuidCache.Add(project, guidsString);
                }
            }

            return guidsString;
        }

        public static void UnlockRunningDocument(IServiceProvider site, uint docCookie, _VSRDTFLAGS unlockFlags)
        {
            var rdt = new RunningDocumentTable(site);
            rdt.UnlockDocument(unlockFlags, docCookie);
        }

        internal static IVsHierarchy GetVsHierarchy(Project project, IServiceProvider serviceProvider)
        {
            return VSHelpers.GetVsHierarchy(project, serviceProvider);
        }

        internal static uint GetProjectItemId(IVsHierarchy hierarchy, ProjectItem projectItem)
        {
            int iFound;
            uint itemId;
            var project = hierarchy as IVsProject;
            var pdwPriority = new VSDOCUMENTPRIORITY[1];

            // pass 1 to get_FileNames because it asserts if we pass 0
            Debug.Assert(project != null, "project != null");
            NativeMethods.ThrowOnFailure(project.IsDocumentInProject(projectItem.FileNames[1], out iFound, pdwPriority, out itemId));

            // in some cases IsDocumentInProject could return S_OK but still not find the document we're looking for.
            // this way we make sure to return a 0 itemId in those cases.
            return iFound != 0 ? itemId : 0;
        }

        internal static void SetTextForVsTextLines(IVsTextLines vsTextLines, string text)
        {
            int endLine, endCol;
            VSErrorHandler.ThrowOnFailure(vsTextLines.GetLastLineIndex(out endLine, out endCol));
            var length = (text == null) ? 0 : text.Length;

            var textPtr = IntPtr.Zero;
            try
            {
                textPtr = Marshal.StringToCoTaskMemAuto(text);
                VSErrorHandler.ThrowOnFailure(vsTextLines.ReplaceLines(0, 0, endLine, endCol, textPtr, length, null));
            }
            finally
            {
                if (textPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(textPtr);
                }
            }
        }

        /// <summary>
        ///     Changes the current cursor to an hourglass, and restores it
        ///     when Dispose is called.
        /// </summary>
        internal sealed class HourglassHelper : IDisposable
        {
            private readonly Cursor _previousCursor;

            public HourglassHelper()
            {
                //Change the cursor to an hourglass
                _previousCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
            }

            public void Dispose()
            {
                //Clear the hourglass cursor, and restore
                Cursor.Current = _previousCursor;
            }
        }

        /// <summary>
        ///     Helper method to show an error message within the shell.  This should be used
        ///     instead of MessageBox.Show();
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="errorText">Text to display.</param>
        public static void ShowError(IServiceProvider serviceProvider, string errorText)
        {
            ShowMessageBox(
                serviceProvider, errorText, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_CRITICAL);
        }

        /// <summary>
        ///     Helper method to show a message box within the shell.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="messageText">Text to show.</param>
        /// <param name="messageButtons">Buttons which should appear in the dialog.</param>
        /// <param name="defaultButton">Default button (invoked when user presses return).</param>
        /// <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        /// <returns>result corresponding to the button clicked by the user.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        public static DialogResult ShowMessageBox(
            IServiceProvider serviceProvider, string messageText, OLEMSGBUTTON messageButtons, OLEMSGDEFBUTTON defaultButton,
            OLEMSGICON messageIcon)
        {
            return ShowMessageBox(serviceProvider, messageText, null, messageButtons, defaultButton, messageIcon);
        }

        /// <summary>
        ///     Helper method to show a message box within the shell.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="messageText">Text to show.</param>
        /// <param name="f1Keyword">F1-keyword.</param>
        /// <param name="messageButtons">Buttons which should appear in the dialog.</param>
        /// <param name="defaultButton">Default button (invoked when user presses return).</param>
        /// <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        /// <returns>result corresponding to the button clicked by the user.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        public static DialogResult ShowMessageBox(
            IServiceProvider serviceProvider, string messageText, string f1Keyword, OLEMSGBUTTON messageButtons,
            OLEMSGDEFBUTTON defaultButton, OLEMSGICON messageIcon)
        {
            var result = 0;

            Debug.Assert(serviceProvider != null, "ServiceProvider was null");
            if (serviceProvider != null)
            {
                var uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));

                Debug.Assert(uiShell != null, "ServiceProvider returned null for SVsUIShell");
                if (uiShell != null)
                {
                    var rclsidComp = Guid.Empty;
                    VSErrorHandler.ThrowOnFailure(
                        uiShell.ShowMessageBox(
                            0, ref rclsidComp, null, messageText, f1Keyword, 0, messageButtons, defaultButton, messageIcon, 0, out result));
                }
            }
            else
            {
                Debug.Fail("Unable to get IUIService.  Falling back on MessageBox.Show()");

                MessageBoxOptions options = 0;
                if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                {
                    options = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
                }
                MessageBox.Show(
                    null, messageText, Resources.Application_Caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1, options);
            }

            return (DialogResult)result;
        }

        internal const string ProjectKindVb = PrjKind.prjKindVBProject;
        internal const string ProjectKindCSharp = PrjKind.prjKindCSharpProject;
        internal const string ProjectKindWeb = "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";
        internal const string WebAppProjectGuid = "{349C5851-65DF-11DA-9384-00065B846F21}";

        internal enum ProjectKind
        {
            Unknown,
            VB,
            CSharp,
            Web
        }

        internal static ProjectKind GetProjectKind(Project project)
        {
            if (project == null)
            {
                Debug.Fail("project is null");
                return ProjectKind.Unknown;
            }
            if (String.Equals(project.Kind, ProjectKindVb, StringComparison.OrdinalIgnoreCase))
            {
                return ProjectKind.VB;
            }
            if (String.Equals(project.Kind, ProjectKindCSharp, StringComparison.OrdinalIgnoreCase))
            {
                return ProjectKind.CSharp;
            }
            if (String.Equals(project.Kind, ProjectKindWeb, StringComparison.OrdinalIgnoreCase))
            {
                return ProjectKind.Web;
            }

            return ProjectKind.Unknown;
        }

        internal static bool IsWebSiteCSharpProject(Project project)
        {
            if (IsWebSiteProject(project))
            {
                var prop = project.Properties.Item("CurrentWebsiteLanguage");
                if (prop != null)
                {
                    var currentWebsiteLanguage = prop.Value as string;
                    if (currentWebsiteLanguage != null)
                    {
                        if (currentWebsiteLanguage.Equals("C#", StringComparison.OrdinalIgnoreCase)
                            || currentWebsiteLanguage.Equals("Visual C#", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static bool IsWebSiteVBProject(Project project)
        {
            if (!IsWebSiteProject(project))
            {
                return false;
            }

            var prop = project.Properties.Item("CurrentWebsiteLanguage");
            if (prop == null)
            {
                return false;
            }

            var currentWebsiteLanguage = prop.Value as string;
            if (currentWebsiteLanguage == null)
            {
                return false;
            }

            return currentWebsiteLanguage.Equals("VB", StringComparison.OrdinalIgnoreCase)
                   || currentWebsiteLanguage.Equals("Visual Basic", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsWebSiteProject(Project project)
        {
            return GetProjectKind(project) == ProjectKind.Web;
        }

        internal static LangEnum GetLanguageForProject(Project project)
        {
            switch (GetProjectKind(project))
            {
                case ProjectKind.CSharp:
                    return LangEnum.CSharp;
                case ProjectKind.VB:
                    return LangEnum.VisualBasic;
                case ProjectKind.Web:
                    if (IsWebSiteVBProject(project))
                    {
                        return LangEnum.VisualBasic;
                    }
                    if (IsWebSiteCSharpProject(project))
                    {
                        return LangEnum.CSharp;
                    }
                    break;
            }
            return LangEnum.Unknown;
        }

        internal static ProjectItem GetProjectItem(IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy == null)
            {
                Debug.Fail("null hierarchy passed to GetProjectItem?");
                return null;
            }

            object o;
            var hr = hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out o);
            if (NativeMethods.Succeeded(hr))
            {
                return o as ProjectItem;
            }
            return null;
        }

        internal static IEnumerable<IVsHierarchy> GetAllHierarchiesInSolution(IServiceProvider serviceProvider)
        {
            var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
            {
                var guid = Guid.Empty;
                IEnumHierarchies hierarchyEnum = null;

                solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref guid, out hierarchyEnum);
                if (hierarchyEnum != null)
                {
                    hierarchyEnum.Reset();

                    uint numFetched = 1;
                    var item = new IVsHierarchy[1];

                    hierarchyEnum.Next(1, item, out numFetched);
                    while (numFetched == 1)
                    {
                        yield return item[0];
                        hierarchyEnum.Next(1, item, out numFetched);
                    }
                }
            }
            else
            {
                Debug.Fail("Unable to find solution service");
            }
        }

        internal static IEnumerable<Project> GetAllProjectsInSolution(IServiceProvider serviceProvider)
        {
            return GetAllHierarchiesInSolution(serviceProvider)
                .Select(VSHelpers.GetProject)
                .Where(project => project != null);
        }

        internal static ProjectItem GetProjectItemForDocument(string projectItemFileName, IServiceProvider serviceProvider)
        {
            IVsHierarchy hier;
            Project proj;
            uint itemId;
            bool isDocInProj;
            object o;

            VSHelpers.GetProjectAndFileInfoForPath(projectItemFileName, serviceProvider, out hier, out proj, out itemId, out isDocInProj);
            if (null == hier)
            {
                return null;
            }

            if (VSErrorHandler.Succeeded(hier.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out o)))
            {
                var projectItem = o as ProjectItem;
                if (itemId != VSConstants.VSITEMID_ROOT)
                {
                    // Don't bother tracing for VSITEMID_ROOT - it returns an EnvDTE.Project
                    Debug.Assert(
                        projectItem != null,
                        "The filename you passed in does not correspond to a ProjectItem; it may be a Project or something else");
                }
                if (projectItem != null)
                {
                    return projectItem;
                }
            }
            return null;
        }

        public static bool IsMiscellaneousProject(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            return project.UniqueName == Constants.vsMiscFilesProjectUniqueName;
        }

        internal static object GetProjectPropertyByName(Project project, string propertyName)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(propertyName), "propertyName is null or empty.");

            return GetPropertyByName(project.Properties, propertyName);
        }

        private static object GetProjectConfigurationPropertyByName(Project project, string propertyName)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(propertyName), "propertyName is null or empty.");

            return GetPropertyByName(
                project.ConfigurationManager.ActiveConfiguration.Properties,
                propertyName);
        }

        private static object GetPropertyByName(Properties properties, string propertyName)
        {
            Debug.Assert(properties != null, "properties is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(propertyName), "propertyName is null or empty.");

            return (from Property p in properties
                    where p.Name == propertyName
                    select p.Value).FirstOrDefault();
        }

        internal static string GetTargetFrameworkMonikerForProject(Project project, IServiceProvider serviceProvider)
        {
            return GetTargetFrameworkMonikerForDocument(project, VSConstants.VSITEMID_ROOT, serviceProvider);
        }

        /// <summary>
        ///     Gets the text from DocData if it's available, otherwise retreives it from disk.
        /// </summary>
        internal static string GetFileContentsFromBufferOrDisk(string filePath)
        {
            // attempt to construct the text from the doc data if it is available
            var fileText = string.Empty;
            var docData = VSHelpers.GetDocData(Services.ServiceProvider, filePath);
            var textLines = docData as IVsTextLines;
            if (textLines != null)
            {
                // get the text from the VS buffer and construct an XML DOM object from it
                fileText = VSHelpers.GetTextFromVsTextLines(textLines);
            }
            else
            {
                fileText = File.ReadAllText(filePath);
            }

            return fileText;
        }

        /// <summary>
        ///     Return target framework moniker for given document.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static string GetTargetFrameworkMonikerForDocument(Project project, uint itemId, IServiceProvider serviceProvider)
        {
            if (IsMiscellaneousProject(project))
            {
                return string.Empty;
            }

            var hierarchy = VSHelpers.GetVsHierarchy(project, serviceProvider);
            Debug.Assert(hierarchy != null, "Hierarchy should not be null");

            object moniker;

            if (NativeMethods.Succeeded(
                hierarchy.GetProperty(itemId, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out moniker)))
            {
                return moniker as string;
            }

            if (NativeMethods.Succeeded(
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out moniker)))
            {
                return moniker as string;
            }

            return String.Empty;
        }

        /// <summary>
        ///     Gets the relative path of a given absolute path within a project
        /// </summary>
        /// <param name="project">DTE Project</param>
        /// <param name="path">path that may contain the project path</param>
        /// <param name="relativePath">relative path in the project</param>
        /// <returns>true if the path contains the project path or false otherwise</returns>
        internal static bool TryGetRelativePathInProject(Project project, string path, out string relativePath)
        {
            relativePath = String.Empty;
            var projectDirectory = GetProjectRoot(project, Services.ServiceProvider);
            return projectDirectory != null && TryGetRelativePathInParentPath(projectDirectory.FullName, path, out relativePath);
        }

        internal static bool TryGetRelativePathInParentPath(string parentPath, string path, out string relativePath)
        {
            relativePath = String.Empty;
            var indexOfProjectPath = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase);
            if (indexOfProjectPath == 0)
            {
                relativePath = path.Substring(parentPath.Length + 1, path.Length - (parentPath.Length + 1));
                return true;
            }

            return false;
        }

        internal static string GetVisualStudioRegistryPath()
        {
            var localRegistry = Services.ServiceProvider.GetService(typeof(ILocalRegistry)) as ILocalRegistry3;
            Debug.Assert(localRegistry != null, "ILocalRegistry3 could not be obtained");
            var root = String.Empty;
            if (localRegistry != null)
            {
                if (!VSErrorHandler.Succeeded(localRegistry.GetLocalRegistryRoot(out root)))
                {
                    Debug.Fail("Could not find the local VS Registry root path");
                }
            }
            return root;
        }

        /// <summary>
        ///     Read the Visual Studio ApplicationID from the registry.  This should return something like 'VisualStudio' for VS or 'VBExpress' for VB Express.
        /// </summary>
        internal static string GetVisualStudioApplicationID()
        {
            var applicationID = String.Empty;
            var root = GetVisualStudioRegistryPath();
            Debug.Assert(!String.IsNullOrEmpty(root), "The registry path could not be found..");
            if (!String.IsNullOrEmpty(root))
            {
                using (var subKey = Registry.LocalMachine.OpenSubKey(root, false))
                {
                    applicationID = subKey.GetValue("ApplicationID") as String;
                    Debug.Assert(applicationID != null, "Couldn't find registry entry for VisualStudio ApplicationID");
                }
            }

            return applicationID;
        }

        /// <summary>
        ///     Read the Visual Studio Install Dir from the registry.  This should return something like C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\
        /// </summary>
        internal static string GetVisualStudioInstallDir()
        {
            var installDir = String.Empty;
            var root = GetVisualStudioRegistryPath();
            Debug.Assert(!String.IsNullOrEmpty(root), "The registry path could not be found..");
            if (!String.IsNullOrEmpty(root))
            {
                using (var subKey = Registry.LocalMachine.OpenSubKey(root, false))
                {
                    installDir = subKey.GetValue("InstallDir") as String;
                    Debug.Assert(installDir != null, "Couldn't find registry entry for VisualStudio InstallDir");
                }
            }

            Debug.Assert(new DirectoryInfo(installDir).Exists, "Read VS install dir of " + installDir + " but it doesn't exist!");
            return installDir;
        }

        internal static void WriteCheckoutXmlFilesInProject(IDictionary<string, object> filesMap)
        {
            Debug.Assert(
                filesMap.Values.OfType<XmlDocument>().Count() == filesMap.Count, "All objects in filesMap should be of type 'XmlDocument'");

            WriteCheckoutFilesInProject(
                Services.ServiceProvider,
                filesMap,
                fileDataObject => ((XmlDocument)fileDataObject).InnerXml,
                (fileDataObject, filePath) =>
                    {
                        var xmlWriterSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
                        using (var writer = XmlWriter.Create(filePath, xmlWriterSettings))
                        {
                            ((XmlDocument)fileDataObject).Save(writer);
                        }
                    });
        }

        internal static void WriteCheckoutTextFilesInProject(IDictionary<string, object> filesMap)
        {
            Debug.Assert(filesMap.Values.OfType<string>().Count() == filesMap.Count, "All objects in filesMap should be of type 'string'");
            WriteCheckoutFilesInProject(
                Services.ServiceProvider, filesMap, fileDataObject => fileDataObject as string,
                (fileDataObject, filePath) => File.WriteAllText(filePath, fileDataObject as string));
        }

        internal delegate string GetObjectDataDelegate(object fileDataObject);

        internal delegate void PersistObjectDataDelegate(object fileDataObject, string filePath);

        /// <summary>
        ///     Update the content of files in the project.
        ///     The method loops through the file names collection. For each file, it will:
        ///     - Check whether a doc data exist for the document
        ///     - If yes, write the content to the doc data and continue.
        ///     - If No, check whether the document is under source control and is not checked out.
        ///     - If the document is not under the source control, directly write to the file, save, and continue.
        ///     - If the document is under source control and not checked out, add the file to temporary collection; at the end, the code will check this
        ///     temporary collection and do bulk checkout for the files and update.
        ///     We are currently using this method to write build provider information in web.config as well as to update namespaces in the edmx files.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsPersistDocData2.SetDocDataDirty(System.Int32)")]
        internal static void WriteCheckoutFilesInProject(
            IServiceProvider serviceProvider,
            IDictionary<string, object> filesMap,
            GetObjectDataDelegate getObjectDataCallback,
            PersistObjectDataDelegate persistObjectDataCallback)
        {
            var filesToQueryEdit = new Dictionary<string, object>();
            var filesToQuerySave = new Dictionary<string, object>();
            foreach (var filePath in filesMap.Keys)
            {
                var fileDataObject = filesMap[filePath];

                var docCookie = VSConstants.VSCOOKIE_NIL;
                var docData = VSHelpers.GetDocData(serviceProvider, filePath, _VSRDTFLAGS.RDT_NoLock, out docCookie);
                if (docData != null)
                {
                    filesToQueryEdit.Add(filePath, fileDataObject);
                    filesToQuerySave.Add(filePath, fileDataObject); // we will also save the files after editing them
                }
                else
                {
                    filesToQuerySave.Add(filePath, fileDataObject);
                }
            }

            // Process all files that need to be edit-queried
            if (!VSHelpers.CheckOutFilesIfEditable(serviceProvider, filesToQueryEdit.Keys.ToArray()))
            {
                return;
            }

            // Process all files that need to be save-queried
            if (!VSHelpers.CheckOutFilesIfSaveable(serviceProvider, filesToQuerySave.Keys.ToArray()))
            {
                return;
            }

            // If we've gotten this far, we are ready to process the files. Start with the ones that are open.
            foreach (var filePath in filesToQueryEdit.Keys)
            {
                var fileDataObject = filesMap[filePath];

                // do a double-check here to see if a doc data exists. If there is one, we should update
                // that instead of propagating changes to disk.
                var docCookie = VSConstants.VSCOOKIE_NIL;
                try
                {
                    var docData = VSHelpers.GetDocData(serviceProvider, filePath, _VSRDTFLAGS.RDT_EditLock, out docCookie);
                    Debug.Assert(docData != null, "Should have retrieved the doc data for the open file");
                    if (docData != null)
                    {
                        var textLines = VSHelpers.GetVsTextLinesFromDocData(docData);
                        Debug.Assert(textLines != null, "unable to get IVsTextLines from doc data");
                        if (textLines != null)
                        {
                            SetTextForVsTextLines(textLines, getObjectDataCallback(fileDataObject));
                            var persistDocData = docData as IVsPersistDocData2;
                            if (persistDocData != null)
                            {
                                persistDocData.SetDocDataDirty(1);
                                persistObjectDataCallback(fileDataObject, filePath);
                            }
                        }
                    }
                }
                finally
                {
                    if (docCookie != VSConstants.VSCOOKIE_NIL)
                    {
                        UnlockRunningDocument(Services.ServiceProvider, docCookie, _VSRDTFLAGS.RDT_EditLock);
                    }
                }
            }

            // Process all files that are not open
            foreach (var filePath in filesToQuerySave.Keys)
            {
                var fileDataObject = filesMap[filePath];
                persistObjectDataCallback(fileDataObject, filePath);
            }
        }

        /// <summary>
        ///     Check if the project item is a linked item (the item was added as a link file).
        /// </summary>
        internal static bool IsLinkProjectItem(ProjectItem projectItem)
        {
            // Check whether project item is a  linked item
            var isLinkItem = false;

            // immediately state false if this is a website project since websites don't
            // support this. The DTE calls after this will throw COM Exceptions if we continue.
            // As of build 20815.00
            if (!IsWebSiteProject(projectItem.ContainingProject))
            {
                try
                {
                    var prop = projectItem.Properties.Item("IsLink");
                    if (prop != null)
                    {
                        try
                        {
                            isLinkItem = bool.Parse(prop.Value.ToString());
                        }
                        catch (FormatException)
                        {
                            isLinkItem = false;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // Do nothing here.
                    // For WebSite project, if "IsLink" property is not set, ArgumentException is thrown.
                }
            }
            return isLinkItem;
        }

        internal static ProjectItem GetGeneratedCodeProjectItem(ProjectItem sourceProjectItem)
        {
            if (sourceProjectItem != null
                && sourceProjectItem.ProjectItems != null
                && sourceProjectItem.ProjectItems.OfType<ProjectItem>() != null
                && sourceProjectItem.ProjectItems.OfType<ProjectItem>().FirstOrDefault() != null)
            {
                return sourceProjectItem.ProjectItems.OfType<ProjectItem>().FirstOrDefault(
                    p =>
                        {
                            var extension = Path.GetExtension(p.Name);
                            return FileExtensions.CsExt.Equals(extension, StringComparison.OrdinalIgnoreCase)
                                   || FileExtensions.VbExt.Equals(extension, StringComparison.OrdinalIgnoreCase);
                        });
            }

            // For website projects projectItem.ProjectItems does not return the code-generated dependent files
            // so need to look for them by hand
            if (IsWebSiteProject(sourceProjectItem.ContainingProject))
            {
                var sourceFileName = sourceProjectItem.get_FileNames(1);
                if (".tt".Equals(Path.GetExtension(sourceFileName), StringComparison.OrdinalIgnoreCase))
                {
                    switch (GetLanguageForProject(sourceProjectItem.ContainingProject))
                    {
                        case LangEnum.CSharp:
                            return GetProjectItemForDocument(
                                Path.ChangeExtension(sourceFileName, FileExtensions.CsExt), Services.ServiceProvider);

                        case LangEnum.VisualBasic:
                            return GetProjectItemForDocument(
                                Path.ChangeExtension(sourceFileName, FileExtensions.VbExt), Services.ServiceProvider);
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Attempts to run the custom tool for the project item
        ///     Should work with both client and web project items.
        ///     This will attempt to checkout the file, or not do anything
        ///     if the file is not editable
        /// </summary>
        internal static void RunCustomTool(ProjectItem projectItem)
        {
            var canEdit = false;
            var generatedCodeItem = GetGeneratedCodeProjectItem(projectItem);

            // If we don't have a generated code item then it is about to be created.
            // Need to ensure the project is editable so that we can add the generated code file.
            if (generatedCodeItem == null)
            {
                if (IsWebSiteProject(projectItem.ContainingProject))
                {
                    // no project file to check out for a Website project
                    canEdit = true;
                }
                else
                {
                    bool projectHasFilename;
                    var projectFullPath = GetProjectPathWithName(projectItem.ContainingProject, out projectHasFilename);
                    if (projectHasFilename)
                    {
                        canEdit = VSHelpers.CheckOutFilesIfEditable(Services.ServiceProvider, new[] { projectFullPath });
                    }
                }
            }
            else
            {
                canEdit = VSHelpers.CheckOutFilesIfEditable(Services.ServiceProvider, new[] { generatedCodeItem.get_FileNames(1) });
            }

            if (canEdit)
            {
                var piObj = GetProjectItemObject(projectItem);
                var vsProjectItem = piObj as VSProjectItem;
                var vsWebProjectItem2 = piObj as VSWebProjectItem2;
                if (vsProjectItem != null)
                {
                    vsProjectItem.RunCustomTool();
                }
                else if (vsWebProjectItem2 != null)
                {
                    vsWebProjectItem2.RunCustomTool();
                }
            }
        }

        /// <summary>
        ///     Gets the COM object that represents the project item
        /// </summary>
        private static object GetProjectItemObject(ProjectItem projectItem)
        {
            var projectItemObj = projectItem.Object;
            if (((IsWebSiteProject(projectItem.ContainingProject) && (projectItemObj is VSWebProjectItem)))
                || (projectItemObj is VSProjectItem))
            {
                return projectItemObj;
            }
            Debug.Fail("ProjectItem.Object does not implement VSWebProjectItem or VSProjectItem.");
            return null;
        }

        internal static string ConstructTargetPathForDatabaseFile(Project project, ProjectItems targetCollection, string fileName)
        {
            // First construct target path where the file is proposed to be copied to
            string targetPath = null;
            var projectItems = targetCollection;
            if (projectItems.Kind == Constants.vsProjectItemKindPhysicalFolder)
            {
                while (!(projectItems.Parent is Project))
                {
                    if (null == targetPath)
                    {
                        targetPath = ((ProjectItem)projectItems.Parent).Name;
                    }
                    else
                    {
                        targetPath = ((ProjectItem)projectItems.Parent).Name + "\\" + targetPath;
                    }

                    projectItems = ((ProjectItem)projectItems.Parent).Collection;
                }
            }

            var projectDir = GetProjectRoot(project, Services.ServiceProvider);
            if (null != projectDir)
            {
                if (null == targetPath)
                {
                    // The target path is the project root.
                    targetPath = projectDir.FullName;
                }
                else
                {
                    try
                    {
                        targetPath = Path.Combine(projectDir.FullName, targetPath);
                    }
                    catch (ArgumentException)
                    {
                        // do nothing - path contains invalid chars or one of the args is null
                    }
                }
            }

            if (targetPath == null)
            {
                Debug.Fail(
                    "Could not get the path to copy the local data file to. Unable to find project root for project " + project.UniqueName);
                return null;
            }

            // Now concatenate on to the targetPath the name of the actual file
            targetPath = Path.Combine(targetPath, fileName);
            return targetPath;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static ProjectItem BringDatabaseFileIntoProject(
            IServiceProvider serviceProvider, Project project, ProjectItems targetCollection, string filePath)
        {
            // Construct target path where the file is proposed to be copied to.
            var fileName = Path.GetFileName(filePath);
            var targetPath = ConstructTargetPathForDatabaseFile(project, targetCollection, fileName);

            // Check if the file is in the project
            ProjectItem existingMDFItem = null;
            try
            {
                existingMDFItem = targetCollection.Item(fileName);
            }
            catch
            {
            }

            var useExistingFile = false;

            // If data file in project or on disk, delete it.
            if (null != existingMDFItem
                || File.Exists(targetPath))
            {
                // There is already a file at the proposed location, ask the user if they'd like to delete it
                var result = ShowMessageBox(
                    serviceProvider,
                    string.Format(CultureInfo.CurrentCulture, Resources.LocalDatabaseFileAlreadyExists, Path.GetFileName(filePath)),
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY);
                if (DialogResult.No == result)
                {
                    if (existingMDFItem != null)
                    {
                        // Nothing to do, the file is already there.
                        return existingMDFItem;
                    }
                    else
                    {
                        // File is there but not in the project.
                        useExistingFile = true;
                    }
                }
            }

            if (!useExistingFile)
            {
                if (existingMDFItem != null)
                {
                    // This can throw for a number of reasons including the user canceling SCC checkout.
                    // Let the exception bubble up.
                    existingMDFItem.Delete();
                }

                // VSW 599624 - If the filePath == targetPath, skip the delete.
                filePath = Path.GetFullPath(filePath);
                targetPath = Path.GetFullPath(targetPath);
                if (0 != string.Compare(filePath, targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                }

                // If SSE, also delete the log file
                if (LocalDataUtil.IsSqlExpressDataFile(Path.GetExtension(targetPath)))
                {
                    var logFilePath = LocalDataUtil.GetSqlExpressLogFilePath(targetPath);
                    var logFileName = Path.GetFileName(logFilePath);

                    ProjectItem existingLDFItem = null;
                    try
                    {
                        existingLDFItem = targetCollection.Item(logFileName);
                    }
                    catch (ArgumentException)
                    {
                        // do nothing - cannot find item
                    }
                    catch (Exception)
                    {
                    }

                    if (existingLDFItem != null)
                    {
                        existingLDFItem.Delete();
                    }

                    if (string.Compare(filePath, targetPath, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (File.Exists(logFilePath))
                        {
                            File.Delete(logFilePath);
                        }
                    }
                }
            }

            // suppress the DataSet creation wizard which would otherwise
            // be automatically raised when the file is added to the project
            SetDataSourceWizardSuppressed(serviceProvider);

            // add the file to the project
            ProjectItem dbProjectItem = null;
            if (useExistingFile)
            {
                // User wants to use a file which exists but is not in the project 
                // so just add it to the project.
                dbProjectItem = targetCollection.AddFromFile(targetPath);
            }
            else
            {
                dbProjectItem = AddChildFile(targetCollection, filePath);
            }

            return dbProjectItem;
        }

        /// <summary>
        ///     Create the file if it does not exist
        ///     Add the file to the project as a dependent file of the project item.
        /// </summary>
        internal static ProjectItem AddChildFile(ProjectItems projItems, string childFilePath)
        {
            if (projItems == null)
            {
                // Misc-Files project does not support ProjectItems extensibility
                // and so in this case, there's nothing we can really do but
                // bail and return.
                return null;
            }

            ProjectItem childProjItem = null;

            if (!File.Exists(childFilePath))
            {
                CreateEmptyFile(childFilePath, Encoding.UTF8);
            }

            try
            {
                childProjItem = projItems.Item(Path.GetFileName(childFilePath));
            }
            catch (ArgumentException)
            {
                // do nothing - cannot find child item which is dealt with below
            }

            // if it's not in the project yet, then add it as a dependent
            // file to our schema projectItems collection
            if (null == childProjItem)
            {
                childProjItem = projItems.AddFromFileCopy(childFilePath);
                Debug.Assert(null != childProjItem, "ProjectItems.AddFromFileCopy returned null for child file path " + childFilePath);
            }

            return childProjItem;
        }

        internal static void CreateEmptyFile(string filePath, Encoding encoding)
        {
            try
            {
                using (var outWriter = new StreamWriter(filePath, false, encoding))
                {
                    outWriter.Flush();
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
        }

        /// <summary>
        ///     Ensure the UI context DataSourceWizardSuppressed is set.
        ///     Ordinarily, when a database file is added to a project, the Data Source
        ///     Wizard will be initiated offering to create a DataSet from the file.
        ///     Since we are already creating a .EDMX file based on the database file
        ///     we need to suppress this wizard.
        ///     Note: when the wizard checks whether it is suppressed it will reset this
        ///     context back to "unsuppressed" - so this action needs to be repeated
        ///     every time we copy in a file.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.SetCmdUIContext(System.UInt32,System.Int32)")]
        internal static void SetDataSourceWizardSuppressed(IServiceProvider serviceProvider)
        {
            Debug.Assert(null != serviceProvider, "Invalid null serviceProvider specified.");

            if (null != serviceProvider)
            {
                var monitorSelection = serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                if (monitorSelection != null)
                {
                    var guidCmdUI = VSConstants.UICONTEXT.DataSourceWizardSuppressed_guid;
                    uint dwCmdUICookie;
                    int fActive;
                    if (NativeMethods.Succeeded(monitorSelection.GetCmdUIContextCookie(ref guidCmdUI, out dwCmdUICookie))
                        && NativeMethods.Succeeded(monitorSelection.IsCmdUIContextActive(dwCmdUICookie, out fActive)))
                    {
                        var dataSetWizardIsSuppressed = (fActive != 0);
                        // If the context is not already active, activate it.
                        if (!dataSetWizardIsSuppressed)
                        {
                            var hr = monitorSelection.SetCmdUIContext(dwCmdUICookie, 1);
                            Debug.Assert(
                                NativeMethods.Succeeded(hr), "Cannot set DataSourceWizardSuppressed in the UI context. Return code is " + hr);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Helper method to construct an error message containing the messages of inner
        ///     exceptions if present
        /// </summary>
        /// <returns>the error message</returns>
        internal static string ConstructInnerExceptionErrorMessage(Exception e)
        {
            if (e == null)
            {
                Debug.Fail("Null exception in ConstructInnerExceptionErrorMessage()");
                return null;
            }

            // construct the error message including inner exceptions
            var sbErrMsg = new StringBuilder();
            sbErrMsg.Append(e.Message);
            var innerException = e.InnerException;
            while (null != innerException)
            {
                sbErrMsg.Append(Environment.NewLine);
                sbErrMsg.Append(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.VSUtils_InnerExceptionErrorFormat, innerException.GetType().FullName,
                        innerException.Message));
                innerException = innerException.InnerException;
            }

            return sbErrMsg.ToString();
        }

        /// <summary>
        ///     This will close the DbConnection (or SQLConnection or EntityConnection) safely, Trace and Debug if it can't.
        ///     It will also call SqlConnection.ClearAllPools to release any read lock if the connection is pointing to a database file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void SafeCloseDbConnection(
            DbConnection dbConnection, string designTimeProviderName, string designTimeConnectionString)
        {
            try
            {
                if (null != dbConnection)
                {
                    dbConnection.Close();
                }
            }
            catch
            {
            }

            SafeCloseDbConnectionOnFile(designTimeProviderName, designTimeConnectionString);
        }

        /// <summary>
        ///     Checks to see if the connection string is pointing to a database file and if so, calls SqlConnection.ClearAllPools()
        ///     to release any read lock on the database file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void SafeCloseDbConnectionOnFile(string designTimeProviderName, string designTimeConnectionString)
        {
            try
            {
                // Even though this is for the general case, let's cleanup SqlConnection appropriately.
                if (LocalDataUtil.IsLocalDbFileConnectionString(designTimeProviderName, designTimeConnectionString))
                {
                    SqlConnection.ClearAllPools();
                }
            }
            catch
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static IEnumerable<KeyValuePair<string, Version>> GetProjectReferenceAssemblyNames(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            var vsProject = project.Object as VSProject2;
            if (vsProject != null)
            {
                return from r in vsProject.References.Cast<Reference>()
                       select new KeyValuePair<string, Version>(r.Name, new Version(r.Version));
            }

            var vsWebSite = project.Object as VSWebSite;
            if (vsWebSite != null)
            {
                var references = new List<KeyValuePair<string, Version>>();
                foreach (var reference in vsWebSite.References.Cast<AssemblyReference>())
                {
                    try
                    {
                        var assemblyName = new AssemblyName(reference.StrongName);
                        references.Add(
                            new KeyValuePair<string, Version>(
                                assemblyName.Name, assemblyName.Version));
                    }
                    catch
                    {
                        // do nothing - creating a new AssemblyName can throw
                        // but we want to just ignore that reference
                    }
                }

                return references;
            }

            Debug.Assert(IsMiscellaneousProject(project), "Project type is not recognized.");

            return Enumerable.Empty<KeyValuePair<string, Version>>();
        }

        public static void AddProjectReference(Project project, string assemblyName)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!String.IsNullOrEmpty(assemblyName), "assemblyName is null or empty.");

            var vsProject = project.Object as VSProject2;
            var vsWebSite = project.Object as VSWebSite;

            if (vsProject != null)
            {
                vsProject.References.Add(assemblyName);
            }
            else if (vsWebSite != null)
            {
                vsWebSite.References.AddFromGAC(assemblyName);
            }
            else
            {
                Debug.Fail("Unknown project type.");
            }
        }

        /// <summary>
        ///     Returns the version of EntityFramework.dll or System.Data.Entity.dll referenced by a project or null if none.
        /// </summary>
        /// <remarks>
        ///     Note that:
        ///     - the method checks for references to both EntityFramework.dll and System.Data.Entity.dll
        ///     - the version of EntityFramework.dll wins if references to both EntityFramework.dll and System.Data.Entity.dll are present
        ///     - the assembly versions are not overlapping so it is possible to distinguish the assembly just by the version
        ///     - the version of System.Data.Entity.dll assembly will be 4.0.0.0 for both .NET Framework 4 and .NET Framework 4.5
        ///     but the runtime version should be 5.0.0.0 (EF5) when targeting .NET Framework 4.5 and 4.0.0.0 (EF4) otherwise
        /// </remarks>
        public static Version GetInstalledEntityFrameworkAssemblyVersion(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            // EF and SDE references are ordered by version descending and we select the first one
            return GetInstalledEntityFrameworkAssemblyVersions(project).FirstOrDefault();
        }

        /// <summary>
        ///     This method checks if the passed <paramref name="schemaVersion" /> is the latest
        ///     supported in project (or supported at all).
        /// </summary>
        /// <returns>
        ///     True if the schema version matches the latest schema version supported by model, false otherwise.
        /// </returns>
        /// <remarks>
        ///     We use this method to determine if we can open an edmx file of the given <paramref name="schemaVersion" />.
        ///     In general we want to open only edmx files with the latest version of the schema supported in the project
        ///     with two exceptions:
        ///     - Misc project - any version can be opened
        ///     - A project that targets .NET Framework 4 and contains references to both System.Data.Entity.dll and
        ///     EF6 EntityFramework.dll in which case we allow opening both v2 and v3 edmx files.
        ///     If no EF dll is referenced by a project we use 1:1 mapping between schema version and .NET Framework version
        ///     i.e. v1 - .NET Framework 3.5, v2 - .NET Framework 4, v3 - .NET Framework 4.5 and later
        /// </remarks>
        public static bool SchemaVersionSupportedInProject(Project project, Version schemaVersion, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project != null");
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "invalid schema version");
            Debug.Assert(serviceProvider != null, "serviceProvider != null");

            if (IsMiscellaneousProject(project))
            {
                return true;
            }

            var efAssemblyVersions = GetInstalledEntityFrameworkAssemblyVersions(project).ToArray();
            var targetNetFrameworkVersion =
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(project, serviceProvider);

            if (efAssemblyVersions.Length == 0)
            {
                return schemaVersion == RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(targetNetFrameworkVersion);
            }

            // There may not be any references if this is a Misc project or the user created 
            // a new empty model and no Entity Framework assemblies have been added yet. In both 
            // cases we should allow the model to be opened in the designer.
            return efAssemblyVersions.Any(
                efAssemblyVersion =>
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    schemaVersion, efAssemblyVersion, targetNetFrameworkVersion));
        }

        private static IEnumerable<Version> GetInstalledEntityFrameworkAssemblyVersions(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            // order EF and SDE references by version descending. EntityFramework.dll will always takes precedence
            // since the latest System.Data.Entity.dll version is 4.0.0.0 (note that the version is the same on both 
            // .NET Framework 4 and .NET Framework 4.5) and EntityFramework.dll is always greater than 4.0.0.0
            return GetProjectReferenceAssemblyNames(project)
                .Where(
                    r => string.Equals(r.Key, EntityFrameworkAssemblyName, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(r.Key, SystemDataEntityAssemblyName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Value)
                .Select(r => r.Value);
        }

        public static bool EntityFrameworkSupportedInProject(Project project, IServiceProvider serviceProvider, bool allowMiscProject)
        {
            // we do support opening edmx files when there is no project
            if (IsMiscellaneousProject(project))
            {
                return allowMiscProject;
            }

            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.TargetNetFrameworkVersion(project, serviceProvider);
            return targetNetFrameworkVersion != null &&
                   targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion3_5;
        }

        public static string GetProjectTargetDir(Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!IsMiscellaneousProject(project), "project is misc files project.");

            var outputPath = IsWebSiteProject(project)
                                 ? "Bin" + Path.DirectorySeparatorChar
                                 : GetProjectConfigurationPropertyByName(project, "OutputPath") as string;

            return Path.Combine(GetProjectRoot(project, serviceProvider).FullName, outputPath);
        }

        public static string GetProjectConfigurationFile(Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            var applicationType = GetApplicationType(serviceProvider, project);

            return GetFileName(
                applicationType == VisualStudioProjectSystem.WebApplication
                || applicationType == VisualStudioProjectSystem.Website
                    ? WebConfigFileName
                    : AppConfigFileName,
                project,
                serviceProvider);
        }

        public static string GetProjectDataDirectory(Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            var applicationType = GetApplicationType(serviceProvider, project);

            return applicationType == VisualStudioProjectSystem.WebApplication
                   || applicationType == VisualStudioProjectSystem.Website
                       ? Path.Combine(GetProjectRoot(project, serviceProvider).FullName, LocalDataUtil.DATAFOLDERNAME)
                       : null;
        }

        public static string GetProjectTargetFileName(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            return GetProjectPropertyByName(project, "OutputFileName") as string;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static string GetFileName(string projectItemName, Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(projectItemName), "projectItemName is null or empty.");

            ProjectItem projectItem;
            try
            {
                projectItem = project.ProjectItems.Item(projectItemName);
            }
            catch
            {
                return Path.Combine(GetProjectRoot(project, serviceProvider).FullName, projectItemName);
            }

            Debug.Assert(projectItem.FileCount == 1);

            return projectItem.FileNames[0];
        }

        public static Type GetTypeFromProject(
            string typeName,
            Project project,
            IServiceProvider serviceProvider)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(typeName), "typeName is null or empty.");
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            var dynamicTypeService = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;
            var hierarchy = GetVsHierarchy(project, serviceProvider);
            var typeResolutionService = dynamicTypeService.GetTypeResolutionService(hierarchy);

            return typeResolutionService.GetType(typeName);
        }

        /// <summary>
        ///     Determines whether a modern (or non-legacy) provider is available for the
        ///     specified invariant name
        /// </summary>
        public static bool IsModernProviderAvailable(
            string providerInvariantName,
            Project project,
            IServiceProvider serviceProvider)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(providerInvariantName), "providerInvariantName is null or empty.");

            return GetKnownModernProvider(providerInvariantName) != null
                   || GetModernProviderTypeNameFromProject(providerInvariantName, project, serviceProvider) != null;
        }

        /// <summary>
        ///     Gets the <see cref="DbProviderServices" /> type of the modern (or non-legacy) provider for the
        ///     specified invariant name
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static Type GetModernProvider(string providerInvariantName, Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(providerInvariantName), "providerInvariantName is null or empty.");
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");
            Debug.Assert(
                providerInvariantName != "Microsoft.SqlServerCe.Client.4.0",
                "providerInvariantName is for design-time.");

            var modernProvider = GetKnownModernProvider(providerInvariantName);

            // If unknown, detect EF6 providers installed in the user's project
            if (modernProvider == null)
            {
                try
                {
                    var providerServicesTypeName = GetModernProviderTypeNameFromProject(
                        providerInvariantName,
                        project,
                        serviceProvider);
                    if (providerServicesTypeName != null)
                    {
                        modernProvider = GetTypeFromProject(providerServicesTypeName, project, serviceProvider);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                }
            }

            Debug.Assert(
                modernProvider == null
                || (modernProvider != typeof(DbProviderServices)
                    && typeof(DbProviderServices).IsAssignableFrom(modernProvider)),
                "modernProvider does not a derive from DbProviderServices");

            return modernProvider;
        }

        public static void EnsureProvider(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact should not be null");
            // artifact.StorageModel() can be null in e.g. situations where the EDMX is invalid
            if (artifact.StorageModel() != null)
            {
                var providerInvariantName = artifact.StorageModel().Provider.Value;
                var useLegacyProvider = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                    OptionsDesignerInfo.ElementName,
                    OptionsDesignerInfo.AttributeUseLegacyProvider,
                    OptionsDesignerInfo.UseLegacyProviderDefault,
                    artifact);
                var project = VSHelpers.GetProjectForDocument(artifact.Uri.LocalPath);
                EnsureProvider(providerInvariantName, useLegacyProvider, project, Services.ServiceProvider);
            }
        }

        public static void EnsureProvider(
            string providerInvariantName,
            bool useLegacyProvider,
            Project project,
            IServiceProvider serviceProvider)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(providerInvariantName), "providerInvariantName is null or empty.");
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            Type modernProvider = null;
            if (!useLegacyProvider)
            {
                modernProvider = GetModernProvider(providerInvariantName, project, serviceProvider);
                Debug.Assert(modernProvider != null, "modernProvider is null.");
            }

            DependencyResolver.EnsureProvider(providerInvariantName, modernProvider);
        }

        private static Type GetKnownModernProvider(string providerInvariantName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(providerInvariantName), "providerInvariantName is null or empty.");

            if (providerInvariantName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(SqlProviderServices);
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static string GetModernProviderTypeNameFromProject(
            string invariantName,
            Project project,
            IServiceProvider serviceProvider)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName is null or empty.");
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            var installedEntityFrameworkVersion = GetInstalledEntityFrameworkAssemblyVersion(project);
            if (installedEntityFrameworkVersion == null
                || installedEntityFrameworkVersion < RuntimeVersion.Version6)
            {
                return null;
            }

            string providerServicesTypeName = null;
            try
            {
                using (var context = new ProjectExecutionContext(project, serviceProvider))
                {
                    providerServicesTypeName = context.Executor.GetProviderServices(invariantName);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

            return providerServicesTypeName;
        }
    }
}
