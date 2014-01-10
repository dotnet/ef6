// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents renaming an existing index.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class RenameIndexOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly string _name;
        private string _newName;

        /// <summary>
        /// Initializes a new instance of the RenameIndexOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table"> Name of the table the index belongs to. </param>
        /// <param name="name"> Name of the index to be renamed. </param>
        /// <param name="newName"> New name for the index. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public RenameIndexOperation(string table, string name, string newName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            _table = table;
            _name = name;
            _newName = newName;
        }

        /// <summary>
        /// Gets the name of the table the index belongs to.
        /// </summary>
        public virtual string Table
        {
            get { return _table; }
        }

        /// <summary>
        /// Gets the name of the index to be renamed.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the new name for the index.
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
            get { return new RenameIndexOperation(Table, NewName, Name); }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
