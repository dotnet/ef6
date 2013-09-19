// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents renaming a stored procedure in the database.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class RenameProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameProcedureOperation"/> class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">The name of the stored procedure to rename.</param>
        /// <param name="newName">The new name for the stored procedure.</param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public RenameProcedureOperation(string name, string newName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            _name = name;
            _newName = newName;
        }

        /// <summary>
        /// Gets the name of the stored procedure to rename.
        /// </summary>
        /// <value>
        /// The name of the stored procedure to rename.
        /// </value>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the new name for the stored procedure.
        /// </summary>
        /// <value>
        /// The new name for the stored procedure.
        /// </value>
        public virtual string NewName
        {
            get { return _newName; }
        }

        /// <summary>
        /// Gets an operation that will revert this operation.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var originalName = DatabaseName.Parse(_name);
                var newTable = DatabaseName.Parse(_newName).Name;

                return new RenameProcedureOperation(
                    new DatabaseName(newTable, originalName.Schema).ToString(),
                    originalName.Name);
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
