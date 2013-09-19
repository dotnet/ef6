// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    /// <summary>
    /// This enumeration describes the current SQL Server version.
    /// </summary>
    internal enum SqlVersion
    {
        /// <summary>
        /// SQL Server 8 (2000).
        /// </summary>
        Sql8 = 80,

        /// <summary>
        /// SQL Server 9 (2005).
        /// </summary>
        Sql9 = 90,

        /// <summary>
        /// SQL Server 10 (2008).
        /// </summary>
        Sql10 = 100,

        /// <summary>
        /// SQL Server 11 (2012).
        /// </summary>
        Sql11 = 110,

        // Higher versions go here
    }
}
