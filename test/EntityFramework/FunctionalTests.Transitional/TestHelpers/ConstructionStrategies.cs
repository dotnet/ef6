// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    /// <summary>
    ///     Describes the format of connection strings used for DbContext construction
    ///     that takes a connection string
    /// </summary>
    public enum ConnectionStringFormat
    {
        /// <summary>
        ///     Describes the string passed in to be the Database Name
        /// </summary>
        DatabaseName,

        /// <summary>
        ///     Describes the string passed in to be a Named Connection String
        /// </summary>
        NamedConnectionString,

        /// <summary>
        ///     Describes the string passed in to be a Provider Connection String
        /// </summary>
        ProviderConnectionString
    };

    /// <summary>
    ///     Describes the ways of DbContext construction
    /// </summary>
    public enum DbContextConstructorArgumentType
    {
        /// <summary>
        ///     DbContext constructed via DbContext Parameterless constructor
        /// </summary>
        Parameterless,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes a DbCompiledModel parameter
        /// </summary>
        DbCompiledModel,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes a DbConnection parameter
        /// </summary>
        Connection,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes a ConnectionString parameter
        /// </summary>
        ConnectionString,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes Connection and DbCompiledModel parameters
        /// </summary>
        ConnectionAndDbCompiledModel,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes Connection String and DbCompiledModel parameters
        /// </summary>
        ConnectionStringAndDbCompiledModel,

        /// <summary>
        ///     DbContext constructed via DbContext constructor that takes an ObjectContext parameter
        /// </summary>
        ObjectContext,
    };

    /// <summary>
    ///     Enumeration to denote the different types of DbCompiledModel that could be constructed
    /// </summary>
    public enum DbCompiledModelContents
    {
        /// <summary>
        ///     Empty DbCompiledModel
        /// </summary>
        IsEmpty,

        /// <summary>
        ///     Null DbCompiledModel
        /// </summary>
        IsNull,

        /// <summary>
        ///     DbCompiledModel that defines entities which are a subset of those defined on DbContext
        /// </summary>
        IsSubset,

        /// <summary>
        ///     DbCompiledModel that defines entities which are a superset of those defined on DbContext
        /// </summary>
        IsSuperset,

        /// <summary>
        ///     DbCompiledModel that defines the same entities as those defined on DbContext
        /// </summary>
        Match,

        /// <summary>
        ///     DbCompiledModel that defines entities which are not defined on DbContext
        /// </summary>
        DontMatch,
    };
}
