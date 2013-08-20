// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper methods that provide functionality needed to run against azure.
    /// </summary>
    public class AzureTestHelpers
    {
        public static bool IsSqlAzure(string connectionString)
        {
            // try to guess if we are targeting SQL Azure
            // heuristic - connection string contains: "...User ID=user@server..."
            var isAzureConnectionString = new Regex("User ID.*=.*@", RegexOptions.IgnoreCase);
            return isAzureConnectionString.IsMatch(connectionString);
        }
    }
}
