// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper methods that provide functionality needed to run against azure.
    /// </summary>
    public static class DatabaseTestHelpers
    {
        public static bool IsSqlAzure(string connectionString)
        {
            // try to guess if we are targeting SQL Azure
            // heuristic - connection string contains: "...User ID=user@server..."
            var isAzureConnectionString = new Regex("User ID.*=.*@", RegexOptions.IgnoreCase);
            return isAzureConnectionString.IsMatch(connectionString);
        }

        public static bool IsLocalDb(string connectionString)
        {
            // try to guess if we are targeting LocalDB
            // heuristic - connection string contains: "(localdb)"
            return connectionString.ToLower().Contains(@"(localdb)");
        }

        public static bool IsIntegratedSecutity(string connectionString)
        {
            var formattedConnectionString = connectionString.ToLower().Replace(" ", "");
            
            return formattedConnectionString.Contains("integratedsecurity=true") ||
                formattedConnectionString.Contains("integratedsecurity=sspi") ||
                formattedConnectionString.Contains("integratedsecurity=yes") ||
                formattedConnectionString.Contains("trusted_connection=true") ||
                formattedConnectionString.Contains("trusted_connection=sspi") ||
                formattedConnectionString.Contains("trusted_connection=yes");
        }

        public static bool PersistsSecurityInfo(string connectionString)
        {
            var formattedConnectionString = connectionString.ToLower().Replace(" ", "");

            return formattedConnectionString.Contains("persistsecurityinfo=true") ||
                formattedConnectionString.Contains("persistsecurityinfo=yes");
        }
    }
}
