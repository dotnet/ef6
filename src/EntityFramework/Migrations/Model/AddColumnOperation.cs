// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a column being added to a table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class AddColumnOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly ColumnModel _column;

        /// <summary>
        /// Initializes a new instance of the AddColumnOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table"> The name of the table the column should be added to. </param>
        /// <param name="column"> Details of the column being added. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AddColumnOperation(string table, ColumnModel column, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(column, "column");

            _table = table;
            _column = column;
        }

        /// <summary>
        /// Gets the name of the table the column should be added to.
        /// </summary>
        public string Table
        {
            get { return _table; }
        }

        /// <summary>
        /// Gets the details of the column being added.
        /// </summary>
        public ColumnModel Column
        {
            get { return _column; }
        }

        /// <summary>
        /// Gets an operation that represents dropping the added column.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                return new DropColumnOperation(
                    Table, Column.Name, Column.Annotations.ToDictionary(a => a.Key, a => a.Value.NewValue));
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
