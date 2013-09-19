// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Sql
{
    /// <summary>
    /// Represents a migration operation that has been translated into a SQL statement.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class MigrationStatement
    {
        /// <summary>
        /// Gets or sets the SQL to be executed to perform this migration operation.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this statement should be performed outside of
        /// the transaction scope that is used to make the migration process transactional.
        /// If set to true, this operation will not be rolled back if the migration process fails.
        /// </summary>
        public bool SuppressTransaction { get; set; }

        /// <summary>
        /// Gets or sets the batch terminator for the database provider.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <value>
        /// The batch terminator for the database provider.
        /// </value>
        public string BatchTerminator { get; set; }
    }
}
