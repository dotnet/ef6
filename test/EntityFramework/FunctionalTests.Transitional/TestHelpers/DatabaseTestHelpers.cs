// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper methods that provide functionality needed to run against azure.
    /// </summary>
    public static class DatabaseTestHelpers
    {
        private static readonly Regex _isAzureServer = new Regex(
            @"(Data Source|Server)\s*=\s*(tcp:)?\s*\w*\.database\.windows\.net", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsSqlAzure(string connectionString)
        {
            // try to guess if we are targeting SQL Azure
            // heuristic - connection string contains: "Data Source=abcd1234.database.windows.net"
            return _isAzureServer.IsMatch(connectionString);
        }

        public static bool IsLocalDb(string connectionString)
        {
            // try to guess if we are targeting LocalDB
            // heuristic - connection string contains: "(localdb)"
            return connectionString.ToLower().Contains(@"(localdb)");
        }

        public static bool IsIntegratedSecurity(string connectionString)
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

        public static int GetSqlDatabaseVersion<TContext>(Func<TContext> contextCreator) 
            where TContext : DbContext
        {
            string version = null;
            using (var context = contextCreator())
            {
                context.Database.Initialize(false);
                if (context.Database.Connection.State == ConnectionState.Closed)
                {
                    context.Database.Connection.Open();
                    version = context.Database.Connection.ServerVersion;
                    context.Database.Connection.Close();
                }
                else
                {
                    version = context.Database.Connection.ServerVersion;
                }
            }

            int parsedVersion;
            if (!int.TryParse(version.Substring(0, 2), out parsedVersion))
            {
                throw new InvalidOperationException("Could not parse server version: " + version);
            }

            return parsedVersion;
        }
    }
}
