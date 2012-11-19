// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents altering an existing column.
    /// </summary>
    public class AlterColumnOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly ColumnModel _column;
        private readonly AlterColumnOperation _inverse;
        private readonly bool _destructiveChange;

        /// <summary>
        ///     Initializes a new instance of the AlterColumnOperation class.
        /// </summary>
        /// <param name="table"> The name of the table that the column belongs to. </param>
        /// <param name="column"> Details of what the column should be altered to. </param>
        /// <param name="isDestructiveChange"> Value indicating if this change will result in data loss. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AlterColumnOperation(
            string table, ColumnModel column, bool isDestructiveChange, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(column, "column");

            _table = table;
            _column = column;
            _destructiveChange = isDestructiveChange;
        }

        /// <summary>
        ///     Initializes a new instance of the AlterColumnOperation class.
        /// </summary>
        /// <param name="table"> The name of the table that the column belongs to. </param>
        /// <param name="column"> Details of what the column should be altered to. </param>
        /// <param name="isDestructiveChange"> Value indicating if this change will result in data loss. </param>
        /// <param name="inverse"> An operation to revert this alteration of the column. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AlterColumnOperation(
            string table, ColumnModel column, bool isDestructiveChange, AlterColumnOperation inverse,
            object anonymousArguments = null)
            : this(table, column, isDestructiveChange, anonymousArguments)
        {
            Check.NotNull(inverse, "inverse");

            _inverse = inverse;
        }

        /// <summary>
        ///     Gets the name of the table that the column belongs to.
        /// </summary>
        public string Table
        {
            get { return _table; }
        }

        /// <summary>
        ///     Gets the new definition for the column.
        /// </summary>
        public ColumnModel Column
        {
            get { return _column; }
        }

        /// <summary>
        ///     Gets an operation that represents reverting the alteration.
        ///     The inverse cannot be automatically calculated,
        ///     if it was not supplied to the constructor this property will return null.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return _inverse; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return _destructiveChange; }
        }
    }
}
