// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VSDesigner.Data.Local;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.DataTools.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Constants = EnvDTE.Constants;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     The Connection Manager allows interaction with App.Config and Web.Config. It stores a "project dictionary" where each bucket corresponds
    ///     to a dictionary that associates entity container names with their corresponding connection strings, stored as
    ///     ConnectionString objects. The dictionaries should mirror App.Config exactly.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class ConnectionManager : IDisposable
    {
        #region Fields

        private Dictionary<Project, Dictionary<string, ConnectionString>> _connStringsByProjectHash;
        private static readonly object _hashSyncRoot = new object();

        private const string Provider = "System.Data.EntityClient";
        private const string AppConfigItemTemplateCs = "AppConfigInternal.zip";
        private const string AppConfigItemTemplateVb = "AppConfigurationInternal.zip";
        private const string WebConfigItemTemplate = "WebConfig.zip";

        internal static readonly string SqlClientProviderName = "System.Data.SqlClient";
        internal static readonly string SqlCe35ConnectionStringProvider = "provider=System.Data.SqlServerCe.3.5";
        internal static readonly string SqlCe40ConnectionStringProvider = "provider=System.Data.SqlServerCe.4.0";
        internal static readonly string PreUpgradeSqlDatabaseFileConnectionStringDataSource = "data source=.\\sqlexpress";

        internal static readonly int PreUpgradeSqlDatabaseFileConnectionStringDataSourceLength =
            PreUpgradeSqlDatabaseFileConnectionStringDataSource.Length;

        internal static readonly string PostUpgradeSqlDatabaseFileConnectionStringDataSource = "Data Source=(LocalDB)\\v11.0";
        internal static readonly string SqlDatabaseFileConnectionStringUserInstance = "user instance=true";

        internal static readonly int SqlDatabaseFileConnectionStringUserInstanceLength =
            SqlDatabaseFileConnectionStringUserInstance.Length;

        internal static readonly string XmlAttrNameConnectionString = "connectionString";
        internal static readonly string XmlAttrNameEntityContainerName = "name";
        internal static readonly string XmlAttrNameProviderName = "providerName";
        internal static readonly string XmlAttrNameMultipleActiveResultSets = "MultipleActiveResultSets";

        private const string XpathConnectionStrings = "configuration/connectionStrings";

        private const string XpathConnectionStringsAddEntity = 
            "configuration/connectionStrings/add[@providerName='" + Provider + "']";

        private const string XpathConnectionStringsAdd = "configuration/connectionStrings/add";

        internal static readonly string EmbedAsResourcePrefix = "res://*";

        private string _staleEntityContainerName;
        private string _staleMetadataArtifactProcessing;

        private const string CsWebApplicationKind = "{349C5853-65DF-11DA-9384-00065B846F21}";
        private const string VbWebApplicationKind = "{349C5854-65DF-11DA-9384-00065B846F21}";

        private const string ProviderConnectionStringPropertyNameApp = "App";
        private const string ProviderConnectionStringPropertyNameApplicationName = "Application Name";

        #endregion

        /// <summary>
        ///     A wrapper around EntityConnectionStringBuilder so we can add our own properties
        ///     or different builders at a later point in time.
        /// </summary>
        internal class ConnectionString
        {
            private EntityConnectionStringBuilder _builder;

            internal string Text
            {
                get { return _builder != null ? _builder.ConnectionString : null; }
                set { _builder.ConnectionString = value; }
            }

            internal string ProviderConnectionStringText
            {
                get { return _builder != null ? _builder.ProviderConnectionString : String.Empty; }
            }

            internal EntityConnectionStringBuilder Builder
            {
                get { return _builder; }
                set { _builder = value; }
            }

            public override int GetHashCode()
            {
                if (_builder != null
                    && _builder.ConnectionString != null)
                {
                    return _builder.ConnectionString.GetHashCode();
                }
                return base.GetHashCode();
            }

            // gets the value of a key-value pair from the provider connection string property of the
            // underlying EntityConnectionStringBuilder. Returns null if not present.
            internal object GetProviderConnectionStringProperty(string keyword)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(keyword), "cannot get null or whitespace keyword");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return null;
                }

                DbConnectionStringBuilder providerConnectionStringBuilder;
                if (TryCreateDbConnectionStringBuilder(_builder.ProviderConnectionString, out providerConnectionStringBuilder))
                {
                    object value;
                    if (providerConnectionStringBuilder.TryGetValue(keyword, out value))
                    {
                        return value;
                    }
                }

                return null;
            }

            // sets the value of a key-value pair from the provider connection string property of the
            // underlying EntityConnectionStringBuilder
            internal void SetProviderConnectionStringProperty(string keyword, object value)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    Debug.Fail("cannot set null or whitespace keyword");
                    return;
                }

                DbConnectionStringBuilder providerConnectionStringBuilder;
                if (TryCreateDbConnectionStringBuilder(_builder.ProviderConnectionString, out providerConnectionStringBuilder))
                {
                    providerConnectionStringBuilder[keyword] = value;
                    _builder.ProviderConnectionString = providerConnectionStringBuilder.ConnectionString;
                }
            }

            public override bool Equals(object obj)
            {
                var connString = obj as ConnectionString;
                if (connString == null)
                {
                    return false;
                }
                return (connString.Text.Equals(Text, StringComparison.CurrentCultureIgnoreCase));
            }

            internal ConnectionString(string connStringText)
            {
                _builder = new EntityConnectionStringBuilder(connStringText);
            }

            internal ConnectionString(EntityConnectionStringBuilder builder)
            {
                _builder = builder;
            }

            internal string DesignTimeProviderInvariantName
            {
                get { return TranslateInvariantName(_builder.Provider, _builder.ProviderConnectionString, false); }
            }

            internal string GetDesignTimeProviderConnectionString(Project project)
            {
                return TranslateConnectionString(project, _builder.Provider, _builder.ProviderConnectionString, false);
            }

            internal Guid Provider
            {
                get { return TranslateProviderGuid(_builder.Provider, _builder.ProviderConnectionString); }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal ConnectionManager()
        {
            lock (_hashSyncRoot)
            {
                try
                {
                    RegisterModelListenerEvents();
                }
                catch (Exception e)
                {
                    var s = Resources.ConnectionManager_InitializeError;
                    s = String.Format(CultureInfo.CurrentCulture, s, e.Message);
                    Project project = null;
                    foreach (var p in VsUtils.GetAllProjectsInSolution(PackageManager.Package))
                    {
                        project = p;
                        break;
                    }

                    Debug.Assert(project != null);
                    if (project != null)
                    {
                        VsUtils.LogOutputWindowPaneMessage(project, s);
                    }
                }
            }
        }

        ~ConnectionManager()
        {
            Dispose(false);
        }

        private void InitializeConnectionStringsHash()
        {
            if (_connStringsByProjectHash == null)
            {
                lock (_hashSyncRoot)
                {
                    try
                    {
                        // since we started listening only after the package loaded, parse the first project's config.
                        _connStringsByProjectHash = new Dictionary<Project, Dictionary<string, ConnectionString>>();

                        // we might have opened up a solution with multiple projects, so iterate through them, building
                        // our dictionary
                        foreach (var eachProject in VsUtils.GetAllProjectsInSolution(PackageManager.Package))
                        {
                            ExtractConnStringsIntoHash(eachProject, false);
                        }
                    }
                    catch (Exception e)
                    {
                        var s = Resources.ConnectionManager_InitializeError;
                        s = String.Format(CultureInfo.CurrentCulture, s, e.Message);
                        Project project = null;
                        foreach (var p in VsUtils.GetAllProjectsInSolution(PackageManager.Package))
                        {
                            project = p;
                            break;
                        }

                        Debug.Assert(project != null);
                        if (project != null)
                        {
                            VsUtils.LogOutputWindowPaneMessage(project, s);
                        }

                        if (CriticalException.IsCriticalException(e))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private Dictionary<Project, Dictionary<string, ConnectionString>> ConnStringsByProjectHash
        {
            get
            {
                InitializeConnectionStringsHash();
                return _connStringsByProjectHash;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            Debug.Assert(disposing, "Connection Manager is finalized before disposing");
            if (disposing)
            {
                UnregisterModelListenerEvents();
                _connStringsByProjectHash = null;
            }
        }

        /// <summary>
        ///     Takes our Xml .config file and destructively updates our local hash.
        /// </summary>
        /// <param name="project">DTE Project that owns App.Config we want to look at.</param>
        internal void ExtractConnStringsIntoHash(Project project, bool createConfig)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            var configFilePath = GetConfigFilePath(project, createConfig);
            if (configFilePath != null)
            {
                var configXmlDoc = LoadConfigFile(configFilePath);

                if (configXmlDoc != null)
                {
                    var xmlNodeList = configXmlDoc.SelectNodes(XpathConnectionStringsAddEntity);

                    var stringHash = new Dictionary<string, ConnectionString>();
                    foreach (XmlNode node in xmlNodeList)
                    {
                        var connStringObj = new ConnectionString(node.Attributes.GetNamedItem(XmlAttrNameConnectionString).Value);
                        stringHash.Add(node.Attributes.GetNamedItem(XmlAttrNameEntityContainerName).Value, connStringObj);
                    }

                    // from msdn: UniqueName: This [property] returns a temporary, unique string value that you can use to 
                    // differentiate one project from another.
                    ConnStringsByProjectHash[project] = stringHash;
                }
            }
        }

        /// <summary>
        ///     Returns an EntityContainer name unique in the app/web.config for the given project
        ///     given a proposed EntityContainer name
        /// </summary>
        internal string ConstructUniqueEntityContainerName(string proposedEntityContainerName, Project project)
        {
            Debug.Assert(null != project, "Null project in GetUniqueEntityContainerNameForProject()");
            Debug.Assert(
                null != proposedEntityContainerName, "Null proposedEntityContainerName in GetUniqueEntityContainerNameForProject()");

            var entityContainerName = proposedEntityContainerName;
            var suffix = 1;

            InitializeConnectionStringsHash();
            Dictionary<string, ConnectionString> connStringsInProject;
            if (null != ConnStringsByProjectHash
                && ConnStringsByProjectHash.TryGetValue(project, out connStringsInProject))
            {
                // keep incrementing the suffix until the existing connection string names 
                // does not contain the result
                while (connStringsInProject.ContainsKey(entityContainerName))
                {
                    entityContainerName = proposedEntityContainerName + suffix;
                    ++suffix;
                }
            }

            return entityContainerName;
        }

        internal static bool TryCreateDbConnectionStringBuilder(string connectionString, out DbConnectionStringBuilder builder)
        {
            var success = false;
            builder = new DbConnectionStringBuilder();
            try
            {
                builder.ConnectionString = connectionString;
                success = true;
            }
            catch (ArgumentException)
            {
            }
            return success;
        }

        /// <summary>
        ///     Takes our local hash and destructively updates the .config file.
        /// </summary>
        /// <param name="project">DTE Project that owns the .config file we want to look at.</param>
        private void InsertConnStringsFromHash(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            InitializeConnectionStringsHash();

            var configFilePath = GetConfigFilePath(project, true);

            var configXmlDoc = LoadConfigFile(configFilePath);
            if (configXmlDoc != null)
            {
                var connStringsNode = FindOrCreateXmlElement(configXmlDoc, XpathConnectionStrings, false);
                if (connStringsNode == null)
                {
                    throw new XmlException(Resources.ConnectionManager_CorruptConfig);
                }

                // delete all previous System.Data.Entity connection strings that are in the hash
                var xmlNodeList = configXmlDoc.SelectNodes(XpathConnectionStringsAddEntity);
                foreach (XmlNode node in xmlNodeList)
                {
                    var prevSibling = node.PreviousSibling;
                    var nextSibling = node.NextSibling;
                    node.ParentNode.RemoveChild(node);
                    if (prevSibling != null
                        && prevSibling.NodeType == XmlNodeType.Whitespace)
                    {
                        prevSibling.ParentNode.RemoveChild(prevSibling);
                    }
                    if (nextSibling != null
                        && nextSibling.NodeType == XmlNodeType.Whitespace)
                    {
                        nextSibling.ParentNode.RemoveChild(nextSibling);
                    }
                }

                Dictionary<string, ConnectionString> hash = null;
                if (!ConnStringsByProjectHash.TryGetValue(project, out hash))
                {
                    var s = String.Format(CultureInfo.CurrentCulture, Resources.ConnectionManager_GetConfigError);
                    VsUtils.LogOutputWindowPaneMessage(project, s);
                }
                var hashEnum = hash.GetEnumerator();

                var needToUpdate = false;

                // iterate through the hash, using the keys/values to populate an XmlElement
                while (hashEnum.MoveNext())
                {
                    var addNode = configXmlDoc.CreateElement("add");

                    addNode.SetAttribute(XmlAttrNameEntityContainerName, hashEnum.Current.Key);
                    addNode.SetAttribute(XmlAttrNameConnectionString, hashEnum.Current.Value.Text);
                    addNode.SetAttribute(XmlAttrNameProviderName, Provider);

                    connStringsNode.AppendChild(addNode);
                    needToUpdate = true;
                }

                if (needToUpdate)
                {
                    try
                    {
                        UpdateConfigFile(configXmlDoc, configFilePath);
                    }
                    catch (Exception e)
                    {
                        var s = Resources.ConnectionManager_SaveXmlError;
                        s = String.Format(CultureInfo.CurrentCulture, s, configFilePath, e.Message);
                        VsUtils.LogOutputWindowPaneMessage(project, s);
                        throw;
                    }
                }
            }
        }

        // Update the .config file if the nodes have a SQL CE 3.5 provider to use 4.0 instead
        internal static bool UpdateSqlCeProviderInConnectionStrings(string configFilePath, out XmlDocument configXmlDoc)
        {
            configXmlDoc = LoadConfigFile(configFilePath);
            var docUpdated = false;
            if (configXmlDoc != null)
            {
                // update all nodes that have SQL CE 3.5 provider
                var xmlNodeList = configXmlDoc.SelectNodes(XpathConnectionStringsAdd);
                foreach (XmlNode node in xmlNodeList)
                {
                    var e = node as XmlElement;
                    if (e != null)
                    {
                        var connectionString = e.GetAttribute(XmlAttrNameConnectionString);
                        if (null != connectionString
                            && connectionString.Contains(SqlCe35ConnectionStringProvider))
                        {
                            var newConnString = connectionString.Replace(SqlCe35ConnectionStringProvider, SqlCe40ConnectionStringProvider);
                            e.SetAttribute(XmlAttrNameConnectionString, newConnString);
                            docUpdated = true;
                        }
                    }
                }
            }

            return docUpdated;
        }

        // Update the .config file if the nodes have an old style SQL Database File Data Source to have the new style instead
        internal static bool UpdateSqlDatabaseFileDataSourceInConnectionStrings(string configFilePath, out XmlDocument configXmlDoc)
        {
            configXmlDoc = LoadConfigFile(configFilePath);
            var docUpdated = false;
            if (configXmlDoc != null)
            {
                // update SQL Database File connections
                var xmlNodeList = configXmlDoc.SelectNodes(XpathConnectionStringsAdd);
                foreach (XmlNode node in xmlNodeList)
                {
                    var e = node as XmlElement;
                    if (e != null)
                    {
                        var connectionString = e.GetAttribute(XmlAttrNameConnectionString);
                        if (null != connectionString)
                        {
                            // a SQL Database File connection must contain AttachDbFileName
                            var offset = connectionString.IndexOf(
                                LocalDataUtil.CONNECTION_PROPERTY_ATTACH_DB_FILENAME, StringComparison.OrdinalIgnoreCase);
                            if (offset > -1)
                            {
                                var connStringUpdated = false;

                                // check whether connection string contains "Data Source=.\SQLEXPRESS" (case-insensitive)
                                // if so replace with LocalDB Data Source
                                offset = connectionString.IndexOf(
                                    PreUpgradeSqlDatabaseFileConnectionStringDataSource, StringComparison.OrdinalIgnoreCase);
                                if (offset > -1)
                                {
                                    connectionString = connectionString.Substring(0, offset) +
                                                       PostUpgradeSqlDatabaseFileConnectionStringDataSource +
                                                       connectionString.Substring(
                                                           offset + PreUpgradeSqlDatabaseFileConnectionStringDataSourceLength);
                                    connStringUpdated = true;
                                }

                                // check whether connection string contains "User Instance=True" (case-insensitive)
                                // if so remove
                                offset = connectionString.IndexOf(
                                    SqlDatabaseFileConnectionStringUserInstance, StringComparison.OrdinalIgnoreCase);
                                if (offset > -1)
                                {
                                    // if User Instance=True was followed by a semi-colon then remove that too
                                    var afterUserInstance =
                                        connectionString.Substring(offset + SqlDatabaseFileConnectionStringUserInstanceLength);
                                    if (afterUserInstance.StartsWith(";", StringComparison.Ordinal))
                                    {
                                        afterUserInstance = afterUserInstance.Substring(1);
                                    }
                                    connectionString = connectionString.Substring(0, offset) + afterUserInstance;
                                    connStringUpdated = true;
                                }

                                // update XML document
                                if (connStringUpdated)
                                {
                                    e.SetAttribute(XmlAttrNameConnectionString, connectionString);
                                    docUpdated = true;
                                }
                            }
                        }
                    }
                }
            }

            return docUpdated;
        }

        /// <summary>
        ///     Subscribe to VS events via ModelChangeEventListener's delegates
        /// </summary>
        private void RegisterModelListenerEvents()
        {
            var listener = PackageManager.Package.ModelChangeEventListener;
            if (listener != null)
            {
                listener.AfterOpenProject += OnAfterOpenProject;
                listener.AfterAddFile += OnAfterOpenProject;
                listener.AfterRemoveFile += OnAfterRemoveFile;
                listener.AfterRenameFile += OnAfterRenameFile;
                listener.AfterEntityContainerNameChange += OnAfterEntityContainerNameChange;
                listener.AfterMetadataArtifactProcessingChange += OnAfterMetadataArtifactProcessingChange;
                listener.AfterSaveFile += OnAfterSaveFile;
            }
        }

        /// <summary>
        ///     Unsubscribe to VS events via ModelChangeEventListener's delegates
        /// </summary>
        private void UnregisterModelListenerEvents()
        {
            var listener = PackageManager.Package.ModelChangeEventListener;
            if (listener != null)
            {
                listener.AfterOpenProject -= OnAfterOpenProject;
                listener.AfterAddFile -= OnAfterOpenProject;
                listener.AfterRemoveFile -= OnAfterRemoveFile;
                listener.AfterRenameFile -= OnAfterRenameFile;
                listener.AfterEntityContainerNameChange -= OnAfterEntityContainerNameChange;
                listener.AfterMetadataArtifactProcessingChange -= OnAfterMetadataArtifactProcessingChange;
                listener.AfterSaveFile -= OnAfterSaveFile;
            }
        }

        #region Methods for resolving config file

        /// <summary>
        ///     Determine the .config file path by finding or creating the file
        /// </summary>
        /// <param name="project">DTE Project that owns the config file</param>
        /// <returns>string that represents the config's file path.</returns>
        internal static string GetConfigFilePath(Project project, bool createConfig)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            ProjectItem projectItemConfig = null;

            var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, project);
            try
            {
                if (applicationType == VisualStudioProjectSystem.WebApplication
                    ||
                    applicationType == VisualStudioProjectSystem.Website)
                {
                    projectItemConfig = FindOrCreateWebConfig(project, createConfig);
                }
                else
                {
                    projectItemConfig = FindOrCreateAppConfig(project, createConfig);
                }
            }
            catch (NotSupportedException)
            {
                return null;
            }

            if (projectItemConfig == null)
            {
                return null;
            }

            return projectItemConfig.get_FileNames(1);
        }

        internal static ProjectItem FindOrCreateAppConfig(Project project)
        {
            return FindOrCreateAppConfig(project, true);
        }

        /// <summary>
        ///     Finds or creates a .config file for a windows or web application
        /// </summary>
        /// <param name="project">DTE Project that owns the App.Config we want to find/create.</param>
        /// <returns>DTE ProjectItem that represents the config file.</returns>
        private static ProjectItem FindOrCreateAppConfig(Project project, bool createConfig)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            // Get the right item template name for the .config based on the project type (C# or VB)
            var itemTemplateName = string.Empty;

            var langEnum = VsUtils.GetLanguageForProject(project);

            if (langEnum == EFArtifact.LangEnum.CSharp)
            {
                itemTemplateName = AppConfigItemTemplateCs;
            }
            else if (langEnum == EFArtifact.LangEnum.VisualBasic)
            {
                itemTemplateName = AppConfigItemTemplateVb;
            }
            else
            {
                throw new NotSupportedException(Resources.UnsupportedProjectLanguage);
            }

            return FindOrCreateConfig(project, VsUtils.AppConfigFileName, itemTemplateName, createConfig);
        }

        private static ProjectItem FindOrCreateWebConfig(Project project, bool createConfig)
        {
            return FindOrCreateConfig(project, VsUtils.WebConfigFileName, WebConfigItemTemplate, createConfig);
        }

        /// <summary>
        ///     Finds or creates an Web.Config file in the project based on the built-in VS templates.
        /// </summary>
        /// <param name="project">DTE Project that owns the Web.Config we're interested in.</param>
        /// <returns>DTE ProjectItem that represents the config file.</returns>
        internal static ProjectItem FindOrCreateWebConfig(Project project)
        {
            return FindOrCreateConfig(project, VsUtils.WebConfigFileName, WebConfigItemTemplate, true);
        }

        /// <summary>
        ///     Command implementation of finding or creating a .config file for VS projects
        /// </summary>
        /// <param name="project">DTE project that owns the .config file</param>
        /// <param name="configFileName"></param>
        /// <param name="configItemTemplate"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        private static ProjectItem FindOrCreateConfig(
            Project project,
            string configFileName,
            string configItemTemplate,
            bool createConfig)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            var projectItemConfig = VsUtils.FindFirstProjectItemWithName(project.ProjectItems, configFileName);
            var projectDirectoryInfo = VsUtils.GetProjectRoot(project, Services.ServiceProvider);
            var configFileInfo = new FileInfo(projectDirectoryInfo.FullName + "\\" + configFileName);

            if ((null == projectItemConfig || !configFileInfo.Exists) && createConfig)
            {
                // first we'll try to add .config file if it already exists in the project directory but is not included in the project
                if (configFileInfo.Exists)
                {
                    project.ProjectItems.AddFromFile(configFileInfo.FullName);
                }
                else
                {
                    // Project doesn't have this .config so create it & add it to the project
                    var solution2 = project.DTE.Solution as Solution2;
                    if (null != solution2)
                    {
                        // get the path to the standard VS template
                        string itemTemplatePath = null;
                        if (VsUtils.GetApplicationType(Services.ServiceProvider, project) == VisualStudioProjectSystem.WebApplication)
                        {
                            // in this case project.Kind does not indicate the language, so use fixed Guids instead
                            var projectLanguage = VsUtils.GetLanguageForProject(project);
                            if (projectLanguage == EFArtifact.LangEnum.CSharp)
                            {
                                itemTemplatePath = solution2.GetProjectItemTemplate(configItemTemplate, CsWebApplicationKind);
                            }
                            else if (projectLanguage == EFArtifact.LangEnum.VisualBasic)
                            {
                                itemTemplatePath = solution2.GetProjectItemTemplate(configItemTemplate, VbWebApplicationKind);
                            }
                            else
                            {
                                // could not find project language
                                throw new NotSupportedException(Resources.UnsupportedProjectLanguage);
                            }
                        }
                        else
                        {
                            itemTemplatePath = solution2.GetProjectItemTemplate(configItemTemplate, project.Kind);
                        }

                        Debug.Assert(itemTemplatePath != null, "Config template path is null");

                        // create it
                        project.ProjectItems.AddFromTemplate(itemTemplatePath, configFileName); // always returns null

                        // projects from previous VS versions might contain .exe.config (or .dll.config) file
                        // if so, we should warn the user
                        try
                        {
                            var outputFileName = (string)project.Properties.Item("OutputFileName").Value;
                            var fi = new FileInfo(projectDirectoryInfo.FullName + "\\" + outputFileName + ".config");
                            if (fi.Exists)
                            {
                                VsUtils.LogStandardWarning(String.Format(
                                    CultureInfo.CurrentCulture, Resources.ExistingConfigurationFileWarning, fi.Name, configFileName),
                                    fi.FullName, 0, 0);
                            }
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }

                // now go look for it - we should find it now that we've created it
                projectItemConfig = VsUtils.FindFirstProjectItemWithName(project.ProjectItems, configFileName);
            }

            return projectItemConfig;
        }

        internal static XmlDocument LoadConfigFile(string configFilePath)
        {
            // attempt to construct the config xml from the doc data if it is available
            try
            {
                var textLines = VSHelpers.GetDocData(Services.ServiceProvider, configFilePath) as IVsTextLines;
                return textLines != null
                           ? EdmUtils.SafeLoadXmlFromString(
                               VSHelpers.GetTextFromVsTextLines(textLines), preserveWhitespace: true)
                           : EdmUtils.SafeLoadXmlFromPath(configFilePath);
            }
            catch (XmlException e)
            {
                VsUtils.LogStandardError(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.VSUtils_ExceptionParsingXml, configFilePath, e.Message),
                    configFilePath,
                    e.LineNumber,
                    e.LinePosition);

                throw;
            }
        }

        internal static void UpdateConfigFile(XmlDocument configXmlDoc, string configFilePath)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add(configFilePath, configXmlDoc);
            VsUtils.WriteCheckoutXmlFilesInProject(map);
        }

        #endregion

        internal static DesignerProperty GetMetadataPropertyFromArtifact(EFArtifact artifact)
        {
            var designerRoot = artifact.DesignerInfo();
            DesignerProperty mapProperty = null;
            if (designerRoot != null)
            {
                DesignerInfo designerInfo;
                if (designerRoot.TryGetDesignerInfo(ConnectionDesignerInfo.ElementName, out designerInfo))
                {
                    var connectionDesignerInfo = designerInfo as ConnectionDesignerInfo;
                    Debug.Assert(
                        connectionDesignerInfo != null,
                        "We should have associated the ConnectionDesignerInfo with " + ConnectionDesignerInfo.ElementName);

                    if (connectionDesignerInfo != null)
                    {
                        mapProperty = connectionDesignerInfo.MetadataArtifactProcessingProperty;
                    }
                }
            }
            return mapProperty;
        }

        internal static string[] GetMetadataFileNamesFromArtifactFileName(
            Project project, string filename, IServiceProvider serviceProvider)
        {
            return GetMetadataFileNamesFromArtifactFileName(project, filename, serviceProvider, VsUtils.GetProjectItemForDocument);
        }

        internal static string[] GetMetadataFileNamesFromArtifactFileName(
            Project project, string filename, IServiceProvider serviceProvider,
            Func<string, IServiceProvider, ProjectItem> getProjectItemForDocument)
        {
            var unescapedArtifactPath = Uri.UnescapeDataString(filename);
            var edmxFileInfo = new FileInfo(unescapedArtifactPath);
            var modelName = Path.GetFileNameWithoutExtension(edmxFileInfo.FullName);
            var projectRootDirInfo = VsUtils.GetProjectRoot(project, serviceProvider);
            string relativeFolderPath;

            var projectItem = getProjectItemForDocument(edmxFileInfo.FullName, serviceProvider);
            // when generating model from the database project the item will be null 
            // since the actual model is generated in the very last step
            if (projectItem != null)
            {
                // since the given file can be a link, create the directory path by combining parent directories names
                // folowing code will create correct relativeFolderPath regardles whether projectItem is a link or not
                // Example - for following project hierarchy:
                // ProjectRoot
                // |__Folder1
                //    |__Folder2
                //       |__Model.edmx
                // code below will produce "Folder2" path in the first step and "Folder1\Folder2" path in the second (and last) step
                relativeFolderPath = "";
                var parentItem = projectItem.Collection.Parent as ProjectItem;
                while (parentItem != null)
                {
                    relativeFolderPath = Path.Combine(parentItem.Name, relativeFolderPath);
                    parentItem = parentItem.Collection.Parent as ProjectItem;
                }
            }
            else
            {
                relativeFolderPath = EdmUtils.GetRelativePath(edmxFileInfo.Directory, projectRootDirInfo);
            }

            var folderPath = Path.Combine(projectRootDirInfo.FullName, relativeFolderPath);
            return EdmUtils.GetRelativeMetadataPaths(folderPath, project, modelName, EdmUtils.CsdlSsdlMslExtensions, serviceProvider);
        }

        /// <summary>
        ///     Helper function to construct the metadata, depending on what type of application and output path
        /// </summary>
        private static string GetConnectionStringMetadata(
            IEnumerable<string> metadataFiles, string outputPath, Project project, string metadataProcessingType)
        {
            // fix up outputPath 
            if (null == outputPath)
            {
                outputPath = String.Empty;
            }
            else if (!outputPath.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                outputPath += "\\";
            }
            outputPath = outputPath.Replace("\\", "/");

            var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, project);

            // construct metadata portion of connection string
            if (metadataFiles == null
                || !metadataFiles.Any())
            {
                if (metadataProcessingType == ConnectionDesignerInfo.MAP_EmbedInOutputAssembly
                    || applicationType == VisualStudioProjectSystem.Website)
                {
                    return EmbedAsResourcePrefix;
                }
                if (applicationType == VisualStudioProjectSystem.WebApplication)
                {
                    // web-app's need to have the outputPath (usually "bin") appended
                    return "~/" + outputPath;
                }
                else
                {
                    return ".";
                }
            }
            else
            {
                var md = new StringBuilder();
                var i = 0;
                var metadataFileCount = metadataFiles.Count();
                foreach (var f in metadataFiles)
                {
                    // if this is a web app, then change relative path to virtual path
                    if (applicationType == VisualStudioProjectSystem.WebApplication
                        && metadataProcessingType == ConnectionDesignerInfo.MAP_CopyToOutputDirectory)
                    {
                        md.Append(f.Replace(".\\", "~/" + outputPath));
                    }
                    else if (applicationType == VisualStudioProjectSystem.Website)
                    {
                        //
                        // The 3.5 runtime's Build Provider will include the web site's virtual root in the resource name.  This can change when
                        // a web app is deployed, so we must use res:\\* for the connection string when targeting netfx 3.5.
                        //
                        if (NetFrameworkVersioningHelper.TargetNetFrameworkVersion(project, PackageManager.Package)
                            == NetFrameworkVersioningHelper.NetFrameworkVersion3_5)
                        {
                            return EmbedAsResourcePrefix;
                        }
                        else
                        {
                            md.Append(EmbedAsResourcePrefix);
                            md.Append("/");

                            if (f[0] == '.'
                                && f[1] == Path.DirectorySeparatorChar)
                            {
                                var folderAndFile = f.Substring(2);
                                md.Append(folderAndFile.Replace(Path.DirectorySeparatorChar, '.'));
                            }
                            else
                            {
                                Debug.Fail("Unexpected start characters in metadata file");
                                return EmbedAsResourcePrefix;
                            }
                        }
                    }
                    else
                    {
                        if (metadataProcessingType == ConnectionDesignerInfo.MAP_EmbedInOutputAssembly)
                        {
                            md.Append(EmbedAsResourcePrefix);
                            md.Append("/");
                            md.Append(f.Replace(Path.DirectorySeparatorChar, '.').TrimStart('.'));
                        }
                        else if (metadataProcessingType == ConnectionDesignerInfo.MAP_CopyToOutputDirectory)
                        {
                            md.Append(f);
                        }
                    }

                    if (i++ < metadataFileCount - 1)
                    {
                        // Character used by framework to separate paths to artifacts in the Entity Connection String
                        md.Append("|");
                    }
                }

                return md.ToString();
            }
        }

        /// <summary>
        ///     Helper to wrap the given sql connection string with a entity client connection string and return it
        /// </summary>
        /// <param name="sqlConnectionString">sql connection string</param>
        /// <returns>map connection string containing the sql connection string</returns>
        internal static ConnectionString ConstructConnectionStringObject(
            string sqlConnectionString, string providerInvariantName,
            IEnumerable<string> metadataFiles, Project project, string outputPath)
        {
            if (null == sqlConnectionString)
            {
                throw new ArgumentNullException("sqlConnectionString");
            }

            if (String.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentNullException("providerInvariantName");
            }

            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            // Wrap the given sql connection string in a map connection string
            var builder = new EntityConnectionStringBuilder();

            builder.Provider = providerInvariantName;
            builder.ProviderConnectionString = sqlConnectionString;

            // we don't want to mess with the model when we are in the process of adding it, so just feed in the default value for metadata artifact processing
            builder.Metadata = GetConnectionStringMetadata(metadataFiles, outputPath, project, GetMetadataArtifactProcessingDefault());

            return new ConnectionString(builder);
        }

        internal static string GetMetadataArtifactProcessingDefault()
        {
            // for now all projects have "Embed in Output Assembly" as their default
            return ConnectionDesignerInfo.MAP_EmbedInOutputAssembly;
        }

        internal HashSet<string> GetExistingConnectionStringNames(Project project)
        {
            if (!ConnStringsByProjectHash.ContainsKey(project))
            {
                return new HashSet<string>();
            }
            return new HashSet<string>(ConnStringsByProjectHash[project].Keys);
        }

        internal ConnectionString GetConnectionStringByMetadataFileName(Project project, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var connStringsInProject = ConnStringsByProjectHash[project];
            if (connStringsInProject != null)
            {
                var enumerator = connStringsInProject.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var connString = enumerator.Current.Value;
                    var metadata = connString.Builder.Metadata;
                    if (metadata.Contains(fileName + EntityDesignArtifact.EXTENSION_CSDL)
                        && metadata.Contains(fileName + EntityDesignArtifact.EXTENSION_MSL)
                        && metadata.Contains(fileName + EntityDesignArtifact.EXTENSION_SSDL))
                    {
                        return connString;
                    }
                }
            }
            return null;
        }

        internal static ConnectionString GetConnectionStringObject(Project project, string entityContainerName)
        {
            return GetConnectionStringObject(project, entityContainerName, PackageManager.Package.ConnectionManager);
        }

        internal static ConnectionString GetConnectionStringObject(
            Project project, string entityContainerName, ConnectionManager connectionManager)
        {
            Dictionary<string, ConnectionString> connStringsInProject = null;
            ConnectionString connectionStringObj = null;
            if (project != null
                && connectionManager != null)
            {
                if (!connectionManager.ConnStringsByProjectHash.TryGetValue(project, out connStringsInProject))
                {
                    return null;
                }
                connStringsInProject.TryGetValue(entityContainerName, out connectionStringObj);
            }
            return connectionStringObj;
        }

        internal void UpdateOrAddConnectionString(Project project, string entityContainerName, string entityConnectionString)
        {
            if (HasConnectionString(project, entityContainerName))
            {
                UpdateConnectionString(project, entityContainerName, entityConnectionString);
            }
            else
            {
                var connectionStringObj = new ConnectionString(entityConnectionString);
                AddConnectionString(project, entityContainerName, connectionStringObj);
            }
        }

        internal bool HasConnectionString(Project project, string entityContainerName)
        {
            if (project == null
                || String.IsNullOrEmpty(entityContainerName))
            {
                return false;
            }

            if (!ConnStringsByProjectHash.ContainsKey(project))
            {
                return false;
            }
            return ConnStringsByProjectHash[project].ContainsKey(entityContainerName);
        }

        internal bool HasConnectionString(Project project, XmlNode node)
        {
            if (project == null
                || node == null)
            {
                return false;
            }

            if (!ConnStringsByProjectHash.ContainsKey(project))
            {
                return false;
            }
            var connectionStringAttr = node.Attributes.GetNamedItem(XmlAttrNameConnectionString);
            var connectionNameAttr = node.Attributes.GetNamedItem(XmlAttrNameEntityContainerName);
            if (connectionStringAttr != null
                && connectionNameAttr != null)
            {
                var connStringObj = new ConnectionString(connectionStringAttr.Value);
                return (ConnStringsByProjectHash[project].ContainsKey(connectionNameAttr.Value)
                        && ConnStringsByProjectHash[project][connectionNameAttr.Value].Equals(connStringObj));
            }
            return false;
        }

        /// <summary>
        ///     Add a connection string object to the hash and push the updates directly to the .config file
        /// </summary>
        private void AddConnectionString(Project project, string entityContainerName, ConnectionString connStringObj)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            lock (_hashSyncRoot)
            {
                ExtractConnStringsIntoHash(project, true);
                if (!ConnStringsByProjectHash.ContainsKey(project))
                {
                    ConnStringsByProjectHash[project] = new Dictionary<string, ConnectionString>();
                }

                // bug 556587: we need to delete the connection string from the hash if it is stale
                if (ConnStringsByProjectHash[project].ContainsKey(entityContainerName))
                {
                    ConnStringsByProjectHash[project].Remove(entityContainerName);
                }
                ConnStringsByProjectHash[project].Add(entityContainerName, connStringObj);
                InsertConnStringsFromHash(project);
            }
        }

        /// <summary>
        ///     Construct a connection string and add it to the hash, pushing the update to the .config file
        /// </summary>
        internal void AddConnectionString(Project project, ICollection<string> metadataFiles, string connectionStringName,
            string configFileConnectionStringValue, string designTimeConnectionStringValue, string providerInvariantName, bool? isSql9OrNewer)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            if (String.IsNullOrEmpty(connectionStringName))
            {
                throw new ArgumentNullException("connectionStringName");
            }

            if (String.IsNullOrEmpty(configFileConnectionStringValue))
            {
                throw new ArgumentNullException("configFileConnectionStringValue");
            }

            var newConfigFileConnString = CreateEntityConnectionString(
                project,
                metadataFiles,
                configFileConnectionStringValue,
                designTimeConnectionStringValue,
                providerInvariantName,
                isSql9OrNewer);

            // add the connection string to the hash and update the .config file
            AddConnectionString(project, connectionStringName, newConfigFileConnString);
        }

        internal static ConnectionString CreateEntityConnectionString(
            Project project,
            IEnumerable<string> metadataFiles,
            string configFileConnectionStringValue,
            string providerInvariantName,
            bool? isSql90OrNewer)
        {
            var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, project);
            var outputPath = (VisualStudioProjectSystem.WebApplication == applicationType)
                                 ? project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value as string
                                 : string.Empty;

            // construct the config file connection string object given the separate parameters 
            // (may correctly not have password info if user has selected not to store sensitive info)
            var newConfigFileConnString = ConstructConnectionStringObject(
                configFileConnectionStringValue, providerInvariantName, metadataFiles, project, outputPath);

            DbConnectionStringBuilder tempBuilderForConfigFile;
            if (TryCreateDbConnectionStringBuilder(newConfigFileConnString.Builder.ProviderConnectionString, out tempBuilderForConfigFile))
            {
                InjectEFAttributesIntoConnectionString(
                    project,
                    PackageManager.Package,
                    tempBuilderForConfigFile,
                    newConfigFileConnString.Builder.Provider,
                    null,
                    null,
                    isSql90OrNewer);

                newConfigFileConnString.Builder.ProviderConnectionString = tempBuilderForConfigFile.ConnectionString;
            }
            else
            {
                Debug.Fail(
                    "Unable to create connection string builders for provider connection string. EF Attributes (MARS/App) won't be in the connection string");
            }
            return newConfigFileConnString;
        }

        private static ConnectionString CreateEntityConnectionString(
            Project project,
            ICollection<string> metadataFiles,
            string configFileConnectionStringValue,
            string designTimeConnectionStringValue,
            string providerInvariantName,
            bool? isSql9OrNewer)
        {
            var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, project);
            var outputPath = (VisualStudioProjectSystem.WebApplication == applicationType)
                                 ? project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value as string
                                 : string.Empty;

            // construct the config file connection string object given the separate parameters 
            // (may correctly not have password info if user has selected not to store sensitive info)
            var newConfigFileConnString = ConstructConnectionStringObject(
                configFileConnectionStringValue, providerInvariantName, metadataFiles, project, outputPath);

            // construct design-time connection string (should have password info even if user 
            // has selected not to store sensitive info)
            var designTimeConnString = ConstructConnectionStringObject(
                designTimeConnectionStringValue, providerInvariantName, metadataFiles, project, outputPath);

            DbConnectionStringBuilder tempBuilderForConfigFile, tempBuilderForDesignTime;
            if (TryCreateDbConnectionStringBuilder(newConfigFileConnString.Builder.ProviderConnectionString, out tempBuilderForConfigFile)
                && TryCreateDbConnectionStringBuilder(designTimeConnString.Builder.ProviderConnectionString, out tempBuilderForDesignTime))
            {
                InjectEFAttributesIntoConnectionString(
                    project,
                    PackageManager.Package,
                    tempBuilderForConfigFile,
                    newConfigFileConnString.Builder.Provider,
                    tempBuilderForDesignTime,
                    designTimeConnString.Builder.Provider,
                    isSql9OrNewer);

                newConfigFileConnString.Builder.ProviderConnectionString = tempBuilderForConfigFile.ConnectionString;
                designTimeConnString.Builder.ProviderConnectionString = tempBuilderForDesignTime.ConnectionString;
            }
            else
            {
                Debug.Fail(
                    "Unable to create connection string builders for provider connection string. EF Attributes (MARS/App) won't be in the connection string");
            }
            return newConfigFileConnString;
        }

        /// <summary>
        ///     Injects the MARS/AppFramework attributes into the provider connection string
        ///     without pinging the connection to see if the database supports SQL 90 or newer. This
        ///     does not require a design-time connection.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static void InjectEFAttributesIntoConnectionString(
            Project project,
            IServiceProvider serviceProvider,
            DbConnectionStringBuilder configFileConnectionBuilder,
            string newConnStringProviderName,
            DbConnectionStringBuilder designTimeConnectionBuilder,
            string designTimeProviderName,
            bool? isSql9OrNewer)
        {
            // if the provider connection string's provider property is "System.Data.SqlClient" then add the 
            // MARS attribute (value is true if SQL Server version >= 9, false otherwise). Also add the App
            // attribute (with fixed value EntityFramework) - which is useful for statistics on server.
            if (string.Equals(newConnStringProviderName, SqlClientProviderName, StringComparison.Ordinal))
            {
                // add MARS property if it does not already exist
                object marsValue;
                if (false == configFileConnectionBuilder.TryGetValue(XmlAttrNameMultipleActiveResultSets, out marsValue))
                {
                    if (!isSql9OrNewer.HasValue)
                    {
                        Debug.Assert(designTimeConnectionBuilder != null, "Should have provided a design time connection builder");
                        Debug.Assert(designTimeProviderName != null, "Should have provided a design time provider name");
                        if (designTimeConnectionBuilder != null
                            && designTimeProviderName != null)
                        {
                            SqlConnection versionTestConn = null;
                            try
                            {
                                // use designTimeConnString to connect to the DB as it has the password info 
                                // even if user is not storing that info in the config file
                                versionTestConn = new SqlConnection(designTimeConnectionBuilder.ConnectionString);
                                versionTestConn.Open();
                                isSql9OrNewer = (Int32.Parse(versionTestConn.ServerVersion.Substring(0, 2), CultureInfo.CurrentCulture) >= 9);
                            }
                            catch (Exception e)
                            {
                                if (CriticalException.IsCriticalException(e))
                                {
                                    Debug.Fail(
                                        "caught exception of type " + e.GetType().FullName + " with message: " + e.Message
                                        + " and Stack Trace " + e.StackTrace);
                                }
                            }
                            finally
                            {
                                VsUtils.SafeCloseDbConnection(
                                    versionTestConn, designTimeProviderName, designTimeConnectionBuilder.ConnectionString);
                            }
                        }
                    }

                    if (isSql9OrNewer.HasValue)
                    {
                        configFileConnectionBuilder[XmlAttrNameMultipleActiveResultSets] = isSql9OrNewer.Value.ToString();
                    }
                }

                // add App attribute if neither App nor Application Name property is already set
                if (!configFileConnectionBuilder.ContainsKey(ProviderConnectionStringPropertyNameApp)
                    && !configFileConnectionBuilder.ContainsKey(ProviderConnectionStringPropertyNameApplicationName)
                    && VsUtils.EntityFrameworkSupportedInProject(project, serviceProvider, allowMiscProject: false))
                {
                    configFileConnectionBuilder[ProviderConnectionStringPropertyNameApp] = "EntityFramework";
                    // note: fixed value so no localization;
                }
            }
        }

        /// <summary>
        ///     Given an old metadata name and a new one, find the connection string keyed by the old metadata name in the hash,
        ///     update its metadata name, then rewrite the .config file.
        /// </summary>
        /// <param name="project">DTE Project that owns the .config file</param>
        /// <param name="entityContainerName"></param>
        /// <param name="newMetadata"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal void UpdateMetadataName(Project project, string entityContainerName, string newMetadata)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            if (String.IsNullOrEmpty(entityContainerName))
            {
                throw new ArgumentNullException("entityContainerName");
            }

            if (String.IsNullOrEmpty(newMetadata))
            {
                throw new ArgumentNullException("newMetadata");
            }

            lock (_hashSyncRoot)
            {
                ExtractConnStringsIntoHash(project, true);
                var localConnStringToChange = GetConnectionStringObject(project, entityContainerName);

                if (localConnStringToChange != null)
                {
                    localConnStringToChange.Builder.Metadata = newMetadata;
                    InsertConnStringsFromHash(project);
                }
                else
                {
                    var s = String.Format(CultureInfo.CurrentCulture, Resources.ConnectionManager_NoConnectionString, entityContainerName);
                    VsUtils.LogOutputWindowPaneMessage(project, s);
                }
            }
        }

        /// <summary>
        ///     Change the entity container name in the hash, then rewrite the .config file.
        /// </summary>
        /// <param name="project">DTE Project that owns the .config file</param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal void UpdateEntityContainerName(Project project, string oldName, string newName)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            if (String.IsNullOrEmpty(oldName))
            {
                throw new ArgumentNullException("oldName");
            }

            if (String.IsNullOrEmpty(newName))
            {
                throw new ArgumentNullException("newName");
            }

            lock (_hashSyncRoot)
            {
                ExtractConnStringsIntoHash(project, true);
                ConnectionString tempString = null;

                // there definitely has to be a connection string keyed by the old entity container name.
                if (ConnStringsByProjectHash.ContainsKey(project)
                    && ConnStringsByProjectHash[project].TryGetValue(oldName, out tempString))
                {
                    ConnStringsByProjectHash[project].Remove(oldName);
                    // if the user opens up a .config with connection strings that aren't being used, one of them
                    // could contain the new entity container name, so we make sure we remove that before adding.
                    if (ConnStringsByProjectHash[project].ContainsKey(newName))
                    {
                        ConnStringsByProjectHash[project].Remove(newName);
                    }
                    ConnStringsByProjectHash[project].Add(newName, tempString);

                    InsertConnStringsFromHash(project);
                }
            }
        }

        internal void UpdateConnectionString(Project project, string entityContainerName, string newConnectionString)
        {
            if (null == project)
            {
                throw new ArgumentNullException("project");
            }

            if (project.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return;
            }

            lock (_hashSyncRoot)
            {
                ExtractConnStringsIntoHash(project, true);
                ConnectionString existingConnectionString = null;

                // there definitely has to be a connection string keyed by the old entity container name.
                if (ConnStringsByProjectHash.ContainsKey(project)
                    && ConnStringsByProjectHash[project].TryGetValue(entityContainerName, out existingConnectionString))
                {
                    var ecsb = new EntityConnectionStringBuilder();
                    try
                    {
                        ecsb = new EntityConnectionStringBuilder(newConnectionString);
                    }
                    catch (ArgumentException)
                    {
                        Debug.WriteLine("Encountered argument exception while parsing the entity connection string");
                    }

                    ConnStringsByProjectHash[project][entityContainerName].Builder = ecsb;

                    InsertConnStringsFromHash(project);
                }
            }
        }

        /// <summary>
        ///     Helper to find or create elements along an XPath expression, under a parent node
        /// </summary>
        private static XmlNode FindOrCreateXmlElement(XmlNode parentNode, string elementPath, bool prependChild)
        {
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

        #region Event handlers

        /// <summary>
        ///     After removing a *.config file, the connection manager should clear the internal connection string hash table
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private int OnAfterRemoveFile(object sender, ModelChangeEventArgs args)
        {
            if (args.ProjectObj == null)
            {
                Debug.Fail("Could not find the project object to attempt to clear the connection manager's hashtable if necessary");
                return VSConstants.E_INVALIDARG;
            }

            if (String.IsNullOrEmpty(args.OldFileName))
            {
                Debug.Fail(
                    "We are trying to figure out if we should clear the connection manager's hashtable if the file is a *.config, but where is the filename?");
                return VSConstants.E_INVALIDARG;
            }

            if ((VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj) == VisualStudioProjectSystem.WindowsApplication &&
                 Path.GetFileName(args.OldFileName).Equals(VsUtils.AppConfigFileName, StringComparison.CurrentCultureIgnoreCase))
                || (VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj) != VisualStudioProjectSystem.WindowsApplication &&
                    Path.GetFileName(args.OldFileName).Equals(VsUtils.WebConfigFileName, StringComparison.CurrentCultureIgnoreCase)))
            {
                lock (_hashSyncRoot)
                {
                    try
                    {
                        _connStringsByProjectHash.Clear();
                        _connStringsByProjectHash = null;
                    }
                    catch (Exception)
                    {
                        return VSConstants.E_FAIL;
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     After renaming a file, we need to update the metadata portion of the connection string
        ///     to reflect the new name of the edmx file.
        /// </summary>
        internal int OnAfterRenameFile(object sender, ModelChangeEventArgs args)
        {
            // ignore files that are not edmx
            if (!Path.GetExtension(args.OldFileName).Equals(EntityDesignArtifact.EXTENSION_EDMX, StringComparison.CurrentCulture))
            {
                return VSConstants.E_NOTIMPL;
            }

            if (args.ProjectObj == null)
            {
                Debug.Fail(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ConnectionManager_UpdateError, "Metadata portion of connection string",
                        "No project was found"));
                return VSConstants.E_INVALIDARG;
            }

            // if we are renaming the extension to a non-edmx extension, then the artifact will be null
            if (args.Artifact == null)
            {
                if (Path.GetExtension(args.NewFileName).Equals(EntityDesignArtifact.EXTENSION_EDMX, StringComparison.CurrentCulture))
                {
                    Debug.Fail("we are renaming the file to one with an edmx extension, why weren't we able to find the artifact?");
                }
                return VSConstants.E_INVALIDARG;
            }

            var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj);

            if (args.Artifact.ConceptualModel() != null
                && args.Artifact.ConceptualModel().FirstEntityContainer != null
                && HasConnectionString(args.ProjectObj, args.Artifact.ConceptualModel().FirstEntityContainer.LocalName.Value))
            {
                var outputPath = (VisualStudioProjectSystem.WebApplication == applicationType)
                                     ? args.ProjectObj.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value as
                                       string
                                     : string.Empty;

                var metadataFileNames = GetMetadataFileNamesFromArtifactFileName(args.ProjectObj, args.Artifact.Uri.LocalPath, PackageManager.Package);
                var mapProperty = GetMetadataPropertyFromArtifact(args.Artifact);
                string mapPropertyValue;
                if (mapProperty != null)
                {
                    mapPropertyValue = mapProperty.ValueAttr.Value;
                }
                else
                {
                    mapPropertyValue = ConnectionDesignerInfo.MAP_CopyToOutputDirectory;
                }

                var newMetaData = GetConnectionStringMetadata(metadataFileNames, outputPath, args.ProjectObj, mapPropertyValue);

                UpdateMetadataName(args.ProjectObj, args.Artifact.ConceptualModel().FirstEntityContainer.LocalName.Value, newMetaData);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     After opening a project, we want to see if there is a .config file, parse it, and
        ///     add the connection strings to our hash.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private int OnAfterOpenProject(object sender, ModelChangeEventArgs args)
        {
            if (args.ProjectObj == null)
            {
                Debug.Fail(Resources.ConnectionManager_InitializeError);
                return VSConstants.E_FAIL;
            }

            if (args.ProjectObj.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return VSConstants.E_INVALIDARG;
            }

            lock (_hashSyncRoot)
            {
                try
                {
                    ExtractConnStringsIntoHash(args.ProjectObj, false);
                }
                catch (NotImplementedException)
                {
                    // if a project doesn't implement any features we try to access along this code path, then ignore it
                }
                catch (Exception e)
                {
                    VsUtils.LogOutputWindowPaneMessage(args.ProjectObj, e.Message);
                    return VSConstants.E_FAIL;
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     When a user edits the .config file directly, we want to pull those changes into our local hash so
        ///     any further changes will be validated against it (if the user edited the entity container name we
        ///     wouldn't be able to find it until the user changes it back in the .config file)
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUIShell.RefreshPropertyBrowser(System.Int32)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private int OnAfterSaveFile(object sender, ModelChangeEventArgs args)
        {
            if (args.ProjectObj == null)
            {
                // don't assert here because a solution could get passed in
                return VSConstants.E_INVALIDARG;
            }

            if (args.ProjectObj.UniqueName.Equals(Constants.vsMiscFilesProjectUniqueName, StringComparison.Ordinal))
            {
                return VSConstants.E_INVALIDARG;
            }

            var hr = VSConstants.S_OK;

            // we're only given a cookie into the RDT so we have to query it to get the filename
            var docTable = Services.IVsRunningDocumentTable;
            uint rdtFlags, readLocks, editLocks, itemId;
            string fileName;
            IVsHierarchy hierarchy;
            var docData = IntPtr.Zero;

            try
            {
                hr = docTable.GetDocumentInfo(
                    args.DocCookie, out rdtFlags, out readLocks, out editLocks, out fileName, out hierarchy, out itemId, out docData);
            }
            finally
            {
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                }
            }

            if (fileName != null)
            {
                var connStringsUpdated = false;
                // update the local hash table if app.config/web.config was updated manually. This is where we recognize if a user tried to fix up the config.
                if (Path.GetFileName(fileName).Equals(VsUtils.AppConfigFileName, StringComparison.CurrentCultureIgnoreCase)
                    || Path.GetFileName(fileName).Equals(VsUtils.WebConfigFileName, StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_hashSyncRoot)
                    {
                        try
                        {
                            ExtractConnStringsIntoHash(args.ProjectObj, true);
                            connStringsUpdated = true;
                        }
                        catch (Exception e)
                        {
                            VsUtils.LogOutputWindowPaneMessage(args.ProjectObj, e.Message);
                            hr = VSConstants.E_FAIL;
                        }
                    }
                }
                else if (VSHelpers.GetDocData(PackageManager.Package, fileName) is IEntityDesignDocData)
                {
                    Debug.Assert(args.Artifact != null, "Artifact must be passed in in order to save the edmx file!");

                    if (args.Artifact != null
                        && args.Artifact.ConceptualModel() != null
                        && args.Artifact.ConceptualModel().FirstEntityContainer != null
                        && args.Artifact.ConceptualModel().FirstEntityContainer.LocalName != null)
                    {
                        ExtractConnStringsIntoHash(args.ProjectObj, true);
                        connStringsUpdated = true;

                        var entityContainerName = args.Artifact.ConceptualModel().FirstEntityContainer.LocalName.Value;

                        // first check if an entity container name was updated in the model and we haven't saved it
                        if (!String.IsNullOrEmpty(_staleEntityContainerName))
                        {
                            UpdateEntityContainerName(args.ProjectObj, _staleEntityContainerName, entityContainerName);
                            _staleEntityContainerName = null;
                        }

                        // at this point we have taken into account all of the user's actions to fix up the connection between the model and the
                        // config file. Now we update the metadata artifact processing if it was updated
                        if (HasConnectionString(args.ProjectObj, entityContainerName)
                            && !String.IsNullOrEmpty(_staleMetadataArtifactProcessing))
                        {
                            var mapProperty = GetMetadataPropertyFromArtifact(args.Artifact);
                            Debug.Assert(mapProperty != null, "Metadata artifact processing property in the model cannot be null");
                            if (mapProperty != null)
                            {
                                var metadataFileNames = GetMetadataFileNamesFromArtifactFileName(
                                    args.ProjectObj, args.Artifact.Uri.LocalPath, PackageManager.Package);
                                var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj);
                                var outputPath = (VisualStudioProjectSystem.WebApplication == applicationType)
                                                     ? args.ProjectObj.ConfigurationManager.ActiveConfiguration.Properties.Item(
                                                         "OutputPath")
                                                           .Value as string
                                                     : string.Empty;

                                var currentMetadataArtifactProcessingValue = mapProperty.ValueAttr.Value;
                                // Compare the new and value of MetadataArtifactProcessingValue, if they are different update the config file.
                                if (String.Compare(
                                    currentMetadataArtifactProcessingValue, _staleMetadataArtifactProcessing,
                                    StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    var metadata = GetConnectionStringMetadata(
                                        metadataFileNames, outputPath, args.ProjectObj, currentMetadataArtifactProcessingValue);
                                    var connectionString = GetConnectionStringObject(args.ProjectObj, entityContainerName, this);
                                    connectionString.Builder.Metadata = metadata;
                                    InsertConnStringsFromHash(args.ProjectObj);
                                }
                            }
                            _staleMetadataArtifactProcessing = null;
                        }
                    }

                    // refresh the property browser if we have updated the connection strings above. This is for the situation where app.config is saved but the user
                    // does not have focus on it. This way, the read-only connection string in the property browser will be updated immediately.
                    if (connStringsUpdated)
                    {
                        var uiShell = Services.ServiceProvider.GetService(typeof(IVsUIShell)) as IVsUIShell;
                        if (uiShell != null)
                        {
                            uiShell.RefreshPropertyBrowser(0);
                        }
                    }
                }
            }

            return hr;
        }

        /// <summary>
        ///     If the user changes the entity container name property we just take note of the old entity container name,
        ///     storing it safely in the connection manager so when we are ready to commit the change we will know what connection string
        ///     to change based on the old name.
        /// </summary>
        private int OnAfterEntityContainerNameChange(object sender, ModelChangeEventArgs args)
        {
            if (_staleEntityContainerName == null)
            {
                _staleEntityContainerName = args.OldEntityContainerName;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     If the user changes the artifact metadata processing value, we store the old value.
        ///     We are going to compare the value with the current value to determine whether we need to commit the value.
        /// </summary>
        private int OnAfterMetadataArtifactProcessingChange(object sender, ModelChangeEventArgs args)
        {
            if (String.IsNullOrEmpty(_staleMetadataArtifactProcessing))
            {
                _staleMetadataArtifactProcessing = args.OldMetadataArtifactProcessing;
            }
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        ///     Translate an invariant name from design-time to runtime or vice versa
        /// </summary>
        internal static string TranslateInvariantName(string invariantName, string connectionString, bool isDesignTime)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (invariantName == null)
            {
                throw new ArgumentNullException("invariantName");
            }
            var translatedInvariantName = invariantName;

            var providerMapper = PackageManager.Package.GetService(typeof(IDTAdoDotNetProviderMapper)) as IDTAdoDotNetProviderMapper;
            var providerMapper2 = providerMapper as IDTAdoDotNetProviderMapper2;

            if (providerMapper2 != null)
            {
                if (isDesignTime)
                {
                    translatedInvariantName = providerMapper2.MapInvariantToRuntimeInvariantName(invariantName, connectionString, false);
                }
                else
                {
                    translatedInvariantName = providerMapper2.MapRuntimeInvariantToInvariantName(invariantName, connectionString, false);
                }
            }

            if (string.IsNullOrEmpty(translatedInvariantName))
            {
                translatedInvariantName = invariantName;
            }
            return translatedInvariantName;
        }

        /// <summary>
        ///     Translate a connection string from design-time to runtime or vice versa.
        /// </summary>
        internal static string TranslateConnectionString(Project project, string invariantName, string connectionString, bool isDesignTime)
        {
            var converter =
                (IConnectionStringConverterService)
                PackageManager.Package.GetService(typeof(IConnectionStringConverterService));
            if (converter == null)
            {
                return connectionString;
            }

            try
            {
                return isDesignTime
                           ? converter.ToRunTime(project, connectionString, invariantName)
                           : converter.ToDesignTime(
                               project, connectionString,
                               TranslateInvariantName(invariantName, connectionString, false));
            }
            catch (ConnectionStringConverterServiceException)
            {
                // ConnectionStringConverterServiceException has no Message - convert to a more descriptive exception
                var errMsg = isDesignTime
                                 ? string.Format(
                                     CultureInfo.CurrentCulture,
                                     Resources.CannotTranslateDesignTimeConnectionString, connectionString)
                                 : string.Format(
                                     CultureInfo.CurrentCulture,
                                     Resources.CannotTranslateRuntimeConnectionString, connectionString);
                throw new ArgumentException(errMsg);
            }
        }

        /// <summary>
        ///     Retrieves the Guid for a particular provider
        /// </summary>
        /// <param name="invariantName">The invariant name for the provider</param>
        /// <param name="connectionString">The connection string being used</param>
        /// <returns>The associated Guid</returns>
        private static Guid TranslateProviderGuid(string invariantName, string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (invariantName == null)
            {
                throw new ArgumentNullException("invariantName");
            }

            var providerGuid = Guid.Empty;
            var providerMapper = PackageManager.Package.GetService(typeof(IDTAdoDotNetProviderMapper)) as IDTAdoDotNetProviderMapper;
            if (providerMapper != null)
            {
                providerGuid = providerMapper.MapInvariantNameToGuid(invariantName, connectionString, false /*fEncryptedString*/);
            }

            return providerGuid;
        }

        internal void UpdateProviderConnectionString(
            Project project, string entityContainerName, string connectionString, bool? isSql90OrNewer)
        {
            var existingConnectionStringObj = GetConnectionStringObject(project, entityContainerName);
            if (existingConnectionStringObj != null)
            {
                // Build an EntityConnectionString
                var ecsb = new EntityConnectionStringBuilder
                    {
                        ProviderConnectionString = connectionString
                    };

                // Inject the existing provider and metadata
                ecsb.Provider = existingConnectionStringObj.Builder.Provider;
                ecsb.Metadata = existingConnectionStringObj.Builder.Metadata;

                // Create a new ConnectionString object
                var newConnectionStringObj = new ConnectionString(ecsb);

                DbConnectionStringBuilder tempBuilder;
                if (TryCreateDbConnectionStringBuilder(newConnectionStringObj.Builder.ProviderConnectionString, out tempBuilder))
                {
                    // Inject the EF metadata as necessary
                    InjectEFAttributesIntoConnectionString(
                        project,
                        PackageManager.Package,
                        tempBuilder,
                        newConnectionStringObj.Builder.Provider,
                        tempBuilder,
                        newConnectionStringObj.Builder.Provider,
                        isSql90OrNewer);

                    newConnectionStringObj.Builder.ProviderConnectionString = tempBuilder.ConnectionString;
                }
                else
                {
                    Debug.Fail("Unable to inject EF attributes (MARS/App) into connection string");
                }

                // Finally, update the full connection string in the config file.
                UpdateConnectionString(project, entityContainerName, newConnectionStringObj.Text);
            }
        }

        internal static bool ShouldUpdateExistingConnectionString(
            Project project,
            ConnectionString existingConnectionStringObj,
            string providerConnectionString,
            string providerName,
            bool isSql90OrNewer)
        {
            DbConnectionStringBuilder tempBuilder;
            if (TryCreateDbConnectionStringBuilder(providerConnectionString, out tempBuilder))
            {
                InjectEFAttributesIntoConnectionString(
                    project,
                    PackageManager.Package,
                    tempBuilder,
                    providerName,
                    tempBuilder,
                    providerName,
                    isSql90OrNewer);

                return
                    !tempBuilder.ConnectionString.Equals(
                        existingConnectionStringObj.Builder.ProviderConnectionString, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
