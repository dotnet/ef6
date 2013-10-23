// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using EnvDTE;

    internal class LocalDataUtil
    {
        internal static readonly string PROVIDER_NAME_SQLCLIENT = "System.Data.SqlClient";
        internal static readonly string PROVIDER_NAME_OLEDB = "System.Data.OleDb";

        // this is the design-time provider name - all versions start with this prefix
        internal static readonly string PROVIDER_NAME_SQLCE_PREFIX = "Microsoft.SqlServerCe.Client";

        internal static readonly string PROVIDER_NAME_JET = "MICROSOFT.JET";
        internal static readonly string PROVIDER_NAME_ACE = "MICROSOFT.ACE";
        internal static readonly string DATAFOLDERNAME = "App_Data";
        internal static readonly string SQL_MOBILE_DEVICE = "Mobile Device";
        internal static readonly string CONNECTION_PROPERTY_DATA_SOURCE = "Data Source";
        internal static readonly string CONNECTION_PROPERTY_ATTACH_DB_FILENAME = "AttachDbFileName";
        internal static readonly string CONNECTION_PROPERTY_PROVIDER = "provider";
        private static string SQL_EXPRESS_DATA_FILE_EXTENSION = ".mdf";
        private static string SQL_EXPRESS_LOG_FILE_SUFFIX = "_log";
        private static string SQL_EXPRESS_LOG_FILE_EXTENSION = ".ldf";

        internal static string GetLocalDbFilePath(string providerInvariantName, string providerConnectionString)
        {
            var filePathKey = GetFilePathKey(providerInvariantName, providerConnectionString);
            if (string.IsNullOrEmpty(filePathKey))
            {
                return null;
            }

            var providerConnectionStringBuilder = new DbConnectionStringBuilder();
            providerConnectionStringBuilder.ConnectionString = providerConnectionString;
            object filePathObject;
            providerConnectionStringBuilder.TryGetValue(filePathKey, out filePathObject);
            var filePath = filePathObject as string;
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            return filePath;
        }

        internal static bool IsLocalDbFileConnectionString(string providerInvariantName, string providerConnectionString)
        {
            if (IsSqlLocalConnectionString(providerInvariantName, providerConnectionString)
                || IsAccessConnectionString(providerInvariantName, providerConnectionString)
                || IsSqlMobileConnectionString(providerInvariantName))
            {
                return true;
            }

            return false;
        }

        internal static bool IsSqlLocalConnectionString(string providerInvariantName, string providerConnectionString)
        {
            if (null == providerInvariantName
                || null == providerConnectionString)
            {
                return false;
            }

            if (0 == string.CompareOrdinal(providerInvariantName, PROVIDER_NAME_SQLCLIENT))
            {
                var providerConnectionStringBuilder = new DbConnectionStringBuilder();
                providerConnectionStringBuilder.ConnectionString = providerConnectionString;
                object ignoreResult;
                if (providerConnectionStringBuilder.TryGetValue(CONNECTION_PROPERTY_ATTACH_DB_FILENAME, out ignoreResult))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsAccessConnectionString(string providerInvariantName, string providerConnectionString)
        {
            if (null == providerInvariantName
                || null == providerConnectionString)
            {
                return false;
            }

            if (0 == string.CompareOrdinal(providerInvariantName, PROVIDER_NAME_OLEDB))
            {
                // This is an OleDb connection string, verify if it is using the Jet provider.
                var providerConnectionStringBuilder = new DbConnectionStringBuilder();
                providerConnectionStringBuilder.ConnectionString = providerConnectionString;
                object oleDbProviderObject;
                providerConnectionStringBuilder.TryGetValue(CONNECTION_PROPERTY_PROVIDER, out oleDbProviderObject);
                var oleDbProvider = oleDbProviderObject as string;

                Debug.Assert(oleDbProvider != null, "Expected the provider connection string to contain a 'provider' property.");
                if (!string.IsNullOrEmpty(oleDbProvider))
                {
                    if (oleDbProvider.StartsWith(PROVIDER_NAME_JET, StringComparison.OrdinalIgnoreCase)
                        || oleDbProvider.StartsWith(PROVIDER_NAME_ACE, StringComparison.OrdinalIgnoreCase))
                    {
                        // This is a Jet or Ace connection string.
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsSqlMobileConnectionString(string providerInvariantName)
        {
            if (null != providerInvariantName
                && providerInvariantName.StartsWith(PROVIDER_NAME_SQLCE_PREFIX, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        internal static bool IsSqlMobileDeviceConnectionString(string providerInvariantName, string providerConnectionString)
        {
            if (IsSqlMobileConnectionString(providerInvariantName))
            {
                var filePath = GetLocalDbFilePath(providerInvariantName, providerConnectionString);
                if (null != filePath
                    && filePath.StartsWith(SQL_MOBILE_DEVICE, StringComparison.OrdinalIgnoreCase))
                {
                    // For mobile devices, if the connection starts with 'Mobile Device' it means that the connection
                    // refers to a location on the device itself.
                    return true;
                }
            }

            return false;
        }

        internal static string GetFilePathKey(string providerInvariantName, string providerConnectionString)
        {
            if (IsAccessConnectionString(providerInvariantName, providerConnectionString))
            {
                return CONNECTION_PROPERTY_DATA_SOURCE;
            }
            else if (IsSqlLocalConnectionString(providerInvariantName, providerConnectionString))
            {
                return CONNECTION_PROPERTY_ATTACH_DB_FILENAME;
            }
            else if (IsSqlMobileConnectionString(providerInvariantName))
            {
                return CONNECTION_PROPERTY_DATA_SOURCE;
            }

            return null;
        }

        internal static ProjectItems GetDefaultCollectionForLocalDataFile(IServiceProvider serviceProvider, Project project)
        {
            // if Web Site project then default location is App_Data directory
            var projectSystem = VsUtils.GetApplicationType(serviceProvider, project);
            if (VisualStudioProjectSystem.WebApplication == projectSystem
                || VisualStudioProjectSystem.Website == projectSystem)
            {
                var dataFolderProjectItem = FindOrCreateAppDataFolder(serviceProvider, project);
                if (null == dataFolderProjectItem)
                {
                    Debug.Fail("Could not find or create App_Data Folder for project " + project.UniqueName);
                }
                else
                {
                    return dataFolderProjectItem.ProjectItems;
                }
            }

            // if not Web App or Web Site then default location is just root of project
            return project.ProjectItems;
        }

        internal static ProjectItem FindOrCreateAppDataFolder(IServiceProvider serviceProvider, Project project)
        {
            var projectSystem = VsUtils.GetApplicationType(serviceProvider, project);
            if (VisualStudioProjectSystem.WebApplication != projectSystem
                && VisualStudioProjectSystem.Website != projectSystem)
            {
                Debug.Fail("can only be called for Web App or Web Site projects");
                return null;
            }

            // see if App_Data folder already exists
            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                if (0 == string.Compare(projectItem.Name, DATAFOLDERNAME, StringComparison.OrdinalIgnoreCase))
                {
                    return projectItem;
                }
            }

            // No data folder found. Create one.
            var dataFolderProjectItem = project.ProjectItems.AddFolder(DATAFOLDERNAME, Constants.vsProjectItemKindPhysicalFolder);
            Debug.Assert(dataFolderProjectItem != null, "Adding the App_Data folder failed for project " + project.UniqueName);
            return dataFolderProjectItem;
        }

        internal static bool IsSqlExpressDataFile(string fileExtension)
        {
            if (0 == string.Compare(SQL_EXPRESS_DATA_FILE_EXTENSION, fileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        internal static string GetSqlExpressLogFilePath(string dataFilePath)
        {
            Debug.Assert(IsSqlExpressDataFile(Path.GetExtension(dataFilePath)), "Expected an .mdf file as input.");

            var newFileName = Path.ChangeExtension(
                Path.GetFileNameWithoutExtension(dataFilePath) + SQL_EXPRESS_LOG_FILE_SUFFIX, SQL_EXPRESS_LOG_FILE_EXTENSION);

            return Path.Combine(Path.GetDirectoryName(dataFilePath), newFileName);
        }
    }
}
