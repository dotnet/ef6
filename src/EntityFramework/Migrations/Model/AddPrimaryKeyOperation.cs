// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents adding a primary key to a table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class AddPrimaryKeyOperation : PrimaryKeyOperation
    {
        /// <summary>
        /// Initializes a new instance of the AddPrimaryKeyOperation class.
        /// The Table and Columns properties should also be populated.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AddPrimaryKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {

        }

        /// <summary>
        /// Gets an operation to drop the primary key.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var dropPrimaryKeyOperation
                    = new DropPrimaryKeyOperation
                          {
                              Name = Name,
                              Table = Table,
                              IsClustered = IsClustered
                          };

                Columns.Each(c => dropPrimaryKeyOperation.Columns.Add(c));

                return dropPrimaryKeyOperation;
            }
        }
    }
}
