// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents dropping a primary key from a table.
    /// </summary>
    public class DropPrimaryKeyOperation : PrimaryKeyOperation
    {
        /// <summary>
        ///     Initializes a new instance of the DropPrimaryKeyOperation class.
        ///     The Table and Columns properties should also be populated.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropPrimaryKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Gets an operation to add the primary key.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var addPrimaryKeyOperation
                    = new AddPrimaryKeyOperation
                          {
                              Name = Name,
                              Table = Table
                          };

                Columns.Each(c => addPrimaryKeyOperation.Columns.Add(c));

                return addPrimaryKeyOperation;
            }
        }

        /// <summary>
        /// Used when altering the migrations history table so that the table can be rebuilt rather than just dropping and adding the primary key.
        /// </summary>
        /// <value>
        /// The create table operation for the migrations history table.
        /// </value>
        public CreateTableOperation CreateTableOperation { get; internal set; }
    }
}
