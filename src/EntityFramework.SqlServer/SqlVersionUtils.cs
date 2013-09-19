// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.SqlServer.Resources;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// This class is a simple utility class that determines the SQL Server version from the
    /// connection.
    /// </summary>
    internal static class SqlVersionUtils
    {
        /// <summary>
        /// Get the SqlVersion from the connection. Returns one of Sql8, Sql9, Sql10, Sql11
        /// The passed connection must be open
        /// </summary>
        /// <param name="connection"> current sql connection </param>
        /// <returns> Sql Version for the current connection </returns>
        internal static SqlVersion GetSqlVersion(DbConnection connection)
        {
            Debug.Assert(connection.State == ConnectionState.Open, "Expected an open connection");
            var majorVersion = Int32.Parse(connection.ServerVersion.Substring(0, 2), CultureInfo.InvariantCulture);

            if (majorVersion >= 11)
            {
                return SqlVersion.Sql11;
            }

            if (majorVersion == 10)
            {
                return SqlVersion.Sql10;
            }

            if (majorVersion == 9)
            {
                return SqlVersion.Sql9;
            }

            Debug.Assert(majorVersion == 8, "not version 8");
            return SqlVersion.Sql8;
        }

        internal static ServerType GetServerType(DbConnection connection)
        {
            Debug.Assert(connection.State == ConnectionState.Open, "Expected an open connection");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select serverproperty('EngineEdition')";

                using (
                    var reader = DbInterception.Dispatch.Command.Reader(command, new DbCommandInterceptionContext()))
                {
                    reader.Read();

                    const int sqlAzureEngineEdition = 5;
                    return reader.GetInt32(0) == sqlAzureEngineEdition ? ServerType.Cloud : ServerType.OnPremises;
                }
            }
        }

        internal static string GetVersionHint(SqlVersion version, ServerType serverType)
        {
            if (serverType == ServerType.Cloud)
            {
                Debug.Assert(version >= SqlVersion.Sql11);
                return SqlProviderManifest.TokenAzure11;
            }

            switch (version)
            {
                case SqlVersion.Sql8:
                    return SqlProviderManifest.TokenSql8;

                case SqlVersion.Sql9:
                    return SqlProviderManifest.TokenSql9;

                case SqlVersion.Sql10:
                    return SqlProviderManifest.TokenSql10;

                case SqlVersion.Sql11:
                    return SqlProviderManifest.TokenSql11;

                default:
                    throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
            }
        }

        internal static SqlVersion GetSqlVersion(string versionHint)
        {
            if (!string.IsNullOrEmpty(versionHint))
            {
                switch (versionHint)
                {
                    case SqlProviderManifest.TokenSql8:
                        return SqlVersion.Sql8;

                    case SqlProviderManifest.TokenSql9:
                        return SqlVersion.Sql9;

                    case SqlProviderManifest.TokenSql10:
                        return SqlVersion.Sql10;

                    case SqlProviderManifest.TokenSql11:
                        return SqlVersion.Sql11;

                    case SqlProviderManifest.TokenAzure11:
                        return SqlVersion.Sql11;
                }
            }

            throw new ArgumentException(Strings.UnableToDetermineStoreVersion);
        }

        internal static bool IsPreKatmai(SqlVersion sqlVersion)
        {
            return sqlVersion == SqlVersion.Sql8 || sqlVersion == SqlVersion.Sql9;
        }
    }
}
