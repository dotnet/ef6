// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents creating a table.
    /// </summary>
    public class CreateTableOperation : MigrationOperation
    {
        private readonly string _name;

        private readonly List<ColumnModel> _columns = new List<ColumnModel>();

        private AddPrimaryKeyOperation _primaryKey;

        /// <summary>
        ///     Initializes a new instance of the CreateTableOperation class.
        /// </summary>
        /// <param name="name"> Name of the table to be created. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateTableOperation(string name, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            _name = name;
        }

        /// <summary>
        ///     Gets the name of the table to be created.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets the columns to be included in the new table.
        /// </summary>
        public virtual ICollection<ColumnModel> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        ///     Gets or sets the primary key for the new table.
        /// </summary>
        public AddPrimaryKeyOperation PrimaryKey
        {
            get { return _primaryKey; }
            set
            {
                Contract.Requires(value != null);

                _primaryKey = value;
                _primaryKey.Table = Name;
            }
        }

        /// <summary>
        ///     Gets an operation to drop the table.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                return new DropTableOperation(Name)
                           {
                               IsSystem = IsSystem
                           };
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
