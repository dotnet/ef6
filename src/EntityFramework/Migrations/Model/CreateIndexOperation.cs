// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents creating a database index.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class CreateIndexOperation : IndexOperation
    {
        /// <summary>
        /// Initializes a new instance of the CreateIndexOperation class.
        /// The Table and Columns properties should also be populated.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateIndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating if this is a unique index.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets an operation to drop this index.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var dropIndexOperation
                    = new DropIndexOperation(this)
                          {
                              Name = Name,
                              Table = Table
                          };

                Columns.Each(c => dropIndexOperation.Columns.Add(c));

                return dropIndexOperation;
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets whether this is a clustered index.
        /// </summary>
        public bool IsClustered { get; set; }
    }
}
