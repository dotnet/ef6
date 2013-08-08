// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents renaming an existing table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class RenameTableOperation : MigrationOperation
    {
        private readonly string _name;
        private string _newName;

        /// <summary>
        /// Initializes a new instance of the RenameTableOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name"> Name of the table to be renamed. </param>
        /// <param name="newName"> New name for the table. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public RenameTableOperation(string name, string newName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            _name = name;
            _newName = newName;
        }

        /// <summary>
        /// Gets the name of the table to be renamed.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the new name for the table.
        /// </summary>
        public virtual string NewName
        {
            get { return _newName; }
            internal set
            {
                DebugCheck.NotEmpty(value);

                _newName = value;
            }
        }

        /// <summary>
        /// Gets an operation that reverts the rename.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var originalName = DatabaseName.Parse(_name);
                var newTable = DatabaseName.Parse(_newName).Name;

                return new RenameTableOperation(new DatabaseName(newTable, originalName.Schema).ToString(), originalName.Name);
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
