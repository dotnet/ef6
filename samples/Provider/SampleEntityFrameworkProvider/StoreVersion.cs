// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;

namespace SampleEntityFrameworkProvider
{
    /// <summary>
    /// This enum describes the current server version
    /// </summary>
    internal enum StoreVersion
    {
        /// <summary>
        /// Sql Server 9
        /// </summary>
        Sql9 = 90,

        /// <summary>
        /// Sql Server 10
        /// </summary>
        Sql10 = 100,

        // higher versions go here
    }

    /// <summary>
    /// This class is a simple utility class that determines the sql version from the 
    /// connection
    /// </summary>
    internal static class StoreVersionUtils
    {
        /// <summary>
        /// Get the StoreVersion from the connection. Returns one of Sql9, Sql10
        /// </summary>
        /// <param name="connection">current sql connection</param>
        /// <returns>Sql Version for the current connection</returns>
        internal static StoreVersion GetStoreVersion(SampleConnection connection)
        {
            // We don't have anything unique for Sql
            if ((connection.ServerVersion.StartsWith("10.", StringComparison.Ordinal)) || 
               (connection.ServerVersion.StartsWith("11.", StringComparison.Ordinal)))
            {
                return StoreVersion.Sql10;
            }
            else if (connection.ServerVersion.StartsWith("09.", StringComparison.Ordinal))
            {
                return StoreVersion.Sql9;
            }
            else
            {
                throw new ArgumentException("The version of SQL Server is not supported via sample provider.");
            }
        }

        internal static StoreVersion GetStoreVersion(string providerManifestToken)
        {
            switch (providerManifestToken)
            {
                case SampleProviderManifest.TokenSql9:
                    return StoreVersion.Sql9;

                case SampleProviderManifest.TokenSql10:
                    return StoreVersion.Sql10;

                default:
                    throw new ArgumentException("Could not determine storage version; a valid provider manifest token is required.");
            }
        }

        internal static string GetVersionHint(StoreVersion version)
        {
            switch (version)
            {
                case StoreVersion.Sql9:
                    return SampleProviderManifest.TokenSql9;

                case StoreVersion.Sql10:
                    return SampleProviderManifest.TokenSql10;
            }

            throw new ArgumentException("Could not determine storage version; a valid storage connection or a version hint is required.");
        }
    }
}
