// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EntityModel = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Entity.Design.VisualStudio.SingleFileGenerator;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal static class EdmUtils
    {
        private static readonly string EdmxTemplate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
            "<edmx:Edmx Version=\"{4}\" xmlns:edmx=\"{3}\">" + Environment.NewLine +
            "  <edmx:Runtime>" + Environment.NewLine +
            "    <!-- SSDL content -->" + Environment.NewLine +
            "    <edmx:StorageModels>" + Environment.NewLine +
            "    {1}" + Environment.NewLine +
            "    </edmx:StorageModels>" + Environment.NewLine +
            "    <!-- CSDL content -->" + Environment.NewLine +
            "    <edmx:ConceptualModels>" + Environment.NewLine +
            "    {0}" + Environment.NewLine +
            "    </edmx:ConceptualModels>" + Environment.NewLine +
            "    <!-- C-S mapping content -->" + Environment.NewLine +
            "    <edmx:Mappings>" + Environment.NewLine +
            "    {2}" + Environment.NewLine +
            "    </edmx:Mappings>" + Environment.NewLine +
            "  </edmx:Runtime>" + Environment.NewLine +
            "  <edmx:Designer>" + Environment.NewLine +
            "    <edmx:Connection />" + Environment.NewLine +
            "    <edmx:Options />" + Environment.NewLine +
            "    <edmx:Diagrams />" + Environment.NewLine +
            "  </edmx:Designer>" + Environment.NewLine +
            "</edmx:Edmx>";

        internal static readonly string[] CsdlSsdlMslExtensions =
            new[] { EntityDesignArtifact.ExtensionCsdl, EntityDesignArtifact.ExtensionSsdl, EntityDesignArtifact.ExtensionMsl };

        internal static readonly string AppCodeFolderName = "." + Path.DirectorySeparatorChar + "App_Code" + Path.DirectorySeparatorChar;

        private const string EscherSettingsRegistryPath = "EDMDesigner";

        // <summary>
        //     Returns whether the proposedModelNamespace is valid as a model namespace.
        // </summary>
        // <param name="proposedModelNamespace">The proposed model namespace</param>
        internal static bool IsValidModelNamespace(string proposedModelNamespace)
        {
            return
                !string.IsNullOrEmpty(proposedModelNamespace) &&
                EscherAttributeContentValidator.IsValidCsdlNamespaceName(proposedModelNamespace);
        }

        // <summary>
        //     Returns a string which is valid as a model namespace given the
        //     proposedModelNamespace as a starting point.
        // </summary>
        // <param name="proposedModelNamespace">The proposed model namespace</param>
        // <param name="defaultModelNamespace">The model namespace to use if removal of invalid chars results in an empty string</param>
        internal static string ConstructValidModelNamespace(string proposedModelNamespace, string defaultModelNamespace)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(defaultModelNamespace), "defaultModelNamespace must not be null or empty");

            if (IsValidModelNamespace(proposedModelNamespace))
            {
                return proposedModelNamespace;
            }

            if (String.IsNullOrEmpty(proposedModelNamespace))
            {
                return defaultModelNamespace;
            }

            var trialModelNamespace = proposedModelNamespace.Replace("<", "").Replace(">", "").Replace("&", "");
            if (!EscherAttributeContentValidator.IsValidXmlAttributeValue(trialModelNamespace)
                || string.IsNullOrEmpty(trialModelNamespace))
            {
                return defaultModelNamespace;
            }

            if (IsValidModelNamespace(trialModelNamespace))
            {
                // if trialModelNamespace is now valid just return it
                return trialModelNamespace;
            }

            // if trialModelNamespace is invalid must be an invalid identifier
            // try stripping any leading numbers
            while (trialModelNamespace.Length > 0
                   && !Char.IsLetter(trialModelNamespace[0]))
            {
                trialModelNamespace = trialModelNamespace.Substring(1);
            }

            if (String.IsNullOrEmpty(trialModelNamespace))
            {
                return defaultModelNamespace;
            }

            if (IsValidModelNamespace(trialModelNamespace))
            {
                return trialModelNamespace;
            }

            // try adding a one to the end
            trialModelNamespace += "1";
            if (IsValidModelNamespace(trialModelNamespace))
            {
                return trialModelNamespace;
            }

            // give up and return default
            return defaultModelNamespace;
        }

        // <summary>
        //     Using proposedNamespace as a starting point return a namespace which is not in the
        //     set of existing namespaces recorded in the existingNamespaces Dictionary.
        // </summary>
        internal static string ConstructUniqueNamespace(string proposedNamespace, HashSet<string> existingNamespaces)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(proposedNamespace),
                "ConstructUniqueNamespace: Proposed Namespace cannot be null or empty string");

            if (existingNamespaces == null)
            {
                return proposedNamespace;
            }

            var uniqueNamespace = proposedNamespace;
            var uniqueInteger = 1;

            // NumberFormatInfo below required for localization
            var nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = string.Empty;
            while (existingNamespaces.Contains(uniqueNamespace))
            {
                uniqueNamespace = proposedNamespace + uniqueInteger.ToString(nfi);
                uniqueInteger++;
            }

            return uniqueNamespace;
        }

        internal static string[] GetRelativeMetadataPaths(
            string folderPath, Project project, string modelRootName, string[] extensionsWithDots, IServiceProvider serviceProvider)
        {
            var projectRootFileInfo = VsUtils.GetProjectRoot(project, serviceProvider);

            var relativePathsList = new List<string>(extensionsWithDots.Length);
            foreach (var extensionWithDot in extensionsWithDots)
            {
                var fileName = modelRootName + extensionWithDot;
                var folderPathDirectory = new DirectoryInfo(folderPath);
                var relativePath = GetRelativePath(folderPathDirectory, projectRootFileInfo);
                relativePathsList.Add(relativePath + fileName);
            }

            return relativePathsList.ToArray();
        }

        // <summary>
        //     Returns a path for "directory1" relative to "directory2"
        // </summary>
        internal static string GetRelativePath(DirectoryInfo directory1, DirectoryInfo directory2)
        {
            var d = directory1;
            var root1 = d.Root;
            var root2 = directory2.Root;

            if (!root1.Name.ToUpperInvariant().Equals(root2.Name.ToUpperInvariant()))
            {
                throw new ArgumentException(Resources.ModelObjectItemWizard_CantBeRelative);
            }

            var filePathParts = new LinkedList<DirectoryInfo>();
            var directoryPathParts = new LinkedList<DirectoryInfo>();

            while (d != null
                   && !d.Equals(d.Root))
            {
                filePathParts.AddFirst(new DirectoryInfo(d.Name));
                d = d.Parent;
            }

            d = directory2;
            while (d != null
                   && !d.Equals(d.Root))
            {
                directoryPathParts.AddFirst(new DirectoryInfo(d.Name));
                d = d.Parent;
            }

            // remove all common directories at the head of the list.
            while (directoryPathParts.Count > 0
                   && filePathParts.Count > 0)
            {
                var d1 = filePathParts.First.Value;
                var d2 = directoryPathParts.First.Value;
                if (d1.Name.ToUpperInvariant().Equals(d2.Name.ToUpperInvariant()))
                {
                    filePathParts.RemoveFirst();
                    directoryPathParts.RemoveFirst();
                }
                else
                {
                    break;
                }
            }

            var sb = new StringBuilder();
            sb.Append(".\\");

            // add in a "..\" for each directory part left in the "from" list
            for (var i = 0; i < directoryPathParts.Count; i++)
            {
                sb.Append("..\\");
            }

            // add in the directory name for each directory part left in the "to" list
            foreach (var di in filePathParts)
            {
                sb.Append(di.Name + "\\");
            }

            return sb.ToString();
        }

        internal static bool IsDataServicesEdmx(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return false;
            }

            try
            {
                return IsDataServicesEdmx(XDocument.Load(fileInfo.FullName));
            }
            catch (XmlException)
            {
                // no-op, keep going on exception
            }

            return false;
        }

        internal static bool IsDataServicesEdmx(XDocument inputXml)
        {
            Debug.Assert(inputXml != null, "inputXml != null");

            foreach (XNamespace namespaceName in SchemaManager.GetEDMXNamespaceNames())
            {
                var rootEdmxElement = inputXml.Element(namespaceName + "Edmx");
                if (rootEdmxElement != null
                    && rootEdmxElement.Element(namespaceName + "DataServices") != null)
                {
                    return true;
                }
            }

            return false;
        }

        // <summary>
        //     Gets the target SCHEMA VERSION of the project's installed runtime, or if none,
        //     of the latest runtime available on the project's target framework.
        // </summary>
        public static Version GetEntityFrameworkVersion(Project project, IServiceProvider serviceProvider, bool useLatestIfNoEF = true)
        {
            Debug.Assert(project != null, "project != null");
            Debug.Assert(serviceProvider != null, "serviceProvider != null");

            if (VsUtils.IsMiscellaneousProject(project))
            {
                return null;
            }

            var efAssemblyVersion = VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project);
            var netFrameworkVersion = NetFrameworkVersioningHelper.TargetNetFrameworkVersion(project, serviceProvider);

            if (efAssemblyVersion == null
                && !useLatestIfNoEF)
            {
                return RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(netFrameworkVersion);
            }

            return RuntimeVersion.GetTargetSchemaVersion(efAssemblyVersion, netFrameworkVersion);
        }

        // <summary>
        //     Return the EF Runtime assemblies path.
        // </summary>
        internal static string GetRuntimeAssemblyPath(Project project, IServiceProvider serviceProvider)
        {
            // Get Project Design Time Assembly Resolution.
            // Don't call GetVsHierarchy(project) because it causes dependencies with EDM Package service provider.
            var hierarchy = VSHelpers.GetVsHierarchy(project, serviceProvider);
            var dtar = hierarchy as IVsDesignTimeAssemblyResolution;

            if (dtar != null)
            {
                // There is a bug where the first time you call ResolveAssemblyPathInTargetFx, resolvedAssemblyPathCount is 0.
                // So we are going to try 1 more time if the first call not successful.
                for (var i = 0; i < 2; i++)
                {
                    var resolvedAssemblyPath = new VsResolvedAssemblyPath[1];
                    uint resolvedAssemblyPathCount;
                    if (dtar.ResolveAssemblyPathInTargetFx(
                        new string[1] { "System.Data.Entity" }, 1, resolvedAssemblyPath, out resolvedAssemblyPathCount) == VSConstants.S_OK)
                    {
                        if (resolvedAssemblyPathCount == 1)
                        {
                            return Path.GetDirectoryName(resolvedAssemblyPath[0].bstrResolvedAssemblyPath);
                        }
                    }
                }
            }
            else
            {
                // Project Design Time Assembly Resolution is not found, try Global Design Time Assembly Resolution.
                var targetFrameworkMoniker = VsUtils.GetTargetFrameworkMonikerForProject(project, serviceProvider);
                var multiTargetingService =
                    Services.ServiceProvider.GetService(typeof(SVsFrameworkMultiTargeting)) as IVsFrameworkMultiTargeting;

                string assemblyPath;
                if (multiTargetingService.ResolveAssemblyPath("System.Data.Entity", targetFrameworkMoniker, out assemblyPath)
                    == VSConstants.S_OK)
                {
                    return Path.GetDirectoryName(assemblyPath);
                }
            }

            // if we could not resolve the asembly using global or project design time assembly resolution, return string empty.
            return String.Empty;
        }

