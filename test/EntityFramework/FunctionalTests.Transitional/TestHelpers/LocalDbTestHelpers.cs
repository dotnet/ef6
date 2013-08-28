// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    /// <summary>
    /// Helper methods that provide functionality needed to run against localdb.
    /// </summary>
    public class LocalDbTestHelpers
    {
        public static bool IsLocalDb(string connectionString)
        {
            // try to guess if we are targeting LocalDB
            // heuristic - connection string contains: "(localdb)"
            return connectionString.ToLower().Contains(@"(localdb)");
        }
    }
}
