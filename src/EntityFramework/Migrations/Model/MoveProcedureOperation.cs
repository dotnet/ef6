// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents moving a stored procedure to a new schema in the database.
    /// </summary>
    public class MoveProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveProcedureOperation"/> class.
        /// </summary>
        /// <param name="name">The name of the stored procedure to move.</param>
        /// <param name="newSchema">The new schema for the stored procedure.</param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public MoveProcedureOperation(string name, string newSchema, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _newSchema = newSchema;
        }

        /// <summary>
        /// Gets the name of the stored procedure to move.
        /// </summary>
        /// <value>
        /// The name of the stored procedure to move.
        /// </value>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the new schema for the stored procedure.
        /// </summary>
        /// <value>
        /// The new schema for the stored procedure.
        /// </value>
        public virtual string NewSchema
        {
            get { return _newSchema; }
        }

        /// <summary>
        /// Gets an operation that will revert this operation.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var databaseName = DatabaseName.Parse(_name);

                return new MoveProcedureOperation(
                    new DatabaseName(databaseName.Name, NewSchema).ToString(),
                    databaseName.Schema);
            }
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