#if (!VS12)
        // <summary>
        //     Called when a SQL CE database is upgraded from 3.5 to 4.0.
        //     1. Check the project type, return immediately if project is misc project.
        //     2. Find all the EDMX files in the project.
        //     3. For each EDMX file in the project replace the SSDL Provider and ProviderManifestToken
        //     attributes if needed and update the config file if needed; skip all linked edmx files
        // </summary>
        // <param name="hierarchy">hierarchy for project</param>
        // <param name="logger">log error messages here</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUpgradeLogger.LogMessage(System.UInt32,System.String,System.String,System.String)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void SqlCeUpgradeService_OnUpgradeProject(IVsHierarchy hierarchy, IVsUpgradeLogger logger)
        {
            if (PackageManager.Package != null
                && PackageManager.Package.ModelManager != null)
            {
                // since this is about retargeting EDMX files on disk, no need to process other file extensions from any converters
                var fileFinder = new VSFileFinder(EntityDesignArtifact.ExtensionEdmx);
                fileFinder.FindInProject(hierarchy);

                var project = VSHelpers.GetProject(hierarchy);

                // skip the step if it is a miscellaneous project.
                if (project != null
                    && !VsUtils.IsMiscellaneousProject(project))
                {
                    var hadErrors = false;
                    IDictionary<string, object> documentMap = new Dictionary<string, object>();
                    foreach (var vsFileInfo in fileFinder.MatchingFiles)
                    {
                        try
                        {
                            var projectItem = VsUtils.GetProjectItem(hierarchy, vsFileInfo.ItemId);

                            // Dev 10 bug 648969: skip the process for astoria edmx file.
                            if (EdmUtils.IsDataServicesEdmx(projectItem.get_FileNames(1)))
                            {
                                continue;
                            }

                            // Check whether project item is a  linked item
                            var isLinkItem = VsUtils.IsLinkProjectItem(projectItem);

                            if (!isLinkItem)
                            {
                                var doc = MetadataConverterDriver.SqlCeInstance.Convert(SafeLoadXmlFromPath(vsFileInfo.Path));
                                if (doc != null)
                                {
                                    documentMap.Add(vsFileInfo.Path, doc);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var errMsg = String.Format(
                                CultureInfo.CurrentCulture, Resources.ErrorDuringSqlCeUpgrade, vsFileInfo.Path, ex.Message);
                            logger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, project.Name, vsFileInfo.Path, errMsg);
                            hadErrors = true;
                        }
                    }

                    // now update the config file as needed
                    var configFilePath = ConnectionManager.GetConfigFilePath(project, false);
                    try
                    {
                        if (false == string.IsNullOrWhiteSpace(configFilePath)) // check config file exists
                        {
                            XmlDocument configXmlDoc;
                            if (ConnectionManager.UpdateSqlCeProviderInConnectionStrings(configFilePath, out configXmlDoc))
                            {
                                documentMap.Add(configFilePath, configXmlDoc);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errMsg = String.Format(
                            CultureInfo.CurrentCulture, Resources.ErrorDuringSqlCeUpgrade, configFilePath, ex.Message);
                        logger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, project.Name, configFilePath, errMsg);
                        hadErrors = true;
                    }

                    if (hadErrors)
                    {
                        // if there were errors above then do not try to change the files on disk
                        return;
                    }

                    // Do bulk update here
                    if (documentMap.Count > 0)
                    {
                        VsUtils.WriteCheckoutXmlFilesInProject(documentMap);
                    }
                }
            }
        }

#endif

        // <summary>
        //     Called when a MDF database file is upgraded from Dev10 to Dev11
        //     1. Check the project type, return immediately if project is misc project.
        //     2. Update the config file if needed
        // </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUpgradeLogger.LogMessage(System.UInt32,System.String,System.String,System.String)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "databaseFile")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newConnectionString")]
        internal static void SqlDatabaseFileUpgradeService_OnUpgradeProject(
            IVsHierarchy hierarchy, string databaseFile, string newConnectionString, IVsUpgradeLogger logger)
        {
            if (PackageManager.Package != null
                && PackageManager.Package.ModelManager != null)
            {
                var project = VSHelpers.GetProject(hierarchy);

                // skip the step if it is a miscellaneous project or if the project is using IIS
                // (projects using IIS should not be upgraded - see bug 812074)
                if (project != null
                    && !VsUtils.IsMiscellaneousProject(project)
                    && !IsUsingIIS(project))
                {
                    // update the config file as needed
                    IDictionary<string, object> documentMap = new Dictionary<string, object>();
                    var configFilePath = ConnectionManager.GetConfigFilePath(project, false);
                    try
                    {
                        if (false == string.IsNullOrWhiteSpace(configFilePath)) // check config file exists
                        {
                            XmlDocument configXmlDoc;
                            if (ConnectionManager.UpdateSqlDatabaseFileDataSourceInConnectionStrings(configFilePath, out configXmlDoc))
                            {
                                documentMap.Add(configFilePath, configXmlDoc);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // if there were errors above then do not try to change the files on disk - just log the message and return
                        var errMsg = String.Format(
                            CultureInfo.CurrentCulture, Resources.ErrorDuringSqlDatabaseFileUpgrade, configFilePath, ex.Message);
                        logger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, project.Name, configFilePath, errMsg);
                        return;
                    }

                    // Actually update the file here
                    if (documentMap.Count > 0)
                    {
                        VsUtils.WriteCheckoutXmlFilesInProject(documentMap);
                    }
                }
            }
        }

        // <summary>
        //     Checks whether a project is using IIS for SQL Database File Upgrade purposes
        //     Note: using IISExpress will return false.
        // </summary>
        private static bool IsUsingIIS(Project project)
        {
            var webSiteTypeProjectProperty = VsUtils.GetProjectPropertyByName(project, "WebSiteType") as int?;
            if (webSiteTypeProjectProperty != null
                && (1 == webSiteTypeProjectProperty))
            {
                // it's a WebSite project so check IsUsingIISExpress. If this is null or false then is using IIS.
                var isUsingIISExpress = VsUtils.GetProjectPropertyByName(project, "IsUsingIISExpress") as bool?;
                return (isUsingIISExpress == null || false == isUsingIISExpress);
            }
            else
            {
                // it's a non-Website project so check WebApplication.UseIIS and WebApplication.IsUsingIISExpress
                var webAppUseIIS = VsUtils.GetProjectPropertyByName(project, "WebApplication.UseIIS") as bool?;
                if (webAppUseIIS == null
                    || false == webAppUseIIS)
                {
                    // WebApplication.UseIIS does not exist or is false
                    return false;
                }
                else
                {
                    // WebApplication.UseIIS is true, return true unless webAppIsUsingIISExpress is also true
                    var webAppIsUsingIISExpress = VsUtils.GetProjectPropertyByName(project, "WebApplication.IsUsingIISExpress") as bool?;
                    return (webAppIsUsingIISExpress == null || false == webAppIsUsingIISExpress);
                }
            }
        }

        // <summary>
        //     Enable/Disable custom tool (which reads the code generation property) for an edmx project item.
        // </summary>
        internal static void ToggleEdmxItemCustomToolProperty(ProjectItem projectItem, bool isEnabled)
        {
            var prop = projectItem.Properties.Item("CustomTool");
            if (prop != null)
            {
                prop.Value = isEnabled ? EntityModelCodeGenerator.CodeGenToolName : String.Empty;
            }
        }

        // <summary>
        //     Enable/Disable code generation for an edmx project item.
        // </summary>
        internal static Command SetCodeGenStrategyToNoneCommand(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Artifact should not be null");
            var existingCodeGenStrategy = ModelHelper.GetDesignerPropertyValueFromArtifact(
                OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeCodeGenerationStrategy, artifact);
            if (string.IsNullOrWhiteSpace(existingCodeGenStrategy))
            {
                existingCodeGenStrategy = Resources.Default; // which is the default value of CodeGenerationStrategy attribute
            }

            if (!string.Equals(existingCodeGenStrategy, Resources.None))
            {
                return ModelHelper.CreateSetDesignerPropertyValueCommandFromArtifact(
                    artifact, OptionsDesignerInfo.ElementName,
                    OptionsDesignerInfo.AttributeCodeGenerationStrategy,
                    Resources.None);
            }

            return null;
        }

        // <summary>
        //     Currently, finds the user setting under CURRENTUSER\Software\Microsoft\VisualStudio\10.0\EDMDesigner.
        //     Returns null if the key/value pair cannot be found.
        //     TODO: it would be nicer to have an options page that stored this setting as a C# property with appropriate
        //     property descriptors.
        // </summary>
        internal static string GetUserSetting(string key)
        {
            var valueAsString = String.Empty;
            var root = VsUtils.GetVisualStudioRegistryPath();
            if (false == String.IsNullOrEmpty(root))
            {
                using (var vsRegistryRootKey = Registry.CurrentUser.OpenSubKey(root, false))
                {
                    Debug.Assert(vsRegistryRootKey != null, "Why couldn't we find the registry root in HKCU for registry root " + root);

                    if (vsRegistryRootKey != null)
                    {
                        using (var escherSettingsRegistryKey = vsRegistryRootKey.OpenSubKey(EscherSettingsRegistryPath, false))
                        {
                            if (escherSettingsRegistryKey != null)
                            {
                                var value = escherSettingsRegistryKey.GetValue(key);
                                if (value != null)
                                {
                                    valueAsString = value.ToString();
                                }
                            }
                        }
                    }
                }
            }

            return valueAsString;
        }

        // <summary>
        //     Currently, sets the user setting under CURRENTUSER\Software\Microsoft\VisualStudio\10.0\EDMDesigner.
        //     TODO: it would be nicer to have an options page that stored this setting as a C# property with appropriate
        //     property descriptors.
        // </summary>
        internal static void SaveUserSetting(string key, string value)
        {
            var root = VsUtils.GetVisualStudioRegistryPath();
            if (false == String.IsNullOrEmpty(root))
            {
                using (var vsRegistryRootKey = Registry.CurrentUser.OpenSubKey(root, true))
                {
                    Debug.Assert(vsRegistryRootKey != null, "Why couldn't we find the registry root in HKCU for registry root " + root);

                    if (vsRegistryRootKey != null)
                    {
                        var escherSettingsRegistryKey = vsRegistryRootKey.OpenSubKey(EscherSettingsRegistryPath, true);
                        if (escherSettingsRegistryKey == null)
                        {
                            escherSettingsRegistryKey = vsRegistryRootKey.CreateSubKey(EscherSettingsRegistryPath);
                        }
                        try
                        {
                            escherSettingsRegistryKey.SetValue(key, value);
                        }
                        finally
                        {
                            if (escherSettingsRegistryKey != null)
                            {
                                escherSettingsRegistryKey.Dispose();
                            }
                        }
                    }
                }
            }
        }

        internal static string CreateEdmxString(Version version, string csdl, string msl, string ssdl)
        {
            var edmxNamespaceName = SchemaManager.GetEDMXNamespaceName(version);
            return string.Format(
                CultureInfo.InvariantCulture, EdmxTemplate, csdl, ssdl, msl, edmxNamespaceName, version.ToString(2)
                /* major and minor version. */);
        }

        // <summary>
        //     Return the corresponding FunctionImport's result-column name for a given property.
        // </summary>
        internal static string GetFunctionImportResultColumnName(EntityModel.FunctionImport functionImport, EntityModel.Property property)
        {
            if (functionImport != null)
            {
                var columnName = EntityModel.FunctionImport.GetFunctionImportResultColumnName(functionImport, property);
                if (!String.IsNullOrEmpty(columnName))
                {
                    return columnName;
                }
            }
            return property.LocalName.Value;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)")]
        internal static bool ShouldShowByRefDebugHelpers()
        {
            var showByRefDebugHelpers = false;
            try
            {
                var showByRefDebugHelpersString = GetUserSetting("ByRefDebug");
                Boolean.TryParse(showByRefDebugHelpersString, out showByRefDebugHelpers);
            }
            catch (Exception)
            {
                // exception accessing registry - just return default setting
            }
            return showByRefDebugHelpers;
        }

        internal static XmlDocument SafeLoadXmlFromPath(string filePath, bool preserveWhitespace = false)
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = preserveWhitespace };
            using (var reader = XmlReader.Create(filePath))
            {
                xmlDocument.Load(reader);
            }

            return xmlDocument;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static XmlDocument SafeLoadXmlFromString(string xml, bool preserveWhitespace = false)
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = preserveWhitespace };
            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                xmlDocument.Load(reader);
            }

            return xmlDocument;
        }
    }
}
