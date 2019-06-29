// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Describes the origin of the database connection string associated with a <see cref="DbContext" />.
    /// </summary>
    public enum DbConnectionStringOrigin
    {
        /// <summary>
        /// The connection string was created by convention.
        /// </summary>
        Convention,

        /// <summary>
        /// The connection string was read from external configuration.
        /// </summary>
        Configuration,

        /// <summary>
        /// The connection string was explicitly specified at runtime.
        /// </summary>
        UserCode,

        /// <summary>
        /// The connection string was overridden by connection information supplied to DbContextInfo.
        /// </summary>
        DbContextInfo
    }
}
