namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// This class is a simple utility class that determines the sql version from the 
    /// connection
    /// </summary>
    internal static class SqlVersionUtils
    {
        /// <summary>
        /// Get the SqlVersion from the connection. Returns one of Sql8, Sql9, Sql10
        /// The passed connection must be open
        /// </summary>
        /// <param name="connection">current sql connection</param>
        /// <returns>Sql Version for the current connection</returns>
        internal static SqlVersion GetSqlVersion(SqlConnection connection)
        {
            Debug.Assert(connection.State == ConnectionState.Open, "Expected an open connection");
            var majorVersion = Int32.Parse(connection.ServerVersion.Substring(0, 2), CultureInfo.InvariantCulture);
            if (majorVersion >= 10)
            {
                return SqlVersion.Sql10;
            }
            else if (majorVersion == 9)
            {
                return SqlVersion.Sql9;
            }
            else
            {
                Debug.Assert(majorVersion == 8, "not version 8");
                return SqlVersion.Sql8;
            }
        }

        internal static string GetVersionHint(SqlVersion version)
        {
            switch (version)
            {
                case SqlVersion.Sql8:
                    return SqlProviderManifest.TokenSql8;

                case SqlVersion.Sql9:
                    return SqlProviderManifest.TokenSql9;

                case SqlVersion.Sql10:
                    return SqlProviderManifest.TokenSql10;

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
