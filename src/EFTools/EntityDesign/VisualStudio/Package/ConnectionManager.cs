// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.Web.UI.WebControls;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.DataTools.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VSDesigner.Data.Local;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using Constants = EnvDTE.Constants;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    // <summary>
    //     The Connection Manager allows interaction with App.Config and Web.Config. It stores a "project dictionary" where each bucket corresponds
    //     to a dictionary that associates entity container names with their corresponding connection strings, stored as
    //     ConnectionString objects. The dictionaries should mirror App.Config exactly.
    // </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class ConnectionManager : IDisposable
    {
        #region Fields

        private Dictionary<Project, Dictionary<string, ConnectionString>> _connStringsByProjectHash;
        private static readonly object _hashSyncRoot = new object();

        private const string Provider = "System.Data.EntityClient";

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

        private const string XpathConnectionStringsAddEntity = 
            "configuration/connectionStrings/add[@providerName='" + Provider + "']";

        internal static readonly string XpathConnectionStringsAdd = "configuration/connectionStrings/add";

        internal static readonly string EmbedAsResourcePrefix = "res://*";

        private string _staleEntityContainerName;
        private string _staleMetadataArtifactProcessing;


        private const string ProviderConnectionStringPropertyNameApp = "App";
        private const string ProviderConnectionStringPropertyNameApplicationName = "Application Name";

        #endregion

        // <summary>
        //     A wrapper around EntityConnectionStringBuilder so we can add our own properties
        //     or different builders at a later point in time.
        // </summary>
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
                : this(new EntityConnectionStringBuilder(connStringText))
            {
            }

            internal ConnectionString(EntityConnectionStringBuilder builder)
            {
                _builder = builder;
            }

            internal string DesignTimeProviderInvariantName
            {
                get { return TranslateInvariantName(PackageManager.Package, _builder.Provider, _builder.ProviderConnectionString, false); }
            }

            internal string GetDesignTimeProviderConnectionString(Project project)
            {
                return TranslateConnectionString(
                    PackageManager.Package, project, _builder.Provider, _builder.ProviderConnectionString, false);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Debug.Assert(disposing, "Connection Manager is finalized before disposing");
            if (disposing)
            {
                UnregisterModelListenerEvents();
                _connStringsByProjectHash = null;
            }
        }

        // <summary>
        //     Takes our Xml .config file and destructively updates our local hash.
        // </summary>
        // <param name="project">DTE Project that owns App.Config we want to look at.</param>
        internal void ExtractConnStringsIntoHash(Project project, bool createConfig)
        {
            if (VsUtils.IsMiscellaneousProject(project))
            {
                return;
            }

            var configFileUtils = new ConfigFileUtils(project, PackageManager.Package);
            if (createConfig)
            {
                configFileUtils.GetOrCreateConfigFile();
            }

            var configXmlDoc = configFileUtils.LoadConfig();
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

        // <summary>
        //     Returns an EntityContainer name unique in the app/web.config for the given project
        //     given a proposed EntityContainer name
        // </summary>
        internal string ConstructUniqueEntityContainerName(string proposedEntityContainerName, Project project)
        {
            Debug.Assert(null != project, "project in ConstructUniqueEntityContainerName()");
            Debug.Assert(
                null != proposedEntityContainerName, "Null proposedEntityContainerName in ConstructUniqueEntityContainerName()");

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

        // <summary>
        //     Takes our local hash and destructively updates the .config file.
        // </summary>
        // <param name="project">DTE Project that owns the .config file we want to look at.</param>
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


            var configFileUtils = new ConfigFileUtils(project, PackageManager.Package);
            configFileUtils.GetOrCreateConfigFile();
            var configXmlDoc = configFileUtils.LoadConfig();

            Dictionary<string, ConnectionString> hash;
            if (!ConnStringsByProjectHash.TryGetValue(project, out hash))
            {
                var s = String.Format(CultureInfo.CurrentCulture, Resources.ConnectionManager_GetConfigError);
                VsUtils.LogOutputWindowPaneMessage(project, s);
                return;
            }

            if (hash.Any())
            {
                UpdateEntityConnectionStringsInConfig(configXmlDoc, hash);
                configFileUtils.SaveConfig(configXmlDoc);
            }
        }

        // internal for testing
        internal static void UpdateEntityConnectionStringsInConfig(XmlDocument configXmlDoc, Dictionary<string, ConnectionString> entityConnectionStrings)
        {
            Debug.Assert(configXmlDoc != null, "configXmlDoc is null");
            Debug.Assert(entityConnectionStrings != null, "entityConnectionStrings is null");

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

            var connStringsElement = GetConnectionStringsElement(configXmlDoc);
            if (connStringsElement == null)
            {
                throw new XmlException(Resources.ConnectionManager_CorruptConfig);
            }

            foreach (var nameToConnString in entityConnectionStrings)
            {
                AddConnectionStringElement(connStringsElement, nameToConnString.Key, nameToConnString.Value.Text, Provider);
            }
        }

        public static void AddConnectionStringElement(XmlDocument configXmlDoc, string connStringName, string connString, string providerName)
        {
            var connStringsElement = GetConnectionStringsElement(configXmlDoc); 
            if (connStringsElement == null)
            {
                // can happen if the document element is not "configuration"
                throw new XmlException(Resources.ConnectionManager_CorruptConfig);
            }

            AddConnectionStringElement(connStringsElement, connStringName, connString, providerName);
        }

        private static void AddConnectionStringElement(XmlNode connStringsElement, string connStringName, string connString, string providerName)
        {
            var addNode = connStringsElement.OwnerDocument.CreateElement("add");

            addNode.SetAttribute(XmlAttrNameEntityContainerName, connStringName);
            addNode.SetAttribute(XmlAttrNameConnectionString, connString);
            addNode.SetAttribute(XmlAttrNameProviderName, providerName);

            connStringsElement.AppendChild(addNode);
        }

        private static XmlElement GetConnectionStringsElement(XmlDocument configXml)
        {
            if (!"configuration".Equals(configXml.DocumentElement.Name, StringComparison.Ordinal)
                || !string.IsNullOrEmpty(configXml.DocumentElement.NamespaceURI))
            {
                return null;
            }

            var connStringsElement = (XmlElement)configXml.DocumentElement.SelectSingleNode("connectionStrings");
            if (connStringsElement == null)
            {
                connStringsElement = configXml.CreateElement("connectionStrings");
                configXml.DocumentElement.AppendChild(connStringsElement);
            }

            return connStringsElement;
        }

#if (!VS12)
        // Update the .config file if the nodes have a SQL CE 3.5 provider to use 4.0 instead
        internal static bool UpdateSqlCeProviderInConnectionStrings(XmlDocument configXmlDoc)
        {
            Debug.Assert(configXmlDoc != null, "configXml is null");

            var docUpdated = false;
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

            return docUpdated;
        }
#endif

        // Update the .config file if the nodes have an old style SQL Database File Data Source to have the new style instead
        internal static bool UpdateSqlDatabaseFileDataSourceInConnectionStrings(XmlDocument configXmlDoc)
        {
            Debug.Assert(configXmlDoc != null, "configXml is null");

            var docUpdated = false;

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

            return docUpdated;
        }

        // <summary>
        //     Subscribe to VS events via ModelChangeEventListener's delegates
        // </summary>
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

        // <summary>
        //     Unsubscribe to VS events via ModelChangeEventListener's delegates
        // </summary>
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

        // <summary>
        //     Helper function to construct the metadata, depending on what type of application and output path
        // </summary>
        private static string GetConnectionStringMetadata(
            IEnumerable<string> metadataFiles, Project project, VisualStudioProjectSystem applicationType, string metadataProcessingType)
        {
            var outputPath = GetOutputPath(project, applicationType);

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

        // <summary>
        //     Helper to wrap the given sql connection string with a entity client connection string and return it
        // </summary>
        // <param name="sqlConnectionString">sql connection string</param>
        // <returns>map connection string containing the sql connection string</returns>
        internal static ConnectionString ConstructConnectionStringObject(
            string sqlConnectionString, string providerInvariantName,
            IEnumerable<string> metadataFiles, Project project, VisualStudioProjectSystem applicationType)
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
            var builder = new EntityConnectionStringBuilder
            {
                Provider = providerInvariantName,
                ProviderConnectionString = sqlConnectionString,
                // we don't want to mess with the model when we are in the process of adding it, so just feed in the default value for metadata artifact processing
                Metadata = GetConnectionStringMetadata(
                    metadataFiles, project, applicationType, GetMetadataArtifactProcessingDefault())
            };            

            return new ConnectionString(builder);
        }

        internal static string GetMetadataArtifactProcessingDefault()
        {
            // for now all projects have "Embed in Output Assembly" as their default
            return ConnectionDesignerInfo.MAP_EmbedInOutputAssembly;
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

        // <summary>
        //     Add a connection string object to the hash and push the updates directly to the .config file
        // </summary>
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

        // <summary>
        //     Construct a connection string and add it to the hash, pushing the update to the .config file
        // </summary>
        internal void AddConnectionString(Project project, VisualStudioProjectSystem applicationType, ICollection<string> metadataFiles, string connectionStringName,
            string configFileConnectionStringValue, string providerInvariantName)
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
                applicationType,
                metadataFiles,
                configFileConnectionStringValue,
                providerInvariantName);

            // add the connection string to the hash and update the .config file
            AddConnectionString(project, connectionStringName, newConfigFileConnString);
        }

        internal static ConnectionString CreateEntityConnectionString(
            Project project,
            VisualStudioProjectSystem applicationType, 
            IEnumerable<string> metadataFiles,
            string configFileConnectionStringValue,
            string providerInvariantName)
        {
            // note that this connection string may not have credentials if the user chose to not store sensitive info
            return ConstructConnectionStringObject(
                InjectEFAttributesIntoConnectionString(configFileConnectionStringValue, providerInvariantName),
                providerInvariantName, metadataFiles, project, applicationType);
        }

        private static string GetOutputPath(Project project, VisualStudioProjectSystem applicationType)
        {
            return (VisualStudioProjectSystem.WebApplication == applicationType)
                ? project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value as string
                : string.Empty;
        }

        // computes a unique connection string name based on the input base name
        internal static string GetUniqueConnectionStringName(ConfigFileUtils configFileUtils, string baseConnectionStringName)
        {
            var connectionStringNames = GetExistingConnectionStringNames(configFileUtils);

            var i = 1;
            var uniqueConnectionStringName = baseConnectionStringName;
            while (connectionStringNames.Contains(uniqueConnectionStringName))
            {
                uniqueConnectionStringName = baseConnectionStringName + i++;
            }

            return uniqueConnectionStringName;
        }

        internal static HashSet<string> GetExistingConnectionStringNames(ConfigFileUtils configFileUtils)
        {
            var configXml = configFileUtils.LoadConfig();

            if (configXml == null)
            {
                // can be null if config does not exist in which case there are no connection strings
                return new HashSet<string>();
            }

            // note we return all the connection string names to support CodeFirst scenarios
            return
                new HashSet<string>(
                    configXml.SelectNodes(XpathConnectionStringsAdd).OfType<XmlElement>()
                    .Select(addElement => addElement.GetAttribute("name"))
                    .Where(connectionStringName => !string.IsNullOrEmpty(connectionStringName)));
        }

        public static string CreateDefaultLocalDbConnectionString(string initialCatalog)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(initialCatalog), "invalid initial catalog name");

            return
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"Data Source=(LocalDb)\v11.0;Initial Catalog={0};Integrated Security=True",
                    initialCatalog);
        }

        // <summary>
        //     Injects the MARS/AppFramework attributes into the provider connection string
        //     without pinging the connection to see if the database supports SQL 90 or newer. This
        //     does not require a design-time connection.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static string InjectEFAttributesIntoConnectionString(string sourceConnectionString, string providerInvariantName)
        {
            // if the provider connection string's provider property is "System.Data.SqlClient" then add the 
            // MARS attribute (value is true if SQL Server version >= 9, false otherwise). Also add the App
            // attribute (with fixed value EntityFramework) - which is useful for statistics on server.
            if (!string.Equals(providerInvariantName, SqlClientProviderName, StringComparison.Ordinal))
            {
                return sourceConnectionString;
            }

            var configFileConnectionBuilder = new DbConnectionStringBuilder();

            try
            {
                configFileConnectionBuilder.ConnectionString = sourceConnectionString;
            }
            catch (ArgumentException)
            {
                return sourceConnectionString;
            }

            // add MARS property if it does not already exist
            object marsValue;
            if (!configFileConnectionBuilder.TryGetValue(XmlAttrNameMultipleActiveResultSets, out marsValue))
            {
                configFileConnectionBuilder[XmlAttrNameMultipleActiveResultSets] = true.ToString();
            }

            // add App attribute if neither App nor Application Name property is already set
            if (!configFileConnectionBuilder.ContainsKey(ProviderConnectionStringPropertyNameApp)
                && !configFileConnectionBuilder.ContainsKey(ProviderConnectionStringPropertyNameApplicationName))
            {
                // note: fixed value so no localization;
                configFileConnectionBuilder[ProviderConnectionStringPropertyNameApp] = "EntityFramework";
            }

            return configFileConnectionBuilder.ConnectionString;
        }

        // <summary>
        //     Given an old metadata name and a new one, find the connection string keyed by the old metadata name in the hash,
        //     update its metadata name, then rewrite the .config file.
        // </summary>
        // <param name="project">DTE Project that owns the .config file</param>
        // <param name="entityContainerName"></param>
        // <param name="newMetadata"></param>
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

        // <summary>
        //     Change the entity container name in the hash, then rewrite the .config file.
        // </summary>
        // <param name="project">DTE Project that owns the .config file</param>
        // <param name="oldName"></param>
        // <param name="newName"></param>
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

        #region Event handlers

        // <summary>
        //     After removing a *.config file, the connection manager should clear the internal connection string hash table
        // </summary>
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

        // <summary>
        //     After renaming a file, we need to update the metadata portion of the connection string
        //     to reflect the new name of the edmx file.
        // </summary>
        internal int OnAfterRenameFile(object sender, ModelChangeEventArgs args)
        {
            // ignore files that are not edmx
            if (!Path.GetExtension(args.OldFileName).Equals(EntityDesignArtifact.ExtensionEdmx, StringComparison.CurrentCulture))
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
                if (Path.GetExtension(args.NewFileName).Equals(EntityDesignArtifact.ExtensionEdmx, StringComparison.CurrentCulture))
                {
                    Debug.Fail("we are renaming the file to one with an edmx extension, why weren't we able to find the artifact?");
                }
                return VSConstants.E_INVALIDARG;
            }

            if (args.Artifact.ConceptualModel() != null
                && args.Artifact.ConceptualModel().FirstEntityContainer != null
                && HasConnectionString(args.ProjectObj, args.Artifact.ConceptualModel().FirstEntityContainer.LocalName.Value))
            {                   
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

                var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj);
                var newMetaData = GetConnectionStringMetadata(metadataFileNames, args.ProjectObj, applicationType, mapPropertyValue);

                UpdateMetadataName(args.ProjectObj, args.Artifact.ConceptualModel().FirstEntityContainer.LocalName.Value, newMetaData);
            }

            return VSConstants.S_OK;
        }

        // <summary>
        //     After opening a project, we want to see if there is a .config file, parse it, and
        //     add the connection strings to our hash.
        // </summary>
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

        // <summary>
        //     When a user edits the .config file directly, we want to pull those changes into our local hash so
        //     any further changes will be validated against it (if the user edited the entity container name we
        //     wouldn't be able to find it until the user changes it back in the .config file)
        // </summary>
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
                                
                                var currentMetadataArtifactProcessingValue = mapProperty.ValueAttr.Value;
                                // Compare the new and value of MetadataArtifactProcessingValue, if they are different update the config file.
                                if (String.Compare(
                                    currentMetadataArtifactProcessingValue, _staleMetadataArtifactProcessing,
                                    StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    var applicationType = VsUtils.GetApplicationType(Services.ServiceProvider, args.ProjectObj);
                                    var metadata = GetConnectionStringMetadata(metadataFileNames, args.ProjectObj, 
                                        applicationType, currentMetadataArtifactProcessingValue);
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

        // <summary>
        //     If the user changes the entity container name property we just take note of the old entity container name,
        //     storing it safely in the connection manager so when we are ready to commit the change we will know what connection string
        //     to change based on the old name.
        // </summary>
        private int OnAfterEntityContainerNameChange(object sender, ModelChangeEventArgs args)
        {
            if (_staleEntityContainerName == null)
            {
                _staleEntityContainerName = args.OldEntityContainerName;
            }
            return VSConstants.S_OK;
        }

        // <summary>
        //     If the user changes the artifact metadata processing value, we store the old value.
        //     We are going to compare the value with the current value to determine whether we need to commit the value.
        // </summary>
        private int OnAfterMetadataArtifactProcessingChange(object sender, ModelChangeEventArgs args)
        {
            if (String.IsNullOrEmpty(_staleMetadataArtifactProcessing))
            {
                _staleMetadataArtifactProcessing = args.OldMetadataArtifactProcessing;
            }
            return VSConstants.S_OK;
        }

        #endregion

        // <summary>
        //     Translate an invariant name from design-time to runtime or vice versa
        // </summary>
        internal static string TranslateInvariantName(IServiceProvider serviceProvider, string invariantName, string connectionString, bool isDesignTime)
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

            var providerMapper = serviceProvider.GetService(typeof(IDTAdoDotNetProviderMapper)) as IDTAdoDotNetProviderMapper;
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

        // <summary>
        //     Translate a connection string from design-time to runtime or vice versa.
        // </summary>
        internal static string TranslateConnectionString(IServiceProvider serviceProvider, Project project, string invariantName, string connectionString, bool isDesignTime)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider must not be null");
            Debug.Assert(project != null, "project must not be null");
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invalid invariantName");

            if (string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            var converter = (IConnectionStringConverterService)serviceProvider.GetService(typeof(IConnectionStringConverterService));
            if (converter == null)
            {
                return connectionString;
            }

            try
            {
                return isDesignTime
                    ? converter.ToRunTime(project, connectionString, invariantName)
                    : converter.ToDesignTime(
                        project, connectionString, TranslateInvariantName(serviceProvider, invariantName, connectionString, false));
            }
            catch (ConnectionStringConverterServiceException)
            {
                var ddexNotInstalledMsg = 
                    !DDEXProviderInstalled(serviceProvider, invariantName) ? 
                    string.Format(CultureInfo.CurrentCulture, Resources.DDEXNotInstalled, invariantName) :
                    string.Empty;

                // ConnectionStringConverterServiceException has no Message - convert to a more descriptive exception
                var errMsg = isDesignTime
                                 ? string.Format(
                                     CultureInfo.CurrentCulture,
                                     Resources.CannotTranslateDesignTimeConnectionString,
                                     ddexNotInstalledMsg,
                                     connectionString)
                                 : string.Format(
                                     CultureInfo.CurrentCulture,
                                     Resources.CannotTranslateRuntimeConnectionString,
                                     ddexNotInstalledMsg,
                                     connectionString);
                throw new ArgumentException(errMsg);
            }
        }

        private static bool DDEXProviderInstalled(IServiceProvider serviceProvider, string invariantName)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider must not be null");
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "Invalid invariant name");

            var dataProviderManager = (IVsDataProviderManager)serviceProvider.GetService(typeof(IVsDataProviderManager));
            Debug.Assert(dataProviderManager != null, "Could not find IVsDataProviderManager");

            return
                dataProviderManager.Providers.Values.Any(
                    p => invariantName.Equals((string)p.GetProperty("InvariantName"), StringComparison.Ordinal));
        }

        // <summary>
        //     Retrieves the Guid for a particular provider
        // </summary>
        // <param name="invariantName">The invariant name for the provider</param>
        // <param name="connectionString">The connection string being used</param>
        // <returns>The associated Guid</returns>
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
    }
}
