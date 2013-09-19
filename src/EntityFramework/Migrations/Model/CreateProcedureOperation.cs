// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A migration operation to add a new stored procedure to the database.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class CreateProcedureOperation : ProcedureOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProcedureOperation"/> class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">The name of the stored procedure.</param>
        /// <param name="bodySql">The body of the stored procedure expressed in SQL.</param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateProcedureOperation(string name, string bodySql, object anonymousArguments = null)
            : base(name, bodySql, anonymousArguments)
        {
        }

        /// <summary>
        /// Gets an operation to drop the stored procedure.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return new DropProcedureOperation(Name); }
        }
    }
}
