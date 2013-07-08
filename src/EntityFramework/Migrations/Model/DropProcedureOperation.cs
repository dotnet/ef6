// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Drops a stored procedure from the database.
    /// </summary>
    public class DropProcedureOperation : MigrationOperation
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropProcedureOperation"/> class.
        /// </summary>
        /// <param name="name">The name of the stored procedure to drop.</param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropProcedureOperation(string name, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        /// <summary>
        /// Gets the name of the stored procedure to drop.
        /// </summary>
        /// <value>
        /// The name of the stored procedure to drop.
        /// </value>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets an operation that will revert this operation. 
        ///     Always returns a <see cref="NotSupportedOperation"/>.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return NotSupportedOperation.Instance; }
        }

        /// <summary>
        /// Gets a value indicating if this operation may result in data loss. Always returns false.
        /// </summary>
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
